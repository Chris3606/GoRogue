using System;
using System.IO;
using System.Collections.Generic;
using GoRogue;
using CA = EMK.Cartography;

namespace GoRogue_UnitTests
{
    // Adds functions commonly used in tests
    public static class Utility
    {
        public static void PrintHightlightedPoints(IMapView<bool> map, IEnumerable<Coord> points, char wall = '#', char floor = '.', char path = '*')
        {
            char[,] array = new char[map.Width, map.Height];
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    array[x, y] = (map[x, y]) ? floor : wall;

            foreach (var point in points)
                array[point.X, point.Y] = path;


            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                    Console.Write(array[x, y]);
                Console.WriteLine();
            }
        }

        public static IEnumerable<Coord> ToCoords(IEnumerable<CA.Node> points)
        {
            foreach (var p in points)
                yield return Coord.Get((int)p.X, (int)p.Y);
        }

        public static Tuple<Coord, Coord> ReadStartEnd(string filePath, char startChar = 's', char endChar = 'e')
        {
            Coord start = null;
            Coord end = null;

            using (var reader = new StreamReader(filePath))
            {
                int row = 0;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    for (int col = 0; col < line.Length; col++)
                    {
                        if (line[col] == startChar)
                            start = Coord.Get(col, row);

                        if (line[col] == endChar)
                            end = Coord.Get(col, row);
                    }
                    row++;
                }
            }

            return new Tuple<Coord, Coord>(start, end);
        }

        public static void ReadMap(string filePath, ISettableMapView<bool> map, char wallChar = '#')
        {
            using (var reader = new StreamReader(filePath))
            {
                for (int row = 0; row < map.Height; row++)
                {
                    string line = reader.ReadLine();

                    for (int col = 0; col < map.Width; col++)
                        map[col, row] = (line[col] == wallChar) ? false : true;
                }
            }


        }
    }
}
