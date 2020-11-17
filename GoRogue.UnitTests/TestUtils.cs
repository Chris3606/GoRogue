using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using GoRogue.MapViews;
using SadRogue.Primitives;
using Xunit;

namespace GoRogue.UnitTests
{
    /// <summary>
    /// Static/extension methods to help with creating test variables/enumerables for XUnit
    /// </summary>
    public static class TestUtils
    {
        public static void NotNull([NotNull]object? obj)
        {
            Assert.NotNull(obj);
            if (obj == null)
                throw new Exception("Can't happen, prevents compiler from complaining.");

        }

        /// <summary>
        /// Returns the object as a single-element enumerable.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="obj" />
        /// <returns>The given object as a single-element IEnumerable</returns>
        public static IEnumerable<T> ToEnumerable<T>(this T obj)
        {
            yield return obj;
        }

        /// <summary>
        /// Creates a tuple of each possible pairing of elements in the enumerables.
        /// </summary>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <param name="l1" />
        /// <param name="l2" />
        /// <returns>Tuples containing every possible (unique) pairing of elements between the two enumerables.</returns>
        public static IEnumerable<(T1, T2)> Combinate<T1, T2>(this IEnumerable<T1> l1, IEnumerable<T2> l2)
        {
            var l2List = l2.ToList();
            foreach (var x in l1)
                foreach (var y in l2List)
                    yield return (x, y);
        }

        /// <summary>
        /// Creates a tuple for each pairing of the tuples with the elements from <paramref name="l2" />.
        /// </summary>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        /// <param name="tuples" />
        /// <param name="l2" />
        /// <returns>
        /// An enumerable of 3-element tuples that collectively represent every unique pairing of the initial tuples with
        /// the new values.
        /// </returns>
        public static IEnumerable<(T1, T2, T3)> Combinate<T1, T2, T3>(this IEnumerable<(T1 i1, T2 i2)> tuples,
                                                                      IEnumerable<T3> l2)
        {
            var l2List = l2.ToList();
            foreach (var (i1, i2) in tuples)
                foreach (var y in l2List)
                    yield return (i1, i2, y);
        }

        /// <summary>
        /// Creates a tuple for each pairing of the tuples with the elements from <paramref name="l2" />.
        /// </summary>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        /// <typeparam name="T4" />
        /// <param name="tuples" />
        /// <param name="l2" />
        /// <returns>
        /// An enumerable of 4-element tuples that collectively represent every unique pairing of the initial tuples with
        /// the new values.
        /// </returns>
        public static IEnumerable<(T1, T2, T3, T4)> Combinate<T1, T2, T3, T4>(this IEnumerable<(T1 i1, T2 i2, T3 i3)> tuples,
                                                                              IEnumerable<T4> l2)
        {
            var l2List = l2.ToList();
            foreach (var (i1, i2, i3) in tuples)
                foreach (var y in l2List)
                    yield return (i1, i2, i3, y);
        }

        /// <summary>
        /// Creates a tuple for each pairing of the tuples with the elements from <paramref name="l2" />.
        /// </summary>
        /// <typeparam name="T1" />
        /// <typeparam name="T2" />
        /// <typeparam name="T3" />
        /// <typeparam name="T4" />
        /// <typeparam name="T5" />
        /// <param name="tuples" />
        /// <param name="l2" />
        /// <returns>
        /// An enumerable of 5-element tuples that collectively represent every unique pairing of the initial tuples with
        /// the new values.
        /// </returns>
        public static IEnumerable<(T1, T2, T3, T4, T5)> Combinate<T1, T2, T3, T4, T5>(this IEnumerable<(T1 i1, T2 i2, T3 i3, T4)> tuples,
                                                                              IEnumerable<T5> l2)
        {
            var l2List = l2.ToList();
            foreach (var (i1, i2, i3, i4) in tuples)
                foreach (var y in l2List)
                    yield return (i1, i2, i3, i4, y);
        }

        public static IEnumerable<(T item, int index)> Enumerate<T>(this IEnumerable<T> self)
            => self.Select((item, index) => (item, index));

        /// <summary>
        /// Creates an enumerable of the input parameters.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="objs" />
        /// Input values to have within the resulting enumerable.
        /// <returns>An enumerable containing all the input parameters, in order.</returns>
        public static IEnumerable<T> Enumerable<T>(params T[] objs) => objs;

        /// <summary>
        /// Gets all enum values from an enum.
        /// </summary>
        /// <typeparam name="T"/>
        /// <returns/>
        public static T[] GetEnumValues<T>() where T : Enum
            => (T[])Enum.GetValues(typeof(T));

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
                var line = reader.ReadLine() ??
                           throw new InvalidOperationException("Couldn't read map: invalid format.");

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
                    var line = reader.ReadLine() ??
                               throw new InvalidOperationException("Couldn't read map: invalid format.");

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

    public class IncrementOnlyValue
    {
        private int _value;

        public int Value
        {
            get => _value;
            set
            {
                if (value - _value != 1)
                    throw new InvalidOperationException(
                        $"{nameof(IncrementOnlyValue)} is expected only to have its value incremented by one.  Should have been changed to {value + 1}, but was changed to {_value}.");

                _value = value;
            }
        }
    }
}
