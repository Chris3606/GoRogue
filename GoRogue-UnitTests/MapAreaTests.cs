using GoRogue;
using GoRogue.MapGeneration;
using Generators = GoRogue.MapGeneration.Generators;
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
            Generators.RectangleMap.Generate(map);


            int count = 0;
            foreach (var area in MapAreaFinder.MapAreas(map, Distance.MANHATTAN))
            {
                Console.WriteLine(area.Bounds);
                count++
            }

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void TestTwoAreaRect()
        {
            var map = new ArrayMapOf<bool>(80, 50);
            Generators.RectangleMap.Generate(map);

            for (int y = 0; y < 50; y++)
                map[40, y] = false;

            var areas = MapAreaFinder.MapAreas(map, Distance.MANHATTAN).ToList();

            Assert.AreEqual(2, areaFinder.Count);
        }

        [TestMethod]
        public void TestOneRoomAreaRect()
        {
            var map = new ArrayMapOf<bool>(80, 50);
            Generators.RectangleMap.Generate(map);

            for (int y = 0; y < 50; y++)
                map[40, y] = false;

            map[40, 25] = true;

            var areaFinder = new MapAreaFinder(map, Distance.MANHATTAN);
            areaFinder.FindMapAreas();

            Assert.AreEqual(1, areaFinder.Count);
        }
    }
}