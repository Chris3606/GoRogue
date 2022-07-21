using JetBrains.Annotations;
using ShaiRandom.Generators;

namespace GoRogue.Random
{
    /// <summary>
    /// Static class that contains 1 variable, which is a an RNG instance that is used as the default
    /// rng by other features (dice, etc) wherever needed, and can also be used if you need a random
    /// number generator for your own code.
    /// </summary>
    [PublicAPI]
    public static class GlobalRandom
    {
        /// <summary>
        /// Settable field that specifies what <see cref="IEnhancedRandom" /> instance should be considered the default
        /// RNG. Defaults to an <see cref="ShaiRandom.Generators.MizuchiRandom" />, with a random state.
        /// </summary>
        public static IEnhancedRandom DefaultRNG = new MizuchiRandom();
    }
}
