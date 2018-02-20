namespace GoRogue.Random
{
    /// <summary>
    /// A "random" number generator that always returns the maxValue parameter given. Again this may
    /// be useful in testing, testing the upper range or repeatedly returning a value. Also used in
    /// DiceExpressions for certain max roll functions.
    /// </summary>
    public class MaxRandom : IRandom
    {
        /// <summary>
        /// Gets the next integer from the "generator" (it will always be maxValue).
        /// </summary>
        /// <param name="maxValue">Value always returned when this function is called.</param>
        /// <returns>Value given for maxValue.</returns>
        public int Next(int maxValue) => maxValue;

        /// <summary>
        /// Gets the next integer from the "generator" (it will always be maxValue).
        /// </summary>
        /// <param name="minValue">Irrelevant parameter.</param>
        /// <param name="maxValue">Value always returned when this function is called.</param>
        /// <returns>Value given for maxValue.</returns>
        public int Next(int minValue, int maxValue) => maxValue;
    }
}