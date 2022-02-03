using System;
using JetBrains.Annotations;
using ShaiRandom.Generators;

namespace GoRogue.Random
{
    /// <summary>
    /// A "random" number generator that always returns the maxValue parameter given. This may
    /// be useful in testing, testing the upper range or repeatedly returning a value. Also used in
    /// <see cref="DiceNotation.DiceExpression" /> instances for certain max roll functions.
    /// </summary>
    [PublicAPI]
    public class MaxRandom : IEnhancedRandom
    {
        private static readonly float s_floatAdjust = MathF.Pow(2f, -24f);
        private static readonly double s_doubleAdjust = Math.Pow(2.0, -53.0);
        //private const decimal DecimalEpsilon = 0.0000000000000000000000000001M;

        /// <summary>
        /// Returns <see cref="int.MaxValue" />.
        /// </summary>
        /// <returns><see cref="int.MaxValue" />.</returns>
        public int NextInt() => int.MaxValue;

        /// <summary>
        /// Returns the maximum value that could possibly be returned by this function if it was called on a regular
        /// generator adhering to the IEnhancedRandom contract with these parameters.
        /// </summary>
        /// <param name="maxValue">Maximum bound of the returned number, exclusive.</param>
        /// <returns><paramref name="maxValue" /> - 1.</returns>
        public int NextInt(int maxValue) => NextInt(0, maxValue);

        /// <summary>
        /// Returns the maximum value that could possibly be returned by this function if it was called on a regular
        /// generator adhering to the IEnhancedRandom contract with these parameters.
        /// </summary>
        /// <param name="minValue">
        /// The minimum value that can be returned (unused unless maxValue &lt; minValue).
        /// </param>
        /// <param name="maxValue">The maximum value of the returned number, exclusive.</param>
        /// <returns>Math.Max(minValue, <paramref name="maxValue" /> - 1).</returns>
        public int NextInt(int minValue, int maxValue) => Math.Max(minValue, maxValue - 1);

        /// <summary>
        /// Returns true.
        /// </summary>
        /// <returns>true</returns>
        public bool NextBool() => true;

        /// <summary>
        /// Returns a number very close to (but still less than) 1.0.
        /// </summary>
        /// <returns>A number very close to (but still less than) 1.0.</returns>
        public float NextFloat() => 1.0f - s_floatAdjust;

        /// <summary>
        /// Returns a float very close to (but still less than) <paramref name="outerBound" />, or 0 if outerBound &lt;= 0.
        /// </summary>
        /// <param name="outerBound">Maximum value for the returned value (exclusive).</param>
        /// <returns/>
        public float NextFloat(float outerBound) => NextFloat(0, outerBound);

        /// <summary>
        /// Returns a float very close to (but still less than) <paramref name="outerBound" />, or <paramref name="innerBound"/>
        /// if outerBound &lt; innerBound.
        /// </summary>
        /// <param name="innerBound">
        /// Minimum value for the returned value. Unused unless outerBound &lt; innerBound.
        /// </param>
        /// <param name="outerBound">Maximum value for the returned value (exclusive).</param>
        /// <returns/>
        public float NextFloat(float innerBound, float outerBound) => Math.Max(innerBound, (1.0f - s_floatAdjust) * outerBound);

        /// <summary>
        /// Does nothing since this generator has no state.
        /// </summary>
        /// <param name="seed"/>
        public void Seed(ulong seed)
        { }

        /// <summary>
        /// Simply creates a new max random; there is no state to copy.
        /// </summary>
        /// <returns>A new MaxRandom generator.</returns>
        public IEnhancedRandom Copy() => new MaxRandom();

        /// <summary>
        /// This generator does not support serialization.
        /// </summary>
        public string StringSerialize()
            => throw new NotSupportedException($"{nameof(MaxRandom)} does not support serialization.");

        /// <summary>
        /// This generator does not support serialization.
        /// </summary>
        public IEnhancedRandom StringDeserialize(ReadOnlySpan<char> data)
            => throw new NotSupportedException($"{nameof(MaxRandom)} does not support serialization.");

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
        /// Returns ulong.MaxValue.
        /// </summary>
        /// <returns>ulong.MaxValue</returns>
        public ulong NextULong() => ulong.MaxValue;

        /// <summary>
        /// Returns <paramref name="bound"/> - 1, or 0 if bound was 0.
        /// </summary>
        /// <param name="bound"/>
        /// <returns><paramref name="bound"/> - 1</returns>
        public ulong NextULong(ulong bound) => NextULong(0, bound);

        /// <summary>
        /// Returns the maximum possible value of this function if called with these parameters on a regular generator.
        /// </summary>
        /// <param name="inner"/>
        /// <param name="outer"/>
        /// <returns><paramref name="outer"/>Math.Max(outer - 1, inner).</returns>
        public ulong NextULong(ulong inner, ulong outer) => Math.Max(inner, outer - 1);

        /// <summary>
        /// Returns long.MaxValue.
        /// </summary>
        /// <returns>long.MaxValue</returns>
        public long NextLong() => long.MaxValue;

        /// <summary>
        /// Returns <paramref name="outerBound"/> - 1 if outerBound &gt; 0 otherwise, returns 0.
        /// </summary>
        /// <param name="outerBound"/>
        /// <returns>Math.Max(0, outerBound - 1)</returns>
        public long NextLong(long outerBound) => NextLong(0, outerBound);

        /// <summary>
        /// Returns <paramref name="outer"/> - 1 if outerBound &gt; <paramref name="inner"/> otherwise, returns inner.
        /// </summary>
        /// <param name="inner"/>
        /// <param name="outer"/>
        /// <returns>Math.Max(inner, outer - 1)</returns>
        public long NextLong(long inner, long outer) => Math.Max(inner, outer - 1);

        /// <summary>
        /// Returns a number with the specified number of lower-order bits set to 1 (the max possible value that could
        /// be returned by this function if called on a regular generator).
        /// </summary>
        /// <param name="bits"/>
        /// <returns>A uint with the specified number of its lower-order bits (modulo 32) equal to 1.</returns>
        public uint NextBits(int bits) => (uint)(1 << bits) - 1;

        /// <summary>
        /// Fills the given buffer with bytes of value <see cref="byte.MaxValue" />.
        /// </summary>
        /// <param name="buffer">Buffer to fill.</param>
        public void NextBytes(Span<byte> buffer)
        {
            for (var i = 0; i < buffer.Length; i++)
                buffer[i] = byte.MaxValue;
        }

        /// <summary>
        /// Returns a number very close to (but still less than) 1.0.
        /// </summary>
        /// <returns/>
        public double NextDouble() => 1.0 - s_doubleAdjust;

        /// <summary>
        /// Returns a double very close to (but still less than) <paramref name="maxValue" />, or 0 if maxValue &lt;= 0.
        /// </summary>
        /// <param name="maxValue">Maximum value for the returned value (exclusive).</param>
        /// <returns/>
        public double NextDouble(double maxValue) => NextDouble(0, maxValue);

        /// <summary>
        /// Returns a double very close to (but still less than) <paramref name="maxValue" />, or 0 if maxValue &lt;= <paramref name="minValue"/>.
        /// </summary>
        /// <param name="minValue">
        /// Minimum value for the returned value. Unused unless maxValue &lt;= minValue.
        /// </param>
        /// <param name="maxValue">Maximum value for the returned value (exclusive).</param>
        /// <returns/>
        public double NextDouble(double minValue, double maxValue) => (1.0 - s_doubleAdjust) * maxValue;

        /// <summary>
        /// Returns 1.0.
        /// </summary>
        /// <returns>1.0</returns>
        public double NextInclusiveDouble() => 1.0;

        /// <summary>
        /// Returns Math.Max(0, outerBound).
        /// </summary>
        /// <param name="outerBound"/>
        /// <returns>Math.Max(0, outerBound)</returns>
        public double NextInclusiveDouble(double outerBound) => NextInclusiveDouble(0, outerBound);

        /// <summary>
        /// Returns Math.Max(innerBound, outerBound).
        /// </summary>
        /// <param name="innerBound"/>
        /// <param name="outerBound"/>
        /// <returns>Math.Max(0, outerBound)</returns>
        public double NextInclusiveDouble(double innerBound, double outerBound) => Math.Max(innerBound, outerBound);

        /// <summary>
        /// Returns 1.0.
        /// </summary>
        /// <returns>1.0</returns>
        public float NextInclusiveFloat() => 1.0f;

        /// <summary>
        /// Returns Math.Max(0, outerBound).
        /// </summary>
        /// <param name="outerBound"/>
        /// <returns>Math.Max(0, outerBound)</returns>
        public float NextInclusiveFloat(float outerBound) => NextInclusiveFloat(0, outerBound);

        /// <summary>
        /// Returns Math.Max(innerBound, outerBound).
        /// </summary>
        /// <param name="innerBound"/>
        /// <param name="outerBound"/>
        /// <returns>Math.Max(0, outerBound)</returns>
        public float NextInclusiveFloat(float innerBound, float outerBound) => Math.Max(innerBound, outerBound);

        /// <summary>
        /// Returns 1.0.
        /// </summary>
        /// <returns>1.0</returns>
        public decimal NextInclusiveDecimal() => 1.0M;

        /// <summary>
        /// Returns the maximum of <paramref name="outerBound"/> and 0.
        /// </summary>
        /// <param name="outerBound">Outer bound to return, inclusive.</param>
        /// <returns>Math.Max(0, outerBound)</returns>
        public decimal NextInclusiveDecimal(decimal outerBound) => NextInclusiveDecimal(0, outerBound);

        /// <summary>
        /// Returns the maximum of <paramref name="outerBound"/> and <paramref name="innerBound"/>.
        /// </summary>
        /// <param name="innerBound">Inner bound to return, inclusive.</param>
        /// <param name="outerBound">Outer bound to return, inclusive.</param>
        /// <returns>Math.Max(<paramref name="innerBound"/>, <paramref name="outerBound"/>)</returns>
        public decimal NextInclusiveDecimal(decimal innerBound, decimal outerBound) => Math.Max(innerBound, outerBound);

        /// <summary>
        /// Returns a number very close to, but still less than, 1.0.
        /// </summary>
        /// <returns/>
        public double NextExclusiveDouble() => NextDouble();

        /// <summary>
        /// Returns a value very close to (but still within the bounds) of the range (0.0, outerBound) (both exclusive)
        /// </summary>
        /// <param name="outerBound">Upper bound for the returned double (exclusive)</param>
        /// <returns/>
        public double NextExclusiveDouble(double outerBound)
            => Math.Max(outerBound < 0 ? -double.Epsilon : double.Epsilon, (1.0 - s_doubleAdjust) * outerBound);

        /// <summary>
        /// Returns the maximum of either a value very close to (but still greater than) the maximum bound, or a value
        /// very close to (but still less than) the specified bound.
        /// </summary>
        /// <param name="innerBound">Lower bound for the returned double (exclusive).  Unused unless outerBound &lt;= innerBound</param>
        /// <param name="outerBound">Upper bound for the returned double (exclusive)</param>
        /// <returns>A value very close to, but still less than, the maximum bound specified.</returns>
        public double NextExclusiveDouble(double innerBound, double outerBound)
            => Math.Max((1.0 - s_doubleAdjust) * innerBound, (1.0 - s_doubleAdjust) * outerBound);

        /// <summary>
        /// Returns a number very close to, but still less than, 1.0f.
        /// </summary>
        /// <returns/>
        public float NextExclusiveFloat() => NextFloat();

        /// <summary>
        /// Returns a value very close to (but still within the bounds) of the range (0.0f, outerBound) (both exclusive)
        /// </summary>
        /// <param name="outerBound">Upper bound for the returned float (exclusive)</param>
        /// <returns/>
        public float NextExclusiveFloat(float outerBound) => NextExclusiveFloat(0, outerBound);

        /// <summary>
        /// Returns the maximum of either a value very close to (but still greater than) the maximum bound, or a value
        /// very close to (but still less than) the specified bound.
        /// </summary>
        /// <param name="innerBound">Lower bound for the returned double (exclusive).  Unused unless outerBound &lt;= innerBound</param>
        /// <param name="outerBound">Upper bound for the returned double (exclusive)</param>
        /// <returns>A value very close to, but still less than, the maximum bound specified.</returns>
        public float NextExclusiveFloat(float innerBound, float outerBound)
            => Math.Max((1.0f - s_floatAdjust) * innerBound, (1.0f - s_floatAdjust) * outerBound);

        public decimal NextExclusiveDecimal() => 1.0M - DecimalEpsilon;

        public decimal NextExclusiveDecimal(decimal outerBound) => Math.Max(DecimalEpsilon, outerBound - DecimalEpsilon);

        public decimal NextExclusiveDecimal(decimal innerBound, decimal outerBound) => Math.Max(innerBound + DecimalEpsilon, outerBound - DecimalEpsilon);

        /// <summary>
        /// Returns a number very close to (but still less than) 1.0.
        /// </summary>
        /// <returns>A number very close to (but still less than) 1.0.</returns>
        public decimal NextDecimal() => 1.0M - DecimalEpsilon;

        /// <summary>
        /// Returns a number very close to (but still less than) <paramref name="outerBound"/>.
        /// </summary>
        /// <returns>A number very close to (but still less than) <paramref name="outerBound"/>.</returns>
        public decimal NextDecimal(decimal outerBound) => outerBound - DecimalEpsilon;

        /// <summary>
        /// Returns a number very close to (but still less than) <paramref name="outerBound"/>, or
        /// <paramref name="innerBound"/> if the outerBound is below innerBound..
        /// </summary>
        /// <returns>Math.Max(innerBound, outerBound - 0.0000000000000000000000000001M).</returns>
        public decimal NextDecimal(decimal innerBound, decimal outerBound) => Math.Max(innerBound, outerBound - DecimalEpsilon);

        /// <summary>
        /// Supported, but does nothing since the unbounded generation functions always return the same value.
        /// </summary>
        /// <returns>ulong.MaxValue</returns>
        public ulong Skip(ulong distance) => ulong.MaxValue;

        /// <summary>
        /// Supported, but does nothing since the unbounded generation functions always return the same value.
        /// </summary>
        /// <returns>ulong.MaxValue</returns>
        public ulong PreviousULong() =>  ulong.MaxValue;

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
        /// Throws exception since this generator does not support serialization.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public string Tag => throw new NotSupportedException();

        /// <summary>
        /// Returns <see cref="uint.MaxValue" />.
        /// </summary>
        /// <returns><see cref="uint.MaxValue" />.</returns>
        public uint NextUInt() => uint.MaxValue;

        /// <summary>
        /// Returns <paramref name="maxValue" /> - 1, min 0.
        /// </summary>
        /// <param name="maxValue">The maximum bound for the returned number, exclusive.</param>
        /// <returns>Math.Max(0, <paramref name="maxValue" /> - 1)</returns>
        public uint NextUInt(uint maxValue) => NextUInt(0, maxValue);

        /// <summary>
        /// Returns <paramref name="maxValue" /> - 1, min <see cref="minValue"/>.
        /// </summary>
        /// <param name="minValue">
        /// The minimum value that can be returned.  Unused unless maxValue &lt;= minValue.
        /// </param>
        /// <param name="maxValue">The maximum bound for the returned number, exclusive.</param>
        /// <returns>Math.Max(<paramref name="minValue"/>, <paramref name="maxValue" /> - 1)</returns>
        public uint NextUInt(uint minValue, uint maxValue) => Math.Max(minValue, maxValue - 1);
    }
}
