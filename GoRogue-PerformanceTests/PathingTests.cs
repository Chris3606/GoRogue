using GoRogue;
using GoRogue.MapViews;
using GoRogue.Pathing;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Generators = GoRogue.MapGeneration.Generators;

namespace GoRogue_PerformanceTests
{

    public static class PathingTests
    {
        private static Coord END = Coord.Get(17, 14);
        private static Coord START = Coord.Get(1, 2);

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

        public static TimeSpan TimeForSingleSourceDijkstra(IMapView<bool> map, Coord goal, int iterations)
        {
            Stopwatch s = new Stopwatch();

            DijkstraMap dMap = new DijkstraMap(map);

            dMap.AddGoal(goal.X, goal.Y);

            dMap.Calculate(); // warm-up value

            s.Start();
            for (int i = 0; i < iterations; i++)
                dMap.Calculate();

            s.Stop();

            return s.Elapsed;
        }

        public static TimeSpan TimeForSingleSourceGoalMap(IMapView<bool> map, Coord goal, int iterations)
        {
            Stopwatch s = new Stopwatch();

            var mapGoals = new ArrayMap<GoalState>(map.Width, map.Height);
            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                    mapGoals[x, y] = map[x, y] ? GoalState.Clear : GoalState.Obstacle;

                mapGoals[goal] = GoalState.Goal;

            var mapBuilder = new GoalMap<GoalState>(mapGoals, (c, e) => c); ;
            mapBuilder.Update();

            s.Start();
            for (int i = 0; i < iterations; i++)
                mapBuilder.Update();
            s.Stop();

            return s.Elapsed;
        }

        public static TimeSpan TimeForMultiSourceDijkstra(IMapView<bool> map, IEnumerable<Coord> goals, int iterations)
        {
            Stopwatch s = new Stopwatch();

            DijkstraMap dMap = new DijkstraMap(map);

            foreach (var goal in goals)
                dMap.AddGoal(goal.X, goal.Y);

            dMap.Calculate(); // warm-up value

            s.Start();
            for (int i = 0; i < iterations; i++)
                dMap.Calculate();
            s.Stop();

            return s.Elapsed;
        }

        public static TimeSpan TimeForMultiSourceGoalMap(IMapView<bool> map, IEnumerable<Coord> goals, int iterations)
        {
            Stopwatch s = new Stopwatch();

            var mapGoals = new ArrayMap<GoalState>(map.Width, map.Height);
            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                    mapGoals[x, y] = map[x, y] ? GoalState.Clear : GoalState.Obstacle;

            foreach (var goal in goals)
                mapGoals[goal] = GoalState.Goal;

            var mapBuilder = new GoalMap<GoalState>(mapGoals, (c, e) => c);
            
            mapBuilder.Update();

            s.Start();
            for (int i = 0; i < iterations; i++)
                mapBuilder.Update();
            s.Stop();

            return s.Elapsed;
        } 
    }
}