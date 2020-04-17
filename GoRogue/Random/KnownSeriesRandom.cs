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
		private int _boolIndex;
		private readonly List<bool> _boolSeries;
		private int _byteIndex;
		private readonly List<byte> _byteSeries;
		private int _doubleIndex;
		private readonly List<double> _doubleSeries;
		private int _intIndex;
		private readonly List<int> _intSeries;
		private int _uintIndex;
		private readonly List<uint> _uintSeries;

		/// <summary>
		/// Creates a new known series generator, with parameters to indicate which series to use for
		/// the integer, unsigned integer, double, bool, and byte-based RNG functions. If null is
		/// specified, no values of that type may be returned, and functions that try to return a
		/// value of that type will throw an exception.
		/// </summary>
		public KnownSeriesGenerator(IEnumerable<int>? intSeries = null, IEnumerable<uint>? uintSeries = null, IEnumerable<double>? doubleSeries = null, IEnumerable<bool>? boolSeries = null, IEnumerable<byte>? byteSeries = null)
		{
			if (intSeries == null)
				_intSeries = new List<int>();
			else
				_intSeries = intSeries.ToList();

			if (uintSeries == null)
				_uintSeries = new List<uint>();
			else
				_uintSeries = uintSeries.ToList();

			if (doubleSeries == null)
				_doubleSeries = new List<double>();
			else
				_doubleSeries = doubleSeries.ToList();

			if (boolSeries == null)
				_boolSeries = new List<bool>();
			else
				_boolSeries = boolSeries.ToList();

			if (byteSeries == null)
				_byteSeries = new List<byte>();
			else
				_byteSeries = byteSeries.ToList();
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
		/// than/equal to <paramref name="maxValue"/>, throws an exception.
		/// </summary>
		/// <param name="maxValue">Maximum allowable number that can be returned (exclusive).</param>
		/// <returns>The appropriate number from the series.</returns>
		public int Next(int maxValue) => Next(0, maxValue);

		/// <summary>
		/// Gets the next number in the underlying series. If the value is less than <paramref name="minValue"/> or
		/// greater than/equal to <paramref name="maxValue"/>, throws an exception.
		/// </summary>
		/// <param name="minValue">Minimum allowable number that can be returned.</param>
		/// <param name="maxValue">Maximum allowable number that can be returned (exclusive).</param>
		/// <returns>The appropriate number from the series.</returns>
		public int Next(int minValue, int maxValue) => returnIfRange(minValue, maxValue, _intSeries, ref _intIndex);

		/// <summary>
		/// Gets the next integer in the underlying series. If the integer is equal to <see cref="int.MaxValue"/>,
		/// throws an exception.
		/// </summary>
		/// <returns>The next integer in the underlying series.</returns>
		public int Next() => Next(0, int.MaxValue);

		/// <summary>
		/// Returns the next boolean in the underlying series.
		/// </summary>
		/// <returns>The next boolean value in the underlying series.</returns>
		public bool NextBoolean() => returnValueFrom(_boolSeries, ref _boolIndex);

        /// <summary>
		/// Fills the specified buffer with values from the underlying byte series.
		/// </summary>
		/// <param name="buffer">Buffer to fill.</param>
		public void NextBytes(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = returnValueFrom(_byteSeries, ref _byteIndex);
        }

        /// <summary>
        /// Fills the specified buffer with values from the underlying byte series.
        /// </summary>
        /// <param name="buffer">Buffer to fill.</param>
        public void NextBytes(Span<byte> buffer)
		{
			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = returnValueFrom(_byteSeries, ref _byteIndex);
		}

		/// <summary>
		/// Returns the next double in the underlying series. If the double is less than 0.0 or
		/// greater than/equal to 1.0, throws an exception.
		/// </summary>
		/// <returns>The next double in the underlying series.</returns>
		public double NextDouble() => NextDouble(0.0, 1.0);

		/// <summary>
		/// Returns the next double in the underlying series. If the double is less than 0.0 or
		/// greater than/equal to <paramref name="maxValue"/>, throws an exception.
		/// </summary>
		/// <param name="maxValue">The maximum value for the returned value, exclusive.</param>
		/// <returns>The next double in the underlying series.</returns>
		public double NextDouble(double maxValue) => NextDouble(0, maxValue);

		/// <summary>
		/// Returns the next double in the underlying series. If the double is less than <paramref name="minValue"/>
		/// or greater than/equal to <paramref name="maxValue"/>, throws an exception.
		/// </summary>
		/// <param name="minValue">Minimum value for the returned number, inclusive.</param>
		/// <param name="maxValue">Maximum value for the returned number, exclusive.</param>
		/// <returns>The next double in the underlying series.</returns>
		public double NextDouble(double minValue, double maxValue) => returnIfRange(minValue, maxValue, _doubleSeries, ref _doubleIndex);

		/// <summary>
		/// Returns the next integer in the underlying series. If the value is less than 0, throws an exception.
		/// </summary>
		/// <returns>The next integer in the underlying series.</returns>
		public int NextInclusiveMaxValue() => returnIfRangeInclusive(0, int.MaxValue, _intSeries, ref _intIndex);

		/// <summary>
		/// Returns the next unsigned integer in the underlying series. If the value is equal to
		/// <see cref="uint.MaxValue"/>, throws an exception.
		/// </summary>
		/// <returns>The next unsigned integer in the underlying series.</returns>
		public uint NextUInt() => NextUInt(0, uint.MaxValue);

		/// <summary>
		/// Returns the next unsigned integer in the underlying series. If the value is greater than
		/// or equal to <paramref name="maxValue"/>, throws an exception.
		/// </summary>
		/// <param name="maxValue">The maximum value for the returned number, exclusive.</param>
		/// <returns>The next unsigned integer in the underlying series.</returns>
		public uint NextUInt(uint maxValue) => NextUInt(0, maxValue);

		/// <summary>
		/// Returns the next unsigned integer in the underlying series. If the value is less than
		/// <paramref name="minValue"/>, or greater than/equal to <paramref name="maxValue"/>, throws an exception.
		/// </summary>
		/// <param name="minValue">The minimum value for the returned number, inclusive.</param>
		/// <param name="maxValue">The maximum value for the returned number, exclusive.</param>
		/// <returns>The next unsigned integer in the underlying series.</returns>
		public uint NextUInt(uint minValue, uint maxValue) => returnIfRange(minValue, maxValue, _uintSeries, ref _uintIndex);

		/// <summary>
		/// Returns the next unsigned integer in the underlying series. If the value is equal to
		/// <see cref="uint.MaxValue"/>, throws an exception.
		/// </summary>
		/// <returns>The next unsigned integer in the underlying series.</returns>
		public uint NextUIntExclusiveMaxValue() => NextUInt();

		/// <summary>
		/// Returns the next unsigned integer in the underlying series.
		/// </summary>
		/// <returns>The next unsinged integer in the underlying series.</returns>
		public uint NextUIntInclusiveMaxValue() => returnIfRangeInclusive((uint)0, uint.MaxValue, _uintSeries, ref _uintIndex);

		/// <summary>
		/// Resets the random number generator, such that it starts returning values from the
		/// beginning of the underlying series.
		/// </summary>
		/// <returns>True, since the reset cannot fail.</returns>
		public bool Reset()
		{
			_intIndex = 0;
			_uintIndex = 0;
			_doubleIndex = 0;
			_boolIndex = 0;
			_byteIndex = 0;

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
