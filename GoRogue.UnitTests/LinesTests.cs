using System;
using System.Linq;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests
{
    public class LineTests
    {
        private static readonly (int, int) _end = (8, 6);
        private const int _mapHeight = 10;
        private const int _mapWidth = 10;
        private static readonly (int, int) _start = (1, 1);
        private readonly System.Random _random = new System.Random();

        private Point RandomPosition()
        {
            var x = _random.Next(0, _mapWidth);
            var y = _random.Next(0, _mapHeight);
            return (x, y);
        }

        private static void DrawLine(Point start, Point end, int width, int height, Lines.Algorithm type)
        {
            var myChars = new char[width, height];

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    myChars[x, y] = x == 0 || y == 0 || x == width - 1 || y == height - 1 ? '#' : '.';

            foreach (var point in Lines.Get(start.X, start.Y, end.X, end.Y, type))
                myChars[point.X, point.Y] = '*';

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                    Console.Write(myChars[x, y]);

                Console.WriteLine();
            }
        }

        [Fact]
        public void ManualBresenhamTest() => DrawLine(_start, _end, _mapWidth, _mapHeight, Lines.Algorithm.Bresenham);

        [Fact]
        public void ManualDDATest() => DrawLine(_start, _end, _mapWidth, _mapHeight, Lines.Algorithm.DDA);

        [Fact]
        public void ManualOrthoTest() => DrawLine(_start, _end, _mapWidth, _mapHeight, Lines.Algorithm.Orthogonal);

        [Fact]
        public void OrderedBresenhamTest()
        {
            //Random. rand = Random.GlobalRandom.DefaultRNG;
            for (var i = 0; i < 100; i++)
            {
                var start = RandomPosition();
                var end = RandomPosition();

                var line = Lines.Get(start, end, Lines.Algorithm.BresenhamOrdered).ToList();
                Assert.Equal(start, line[0]);
            }
        }
    }
}
