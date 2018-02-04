using GoRogue;
using GoRogue.Pathing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Generators = GoRogue.MapGeneration.Generators;

namespace GoRogue_PerformanceTests
{
    public static class PathingTests
    {
        private static Coord START = Coord.Get(1, 2);
        private static Coord END = Coord.Get(17, 14);

        public static TimeSpan TimeForSingleSourceDijkstra(int mapWidth, int mapHeight, int iterations)
        {
            Stopwatch s = new Stopwatch();

            var map = new ArrayMap<bool>(mapWidth, mapHeight);
            Generators.RectangleMapGenerator.Generate(map);

            DijkstraMap dMap = new DijkstraMap(map);

            dMap.AddGoal(5, 5);

            dMap.Calculate(); // warm-up value

            s.Start();
            for (int i = 0; i < iterations; i++)
                dMap.Calculate();

            s.Stop();

            return s.Elapsed;
        }

        public static TimeSpan TimeForAStar(int mapWidth, int mapHeight, int iterations)
        {
            var s = new Stopwatch();

            var map = new ArrayMap<bool>(mapWidth, mapHeight);
            Generators.RectangleMapGenerator.Generate(map);

            var pather = new AStar(map, Distance.CHEBYSHEV);
            var path = pather.ShortestPath(START, END); // Cache warmup

            s.Start();
            for (int i = 0; i < iterations; i++)
                path = pather.ShortestPath(START, END);
            s.Stop();

            return s.Elapsed;
        }
    }
}