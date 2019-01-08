using GoRogue.MapViews;
using GoRogue.MapGeneration;
using System;
using System.Diagnostics;

namespace GoRogue_PerformanceTests
{
	internal static class MapGenerationTests
	{
		public static TimeSpan TimeForRandomRooms(int mapWidth, int mapHeight, int iterations, int maxRooms, int roomMinSize, int roomMaxSize)
		{
			var s = new Stopwatch();
			// For caching
			var mapToGenerate = new ArrayMap<bool>(mapWidth, mapHeight);
			QuickGenerators.GenerateRandomRoomsMap(mapToGenerate, maxRooms, roomMinSize, roomMaxSize);

			s.Start();
			for (int i = 0; i < iterations; i++)
			{
				mapToGenerate = new ArrayMap<bool>(mapWidth, mapHeight);
				QuickGenerators.GenerateRandomRoomsMap(mapToGenerate, maxRooms, roomMinSize, roomMaxSize);
			}
			s.Stop();

			return s.Elapsed;
		}
	}
}
