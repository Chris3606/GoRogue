using GoRogue;
using SadRogue.Primitives;
using System;
using System.Linq;
using Xunit;

namespace GoRogue.UnitTests
{
    public class LineTests
    {
        private static readonly (int,int) END = (8, 6);
        private static readonly int MAP_HEIGHT = 10;
        private static readonly int MAP_WIDTH = 10;
        private static readonly (int,int) START = (1, 1);
        System.Random random = new System.Random();
        [Fact]
        public void ManualBresenhamTest() => DrawLine(START, END, MAP_WIDTH, MAP_HEIGHT, Lines.Algorithm.Bresenham);

        [Fact]
        public void ManualDDATest() => DrawLine(START, END, MAP_WIDTH, MAP_HEIGHT, Lines.Algorithm.DDA);

        [Fact]
        public void ManualOrthoTest() => DrawLine(START, END, MAP_WIDTH, MAP_HEIGHT, Lines.Algorithm.Ortho);

        [Fact]
        public void OrderedBresenhamTest()
        {
            var rectangle = new Rectangle(0, 0, 60, 50);
            //Random. rand = Random.GlobalRandom.DefaultRNG;
            for (var i = 0; i < 100; i++)
            {
                Point start = RandomPosition();
                Point end = RandomPosition();

                var line = Lines.Get(start, end, Lines.Algorithm.BresenhamOrdered).ToList();
                Assert.Equal(start, line[0]);
            }
        }

        private Point RandomPosition()
        {
            int x = random.Next(0, MAP_WIDTH);
            int y = random.Next(0, MAP_HEIGHT);
            return (x, y);
        }

        private void DrawLine(Point start, Point end, int width, int height, Lines.Algorithm type)
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
    }
}
