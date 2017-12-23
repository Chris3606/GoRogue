using System;
using GoRogue;
using GoRogue.MapGeneration;
using GoRogue.SenseMapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoRogue_UnitTests
{
    [TestClass]
    public class RadiusFOVSyncTests
    {
        private static readonly int MAP_WIDTH = 30;
        private static readonly int MAP_HEIGHT = 30;
        private static readonly Coord CENTER = Coord.Get(15, 15);
        private static readonly int RADIUS_LEGNTH = 10;

        [TestMethod]
        public void CircleLosShape()
        {
            Assert.AreEqual(true, testLOS(Radius.CIRCLE));
        }

        [TestMethod]
        public void SquareLosShape()
        {
            Assert.AreEqual(true, testLOS(Radius.SQUARE));
        }

        [TestMethod]
        public void DiamondLosShape()
        {
            Assert.AreEqual(true, testLOS(Radius.DIAMOND));
        }

        [TestMethod]
        public void ShadowSenseMapCircleShape()
        {
            Assert.AreEqual(true, testSenseMap(SourceType.SHADOW, Radius.CIRCLE));
        }

        [TestMethod]
        public void ShadowSenseMapSquareShape()
        {
            Assert.AreEqual(true, testSenseMap(SourceType.SHADOW, Radius.SQUARE));
        }

        [TestMethod]
        public void ShadowSenseMapDiamondShape()
        {
            Assert.AreEqual(true, testSenseMap(SourceType.SHADOW, Radius.DIAMOND));
        }

        [TestMethod]
        public void RippleSenseMapCircleShape()
        {
            Assert.AreEqual(true, testSenseMap(SourceType.RIPPLE, Radius.CIRCLE));
        }

        [TestMethod]
        public void RippleSenseMapSquareShape()
        {
            Assert.AreEqual(true, testSenseMap(SourceType.RIPPLE, Radius.SQUARE));
        }

        [TestMethod]
        public void RippleSenseMapDiamondShape()
        {
            Assert.AreEqual(true, testSenseMap(SourceType.RIPPLE, Radius.DIAMOND));
        }

        private bool testLOS(Radius shape)
        {
            var map = rectResMap(MAP_WIDTH, MAP_HEIGHT);

            // Start out at false
            bool[,] radiusMap = new bool[MAP_WIDTH, MAP_HEIGHT];
            bool[,] losMap = new bool[MAP_WIDTH, MAP_HEIGHT];

            var los = new LOS(map);
            los.Calculate(CENTER.X, CENTER.Y, RADIUS_LEGNTH, shape);

            for (int x = 0; x < MAP_WIDTH; x++)
                for (int y = 0; y < MAP_HEIGHT; y++)
                    if (los[x, y] > 0)
                        losMap[x, y] = true;

            var radArea = new RadiusAreaProvider(CENTER, RADIUS_LEGNTH, shape);
            foreach (var pos in radArea.Positions())
                radiusMap[pos.X, pos.Y] = true;

            Console.WriteLine("Radius Shape: ");
            printArray(radiusMap);

            Console.WriteLine("LOS Shape: ");
            printArray(losMap);

            return equivalentArrays(radiusMap, losMap);
        }

        private bool testSenseMap(SourceType algorithm, Radius shape)
        {
            var map = rectResMap(MAP_WIDTH, MAP_HEIGHT);

            // Start out at false
            bool[,] radiusMap = new bool[MAP_WIDTH, MAP_HEIGHT];
            bool[,] losMap = new bool[MAP_WIDTH, MAP_HEIGHT];

            var los = new SenseMap(map);
            var lightSource = new SenseSource(algorithm, CENTER, RADIUS_LEGNTH, shape);
            los.AddSenseSource(lightSource);
            los.Calculate();

            for (int x = 0; x < MAP_WIDTH; x++)
                for (int y = 0; y < MAP_HEIGHT; y++)
                    if (los[x, y] > 0)
                        losMap[x, y] = true;

            var radArea = new RadiusAreaProvider(CENTER, RADIUS_LEGNTH, shape);
            foreach (var pos in radArea.Positions())
                radiusMap[pos.X, pos.Y] = true;

            Console.WriteLine("Radius Shape: ");
            printArray(radiusMap);

            Console.WriteLine("LOS Shape: ");
            printArray(losMap);

            return equivalentArrays(radiusMap, losMap);
        }

        private ArrayMapOf<double> rectResMap(int mapWidth, int mapHeight)
        {
            var map = new ArrayMapOf<bool>(mapWidth, mapHeight);
            var resMap = new ArrayMapOf<double>(mapWidth, mapHeight);

            RectangleMap.Generate(map);

            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                    resMap[x, y] = (map[x, y]) ? 0.0 : 1.0;

            return resMap;
        }

        private bool equivalentArrays(bool[,] arr1, bool[,] arr2)
        {
            if (arr1.GetLength(0) != arr2.GetLength(0) || arr1.GetLength(1) != arr2.GetLength(1))
            {
                Console.WriteLine("Error: Arrays weren't equal sizes");
                return false;
            }

            for (int x = 0; x < arr1.GetLength(0); x++)
            {
                for (int y = 0; y < arr1.GetLength(1); y++)
                {
                    if (arr1[x, y] != arr2[x, y])
                    {
                        Console.WriteLine("Radiuses not equal");
                        return false;
                    }
                }
            }

            return true;
        }

        private void printArray(bool[,] arr)
        {
            for (int y = 0; y < arr.GetLength(1); y++)
            {
                for (int x = 0; x < arr.GetLength(0); x++)
                    if (arr[x, y])
                        Console.Write("1 ");
                    else
                        Console.Write("0 ");
                Console.WriteLine();
            }
        }
    }
}
