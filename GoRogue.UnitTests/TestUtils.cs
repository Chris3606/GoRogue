using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using ShaiRandom.Generators;
using Troschuetz.Random;
using Xunit;

namespace GoRogue.UnitTests
{
    /// <summary>
    /// Class used to wrap Troschuetz generators, and record their outputs in a format that can be converted to a
    /// KnownSeriesGenerator.
    /// </summary>
    /// <remarks>
    /// Only the portions of this class that are strictly necessary for porting GoRogue unit tests have been implemented,
    /// and it by no means represents a production product.
    /// </remarks>
    public class TroschuetzRecorder : IEnhancedRandom
    {
        private readonly IGenerator _wrapped;
        private readonly List<int> _ints;
        private readonly List<float> _floats;

        public TroschuetzRecorder(IGenerator wrapped)
        {
            _wrapped = wrapped;
            _ints = new List<int>();
            _floats = new List<float>();
        }

        public KnownSeriesRandom ToKnownSeries() => new KnownSeriesRandom(_ints, floatSeries: _floats);

        #region IEnhancedRandom Implementation
        public void Seed(ulong seed) => throw new NotSupportedException();

        public IEnhancedRandom Copy() => throw new NotSupportedException();

        public string StringSerialize() => throw new NotSupportedException();

        public IEnhancedRandom StringDeserialize(ReadOnlySpan<char> data) => throw new NotSupportedException();

        public ulong SelectState(int selection) => throw new NotSupportedException();

        public void SetSelectedState(int selection, ulong value) => throw new NotSupportedException();

        public ulong NextULong() => throw new NotSupportedException();

        public long NextLong() => throw new NotSupportedException();

        public ulong NextULong(ulong bound) => throw new NotSupportedException();

        public long NextLong(long outerBound) => throw new NotSupportedException();

        public ulong NextULong(ulong inner, ulong outer) => throw new NotSupportedException();

        public long NextLong(long inner, long outer) => throw new NotSupportedException();

        public uint NextBits(int bits) => throw new NotSupportedException();

        public void NextBytes(Span<byte> bytes) => throw new NotSupportedException();

        public int NextInt()
        {
            int val = _wrapped.NextInclusiveMaxValue();
            _ints.Add(val);

            return val;
        }

        public uint NextUInt() => throw new NotSupportedException();

        public uint NextUInt(uint bound) => throw new NotSupportedException();

        public int NextInt(int outerBound)
        {
            int val = _wrapped.Next(outerBound);
            _ints.Add(val);

            return val;
        }

        public uint NextUInt(uint innerBound, uint outerBound) => throw new NotSupportedException();

        public int NextInt(int innerBound, int outerBound)
        {
            int val = _wrapped.Next(innerBound, outerBound);
            _ints.Add(val);

            return val;
        }

        public bool NextBool() => throw new NotSupportedException();

        public float NextFloat()
        {
            // Troschuetz doesn't support float generation specifically, so any calls to it in our specific cases happen to be the result
            // of the new implementation of PercentageCheck.  So, in a godawful series of hacks, we will simply
            // invoke the old behavior of PercentageCheck, convert it to a value that will produce an equivalent result
            // in the modern one, and go on our way.  This is NOT a generic solution, and will utterly break in an extremely
            // large number of cases.
            float val = _wrapped.Next(1, 101) * 0.01f;
            // Again, we know the percent checked for IN THE PARTICULAR CASES THIS IS USED FOR was a whole number...
            // So we'll defeat the evil floating-point imprecision by just rounding.
            val = MathF.Round(val, 2);
            _floats.Add(val);

            return val;
        }

        public float NextFloat(float outerBound) => throw new NotSupportedException();

        public float NextFloat(float innerBound, float outerBound) => throw new NotSupportedException();

        public double NextDouble() => throw new NotSupportedException();

        public double NextDouble(double outerBound) => throw new NotSupportedException();

        public double NextDouble(double innerBound, double outerBound) => throw new NotSupportedException();

        public double NextInclusiveDouble() => throw new NotSupportedException();

        public double NextInclusiveDouble(double outerBound) => throw new NotSupportedException();

        public double NextInclusiveDouble(double innerBound, double outerBound) => throw new NotSupportedException();

        public float NextInclusiveFloat() => throw new NotSupportedException();

        public float NextInclusiveFloat(float outerBound) => throw new NotSupportedException();

        public float NextInclusiveFloat(float innerBound, float outerBound) => throw new NotSupportedException();
        public decimal NextInclusiveDecimal() => throw new NotSupportedException();

        public decimal NextInclusiveDecimal(decimal outerBound) => throw new NotSupportedException();

        public decimal NextInclusiveDecimal(decimal innerBound, decimal outerBound) => throw new NotSupportedException();

        public double NextExclusiveDouble() => throw new NotSupportedException();

        public double NextExclusiveDouble(double outerBound) => throw new NotSupportedException();

        public double NextExclusiveDouble(double innerBound, double outerBound) => throw new NotSupportedException();

        public float NextExclusiveFloat() => throw new NotSupportedException();

        public float NextExclusiveFloat(float outerBound) => throw new NotSupportedException();

        public float NextExclusiveFloat(float innerBound, float outerBound) => throw new NotSupportedException();
        public decimal NextExclusiveDecimal() => throw new NotSupportedException();

        public decimal NextExclusiveDecimal(decimal outerBound) => throw new NotSupportedException();

        public decimal NextExclusiveDecimal(decimal innerBound, decimal outerBound) => throw new NotSupportedException();

        public decimal NextDecimal() => throw new NotSupportedException();

        public decimal NextDecimal(decimal outerBound) => throw new NotSupportedException();

        public decimal NextDecimal(decimal innerBound, decimal outerBound) => throw new NotSupportedException();

        public ulong Skip(ulong distance) => throw new NotSupportedException();

        public ulong PreviousULong() => throw new NotSupportedException();

        public int StateCount => throw new NotSupportedException();

        public bool SupportsReadAccess => throw new NotSupportedException();

        public bool SupportsWriteAccess => throw new NotSupportedException();

        public bool SupportsSkip => throw new NotSupportedException();

        public bool SupportsPrevious => throw new NotSupportedException();

        public string Tag => throw new NotSupportedException();
        #endregion
    }
    /// <summary>
    /// Static/extension methods to help with creating test variables/enumerables for XUnit
    /// </summary>
    public static class TestUtils
    {
        /// <summary>
        /// Asserts that the value is not null in such a way that the nullable reference type control flow
        /// analyzer understands that the object cannot be null after this function is called.
        /// </summary>
        /// <param name="obj">Object check for null</param>
        public static void NotNull([NotNull]object? obj)
        {
            Assert.NotNull(obj);
            if (obj == null)
                throw new InvalidOperationException("Can't happen, prevents compiler from complaining.");

        }

        /// <summary>
        /// Checks that the two IMatchable implementations given match according to the match function.
        /// </summary>
        /// <param name="m1"/>
        /// <param name="m2"/>
        /// <typeparam name="T">True if the given IMatchable implementations match, false otherwise.</typeparam>
        public static void Matches<T>(T m1, T m2)
            where T : IMatchable<T>
        {
            Assert.True(m1.Matches(m2));
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
        /// Flattens an enumerable of enumerables into a single enumerable.
        /// </summary>
        /// <param name="self"/>
        /// <typeparam name="T">Type of item in the lists.</typeparam>
        /// <returns>A enumerable with all (flattened) values.</returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> self)
        {
            foreach (var enumerable in self)
                foreach (var item in enumerable)
                    yield return item;
        }

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
