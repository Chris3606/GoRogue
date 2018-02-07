using GoRogue.Random;

namespace GoRogue.DiceNotation
{
    /// <summary>
    /// This is likely the class you really care about the most. Basically just a static class that
    /// has methods for tieing all of the DiceNotation classes together. It has functions for parsing
    /// an expression, rolling based upon a given expression, and others.
    /// </summary>
    public static class Dice
    {
        private static readonly IDiceParser diceParser = new DiceParser();

        /// <summary>
        /// Parse the specified string into a DiceExpression. If you intend to roll a given
        /// expression multiple times, chances are you want to use this once to get yourself a
        /// DiceExpression instance representing that particular roll, and store that somewhere.
        /// Then, use the Roll function in DiceExpression to roll it. This is faster as it means the
        /// slow part (using regex to take apart the expression) happens only once.
        /// </summary>
        /// <param name="expression">
        /// The string dice expression to parse, in dice notation format. Ex. 3d6+4.
        /// </param>
        /// <returns>
        /// A DiceExpression representing the parsed string.
        /// </returns>
        public static DiceExpression Parse(string expression) => diceParser.Parse(expression);

        /// <summary>
        /// A convenience method for parsing a dice expression from a string, rolling the dice with a
        /// given IRandom instance (or the default RNG if null is specified), and returning the
        /// total. If you need to do a roll only once, this method or its overload would be the one
        /// to use. If you will be repeating the roll, particularly if you are repeating it multiple
        /// times and care about speed, it is probably best to instead use the Parse function,
        /// retrieve a DiceExpression, and then use its roll function. This prevents the regex
        /// pattern from being matched against more than once (which is relatively expensive compared
        /// to the other operations).
        /// </summary>
        /// <param name="expression">
        /// The string dice expression to parse. Ex. 3d6+4.
        /// </param>
        /// <param name="random">
        /// IRandom RNG to use to perform the Roll.
        /// </param>
        /// <returns>
        /// An integer representing the sum of the dice rolled, including constants and scalars in
        /// the expression.
        /// </returns>
        public static int Roll(string expression, IRandom random = null)
        {
            if (random == null) random = SingletonRandom.DefaultRNG;
            return Parse(expression).Roll(random).Value;
        }
    }
}