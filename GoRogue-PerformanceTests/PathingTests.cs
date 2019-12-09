using GoRogue;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using GoRogue.Pathing;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GoRogue_PerformanceTests
{
	public static class PathingTests
	{
		//private static Point END = new Point(17, 14);
		private static Point START = new Point(1, 2);
		private static Point END = new Point(490,490);

		public static TimeSpan TimeForAStar(int mapWidth, int mapHeight, int iterations)
		{
			var s = new Stopwatch();

			var map = new ArrayMap<bool>(mapWidth, mapHeight);
			QuickGenerators.GenerateRectangleMap(map);

			var pather = new AStar(map, Distance.CHEBYSHEV);
			var path = pather.ShortestPath(START, END); // Cache warmup

			s.Start();
			for (int i = 0; i < iterations; i++)
				path = pather.ShortestPath(START, END);
			s.Stop();

			return s.Elapsed;
		}

		public static TimeSpan TimeForFastAStar(int mapWidth, int mapHeight, int iterations)
		{
			var s = new Stopwatch();

			var map = new ArrayMap<bool>(mapWidth, mapHeight);
			QuickGenerators.GenerateRectangleMap(map);

			var pather = new FastAStar(map, Distance.CHEBYSHEV);
			var path = pather.ShortestPath(START, END); // Cache warmup

			s.Start();
			for (int i = 0; i < iterations; i++)
				path = pather.ShortestPath(START, END);
			s.Stop();

			return s.Elapsed;
		}

		public static TimeSpan TimeForFleeMap(IMapView<bool> map, IEnumerable<Point> goals, int iterations)
		{
			Stopwatch s = new Stopwatch();

			var mapGoals = createGoalStateMap(map, goals);
			var mapBuilder = new GoalMap(mapGoals, Distance.CHEBYSHEV);
			var fleeMap = new FleeMap(mapBuilder);

			mapBuilder.Update();

			s.Start();
			for (int i = 0; i < iterations; i++)
				mapBuilder.Update();
			s.Stop();

			return s.Elapsed;
		}

		public static TimeSpan TimeForGoalMap(IMapView<bool> map, IEnumerable<Point> goals, int iterations)
		{
			Stopwatch s = new Stopwatch();

			var mapGoals = createGoalStateMap(map, goals);
			var mapBuilder = new GoalMap(mapGoals, Distance.CHEBYSHEV);

			mapBuilder.Update();

			s.Start();
			for (int i = 0; i < iterations; i++)
				mapBuilder.Update();
			s.Stop();

			return s.Elapsed;
		}

		private static IMapView<GoalState> createGoalStateMap(IMapView<bool> walkabilityMap, IEnumerable<Point> goals)
		{
			var mapGoals = new ArrayMap<GoalState>(walkabilityMap.Width, walkabilityMap.Height);
			for (int x = 0; x < walkabilityMap.Width; x++)
				for (int y = 0; y < walkabilityMap.Height; y++)
					mapGoals[x, y] = walkabilityMap[x, y] ? GoalState.Clear : GoalState.Obstacle;

			foreach (var goal in goals)
				mapGoals[goal] = GoalState.Goal;

			return mapGoals;
		}
	}
}
