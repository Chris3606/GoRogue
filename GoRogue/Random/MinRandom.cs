using System;
using JetBrains.Annotations;
using ShaiRandom.Generators;

namespace GoRogue.Random
{
    /// <summary>
    /// A "random" number generator that always returns the minimum possible value for a given function call.
    /// May be useful for testing. Also used in <see cref="DiceNotation.DiceExpression" /> for certain minimum roll
    /// functions.
    /// </summary>
    [PublicAPI]
    public class MinRandom : IEnhancedRandom
    {
        private const decimal DecimalEpsilon = 0.0000000000000000000000000001M;

        /// <summary>
        /// Returns int.MinValue.
        /// </summary>
        /// <returns>int.MinValue</returns>
        public int NextInt() => int.MinValue;

        /// <summary>
        /// Returns 0.
        /// </summary>
        /// <returns>0</returns>
        public int NextInt(int maxValue) => 0;

        /// <summary>
        /// Returns <paramref name="minValue" />.
        /// </summary>
        /// <param name="minValue">
        /// The minimum value for the returned number (which is always returned by this generator)
        /// </param>
        /// <param name="maxValue">
        /// The maximum value for the returned number (which is unused since this generator always
        /// returns the minimum).
        /// </param>
        /// <returns>
        ///     <paramref name="minValue" />
        /// </returns>
        public int NextInt(int minValue, int maxValue) => minValue;

        /// <summary>
        /// Returns false.
        /// </summary>
        /// <returns>false</returns>
        public bool NextBool() => false;

        /// <summary>
        /// Returns 0.0.
        /// </summary>
        /// <returns>0.0</returns>
        public float NextFloat() => 0.0f;

        /// <summary>
        /// Returns 0.0.
        /// </summary>
        /// <param name="outerBound">Outer bound for returned value; unused since this function always returns the min (0).</param>
        /// <returns>0.0</returns>
        public float NextFloat(float outerBound) => 0;

        /// <summary>
        /// Returns <paramref name="innerBound"/>.
        /// </summary>
        /// <param name="innerBound">Inner bound for the returned value; always returned by this function.</param>
        /// <param name="outerBound">Outer bound for returned value; unused since this function always returns the min (0).</param>
        /// <returns>0.0</returns>
        public float NextFloat(float innerBound, float outerBound) => innerBound;

        /// <summary>
        /// Does nothing since this generator has no state.
        /// </summary>
        /// <param name="seed"/>
        public void Seed(ulong seed)
        { }

        /// <summary>
        /// Simply creates a new min random; there is no state to copy.
        /// </summary>
        /// <returns>A new MinRandom generator.</returns>
        public IEnhancedRandom Copy() => new MinRandom();

        /// <summary>
        /// This generator does not support serialization.
        /// </summary>
        public string StringSerialize()
            => throw new NotSupportedException($"{nameof(MinRandom)} does not support serialization.");

        /// <summary>
        /// This generator does not support serialization.
        /// </summary>
        public IEnhancedRandom StringDeserialize(ReadOnlySpan<char> data)
            => throw new NotSupportedException($"{nameof(MinRandom)} does not support serialization.");

        /// <summary>
        /// Does nothing since this generator has no state.
        /// </summary>
        public ulong SelectState(int selection) => 0;

        /// <summary>
        /// Does nothing since this generator has no state.
        /// </summary>
        public void SetSelectedState(int selection, ulong value)
        { }

        /// <summary>
        /// Returns 0.
        /// </summary>
        /// <returns>0</returns>
        public ulong NextULong() => 0;

        /// <summary>
        /// Returns 0.
        /// </summary>
        /// <param name="bound"/>
        /// <returns>0</returns>
        public ulong NextULong(ulong bound) => 0;

        /// <summary>
        /// Returns <paramref name="inner"/>.
        /// </summary>
        /// <param name="inner"/>
        /// <param name="outer"/>
        /// <returns><paramref name="inner"/></returns>
        public ulong NextULong(ulong inner, ulong outer) => inner;

        /// <summary>
        /// Returns long.MinValue.
        /// </summary>
        /// <returns>long.MinValue</returns>
        public long NextLong() => long.MinValue;

        /// <summary>
        /// Returns 0.
        /// </summary>
        /// <param name="outerBound"/>
        /// <returns>0</returns>
        public long NextLong(long outerBound) => 0;

        /// <summary>
        /// Returns <paramref name="inner"/>.
        /// </summary>
        /// <param name="inner"/>
        /// <param name="outer"/>
        /// <returns><paramref name="inner"/></returns>
        public long NextLong(long inner, long outer) => inner;

        /// <summary>
        /// Returns a number with all bits 0.
        /// </summary>
        /// <param name="bits"/>
        /// <returns>0</returns>
        public uint NextBits(int bits) => 0;

        /// <summary>
        /// Fills the given buffer with bytes of value 0.
        /// </summary>
        /// <param name="buffer">The buffer to fill.</param>
        public void NextBytes(Span<byte> buffer)
        {
            for (var i = 0; i < buffer.Length; i++)
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
        /// Returns <paramref name="minValue" />.
        /// </summary>
        /// <param name="minValue">
        /// The minimum value for the returned number (always returned since this function always
        /// returns the minimum).
        /// </param>
        /// <param name="maxValue">The maximum value for the returned number (unused).</param>
        /// <returns>
        ///     <paramref name="minValue" />
        /// </returns>
        public double NextDouble(double minValue, double maxValue) => minValue;

        /// <summary>
        /// Returns 0.0.
        /// </summary>
        /// <returns>0.0</returns>
        public double NextInclusiveDouble() => 0.0;

        /// <summary>
        /// Returns 0.0.
        /// </summary>
        /// <param name="outerBound"/>
        /// <returns>0.0</returns>
        public double NextInclusiveDouble(double outerBound) => 0.0;

        /// <summary>
        /// Returns <paramref name="innerBound"/>
        /// </summary>
        /// <param name="innerBound"/>
        /// <param name="outerBound"/>
        /// <returns><paramref name="innerBound"/></returns>
        public double NextInclusiveDouble(double innerBound, double outerBound) => innerBound;

        /// <summary>
        /// Returns 0.0.
        /// </summary>
        /// <returns>0.0</returns>
        public float NextInclusiveFloat() => 0.0f;

        /// <summary>
        /// Returns 0.0.
        /// </summary>
        /// <param name="outerBound"/>
        /// <returns>0.0</returns>
        public float NextInclusiveFloat(float outerBound) => 0.0f;

        /// <summary>
        /// Returns <paramref name="innerBound"/>
        /// </summary>
        /// <param name="innerBound"/>
        /// <param name="outerBound"/>
        /// <returns><paramref name="innerBound"/></returns>
        public float NextInclusiveFloat(float innerBound, float outerBound) => innerBound;

        public decimal NextInclusiveDecimal() => 0;

        public decimal NextInclusiveDecimal(decimal outerBound) => Math.Min(0, outerBound);

        public decimal NextInclusiveDecimal(decimal innerBound, decimal outerBound) => Math.Min(innerBound, outerBound);

        /// <summary>
        /// Returns double.Epsilon.
        /// </summary>
        /// <returns>double.Epsilon</returns>
        public double NextExclusiveDouble() => double.Epsilon;

        /// <summary>
        /// Returns double.Epsilon.
        /// </summary>
        /// <param name="outerBound"/>
        /// <returns>double.Epsilon</returns>
        public double NextExclusiveDouble(double outerBound) => double.Epsilon;

        /// <summary>
        /// Returns <paramref name="innerBound"/> + double.Epsilon.
        /// </summary>
        /// <param name="innerBound"/>
        /// <param name="outerBound"/>
        /// <returns><paramref name="innerBound"/> + double.Epsilon</returns>
        public double NextExclusiveDouble(double innerBound, double outerBound) => innerBound + double.Epsilon;

        /// <summary>
        /// Returns float.Epsilon.
        /// </summary>
        /// <returns>float.Epsilon</returns>
        public float NextExclusiveFloat() => float.Epsilon;

        /// <summary>
        /// Returns float.Epsilon.
        /// </summary>
        /// <param name="outerBound"/>
        /// <returns>float.Epsilon</returns>
        public float NextExclusiveFloat(float outerBound) => float.Epsilon;

        /// <summary>
        /// Returns <paramref name="innerBound"/> + float.Epsilon.
        /// </summary>
        /// <param name="innerBound"/>
        /// <param name="outerBound"/>
        /// <returns><paramref name="innerBound"/> + float.Epsilon</returns>
        public float NextExclusiveFloat(float innerBound, float outerBound) => innerBound + float.Epsilon;

        public decimal NextExclusiveDecimal() => DecimalEpsilon;

        public decimal NextExclusiveDecimal(decimal outerBound) => NextExclusiveDecimal(0, outerBound);

        public decimal NextExclusiveDecimal(decimal innerBound, decimal outerBound) => Math.Min(innerBound + DecimalEpsilon, outerBound - DecimalEpsilon);

        /// <summary>
        /// Returns 0.0.
        /// </summary>
        /// <returns>0.0.</returns>
        public decimal NextDecimal() => 0.0M;

        /// <summary>
        /// Returns 0.0.
        /// </summary>
        /// <param name="outerBound"/>
        /// <returns>0.0</returns>
        public decimal NextDecimal(decimal outerBound) => 0.0M;

        /// <summary>
        /// Returns <paramref name="innerBound"/>.
        /// </summary>
        /// <param name="innerBound"/>
        /// <param name="outerBound"/>
        /// <returns><paramref name="innerBound"/></returns>
        public decimal NextDecimal(decimal innerBound, decimal outerBound) => innerBound;

        /// <summary>
        /// Supported, but does nothing since the unbounded generation functions always return the same value.
        /// </summary>
        /// <returns>0</returns>
        public ulong Skip(ulong distance) => 0;

        /// <summary>
        /// Supported, but does nothing since the unbounded generation functions always return the same value.
        /// </summary>
        /// <returns>0</returns>
        public ulong PreviousULong() =>  0;

        /// <summary>
        /// 0, since this generator has no states.
        /// </summary>
        public int StateCount => 0;

        /// <summary>
        /// This generator supports <see cref="SelectState"/>, although its implementation does nothing.
        /// </summary>
        public bool SupportsReadAccess => true;

        /// <summary>
        /// This generator supports <see cref="SetSelectedState"/>, although its implementation does nothing.
        /// </summary>
        public bool SupportsWriteAccess => true;

        /// <summary>
        /// This generator supports <see cref="Skip"/>, although its implementation does nothing.
        /// </summary>
        public bool SupportsSkip => true;

        /// <summary>
        /// This generator supports <see cref="PreviousULong"/>, although its implementation does nothing.
        /// </summary>
        public bool SupportsPrevious => true;

        /// <summary>
        /// Throws exception since the generator does not support serialization.
        /// </summary>
        public string Tag => throw new NotSupportedException();

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
        /// Returns <paramref name="minValue" />.
        /// </summary>
        /// <param name="minValue">
        /// The minimum value for the returned number (this generator always returns the minimum value).
        /// </param>
        /// <param name="maxValue">The maximum value for the returned number (unused).</param>
        /// <returns>
        ///     <paramref name="minValue" />
        /// </returns>
        public uint NextUInt(uint minValue, uint maxValue) => minValue;
    }
}
