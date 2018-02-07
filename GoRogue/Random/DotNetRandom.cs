using System;

namespace GoRogue.Random
{
    /// <summary>
    /// Implementation of IRandom that is basically wrapper around the built in .NET random number
    /// generator. Not the best RNG out there in terms of statistical non-bias, etc., however it
    /// makes a good starting point.
    /// </summary>
    public class DotNetRandom : IRandom
    {
        //private long _numberGenerated;
        private System.Random random = new System.Random();

        private int seed;

        /// <summary>
        /// Constructs a new pseudo-random number generator, with a seed based on the number of
        /// milliseconds elapsed since the system started.
        /// </summary>
        public DotNetRandom()
           : this(Environment.TickCount)
        {
        }

        /// <summary>
        /// Constructs a new pseudo-random number generator, with the specified seed.
        /// </summary>
        /// <param name="seed">
        /// An integer used to calculate a starting value for the pseudo-random number sequence.
        /// </param>
        public DotNetRandom(int seed)
        {
            this.seed = seed;
            this.random = new System.Random(seed);
        }

        /// <summary>
        /// Gets the next pseudo-random integer between 0 and the specified maxValue, inclusive.
        /// </summary>
        /// <param name="maxValue">
        /// Inclusive maximum result.
        /// </param>
        /// <returns>
        /// Returns a pseudo-random integer between 0 and the specified maxValue, inclusive.
        /// </returns>
        public int Next(int maxValue) => random.Next(0, maxValue + 1);

        /// <summary>
        /// Gets the next pseudo-random integer between the specified minValue and maxValue inclusive.
        /// </summary>
        /// <param name="minValue">
        /// Inclusive minimum result.
        /// </param>
        /// <param name="maxValue">
        /// Inclusive maximum result.
        /// </param>
        /// <returns>
        /// Returns a pseudo-random integer between the specified minValue and maxValue, inclusive.
        /// </returns>
        public int Next(int minValue, int maxValue) => random.Next(minValue, maxValue + 1);
    }
}