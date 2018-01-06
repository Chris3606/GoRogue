using GoRogue;
using GoRogue.MapGeneration;
using Generators = GoRogue.MapGeneration.Generators;
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
            Generators.RandomRoomsGenerator.Generate(map, 7, 4, 7, 5, random);

            displayMap(map);
            // TODO: Some assert here
        }

        [TestMethod]
        public void ManualTestCellAutoGen()
        {
            var random = new DotNetRandom();
            var map = new ArrayMapOf<bool>(80, 50);
            Generators.CellularAutomataGenerator.Generate(map, random, 40, 7, 4);

            displayMap(map);

            // TODO: Asserts
        }

        [TestMethod]
        public void TestCellAutoConnectivityAndEnclosure()
        {
            var random = new DotNetRandom();
            var map = new ArrayMapOf<bool>(80, 50);
            Generators.CellularAutomataGenerator.Generate(map, random, 40, 7, 4);

            for (int i = 0; i < 500; i++)
            {
                Generators.CellularAutomataGenerator.Generate(map, random, 40, 7, 4);

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