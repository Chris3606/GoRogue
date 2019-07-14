using GoRogue;
using GoRogue.MapGeneration;
using GoRogue.MapGeneration.Generators;
using GoRogue.MapViews;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Troschuetz.Random.Generators;

namespace GoRogue_UnitTests
{
	[TestClass]
	public class MapGenTests
	{
		[TestMethod]
		public void BasicRoomsMapGenMin()
		{
			var map = new ArrayMap<bool>(15, 15);
			var rooms = BasicRoomsGenerator.Generate(map, new GoRogue.Random.MinRandom(), 1, 5, 8, 10).ToList();
			Assert.AreEqual(1, rooms.Count);
			Assert.AreEqual(5, rooms[0].Width);
			Assert.AreEqual(5, rooms[0].Height);

			var areas = MapAreaFinder.MapAreasFor(map, AdjacencyRule.CARDINALS).ToList();
			Assert.AreEqual(1, areas.Count);
			Assert.AreEqual(5, areas[0].Bounds.Width);
			Assert.AreEqual(5, areas[0].Bounds.Height);
		}

		[TestMethod]
		public void BasicRoomsMapGenMax()
		{
			var map = new ArrayMap<bool>(15, 15);
			var rooms = BasicRoomsGenerator.Generate(map, new GoRogue.Random.MaxRandom(), 1, 5, 8, 10).ToList();
			Assert.AreEqual(1, rooms.Count);
			Assert.AreEqual(8, rooms[0].Width);
			Assert.AreEqual(8, rooms[0].Height);

			var areas = MapAreaFinder.MapAreasFor(map, AdjacencyRule.CARDINALS).ToList();
			Assert.AreEqual(1, areas.Count);
			Assert.AreEqual(8, areas[0].Bounds.Width);
			Assert.AreEqual(8, areas[0].Bounds.Height);
		}

		/*
		[TestMethod]
		public void DungeonRoomsMapGenMin()
		{
			var map = new ArrayMap<bool>(15, 15);
			
			var rooms = RoomsGenerator.Generate(map, new GoRogue.Random.MinRandom(), 1, 1, 5, 8).ToList();
			Assert.AreEqual(1, rooms.Count);
			Assert.AreEqual(5, rooms[0].Width);
			Assert.AreEqual(5, rooms[0].Height);

			var areas = MapAreaFinder.MapAreasFor(map, AdjacencyRule.CARDINALS).ToList();
			Assert.AreEqual(1, areas.Count);
			Assert.AreEqual(5, areas[0].Bounds.Width);
			Assert.AreEqual(5, areas[0].Bounds.Height);
		}

		[TestMethod]
		public void DungeonRoomsMapGenMax()
		{
			var map = new ArrayMap<bool>(15, 15);
			var rooms = RoomsGenerator.Generate(map, new GoRogue.Random.MaxRandom(), 1, 1, 5, 9).ToList();
			Assert.AreEqual(1, rooms.Count);
			Assert.AreEqual(9, rooms[0].Width);
			Assert.AreEqual(9, rooms[0].Height);

			var areas = MapAreaFinder.MapAreasFor(map, AdjacencyRule.CARDINALS).ToList();
			Assert.AreEqual(1, areas.Count);
			Assert.AreEqual(9, areas[0].Bounds.Width);
			Assert.AreEqual(9, areas[0].Bounds.Height);
		}
		*/
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
		public void ManualTestDungeonMazeGen()
		{
			var random = new StandardGenerator(12345);

			var map = new ArrayMap<bool>(80, 50);
			QuickGenerators.GenerateDungeonMazeMap(map, random, minRooms: 4, maxRooms: 10, roomMinSize: 4, roomMaxSize: 7);
			
			displayMap(map);
			// TODO: Some assert here
		}

		[TestMethod]
		public void ManualMazeTest()
		{
			var map = new ArrayMap<bool>(30, 30);

			GoRogue.MapGeneration.Generators.MazeGenerator.Generate(map);

			displayMap(map);
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
		public void TestCellAutoConnectivityAndEnclosure()
		{
			var random = new StandardGenerator();
			var map = new ArrayMap<bool>(80, 50);

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
		public void TestMazeGenConnectivityAndEnclosure()
		{
			var map = new ArrayMap<bool>(80, 50);
			try
			{
				var random = new StandardGenerator();

				for (int i = 0; i < 1500; i++)
				{
					QuickGenerators.GenerateDungeonMazeMap(map, random, minRooms: 10, maxRooms: 20, roomMinSize: 4, roomMaxSize: 15);

					// Ensure it's connected
					var areas = MapAreaFinder.MapAreasFor(map, Distance.MANHATTAN).ToList();
					if (areas.Count != 1)
					{
						Console.WriteLine($"Map attempt {i + 1}/500 failed, had {areas.Count} areas: ");
						displayMapAreas(map, areas);
					}

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
			catch (Exception e)
			{
				Console.WriteLine($"Map attempt failed with exception on map: ");
				displayMapAreas(map, MapAreaFinder.MapAreasFor(map, Distance.MANHATTAN).ToList());
				throw e;
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
		
		[TestMethod]
		public void TestDungeonGenDefaultRNG()
		{
			// default rng
			var map = new ArrayMap<bool>(100, 100);
			// Assert no exceptions are thrown
			QuickGenerators.GenerateDungeonMazeMap(map, 10, 30, 4, 9);
		}

		[TestMethod]
		public void TestDungeonGenNonDefaultRNG()
		{
			var map = new ArrayMap<bool>(100, 100);
			// Assert no exceptions are thrown
			var gen = new XorShift128Generator();
			for (int i = 0; i < 3; i++) // Due to Troscheutz.Random bug
				gen.Next();

			QuickGenerators.GenerateDungeonMazeMap(map, gen, 10, 30, 4, 9);
		}

		private void displayMapAreas(IMapView<bool> map, IEnumerable<MapArea> areas)
		{
			for (int y = 0; y < map.Height; y++)
			{
				for (int x = 0; x < map.Width; x++)
				{
					if (map[x, y])
					{
						int areaIndex = 0;
						foreach (var area in areas)
						{
							if (area.Contains(x, y))
								break;

							areaIndex++;
						}

						Console.Write((char)('1' + areaIndex));
					}
					else
						Console.Write('-');
				}
				Console.WriteLine();
			}
		}
	}
}