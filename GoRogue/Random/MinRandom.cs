using Troschuetz.Random;

namespace GoRogue.Random
{
	/// <summary>
	/// A "random" number generator that always returns the minValue parameter given, or 0 on the
	/// Next overload that only takes maxValue. Again, may be useful for testing. Also used in
	/// <see cref="DiceNotation.DiceExpression"/> for certain minimum roll functions.
	/// </summary>
	public class MinRandom : IGenerator
	{
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
		/// Returns 0.0
		/// </summary>
		/// <returns>0.0</returns>
		public int Next() => 0;

		/// <summary>
		/// Returns 0.0
		/// </summary>
		/// <returns>0.0</returns>
		public int Next(int maxValue) => 0;

		/// <summary>
		/// Returns <paramref name="minValue"/>.
		/// </summary>
		/// <param name="minValue">
		/// The minimum value for the returned number (which is always returned by this generator)
		/// </param>
		/// <param name="maxValue">
		/// The maximum value for the returned number (which is unused since this generator always
		/// returns the minimum.
		/// </param>
		/// <returns><paramref name="minValue"/></returns>
		public int Next(int minValue, int maxValue) => minValue;

		/// <summary>
		/// Returns false.
		/// </summary>
		/// <returns>false</returns>
		public bool NextBoolean() => false;

		/// <summary>
		/// Fills the given buffer with bytes of value 0.
		/// </summary>
		/// <param name="buffer">The buffer to fill.</param>
		public void NextBytes(byte[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = 0;
		}

		/// <summary>
		/// Returns 0.0.
		/// </summary>
		/// <returns>0.0</returns>
		public double NextDouble() => 0.0;

		/// <summary>
		/// Returns 0.0.
		/// </summary>
		/// <param name="maxValue">
		/// The maximum value for the returned number, exclusive. Unused since this function always
		/// returns the minimum.
		/// </param>
		/// <returns>0.0</returns>
		public double NextDouble(double maxValue) => 0.0;

		/// <summary>
		/// Returns <paramref name="minValue"/>.
		/// </summary>
		/// <param name="minValue">
		/// The minimum value for the returned number (always returned since this function always
		/// returns the minimum).
		/// </param>
		/// <param name="maxValue">The maximum vlaue for the returned number (unused).</param>
		/// <returns><paramref name="minValue"/></returns>
		public double NextDouble(double minValue, double maxValue) => minValue;

		/// <summary>
		/// Returns 0.
		/// </summary>
		/// <returns>0</returns>
		public int NextInclusiveMaxValue() => 0;

		/// <summary>
		/// Returns 0.
		/// </summary>
		/// <returns>0</returns>
		public uint NextUInt() => 0;

		/// <summary>
		/// Returns 0.
		/// </summary>
		/// <param name="maxValue">
		/// The maximum value for the returned number (unused since this generator always returns the minimum).
		/// </param>
		/// <returns>0</returns>
		public uint NextUInt(uint maxValue) => 0;

		/// <summary>
		/// Returns <paramref name="minValue"/>.
		/// </summary>
		/// <param name="minValue">
		/// The minimum value for the returned number (this generator always returns the minimum value).
		/// </param>
		/// <param name="maxValue">The maximum value for the returned number (unused).</param>
		/// <returns><paramref name="minValue"/></returns>
		public uint NextUInt(uint minValue, uint maxValue) => minValue;

		/// <summary>
		/// Returns 0.
		/// </summary>
		/// <returns>0</returns>
		public uint NextUIntExclusiveMaxValue() => 0;

		/// <summary>
		/// Returns 0.
		/// </summary>
		/// <returns>0</returns>
		public uint NextUIntInclusiveMaxValue() => 0;

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