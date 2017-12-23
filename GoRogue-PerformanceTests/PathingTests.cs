using GoRogue;
using Generators = GoRogue.MapGeneration.Generators;
using GoRogue.Pathing;
using System;
using System.Diagnostics;

namespace GoRogue_PerformanceTests
{
    public static class PathingTests
    {
        public static TimeSpan TimeForSingleSourceDijkstra(int mapWidth, int mapHeight, int iterations)
        {
            Stopwatch s = new Stopwatch();

            var map = new ArrayMapOf<bool>(mapWidth, mapHeight);
            Generators.RectangleMap.Generate(map);

            DijkstraMap dMap = new DijkstraMap(map);

            dMap.AddGoal(5, 5);

            dMap.Calculate(); // warm-up value

            s.Start();
            for (int i = 0; i < iterations; i++)
                dMap.Calculate();

            s.Stop();

            return s.Elapsed;
        }
    }
}