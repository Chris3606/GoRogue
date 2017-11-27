namespace GoRogue.Random
{
    /// <summary>
    /// Static class that contains 1 variable, which is a DefaultRNG instance.  Used as the default rng by other features (dice, etc) wherever needed, and can also be used if you need a random number
    /// generator for your own code.
    /// </summary>
    public static class SingletonRandom
    {
        /// <summary>
        /// Returns a DotNetRandom instance that can be used as a default RNG.
        /// Settable in case a custom default RNG is desired, however defaults to
        /// DotNetRandom with TickCount seed.
        /// </summary>
        public static IRandom DefaultRNG = new DotNetRandom();
    }
}