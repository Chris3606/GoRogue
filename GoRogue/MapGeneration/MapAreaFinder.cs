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
    /// The class takes in an <see cref="IGridView{T}" />, where a value of true for a given position indicates it
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
        /// _grid view indicating which cells should be considered part of a map area and which should not.
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
        /// Calculates the list of map areas, returning each unique map area.
        /// </summary>
        /// <returns>An IEnumerable of each (unique) map area.</returns>
        public IEnumerable<Area> MapAreas()
        {
            if (_visited == null || _visited.GetLength(1) != AreasView.Height || _visited.GetLength(0) != AreasView.Width)
                _visited = new bool[AreasView.Width, AreasView.Height];
            else
                Array.Clear(_visited, 0, _visited.Length);

            for (var x = 0; x < AreasView.Width; x++)
                for (var y = 0; y < AreasView.Height; y++)
                {
                    var area = Visit(new Point(x, y));

                    if (!ReferenceEquals(null, area) && area.Count != 0)
                        yield return area;
                }
        }

        private Area? Visit(Point position)
        {
            // NOTE: This function can safely assume that _visited is NOT null, as this is enforced
            // by every public function that calls this one.

            // Don't bother allocating a MapArea, because the starting point isn't valid (either not in any area,
            // or already in another one found.
            if (!AreasView[position] || _visited![position.X, position.Y])
                return null;

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
