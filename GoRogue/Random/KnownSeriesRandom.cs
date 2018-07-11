using System;
using System.Collections.Generic;
using System.Linq;
using Troschuetz.Random;

namespace GoRogue.Random
{
    /// <summary>
    /// "Random number generator" that takes in a series of values, and simply returns them
    /// sequentially when RNG functions are called.
    /// </summary>
    /// <remarks>
    /// This class may be useful for testing, when you want to specify the numbers returned by an RNG
    /// without drastically modifying any code using the RNG.
    /// </remarks>
    public class KnownSeriesGenerator : IGenerator
    {
        private int boolIndex;
        private List<bool> boolSeries;
        private int byteIndex;
        private List<byte> byteSeries;
        private int doubleIndex;
        private List<double> doubleSeries;
        private int intIndex;
        private List<int> intSeries;
        private int uintIndex;
        private List<uint> uintSeries;

        /// <summary>
        /// Creates a new known series generator, with parameters to indicate which series to use for
        /// the integer, unsigned integer, double, bool, and byte-based RNG functions. If null is
        /// specified, no values of that type may be returned, and functions that try to return a
        /// value of that type will throw an exception.
        /// </summary>
        public KnownSeriesGenerator(IEnumerable<int> intSeries = null, IEnumerable<uint> uintSeries = null, IEnumerable<double> doubleSeries = null, IEnumerable<bool> boolSeries = null, IEnumerable<byte> byteSeries = null)
        {
            if (intSeries == null)
                this.intSeries = new List<int>();
            else
                this.intSeries = intSeries.ToList();

            if (uintSeries == null)
                this.uintSeries = new List<uint>();
            else
                this.uintSeries = uintSeries.ToList();

            if (doubleSeries == null)
                this.doubleSeries = new List<double>();
            else
                this.doubleSeries = doubleSeries.ToList();

            if (boolSeries == null)
                this.boolSeries = new List<bool>();
            else
                this.boolSeries = boolSeries.ToList();

            if (byteSeries == null)
                this.byteSeries = new List<byte>();
            else
                this.byteSeries = byteSeries.ToList();
        }

        /// <summary>
        /// Whether or not the RNG is capable of resetting, such that it will return the same series
        /// of values again.
        /// </summary>
        public bool CanReset => true;

        /// <summary>
        /// Since this RNG returns a known series of values, this field will always return 0.
        /// </summary>
        public uint Seed => 0;

        /// <summary>
        /// Gets the next number in the underlying int series. If the value is less than 0 or greater
        /// than/equal to maxValue, throws an exception.
        /// </summary>
        /// <param name="maxValue">Maximum allowable number that can be returned (exclusive).</param>
        /// <returns>The appropriate number from the series.</returns>
        public int Next(int maxValue) => Next(0, maxValue);

        /// <summary>
        /// Gets the next number in the underlying series. If the value is less than minValue or
        /// greater than/equal to maxValue, throws an exception.
        /// </summary>
        /// <param name="minValue">Minimum allowable number that can be returned.</param>
        /// <param name="maxValue">Maximum allowable number that can be returned (exclusive).</param>
        /// <returns>The appropriate number from the series.</returns>
        public int Next(int minValue, int maxValue) => returnIfRange(minValue, maxValue, intSeries, ref intIndex);

        /// <summary>
        /// Gets the next integer in the underlying series. If the integer is equal to int.MaxValue,
        /// throws an exception.
        /// </summary>
        /// <returns>The next integer in the underlying series.</returns>
        public int Next() => Next(0, int.MaxValue);

        /// <summary>
        /// Returns the next boolean in the underlying series.
        /// </summary>
        /// <returns>The next boolean value in the underlying series.</returns>
        public bool NextBoolean() => returnValueFrom(boolSeries, ref boolIndex);

        /// <summary>
        /// Fills the specified buffer with values from the underlying byte series.
        /// </summary>
        /// <param name="buffer">Buffer to fill.</param>
        public void NextBytes(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = returnValueFrom(byteSeries, ref byteIndex);
        }

        /// <summary>
        /// Returns the next double in the underlying series. If the double is less than 0.0 or
        /// greater than/equal to 1.0, throws an exception.
        /// </summary>
        /// <returns>The next double in the underlying series.</returns>
        public double NextDouble() => NextDouble(0.0, 1.0);

        /// <summary>
        /// Returns the next double in the underlying series. If the double is less than 0.0 or
        /// greater than/equal to maxValue, throws an exception.
        /// </summary>
        /// <param name="maxValue">The maximum value for the returned value, exclusive.</param>
        /// <returns>The next double in the underlying series.</returns>
        public double NextDouble(double maxValue) => NextDouble(0, maxValue);

        /// <summary>
        /// Returns the next double in the underlying series. If the double is less than minValue or
        /// greater than/equal to maxValue, throws an exception.
        /// </summary>
        /// <param name="minValue">Minimum value for the returned number, inclusive.</param>
        /// <param name="maxValue">Maximum value for the returned number, exclusive.</param>
        /// <returns>The next double in the underlying series.</returns>
        public double NextDouble(double minValue, double maxValue) => returnIfRange(minValue, maxValue, doubleSeries, ref doubleIndex);

        /// <summary>
        /// Returns the next integer in the underlying series. If the value is less than 0, throws an exception.
        /// </summary>
        /// <returns>The next integer in the underlying series.</returns>
        public int NextInclusiveMaxValue() => returnIfRangeInclusive(0, int.MaxValue, intSeries, ref intIndex);

        /// <summary>
        /// Returns the next unsigned integer in the underlying series. If the value is equal to
        /// uint.MaxValue, throws an exception.
        /// </summary>
        /// <returns>The next unsigned integer in the underlying series.</returns>
        public uint NextUInt() => NextUInt(0, uint.MaxValue);

        /// <summary>
        /// Returns the next unsigned integer in the underlying series. If the value is greater than
        /// or equal to maxValue, throws an exception.
        /// </summary>
        /// <param name="maxValue">The maximum value for the returned number, exclusive.</param>
        /// <returns>The next unsigned integer in the underlying series.</returns>
        public uint NextUInt(uint maxValue) => NextUInt(0, maxValue);

        /// <summary>
        /// Returns the next unsigned integer in the underlying series. If the value is less than
        /// minValue, or greater than/equal to maxValue, throws an exception.
        /// </summary>
        /// <param name="minValue">The minimum value for the returned number, inclusive.</param>
        /// <param name="maxValue">The maximum value for the returned number, exclusive.</param>
        /// <returns>The next unsigned integer in the underlying series.</returns>
        public uint NextUInt(uint minValue, uint maxValue) => returnIfRange(minValue, maxValue, uintSeries, ref uintIndex);

        /// <summary>
        /// Returns the next unsigned integer in the underlying series. If the value is equal to
        /// uint.MaxValue, throws an exception.
        /// </summary>
        /// <returns>The next unsigned integer in the underlying series.</returns>
        public uint NextUIntExclusiveMaxValue() => NextUInt();

        /// <summary>
        /// Returns the next unsigned integer in the underlying series.
        /// </summary>
        /// <returns>The next unsinged integer in the underlying series.</returns>
        public uint NextUIntInclusiveMaxValue() => returnIfRangeInclusive((uint)0, uint.MaxValue, uintSeries, ref uintIndex);

        /// <summary>
        /// Resets the random number generator, such that it starts returning values from the
        /// beginning of the underlying series.
        /// </summary>
        /// <returns>True, since the reset cannot fail.</returns>
        public bool Reset()
        {
            intIndex = 0;
            uintIndex = 0;
            doubleIndex = 0;
            boolIndex = 0;
            byteIndex = 0;

            return true;
        }

        /// <summary>
        /// Resets the random number generator, such that it starts returning the values from the
        /// beginning of the underlying series.
        /// </summary>
        /// <param name="seed">Unused, since there is no seed for a known-series RNG.</param>
        /// <returns>True, since the reset cannot fail.</returns>
        public bool Reset(uint seed) => Reset();

        private static T returnIfRange<T>(T minValue, T maxValue, List<T> series, ref int seriesIndex) where T : IComparable<T>
        {
            T value = returnValueFrom(series, ref seriesIndex);

            if (minValue.CompareTo(value) < 0)
                throw new ArgumentException("Value returned is less than minimum value.");

            if (maxValue.CompareTo(value) >= 0)
                throw new ArgumentException("Value returned is greater than/equal to maximum value.");

            return value;
        }

        private static T returnIfRangeInclusive<T>(T minValue, T maxValue, List<T> series, ref int seriesIndex) where T : IComparable<T>
        {
            T value = returnValueFrom(series, ref seriesIndex);

            if (minValue.CompareTo(value) < 0)
                throw new ArgumentException("Value returned is less than minimum value.");

            if (maxValue.CompareTo(value) > 0)
                throw new ArgumentException("Value returned is greater than/equal to maximum value.");

            return value;
        }

        private static T returnValueFrom<T>(List<T> series, ref int seriesIndex)
        {
            if (series.Count == 0)
                throw new NotSupportedException("Tried to get value of type " + typeof(T).Name + ", but the KnownSeriesGenerator was not given any values of that type.");

            T value = series[seriesIndex];
            seriesIndex = MathHelpers.WrapAround(seriesIndex + 1, series.Count);

            return value;
        }
    }
}