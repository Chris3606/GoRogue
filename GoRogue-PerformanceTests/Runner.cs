﻿using GoRogue;
using GoRogue.SenseMapping;

namespace GoRogue_PerformanceTests
{
	public class Runner
	{
		public static readonly int ITERATIONS_FOR_TIMING = 100;
		public static readonly int LIGHT_RADIUS = 10;
		public static readonly Coord LINE_END = new Coord(3, 5);
		public static readonly Coord LINE_START = new Coord(29, 23);
		public static readonly int MAP_HEIGHT = 500;
		public static readonly int MAP_WIDTH = 500;
		public static readonly int NUM_EFFECTS = 1000;
		public static readonly int NUM_GOALS = 5;
		public static readonly Radius RADIUS_STRATEGY = Radius.CIRCLE;
		public static readonly int RANDROOMS_MAX_ROOMS = 30;
		public static readonly int RANDROOMS_MAX_SIZE = 15;
		public static readonly int RANDROOMS_MIN_SIZE = 4;
		public static readonly SourceType SOURCE_TYPE = SourceType.SHADOW;
		public static bool Quit = false;

		private static void Main()
		{
			while (!Quit)
				new PerfTests().Run();
		}
	}
}