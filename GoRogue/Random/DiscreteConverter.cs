using System;
using Troschuetz.Random;

namespace GoRogue.Random
{
	/// <summary>
	/// Wraps a continuous distribution and allows it to be used as discrete, by rounding double
	/// values produced by NextDouble to nearest int. Its minimum, maximum, mean, median, variance,
	/// and mode(s) are exactly the same as its underlying IContinuousDistribution.
	/// </summary>
	/// <remarks>
	/// Takes a value of type T so that its ContinuousDistribution field can return a value of the
	/// exact wrapped type, which still enables access to any distribution-specified fields, etc.
	/// </remarks>
	/// <typeparam name="T">
	/// The type of continuous distribution being wrapped. Must implement IContinuousDistribution.
	/// </typeparam>
	public class DiscreteConverter<T> : IDiscreteDistribution where T : IContinuousDistribution
	{
		/// <summary>
		/// Constructor. Takes the continuous distribution to wrap.
		/// </summary>
		/// <param name="continuousDistribution">Continuous distribution instance to wrap around.</param>
		public DiscreteConverter(T continuousDistribution)
		{
			ContinuousDistribution = continuousDistribution;
		}

		/// <summary>
		/// Gets a value indicating whether the underlying random number distribution can be reset,
		/// so that it produces the same random number sequence again.
		/// </summary>
		public bool CanReset => ContinuousDistribution.CanReset;

		/// <summary>
		/// The continuous distribution being wrapped.
		/// </summary>
		public T ContinuousDistribution { get; private set; }

		/// <summary>
		/// Gets the IGenerator object that is used as underlying random number generator.
		/// </summary>
		public IGenerator Generator => ContinuousDistribution.Generator;

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

		/// <summary>
		/// Gets the mode of distributed random numbers.
		/// </summary>
		public double[] Mode => ContinuousDistribution.Mode;

		/// <summary>
		/// Gets the variance of distributed random numbers for the underlying distribution.
		/// </summary>
		public double Variance => ContinuousDistribution.Variance;

		/// <summary>
		/// Returns the result of the underlying continuous distribution's NextDouble function, but
		/// rounded to the nearest integer.
		/// </summary>
		/// <returns>
		/// The result of the underlying continuous distribution's NextDouble function, rounded to
		/// the nearest integer.
		/// </returns>
		public int Next() => (int)Math.Round(ContinuousDistribution.NextDouble(), MidpointRounding.AwayFromZero);

		/// <summary>
		/// Returns a distributed floating point random number from the underlying continuous generator.
		/// </summary>
		/// <returns>A distributed double-precision floating point number.</returns>
		public double NextDouble() => ContinuousDistribution.NextDouble();

		/// <summary>
		/// Resets the random number distribution, so that it produces the same random number
		/// sequence again.
		/// </summary>
		/// <returns>true if the random number distribution was reset; otherwise, false.</returns>
		public bool Reset() => ContinuousDistribution.Reset();
	}
}