using GoRogue;
using GoRogue.MapGeneration;
using GoRogue.Random;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GoRogue_UnitTests
{
    [TestClass]
    public class MapGenTests
    {
        [TestMethod]
        public void ManualTestRandomRoomsGen()
        {
            var random = new DotNetRandom();
            var map = new ArrayMapOf<bool>(30, 30);
            new RandomRoomsMapGenerator(map, 7, 4, 7, 5, random).Generate();

            displayMap(map);
            // TODO: Some assert here
        }

        [TestMethod]
        public void ManualTestCellAutoGen()
        {
            var random = new DotNetRandom();
            var map = new ArrayMapOf<bool>(80, 50);
            new CellularAutomataMapGenerator(map, random, 40, 7, 4).Generate();

            displayMap(map);

            // TODO: Asserts
        }

        [TestMethod]
        public void TestCellAutoConnectivityAndEnclosure()
        {
            var random = new DotNetRandom();
            var map = new ArrayMapOf<bool>(80, 50);
            var generator = new CellularAutomataMapGenerator(map, random, 40, 7, 4);

            for (int i = 0; i < 500; i++)
            {
                generator.Generate();

                // Ensure it's connected
                var finder = new MapAreaFinder(map, Distance.MANHATTAN);
                finder.FindMapAreas();
                Assert.AreEqual(1, finder.Count);

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

        private void displayMap(IMapOf<bool> map)
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