namespace GoRogue.Random
{
    /// <summary>
    /// Interface for pseudo-random number generators to implement. TODO:, need to include ways for
    /// doubles, etc. Having this as an interface can be useful, as an RNG can be easily swapped out
    /// for testing. There are classes included that implement this interface that are based on
    /// certain forms of pseudo-random number generation, as well as classes that implement it to do
    /// something that is not actually random (KnownSeriesRandom), etc, which can be useful for testing.
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Gets the next pseudo-random integer between 0 and the specified maxValue, inclusive.
        /// </summary>
        /// <param name="maxValue">
        /// Inclusive maximum result
        /// </param>
        /// <returns>
        /// Returns a pseudo-random integer between 0 and the specified maxValue, inclusive
        /// </returns>
        int Next(int maxValue);

        /// <summary>
        /// Gets the next pseudo-random integer between the specified minValue and maxValue, inclusive.
        /// </summary>
        /// <param name="minValue">
        /// Inclusive minimum result.
        /// </param>
        /// <param name="maxValue">
        /// Inclusive maximum result.
        /// </param>
        /// <returns>
        /// Returns a pseudo-random integer between the specified minValue and maxValue inclusive.
        /// </returns>
        int Next(int minValue, int maxValue);

        // TODO: Save state functions...
    }
}