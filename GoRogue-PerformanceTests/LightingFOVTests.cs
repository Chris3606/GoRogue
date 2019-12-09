using GoRogue;
using GoRogue.MapViews;
using GoRogue.MapGeneration;
using GoRogue.SenseMapping;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GoRogue_PerformanceTests
{
	public static class LightingFOVTests
	{
		public static long MemorySingleLightSourceFOV(int mapWidth, int mapHeight, int lightRadius)
		{
			FOV fov;
			long startingMem, endingMem;
			var map = new ArrayMap<bool>(mapWidth, mapHeight);
			QuickGenerators.GenerateRectangleMap(map);

			// Start mem test
			startingMem = GC.GetTotalMemory(true);
			fov = new FOV(map);
			fov.Calculate(5, 6, lightRadius, Radius.CIRCLE); // Must calculate to force allocations
			endingMem = GC.GetTotalMemory(true);

			return endingMem - startingMem;
		}

		public static long MemorySingleLightSourceLighting(int mapWidth, int mapHeight, int lightRadius)
		{
			SenseMap fov;
			long startingMem, endingMem;
			ArrayMap<double> map = rectangleMap(mapWidth, mapHeight);

			// Start mem test
			startingMem = GC.GetTotalMemory(true);
			fov = new SenseMap(map);
			fov.AddSenseSource(new SenseSource(SourceType.SHADOW, new Point(5, 6), lightRadius, Radius.CIRCLE));
			endingMem = GC.GetTotalMemory(true);
			return endingMem - startingMem;
		}

		public static TimeSpan TimeForNSourcesLighting(int mapWidth, int mapHeight, int lightRadius, int iterations, int lights)
		{
			Stopwatch s = new Stopwatch();
			var map = rectangleMap(mapWidth, mapHeight);
			var fov = new SenseMap(map);

			Point c = new Point(5, 6);
			for (int i = 0; i < lights; i++)
			{
				fov.AddSenseSource(new SenseSource(SourceType.RIPPLE, c, lightRadius, Radius.CIRCLE));
				c += 5;
			}

			// Warm-up for processor, stabilizes cache performance. Also makes it a fair test against
			// fov since we have to do this to force the first memory allocation
			fov.Calculate();

			// Calculate and test
			s.Start();
			for (int i = 0; i < iterations; i++)
			{
				fov.Calculate();
			}
			s.Stop();

			return s.Elapsed;
		}

		public static TimeSpan TimeForSingleLightSourceFOV(int mapWidth, int mapHeight, int lightRadius, int iterations)
		{
			Stopwatch s = new Stopwatch();
			var map = new ArrayMap<bool>(mapWidth, mapHeight);
			QuickGenerators.GenerateRectangleMap(map);
			var fov = new FOV(map);
			// Warm-up for processor, stabilizes cache performance. Also makes it a fair test against
			// fov since we have to do this to force the first memory allocation
			fov.Calculate(5, 6, lightRadius, Radius.CIRCLE);

			// Calculate and test
			s.Start();
			for (int i = 0; i < iterations; i++)
			{
				fov.Calculate(5, 6, lightRadius, Radius.CIRCLE);
			}
			s.Stop();

			return s.Elapsed;
		}

		public static TimeSpan TimeForSingleLightSourceLighting(int mapWidth, int mapHeight, SourceType sourceType, int lightRadius, Radius radiusStrat, int iterations)
		{
			Stopwatch s = new Stopwatch();
			var map = rectangleMap(mapWidth, mapHeight);
			var fov = new SenseMap(map);
			fov.AddSenseSource(new SenseSource(sourceType, new Point(5, 6), lightRadius, radiusStrat));
			// Warm-up for processor, stabilizes cache performance. Also makes it a fair test against
			// fov since we have to do this to force the first memory allocation
			fov.Calculate();

			// Calculate and test
			s.Start();
			for (int i = 0; i < iterations; i++)
			{
				fov.Calculate();
			}
			s.Stop();

			return s.Elapsed;
		}

		// Simulation of function that should probably actually be in FOV
		private static double[,] addFovs(double[,] map1, double[,] map2)
		{
			//for (int x = 0; x < map1.GetLength(0); x++)
			Parallel.For(0, map1.GetLength(0), x =>
			{
				for (int y = 0; y < map1.GetLength(1); y++)
				//Parallel.For(0, map1.GetLength(1), y =>
				{
					map1[x, y] += map2[x, y];
				}//);
			});

			return map1;
		}

		private static ArrayMap<double> rectangleMap(int mapWidth, int mapHeight)
		{
			var map = new ArrayMap<double>(mapWidth, mapHeight);
			for (int i = 0; i < map.Width; i++)
				for (int j = 0; j < map.Height; j++)
				{
					if (i == 0 || j == 0 || i == map.Width - 1 || j == map.Height - 1)
						map[i, j] = 1.0;
					else
						map[i, j] = 0.0;
				}
			return map;
		}
	}
}
