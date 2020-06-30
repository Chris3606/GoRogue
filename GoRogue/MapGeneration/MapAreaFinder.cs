using System;
using System.Collections.Generic;
using GoRogue.MapViews;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// Class designed to calculate and produce a list of MapAreas representing each unique connected
    /// area of the map.
    /// </summary>
    /// <remarks>
    /// The class takes in an <see cref="IMapView{T}" />, where a value of true for a given position indicates it
    /// should be part of a map area, and false indicates it should not be part of any map area. In a
    /// classic roguelike dungeon example, this might be a walkability map where floors return a
    /// value of true and walls a value of false.
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
        /// Map view indicating which cells should be considered part of a map area and which should not.
        /// </summary>
        public IMapView<bool> Map;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="map">
        /// Map view indicating which cells should be considered part of a map area and which should not.
        /// </param>
        /// <param name="adjacencyMethod">The method used for determining connectivity of the grid.</param>
        public MapAreaFinder(IMapView<bool> map, AdjacencyRule adjacencyMethod)
        {
            Map = map;
            _visited = null;
            AdjacencyMethod = adjacencyMethod;
        }

        /// <summary>
        /// Convenience function that creates a MapAreaFinder and returns the result of that
        /// instances <see cref="MapAreas" /> function. Intended to be used for cases in which the area finder
        /// will never be re-used.
        /// </summary>
        /// <param name="map">
        /// Map view indicating which cells should be considered part of a map area and which should not.
        /// </param>
        /// <param name="adjacencyMethod">The method used for determining connectivity of the grid.</param>
        /// <returns>An IEnumerable of each (unique) map area.</returns>
        public static IEnumerable<Area> MapAreasFor(IMapView<bool> map, AdjacencyRule adjacencyMethod)
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
            if (_visited == null || _visited.GetLength(1) != Map.Height || _visited.GetLength(0) != Map.Width)
                _visited = new bool[Map.Width, Map.Height];
            else
                Array.Clear(_visited, 0, _visited.Length);

            for (var x = 0; x < Map.Width; x++)
                for (var y = 0; y < Map.Height; y++)
                {
                    var area = Visit(new Point(x, y));

                    if (area != null && area.Count != 0)
                        yield return area;
                }
        }

        private Area? Visit(Point position)
        {
            // Don't bother allocating a MapArea, because the starting point isn't valid.
            if (!Map[position])
                return null;

            var stack = new Stack<Point>();
            var area = new Area();
            stack.Push(position);

            while (stack.Count != 0)
            {
                position = stack.Pop();
                if (_visited![position.X, position.Y] || !Map[position]
                ) // Already visited, or not part of any mapArea.  Also only called from functions that have allocated visited
                    continue;

                area.Add(position);
                _visited[position.X, position.Y] = true;

                foreach (var c in AdjacencyMethod.Neighbors(position))
                {
                    if (c.X < 0 || c.Y < 0 || c.X >= Map.Width || c.Y >= Map.Height
                    ) // Out of bounds, thus not actually a neighbor
                        continue;

                    if (Map[c] && !_visited[c.X, c.Y])
                        stack.Push(c);
                }
            }

            return area;
        }
    }
}
