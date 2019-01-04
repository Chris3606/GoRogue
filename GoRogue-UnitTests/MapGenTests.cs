using GoRogue;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Troschuetz.Random.Generators;

namespace GoRogue_UnitTests
{
	[TestClass]
	public class MapGenTests
	{
		[TestMethod]
		public void ManualTestCellAutoGen()
		{
			var random = new StandardGenerator();
			var map = new ArrayMap<bool>(80, 50);
			QuickGenerators.GenerateCellularAutomataMap(map, random, 40, 7, 4);

			displayMap(map);

			// TODO: Asserts
		}

		[TestMethod]
		public void ManualTestRandomRoomsGen()
		{
			var random = new StandardGenerator();
			var map = new ArrayMap<bool>(80, 50);
			QuickGenerators.GenerateRandomRoomsMap(map, random, 10, 4, 15, 5);

			displayMap(map);
			// TODO: Some assert here
		}

		[TestMethod]
		public void ManualTestDungeonMazeGen()
		{
			var random = new StandardGenerator(12345);
			//GoRogue.Random.SingletonRandom.DefaultRNG = new StandardGenerator(12345);
			var map = new ArrayMap<bool>(80, 50);
			QuickGenerators.GenerateDungeonMazeMap(map, random, 4, 10, 4, 7);
			//QuickGenerators.GenerateDungeonMazeMap(map, GoRogue.Random.SingletonRandom.DefaultRNG, 4, 10, 4, 7);

			displayMap(map);
			// TODO: Some assert here
		}

		[TestMethod]
		public void TestCellAutoConnectivityAndEnclosure()
		{
			var random = new StandardGenerator();
			var map = new ArrayMap<bool>(80, 50);
			QuickGenerators.GenerateCellularAutomataMap(map, random, 40, 7, 4);

			for (int i = 0; i < 500; i++)
			{
				QuickGenerators.GenerateCellularAutomataMap(map, random, 40, 7, 4);

				// Ensure it's connected
				var areas = MapAreaFinder.MapAreasFor(map, Distance.MANHATTAN).ToList();
				Assert.AreEqual(1, areas.Count);

				// Ensure it's enclosed
				for (int x = 0; x < map.Width; x++)
				{
					Assert.AreEqual(false, map[x, 0]);
					Assert.AreEqual(false, map[x, map.Height - 1]);
				}
				for (int y = 0; y < map.Height; y++)
				{
					Assert.AreEqual(false, map[0, y]);
					Assert.AreEqual(false, map[map.Width - 1, y]);
				}
			}
		}

		[TestMethod]
		public void TestRandomRoomsGenSize()
		{
			var map = new ArrayMap<bool>(40, 40);
			QuickGenerators.GenerateRandomRoomsMap(map, 30, 4, 6, 10);

			Console.WriteLine(map.ToString(b => b ? "." : "#"));
		}

		private void displayMap(IMapView<bool> map)
		{
			for (int y = 0; y < map.Height; y++)
			{
				for (int x = 0; x < map.Width; x++)
					Console.Write((map[x, y] ? '.' : '#'));
				Console.WriteLine();
			}
		}
	}
}