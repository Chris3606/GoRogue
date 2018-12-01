using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using GoRogue.MapViews;

namespace GoRogue_PerformanceTests
{
	public static class MapViewTests
	{
		public static TimeSpan TimeForLambdaTranslationMap1ParamAccess(int mapWidth, int mapHeight, int iterations)
		{
			var s = new Stopwatch();

			var map = new ArrayMap<int>(mapWidth, mapHeight);
			foreach (var pos in map.Positions())
				map[pos] = pos.ToIndex(mapWidth);

			var lambdaTranslationMap = new LambdaTranslationMap<int, double>(map, i => i / 100.0);

			// For caching
			double val;
			foreach (var pos in lambdaTranslationMap.Positions())
				val = lambdaTranslationMap[pos];

			s.Start();
			for (int i = 0; i < iterations; i++)
				foreach (var pos in lambdaTranslationMap.Positions())
					val = lambdaTranslationMap[pos];
			s.Stop();

			return s.Elapsed;
		}

		public static TimeSpan TimeForLambdaTranslationMap2ParamAccess(int mapWidth, int mapHeight, int iterations)
		{
			var s = new Stopwatch();

			var map = new ArrayMap<int>(mapWidth, mapHeight);
			foreach (var pos in map.Positions())
				map[pos] = pos.ToIndex(mapWidth);

			var lambdaTranslationMap = new LambdaTranslationMap<int, double>(map, (c, i) => i / 100.0);

			// For caching
			double val;
			foreach (var pos in lambdaTranslationMap.Positions())
				val = lambdaTranslationMap[pos];

			s.Start();
			for (int i = 0; i < iterations; i++)
				foreach (var pos in lambdaTranslationMap.Positions())
					val = lambdaTranslationMap[pos];
			s.Stop();

			return s.Elapsed;
		}
	}
}
