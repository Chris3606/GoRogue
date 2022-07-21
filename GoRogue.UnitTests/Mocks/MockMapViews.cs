using System;
using System.Collections.Generic;
using System.IO;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.UnitTests.Mocks
{
    internal class ResMap : GridViewBase<double>
    {
        private readonly IGridView<bool> view;

        public ResMap(IGridView<bool> view) => this.view = view;

        public override int Height => view.Height;
        public override int Width => view.Width;
        public override double this[Point pos] => view[pos] ? 0.0 : 1.0;

        public static void PrintHighlightedPoints(IGridView<bool> map, IEnumerable<Point> points, char wall = '#',
                                                   char floor = '.', char path = '*')
        {
            var array = new char[map.Width, map.Height];
            for (var y = 0; y < map.Height; y++)
                for (var x = 0; x < map.Width; x++)
                    array[x, y] = map[x, y] ? floor : wall;

            foreach (var point in points)
                array[point.X, point.Y] = path;

            for (var y = 0; y < map.Height; y++)
            {
                for (var x = 0; x < map.Width; x++)
                    Console.Write(array[x, y]);
                Console.WriteLine();
            }
        }

        public static void ReadMap(string filePath, ISettableGridView<bool> map, char wallChar = '#')
        {
            using var reader = new StreamReader(filePath);
            for (var row = 0; row < map.Height; row++)
            {
                var line = reader.ReadLine()
                           ?? throw new InvalidOperationException("Failed to read map: invalid format.");

                for (var col = 0; col < map.Width; col++)
                    map[col, row] = line[col] != wallChar;
            }
        }

        public static Tuple<Point, Point> ReadStartEnd(string filePath, char startChar = 's', char endChar = 'e')
        {
            var start = Point.None;
            var end = Point.None;

            using (var reader = new StreamReader(filePath))
            {
                var row = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine()
                               ?? throw new InvalidOperationException("Failed to start-end: invalid format.");

                    for (var col = 0; col < line.Length; col++)
                    {
                        if (line[col] == startChar)
                            start = (col, row);

                        if (line[col] == endChar)
                            end = (col, row);
                    }

                    row++;
                }
            }

            return new Tuple<Point, Point>(start, end);
        }
    }
}
