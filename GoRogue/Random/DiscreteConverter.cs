using System;
using JetBrains.Annotations;
using ShaiRandom.Distributions;
using ShaiRandom.Generators;

namespace GoRogue.Random
{
    /// <summary>
    /// Wraps a continuous distribution and allows it to be used as discrete, by rounding double
    /// values produced by <see cref="NextDouble" /> to the nearest int. Its minimum, maximum, mean, median, variance,
    /// and mode(s) are exactly the same as its underlying <see cref="ShaiRandom.Distributions.IEnhancedContinuousDistribution" />.
    /// </summary>
    /// <remarks>
    /// Takes a value of type T so that its <see cref="ContinuousDistribution" /> can return a value of the
    /// exact wrapped type, which still enables access to any distribution-specified fields, etc.
    /// </remarks>
    /// <typeparam name="T">
    /// The type of continuous distribution being wrapped. Must implement <see cref="Troschuetz.Random.IContinuousDistribution" />.
    /// </typeparam>
    [PublicAPI]
    public class DiscreteConverter<T> : IEnhancedDiscreteDistribution where T : IEnhancedContinuousDistribution
    {
        /// <summary>
        /// Constructor. Takes the continuous distribution to wrap.
        /// </summary>
        /// <param name="continuousDistribution">Continuous distribution instance to wrap around.</param>
        public DiscreteConverter(T continuousDistribution) => ContinuousDistribution = continuousDistribution;

        /// <summary>
        /// The continuous distribution being wrapped.
        /// </summary>
        public T ContinuousDistribution { get; private set; }

        /// <inheritdoc />
        public void SetParameterValue(int index, double value) => ContinuousDistribution.SetParameterValue(index, value);

        /// <summary>
        /// Gets the <see cref="IEnhancedRandom" /> object that is used as underlying random number generator.
        /// </summary>
        public IEnhancedRandom Generator => ContinuousDistribution.Generator;

        /// <summary>
        /// Gets the maximum possible value of distributed random numbers for the underlying distribution.
        /// </summary>
        public double Maximum => ContinuousDistribution.Maximum;

        /// <summary>
        /// Gets the mean of distributed random numbers for the underlying distribution.
        /// </summary>
        public double Mean => ContinuousDistribution.Mean;

        /// <summary>
        /// Gets the median of distributed random numbers for the underlying distribution.
        /// </summary>
        public double Median => ContinuousDistribution.Median;

        /// <summary>
        /// Gets the minimum possible value of distributed random numbers for the underlying distribution.
        /// </summary>
        public double Minimum => ContinuousDistribution.Minimum;


#pragma warning disable CA1819 // Must return array in this case as the dependent library does
        /// <summary>
        /// Gets the mode of distributed random numbers.
        /// </summary>
        public double[] Mode => ContinuousDistribution.Mode;
#pragma warning restore CA1819

        /// <summary>
        /// Gets the variance of distributed random numbers for the underlying distribution.
        /// </summary>
        public double Variance => ContinuousDistribution.Variance;

        public int Steps => ContinuousDistribution.Steps;

        public int ParameterCount => ContinuousDistribution.ParameterCount;

        /// <summary>
        /// Returns the result of the underlying continuous distribution's <see cref="NextDouble" /> function, but
        /// rounded to the nearest integer.
        /// </summary>
        /// <returns>
        /// The result of the underlying continuous distribution's <see cref="NextDouble" /> function, rounded to
        /// the nearest integer.
        /// </returns>
        public int NextInt() => (int)Math.Round(ContinuousDistribution.NextDouble(), MidpointRounding.AwayFromZero);

        /// <summary>
        /// Returns a distributed floating point random number from the underlying continuous generator.
        /// </summary>
        /// <returns>A distributed double-precision floating point number.</returns>
        public double NextDouble() => ContinuousDistribution.NextDouble();

        public string ParameterName(int index) => ContinuousDistribution.ParameterName(index);

        public double ParameterValue(int index) => ContinuousDistribution.ParameterValue(index);
    }
}
