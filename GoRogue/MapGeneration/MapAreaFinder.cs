using System;
using System.Collections.Generic;
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

        /// <summary>
        /// The method used for determining connectivity of the grid.
        /// </summary>
        public AdjacencyRule AdjacencyMethod;

        /// <summary>
        /// Grid view indicating which cells should be considered part of a map area and which should not.
        /// </summary>
        public IGridView<bool> _areasView;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="areasView">
        /// Grid view indicating which cells should be considered part of a map area and which should not.
        /// </param>
        /// <param name="adjacencyMethod">The method used for determining connectivity of the grid.</param>
        public MapAreaFinder(IGridView<bool> areasView, AdjacencyRule adjacencyMethod)
        {
            _areasView = areasView;
            _visited = null;
            AdjacencyMethod = adjacencyMethod;
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
        /// <returns>An IEnumerable of each (unique) map area.</returns>
        public static IEnumerable<Area> MapAreasFor(IGridView<bool> map, AdjacencyRule adjacencyMethod)
        {
            var areaFinder = new MapAreaFinder(map, adjacencyMethod);
            return areaFinder.MapAreas();
        }

        /// <summary>
        /// Calculates the list of map areas, returning each unique map area.
        /// </summary>
        /// <returns>An IEnumerable of each (unique) map area.</returns>
        public IEnumerable<Area> MapAreas()
        {
            if (_visited == null || _visited.GetLength(1) != _areasView.Height || _visited.GetLength(0) != _areasView.Width)
                _visited = new bool[_areasView.Width, _areasView.Height];
            else
                Array.Clear(_visited, 0, _visited.Length);

            for (var x = 0; x < _areasView.Width; x++)
                for (var y = 0; y < _areasView.Height; y++)
                {
                    var area = Visit(new Point(x, y));

                    if (!ReferenceEquals(null, area) && area.Count != 0)
                        yield return area;
                }
        }

        private Area? Visit(Point position)
        {
            // Don't bother allocating a MapArea, because the starting point isn't valid.
            if (!_areasView[position])
                return null;

            var stack = new Stack<Point>();
            var area = new Area();
            stack.Push(position);

            while (stack.Count != 0)
            {
                position = stack.Pop();
                // Already visited, or not part of any mapArea.  Also only called from functions that have allocated
                // visited
                if (_visited![position.X, position.Y] || !_areasView[position])
                    continue;

                area.Add(position);
                _visited[position.X, position.Y] = true;

                foreach (var c in AdjacencyMethod.Neighbors(position))
                {
                    // Out of bounds, thus not actually a neighbor
                    if (c.X < 0 || c.Y < 0 || c.X >= _areasView.Width || c.Y >= _areasView.Height)
                        continue;

                    if (_areasView[c] && !_visited[c.X, c.Y])
                        stack.Push(c);
                }
            }

            return area;
        }
    }
}
