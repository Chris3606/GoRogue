using System;
using JetBrains.Annotations;
using Troschuetz.Random;

namespace GoRogue.Random
{
    /// <summary>
    /// Class containing some extension methods for <see cref="IGenerator" /> instances.
    /// </summary>
    [PublicAPI]
    public static class GeneratorExtensions
    {
        /// <summary>
        /// Performs a percentage check that has the specified chance to succeed.
        /// </summary>
        /// <param name="rng">RNG to use.  Never specified manually as this is an extension method.</param>
        /// <param name="percentage">Percentage chance (out of 100) that this check will succeed.</param>
        /// <returns></returns>
        public static bool PercentageCheck(this IGenerator rng, ushort percentage)
        {
            if (percentage > 100)
                throw new ArgumentException($"Percentage given to {nameof(PercentageCheck)} must be in range [0, 100].",
                    nameof(percentage));

            return rng.Next(1, 101) <= percentage;
        }
    }
}
