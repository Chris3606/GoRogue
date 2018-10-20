using GoRogue;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Generators = GoRogue.MapGeneration.Generators;

namespace GoRogue_UnitTests
{
	[TestClass]
	public class MapAreaTests
	{
		[TestMethod]
		public void TestMapArea()
		{
			var map = new MapArea();

			Assert.AreEqual(0, map.Count);

			map.Add(1, 1);
			Assert.AreEqual(1, map.Count);

			map.Add(1, 1);
			Assert.AreEqual(1, map.Count);

			map.Add(2, 1);
			Assert.AreEqual(2, map.Count);
		}

		[TestMethod]
		public void TestMapAreaRemove()
		{
			var mapArea = new MapArea();
			mapArea.Add(GoRogue.Utility.Yield(Coord.Get(1, 1), Coord.Get(2, 2), Coord.Get(3, 2)));

			Assert.AreEqual(3, mapArea.Count);
			Assert.AreEqual(1, mapArea.Bounds.X);
			Assert.AreEqual(1, mapArea.Bounds.Y);
			Assert.AreEqual(3, mapArea.Bounds.MaxExtentX);
			Assert.AreEqual(2, mapArea.Bounds.MaxExtentY);

			mapArea.Remove(Coord.Get(3, 2));

			Assert.AreEqual(2, mapArea.Count);
			Assert.AreEqual(1, mapArea.Bounds.X);
			Assert.AreEqual(1, mapArea.Bounds.Y);
			Assert.AreEqual(2, mapArea.Bounds.MaxExtentX);
			Assert.AreEqual(2, mapArea.Bounds.MaxExtentY);
		}

		[TestMethod]
		public void TestOneRoomAreaRect()
		{
			var map = new ArrayMap<bool>(80, 50);
			Generators.RectangleMapGenerator.Generate(map);

			for (int y = 0; y < 50; y++)
				map[40, y] = false;

			map[40, 25] = true;

			var areas = MapAreaFinder.MapAreasFor(map, Distance.MANHATTAN).ToList();

			Assert.AreEqual(1, areas.Count);
		}

		[TestMethod]
		public void TestSingleAreaRect()
		{
			var map = new ArrayMap<bool>(80, 50);
			Generators.RectangleMapGenerator.Generate(map);

			var areas = MapAreaFinder.MapAreasFor(map, Distance.MANHATTAN).ToList();
			foreach (var area in areas)
				Console.WriteLine(area.Bounds);

			Assert.AreEqual(1, areas.Count);
		}

		[TestMethod]
		public void TestTwoAreaRect()
		{
			var map = new ArrayMap<bool>(80, 50);
			Generators.RectangleMapGenerator.Generate(map);

			for (int y = 0; y < 50; y++)
				map[40, y] = false;

			var areas = MapAreaFinder.MapAreasFor(map, Distance.MANHATTAN).ToList();

			Assert.AreEqual(2, areas.Count);
		}
	}
}