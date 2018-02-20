namespace GoRogue.Random
{
    /// <summary>
    /// A "random" number generator that always returns the minValue parameter given, or 0 on the
    /// Next overload that only takes maxValue. Again, may be useful for testing. Also used in
    /// DiceExpression for certain minimum roll functions.
    /// </summary>
    public class MinRandom : IRandom
    {
        /// <summary>
        /// Gets the next integer from the "generator" (it will always be 0).
        /// </summary>
        /// <param name="maxValue">Irrelevant parameter.</param>
        /// <returns>The integer 0.</returns>
        public int Next(int maxValue) => 0;

        /// <summary>
        /// Gets the next integer from the "generator" (it will always be minValue).
        /// </summary>
        /// <param name="minValue">Value always returned when this function is called.</param>
        /// <param name="maxValue">Irrelevant parameter.</param>
        /// <returns>Value given for minValue.</returns>
        public int Next(int minValue, int maxValue) => minValue;
    }
}