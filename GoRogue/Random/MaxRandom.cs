using Troschuetz.Random;

namespace GoRogue.Random
{
	/// <summary>
	/// A "random" number generator that always returns the maxValue parameter given. Again this may
	/// be useful in testing, testing the upper range or repeatedly returning a value. Also used in
	/// <see cref="DiceNotation.DiceExpression"/> instances for certain max roll functions.
	/// </summary>
	public class MaxRandom : IGenerator
	{
		private static readonly double DOUBLE_EPSILON = 1 - 0.99999999999999978;

		/// <summary>
		/// Whether or not the RNG is capable of resetting, such that it will return the same series
		/// of values again.
		/// </summary>
		public bool CanReset => true;

		/// <summary>
		/// Since this RNG returns the maximum possible value, this field will always return 0.
		///</summary>
		public uint Seed => 0;

		/// <summary>
		/// Returns <see cref="int.MaxValue"/> - 1.
		/// </summary>
		/// <returns><see cref="int.MaxValue"/> - 1.</returns>
		public int Next() => int.MaxValue - 1;

		/// <summary>
		/// Returns <paramref name="maxValue"/> - 1.
		/// </summary>
		/// <param name="maxValue">Maximum bound of the returned number, exclusive.</param>
		/// <returns><paramref name="maxValue"/> - 1.</returns>
		public int Next(int maxValue) => maxValue - 1;

		/// <summary>
		/// Returns <paramref name="maxValue"/> - 1.
		/// </summary>
		/// <param name="minValue">
		/// The minimum value that can be returned (unused since this generator always returns the maximum).
		/// </param>
		/// <param name="maxValue">The maximum value of the returned number, exclusive.</param>
		/// <returns><paramref name="maxValue"/> - 1.</returns>
		public int Next(int minValue, int maxValue) => maxValue - 1;

		/// <summary>
		/// Returns true.
		/// </summary>
		/// <returns>true</returns>
		public bool NextBoolean() => true;

		/// <summary>
		/// Fills the given buffer with bytes of value <see cref="byte.MaxValue"/>.
		/// </summary>
		/// <param name="buffer">Buffer to fill.</param>
		public void NextBytes(byte[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = byte.MaxValue;
		}

		/// <summary>
		/// Returns a number very close to (but still less than) 1.0.
		/// </summary>
		/// <remarks>Value returned is 0.99999999999999978.</remarks>
		/// <returns>A number very close to (but still less than) 1.0.</returns>
		public double NextDouble() => 1 - DOUBLE_EPSILON;

		/// <summary>
		/// Returns a double very close to (but still less than) <paramref name="maxValue"/>.
		/// </summary>
		/// <param name="maxValue">Maximum value for the returned value (exclusive).</param>
		/// <returns>A double very close to (but still less than) <paramref name="maxValue"/>.</returns>
		public double NextDouble(double maxValue) => maxValue - DOUBLE_EPSILON;

		/// <summary>
		/// Returns a double very close to (but still less than) <paramref name="maxValue"/>.
		/// </summary>
		/// <param name="minValue">
		/// Minimum value for the returned value. Unused since this generator always returns the
		/// maximum value.
		/// </param>
		/// <param name="maxValue">Maximum value for the returned value (exclusive).</param>
		/// <returns>A double very close to (but still less than) <paramref name="maxValue"/>.</returns>
		public double NextDouble(double minValue, double maxValue) => maxValue - DOUBLE_EPSILON;

		/// <summary>
		/// Returns <see cref="int.MaxValue"/>.
		/// </summary>
		/// <returns>int.MaxValue</returns>
		public int NextInclusiveMaxValue() => int.MaxValue;

		/// <summary>
		/// Returns <see cref="uint.MaxValue"/>.
		/// </summary>
		/// <returns><see cref="uint.MaxValue"/> - 1.</returns>
		public uint NextUInt() => uint.MaxValue - 1;

		/// <summary>
		/// Returns <paramref name="maxValue"/> - 1.
		/// </summary>
		/// <param name="maxValue">The maximum bound for the returned number, exclusive.</param>
		/// <returns><paramref name="maxValue"/> - 1</returns>
		public uint NextUInt(uint maxValue) => maxValue - 1;

		/// <summary>
		/// Returns <paramref name="maxValue"/> - 1.
		/// </summary>
		/// <param name="minValue">
		/// The minimum value that can be returned (unused since this generator always returns the maximum).
		/// </param>
		/// <param name="maxValue">The maximum bound for the returned number, exclusive.</param>
		/// <returns><paramref name="maxValue"/> - 1</returns>
		public uint NextUInt(uint minValue, uint maxValue) => maxValue - 1;

		/// <summary>
		/// Returns <see cref="uint.MaxValue"/> - 1.
		/// </summary>
		/// <returns><see cref="uint.MaxValue"/> - 1.</returns>
		public uint NextUIntExclusiveMaxValue() => NextUInt();

		/// <summary>
		/// Returns <see cref="uint.MaxValue"/>.
		/// </summary>
		/// <returns><see cref="uint.MaxValue"/>.</returns>
		public uint NextUIntInclusiveMaxValue() => uint.MaxValue;

		/// <summary>
		/// Simply returns true, since this generator has no state.
		/// </summary>
		/// <returns>true</returns>
		public bool Reset() => true;

		/// <summary>
		/// Simply returns true, since this generator has no state.
		/// </summary>
		/// <param name="seed">Unused, since this generator has no seed.</param>
		/// <returns>true</returns>
		public bool Reset(uint seed) => true;
	}
}