using GoRogue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GoRogue_UnitTests
{
    [TestClass]
    public class RadiusAreaProviderTests
    {
        [TestMethod]
        public void CircleRadiusAreaProvider()
        {
            bool[,] radius = new bool[30, 30]; // Initted to false

            var radAProv = new RadiusAreaProvider(Coord.Get(15, 15), 10, Radius.CIRCLE);

            foreach (var pos in radAProv.CalculatePositions())
                radius[pos.X, pos.Y] = true;

            for (int y = 0; y < radius.GetLength(1); y++)
            {
                for (int x = 0; x < radius.GetLength(0); x++)
                    if (radius[x, y])
                        Console.Write("1 ");
                    else
                        Console.Write("0 ");
                Console.WriteLine();
            }

            Assert.AreEqual(true, true);
        }

        [TestMethod]
        public void DiamondRadiusArea()
        {
            bool[,] radius = new bool[30, 30]; // Initted to false

            var radAProv = new RadiusAreaProvider(Coord.Get(15, 15), 10, Radius.DIAMOND);

            foreach (var pos in radAProv.CalculatePositions())
                radius[pos.X, pos.Y] = true;

            for (int y = 0; y < radius.GetLength(1); y++)
            {
                for (int x = 0; x < radius.GetLength(0); x++)
                    if (radius[x, y])
                        Console.Write("1 ");
                    else
                        Console.Write("0 ");
                Console.WriteLine();
            }

            double maxDistance = 0;
            foreach (var pos in radAProv.CalculatePositions())
            {
                double distFromCenter = Distance.MANHATTAN.Calculate(Coord.Get(15, 15), pos);
                if (distFromCenter < maxDistance)
                    Assert.Fail("Square radius area provider isn't returning in distance order, failed on " + pos + "!");

                maxDistance = Math.Max(maxDistance, distFromCenter);
            }
        }

        [TestMethod]
        public void SquareRadiusArea()
        {
            bool[,] radius = new bool[30, 30]; // Initted to false

            var radAProv = new RadiusAreaProvider(Coord.Get(15, 15), 10, Radius.SQUARE);

            foreach (var pos in radAProv.CalculatePositions())
                radius[pos.X, pos.Y] = true;

            for (int y = 0; y < radius.GetLength(1); y++)
            {
                for (int x = 0; x < radius.GetLength(0); x++)
                    if (radius[x, y])
                        Console.Write("1 ");
                    else
                        Console.Write("0 ");
                Console.WriteLine();
            }

            double maxDistance = 0;
            foreach (var pos in radAProv.CalculatePositions())
            {
                double distFromCenter = Distance.CHEBYSHEV.Calculate(Coord.Get(15, 15), pos);
                if (distFromCenter < maxDistance)
                    Assert.Fail("Square radius area provider isn't returning in distance order!");

                maxDistance = Math.Max(maxDistance, distFromCenter);
            }
        }
    }
}