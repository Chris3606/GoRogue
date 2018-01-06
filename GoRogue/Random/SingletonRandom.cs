namespace GoRogue.Random
{
    /// <summary>
    /// Static class that contains 1 variable, which is a DefaultRNG instance.  Used as the default rng by other features (dice, etc) wherever needed, and can also be used if you need a random number
    /// generator for your own code.
    /// </summary>
    public static class SingletonRandom
    {
        /// <summary>
        /// Settable field that specifies what IRandom instance should be considered the default RNG.  Defaults to
        /// DotNetRandom with TickCount used as seed.
        /// </summary>
        public static IRandom DefaultRNG = new DotNetRandom();
    }
}