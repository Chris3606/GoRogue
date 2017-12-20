using System;
using System.Collections.Generic;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// Class designed to calculate and produce a list of MapAreas representing each unique connected
    /// area of the map.
    /// </summary>
    /// <remarks>
    /// The class takes in an IMapOf, where a value of true for a given position indicates it should
    /// be part of a map area, and false indicates it should not be part of any map area.  In a classic
    /// roguelike dungeon example, this might be a walkability map where floors return a value of true and walls
    /// a value of false.
    /// 
    /// Currently, the class only supports manhattan (4-way) connectivity, but will in the future support
    /// 8-way connectivity.
    /// </remarks>
    public class MapAreaFinder
    {
        private IMapOf<bool> map;

        private List<MapArea> _mapAreas;
        /// <summary>
        /// List of (unique) areas in the currently calculated area.
        /// </summary>
        public IList<MapArea> MapAreas { get { return _mapAreas.AsReadOnly(); } }

        /// <summary>
        /// Number of (unique) map areas in the currently calculated list.
        /// </summary>
        public int Count { get { return _mapAreas.Count; } }

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
        /// <param name="map">IMapOf indicating which cells should be considered part of a map area and which should not.</param>
        /// <param name="shape">The shape of a radius - determines calculation used to define distance, and thus what are considered neighbors.</param>
        public MapAreaFinder(IMapOf<bool> map, Radius shape)
            : this(map, (Distance)shape) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="map">IMapOf indicating which cells should be considered part of a map area and which should not.</param>
        /// <param name="distanceCalc">The calculation used to determine distance.</param>
        public MapAreaFinder(IMapOf<bool> map, Distance distanceCalc)
        {
            this.map = map;
            _mapAreas = new List<MapArea>();
            visited = null;
            DistanceCalc = distanceCalc;
        }

        /// <summary>
        /// Calculates the list of map areas, adding each unique map area to the MapAreas list.
        /// </summary>
        public void FindMapAreas()
        {
            _mapAreas.Clear();
            if (visited == null || visited.GetLength(1) != map.Height || visited.GetLength(0) != map.Width)
                visited = new bool[map.Width, map.Height];
            else
                Array.Clear(visited, 0, visited.Length);

            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                {
                    var area = visit(Coord.Get(x, y));

                    if (area.Count != 0)
                        _mapAreas.Add(area);
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