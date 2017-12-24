using System.Collections.Generic;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// Implements function designed to calculate and produce an IEnumerable of MapAreas representing each unique connected
    /// area of the map.
    /// </summary>
    /// <remarks>
    /// The MapAreas function takes in an IMapOf, where a value of true for a given position indicates it should
    /// be part of a map area, and false indicates it should not be part of any map area.  In a classic
    /// roguelike dungeon example, this might be a walkability map where floors return a value of true and walls
    /// a value of false.
    /// </remarks>
    static public class MapAreaFinder
    {
        /// <summary>
        /// Calculates and returns map areas for the given map, given the specified method radius shape, which is used
        /// to determine the proper calculation for distance.
        /// </summary>
        /// <param name="map">IMapOf indicating which cells should be considered part of a map area and which should not.</param>
        /// <param name="shape">The shape of a radius - determines calculation used to define distance, and thus what are considered neighbors.</param>
        static public IEnumerable<MapArea> MapAreas(IMapOf<bool> map, Radius shape) => MapAreas(map, (Distance)shape);

        /// <summary>
        /// Calculates and returns map areas for the given map, given the specified method for calculating distance.
        /// </summary>
        /// <param name="map">IMapOf indicating which cells should be considered part of a map area and which should not.</param>
        /// <param name="distanceCalc">The calculation used to determine distance.</param>
        static public IEnumerable<MapArea> MapAreas(IMapOf<bool> map, Distance distanceCalc)
        {
            var visited = new bool[map.Width, map.Height]; // Defaults to false
            var neighbors = Direction.GetNeighbors(distanceCalc);

            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                {
                    var area = visit(visited, map, neighbors, Coord.Get(x, y));

                    if (area.Count != 0)
                        yield return area;
                }
        }

        private static MapArea visit(bool[,] visited, IMapOf<bool> map, Direction.NeighborsGetter neighbors, Coord position)
        {
            var stack = new Stack<Coord>();
            var area = new MapArea();
            stack.Push(position);

            while (stack.Count != 0)
            {
                position = stack.Pop();
                if (visited[position.X, position.Y] || !map[position]) // Already visited, or not part of any mapArea
                    continue;

                area.Add(position);
                visited[position.X, position.Y] = true;

                foreach (Direction d in neighbors())
                {
                    Coord c = position + d;

                    if (c.X < 0 || c.Y < 0 || c.X >= map.Width || c.Y >= map.Height) // Out of bounds, thus not actually a neighbor
                        continue;

                    if (map[c] && !visited[c.X, c.Y])
                        stack.Push(c);
                }
            }

            return area;
        }
    }
}