using System;
using JetBrains.Annotations;
using ShaiRandom.Generators;
using Troschuetz.Random;

namespace GoRogue.Random
{
    /// <summary>
    /// Class containing some extension methods for <see cref="IEnhancedRandom" /> instances.
    /// </summary>
    [PublicAPI]
    public static class GeneratorExtensions
    {
        /// <summary>
        /// Performs a percentage check that has the specified chance to succeed.  The percentage should be in range
        /// [0, 100] (inclusive).
        /// </summary>
        /// <param name="rng" />
        /// <param name="percentage">Percentage chance (out of 100) that this check will succeed.  Must be in range [0, 100].</param>
        /// <returns></returns>
        public static bool PercentageCheck(this IEnhancedRandom rng, float percentage)
        {
            if (percentage > 100 || percentage < 0)
                throw new ArgumentException($"Percentage given to {nameof(PercentageCheck)} must be in range [0, 100].",
                    nameof(percentage));

            return rng.NextBool(percentage * 0.01f);
        }
    }
}
