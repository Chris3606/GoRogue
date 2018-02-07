using GoRogue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GoRogue_UnitTests
{
    [TestClass]
    public class LineTests
    {
        private static readonly Coord END = Coord.Get(8, 6);
        private static readonly int MAP_HEIGHT = 10;
        private static readonly int MAP_WIDTH = 10;
        private static readonly Coord START = Coord.Get(1, 1);

        [TestMethod]
        public void ManualBresenhamTest() => DrawLine(START, END, MAP_WIDTH, MAP_HEIGHT, Lines.Algorithm.BRESENHAM);

        [TestMethod]
        public void ManualDDATest() => DrawLine(START, END, MAP_WIDTH, MAP_HEIGHT, Lines.Algorithm.DDA);

        [TestMethod]
        public void ManualOrthoTest() => DrawLine(START, END, MAP_WIDTH, MAP_HEIGHT, Lines.Algorithm.ORTHO);

        private void DrawLine(Coord start, Coord end, int width, int height, Lines.Algorithm type)
        {
            char[,] myChars = new char[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    myChars[x, y] = (x == 0 || y == 0 || x == width - 1 || y == height - 1) ? '#' : '.';

            foreach (var point in Lines.Get(start.X, start.Y, end.X, end.Y, type))
                myChars[point.X, point.Y] = '*';

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    Console.Write(myChars[x, y]);

                Console.WriteLine();
            }
        }
    }
}