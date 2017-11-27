using GoRogue;
using GoRogue.MapGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
        public void TestSingleAreaRect()
        {
            var map = new ArrayMapOf<bool>(80, 50);
            var generator = new RectangleMapGenerator(map);
            generator.Generate();

            var areaFinder = new MapAreaFinder(map);
            areaFinder.FindMapAreas();

            foreach (var area in areaFinder.MapAreas)
                Console.WriteLine(area.Bounds);

            Assert.AreEqual(1, areaFinder.Count);
        }

        [TestMethod]
        public void TestTwoAreaRect()
        {
            var map = new ArrayMapOf<bool>(80, 50);
            var generator = new RectangleMapGenerator(map);
            generator.Generate();

            for (int y = 0; y < 50; y++)
                map[40, y] = false;

            var areaFinder = new MapAreaFinder(map);
            areaFinder.FindMapAreas();

            Assert.AreEqual(2, areaFinder.Count);
        }

        [TestMethod]
        public void TestOneRoomAreaRect()
        {
            var map = new ArrayMapOf<bool>(80, 50);
            var generator = new RectangleMapGenerator(map);
            generator.Generate();

            for (int y = 0; y < 50; y++)
                map[40, y] = false;

            map[40, 25] = true;

            var areaFinder = new MapAreaFinder(map);
            areaFinder.FindMapAreas();

            Assert.AreEqual(1, areaFinder.Count);
        }
    }
}