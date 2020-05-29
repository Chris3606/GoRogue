using GoRogue;
using GoRogue.MapViews;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.IO;

namespace GoRogue.UnitTests
{
    public static class Utility
    {
        public static void PrintHightlightedPoints(IMapView<bool> map, IEnumerable<Point> points, char wall = '#', char floor = '.', char path = '*')
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

        public static void ReadMap(string filePath, ISettableMapView<bool> map, char wallChar = '#')
        {
            using (var reader = new StreamReader(filePath))
                for (var row = 0; row < map.Height; row++)
                {
                    var line = reader.ReadLine();

                    for (var col = 0; col < map.Width; col++)
                        map[col, row] = line[col] == wallChar ? false : true;
                }
        }

        public static Tuple<Point, Point> ReadStartEnd(string filePath, char startChar = 's', char endChar = 'e')
        {
            Point start = Point.None;
            Point end = Point.None;

            using (var reader = new StreamReader(filePath))
            {
                var row = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

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
