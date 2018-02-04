using System;
using System.Collections.Generic;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// Class designed to calculate and produce a list of MapAreas representing each unique connected
    /// area of the map.
    /// </summary>
    /// <remarks>
    /// The class takes in an IMapView, where a value of true for a given position indicates it should
    /// be part of a map area, and false indicates it should not be part of any map area.  In a classic
    /// roguelike dungeon example, this might be a walkability map where floors return a value of true and walls
    /// a value of false.
    /// </remarks>
    public class MapAreaFinder
    {
        /// <summary>
        /// IMapView indicating which cells should be considered part of a map area and which should not.
        /// </summary>
        public IMapView<bool> Map;

        private bool[,] visited;
        private Distance _distanceCalc;

        /// <summary>
        /// The calculation used to determine distance between two points.
        /// </summary>
        public Distance DistanceCalc
        {
            get => _distanceCalc;

            set
            {
                _distanceCalc = value;
                neighbors = Direction.GetNeighbors(value);
            }
        }

        private Direction.NeighborsGetter neighbors;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="map">IMapView indicating which cells should be considered part of a map area and which should not.</param>
        /// <param name="shape">The shape of a radius - determines calculation used to define distance, and thus what are considered neighbors.</param>
        public MapAreaFinder(IMapView<bool> map, Radius shape)
            : this(map, (Distance)shape) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="map">IMapView indicating which cells should be considered part of a map area and which should not.</param>
        /// <param name="distanceCalc">The calculation used to determine distance.</param>
        public MapAreaFinder(IMapView<bool> map, Distance distanceCalc)
        {
            Map = map;
            visited = null;
            DistanceCalc = distanceCalc;
        }

        /// <summary>
        /// Calculates the list of map areas, returning each unique map area.
        /// </summary>
        /// <returns>An IEnumerable of each (unique) map area.</returns>
        public IEnumerable<MapArea> MapAreas()
        {
            if (visited == null || visited.GetLength(1) != Map.Height || visited.GetLength(0) != Map.Width)
                visited = new bool[Map.Width, Map.Height];
            else
                Array.Clear(visited, 0, visited.Length);

            for (int x = 0; x < Map.Width; x++)
                for (int y = 0; y < Map.Height; y++)
                {
                    var area = visit(Coord.Get(x, y));

                    if (area.Count != 0)
                        yield return area;
                }
        }

        private MapArea visit(Coord position)
        {
            var stack = new Stack<Coord>();
            var area = new MapArea();
            stack.Push(position);

            while (stack.Count != 0)
            {
                position = stack.Pop();
                if (visited[position.X, position.Y] || !Map[position]) // Already visited, or not part of any mapArea
                    continue;

                area.Add(position);
                visited[position.X, position.Y] = true;

                foreach (Direction d in neighbors())
                {
                    Coord c = position + d;

                    if (c.X < 0 || c.Y < 0 || c.X >= Map.Width || c.Y >= Map.Height) // Out of bounds, thus not actually a neighbor
                        continue;

                    if (Map[c] && !visited[c.X, c.Y])
                        stack.Push(c);
                }
            }

            return area;
        }

        /// <summary>
        /// Convenience function that creates an MapAreaFinder and returns the result of that MapAreaFinder's MapAreas function.
        /// Intended to be used for cases in which the area finder will never be re-used.
        /// </summary>
        /// <param name="map">IMapView indicating which cells should be considered part of a map area and which should not.</param>
        /// <param name="shape"></param>
        /// <returns>An IEnumerable of each (unique) map area.</returns>
        static public IEnumerable<MapArea> MapAreasFor(IMapView<bool> map, Radius shape) => MapAreasFor(map, (Distance)shape);

        /// <summary>
        /// Convenience function that creates an MapAreaFinder and returns the result of that MapAreaFinder's MapAreas function.
        /// Intended to be used for cases in which the area finder will never be re-used.
        /// </summary>
        /// <param name="map">IMapView indicating which cells should be considered part of a map area and which should not.</param>
        /// <param name="distanceCalc">The calculation used to determine distance.</param>
        /// <returns>An IEnumerable of each (unique) map area.</returns>
        static public IEnumerable<MapArea> MapAreasFor(IMapView<bool> map, Distance distanceCalc)
        {
            var areaFinder = new MapAreaFinder(map, distanceCalc);
            return areaFinder.MapAreas();
        }
    }
}