using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// Class designed to calculate and produce a list of Areas representing each unique connected
    /// area of the map.
    /// </summary>
    /// <remarks>
    /// The class takes in an <see cref="SadRogue.Primitives.GridViews.IGridView{T}" />, where a value of true for a given position indicates it
    /// should be part of a map area, and false indicates it should not be part of any map area. In a
    /// classic roguelike dungeon example, this might be a view of "walkability" where floors return a
    /// value of true and walls return a value of false.
    /// </remarks>
    [PublicAPI]
    public class MapAreaFinder
    {
        private bool[,]? _visited;
        private Direction[] _adjacentDirs = null!;

        private AdjacencyRule _adjacencyMethod;
        /// <summary>
        /// The method used for determining connectivity of the grid.
        /// </summary>
        public AdjacencyRule AdjacencyMethod
        {
            get => _adjacencyMethod;
            set
            {
                _adjacencyMethod = value;
                _adjacentDirs = _adjacencyMethod.DirectionsOfNeighbors().ToArray();
            }
        }

        /// <summary>
        /// Point hashing algorithm to use for the areas created.  If set to null, the default point hashing algorithm
        /// will be used.
        /// </summary>
        public IEqualityComparer<Point>? PointHasher;

        /// <summary>
        /// Grid view indicating which cells should be considered part of a map area and which should not.
        /// </summary>
        public IGridView<bool> AreasView;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="areasView">
        /// Grid view indicating which cells should be considered part of a map area and which should not.
        /// </param>
        /// <param name="adjacencyMethod">The method used for determining connectivity of the grid.</param>
        /// <param name="pointHasher">
        /// Point hashing algorithm to use for the areas created.  If set to null the default point hashing algorithm
        /// will be used.
        /// </param>
        public MapAreaFinder(IGridView<bool> areasView, AdjacencyRule adjacencyMethod, IEqualityComparer<Point>? pointHasher = null)
        {
            AreasView = areasView;
            _visited = null;
            AdjacencyMethod = adjacencyMethod;
            PointHasher = pointHasher;
        }

        /// <summary>
        /// Convenience function that creates a MapAreaFinder and returns the result of that
        /// instances <see cref="MapAreas" /> function. Intended to be used for cases in which the area finder
        /// will never be re-used.
        /// </summary>
        /// <param name="map">
        /// Grid view indicating which cells should be considered part of a map area and which should not.
        /// </param>
        /// <param name="adjacencyMethod">The method used for determining connectivity of the grid.</param>
        /// <param name="pointHasher">
        /// Point hashing algorithm to use for the areas created.  If set to null the default point hashing algorithm
        /// will be used.
        /// </param>
        /// <returns>An IEnumerable of each (unique) map area.</returns>
        public static IEnumerable<Area> MapAreasFor(IGridView<bool> map, AdjacencyRule adjacencyMethod, IEqualityComparer<Point>? pointHasher = null)
        {
            var areaFinder = new MapAreaFinder(map, adjacencyMethod, pointHasher);
            return areaFinder.MapAreas();
        }

        /// <summary>
        /// Convenience function that creates a MapAreaFinder and returns the result of that
        /// instance's <see cref="FillFrom(Point, bool)" /> function. Intended to be used for cases
        /// in which the area finder will never be re-used.
        /// </summary>
        /// <param name="map">
        /// Grid view indicating which cells should be considered part of a map area and which should not.
        /// </param>
        /// <param name="adjacencyMethod">The method used for determining connectivity of the grid.</param>
        /// <param name="position">The position to start from.</param>
        /// <param name="pointHasher">
        /// Point hashing algorithm to use for the areas created.  If set to null the default point hashing algorithm
        /// will be used.
        /// </param>
        /// <returns>An IEnumerable of each (unique) map area.</returns>
        public static Area? FillFrom(IGridView<bool> map, AdjacencyRule adjacencyMethod, Point position,
                                     IEqualityComparer<Point>? pointHasher = null)
        {
            var areaFinder = new MapAreaFinder(map, adjacencyMethod, pointHasher);
            return areaFinder.FillFrom(position);
        }

        /// <summary>
        /// Calculates the list of map areas, returning each unique map area.
        /// </summary>
        /// <param name="clearVisited">
        /// Whether or not to reset all cells to unvisited before finding areas.  Visited positions cannot be included
        /// in any of the resulting areas.
        /// </param>
        /// <returns>An IEnumerable of each (unique) map area.</returns>
        public IEnumerable<Area> MapAreas(bool clearVisited = true)
        {
            CheckAndResetVisited(clearVisited);

            for (var x = 0; x < AreasView.Width; x++)
                for (var y = 0; y < AreasView.Height; y++)
                {
                    // Don't bother with a function call or any allocation, because the starting point isn't valid
                    // (either can't be in any area, or already in another one found).
                    var position = new Point(x, y);
                    if (AreasView[position] && !_visited![position.X, position.Y])
                        yield return Visit(new Point(x, y));
                }
        }

        /// <summary>
        /// Calculates and returns an area representing every point connected to the start point given.
        /// </summary>
        /// <param name="position">Position to start from.</param>
        /// <param name="clearVisited">
        /// Whether or not to reset all cells to unvisited before finding areas.  Visited positions cannot be included
        /// in the resulting area.
        /// </param>
        /// <returns>
        /// An area representing every point connected to the start point given, or null if there is no
        /// valid area starting from that point.
        /// </returns>
        public Area? FillFrom(Point position, bool clearVisited = true)
        {
            if (!AreasView[position] || _visited != null && _visited[position.X, position.Y])
                return null;

            CheckAndResetVisited(clearVisited);

            return Visit(position);
        }

        /// <summary>
        /// Resets all positions to "unvisited".  Called automatically if area-finding algorithms have the reset flag
        /// set to true.
        /// </summary>
        public void ResetVisitedPositions()
        {
            if (_visited == null || _visited.GetLength(1) != AreasView.Height || _visited.GetLength(0) != AreasView.Width)
                _visited = new bool[AreasView.Width, AreasView.Height];
            else
                Array.Clear(_visited, 0, _visited.Length);
        }

        private void CheckAndResetVisited(bool canClearVisited)
        {
            if (canClearVisited)
                ResetVisitedPositions();
            else if (_visited == null) // Allocate
                _visited = new bool[AreasView.Width, AreasView.Height];
            else if (_visited.GetLength(1) != AreasView.Height || _visited.GetLength(0) != AreasView.Width)
                throw new ArgumentException(
                    "Fill algorithm not set to clear visited, but the map view size has changed since it was allocated.", nameof(canClearVisited));
        }

        private Area Visit(Point position)
        {
            // NOTE: This function can safely assume that _visited is NOT null, as this is enforced
            // by every public function that calls this one.

            var stack = new Stack<Point>();
            var area = new Area(PointHasher);
            stack.Push(position);

            while (stack.Count != 0)
            {
                position = stack.Pop();
                // Already visited, or not part of any mapArea (eg. visited since it was added via another path)
                if (_visited![position.X, position.Y] || !AreasView[position])
                    continue;

                area.Add(position);
                _visited[position.X, position.Y] = true;

                for (int i = 0; i < _adjacentDirs.Length; i++)
                {
                    var c = position + _adjacentDirs[i];

                    // Out of bounds, thus not actually a neighbor
                    if (c.X < 0 || c.Y < 0 || c.X >= AreasView.Width || c.Y >= AreasView.Height)
                        continue;

                    if (AreasView[c] && !_visited[c.X, c.Y])
                        stack.Push(c);
                }
            }

            return area;
        }
    }
}
