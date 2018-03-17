using GoRogue.Random;
using Troschuetz.Random;

namespace GoRogue.DiceNotation
{
    /// <summary>
    /// The most important DiceNotation class -- contains functions to roll dice, and to retrieve an
    /// IDiceExpression instance representing a given expression (useful and more efficient if a dice
    /// roll is used multiple times).
    /// </summary>
    public static class Dice
    {
        /// <summary>
        /// The parser that will be used to parse dice expressions given to the Parse and Roll functions.
        /// </summary>
        public static IParser DiceParser = new Parser();

        /// <summary>
        /// Uses the IParser specified in the DiceParser variable to produce a IDiceExpression
        /// instance representing the given dice expression.
        /// </summary>
        /// <remarks>
        /// Because parsing can add significant time, retrieving an IDiceExpression instance instead
        /// of using the Roll function can be useful if a given expression will be rolled multiple times.
        /// </remarks>
        /// <param name="expression">The string dice expression to parse.</param>
        /// <returns>An IDiceExpression instance representing the parsed string.</returns>
        public static IDiceExpression Parse(string expression) => DiceParser.Parse(expression);

        /// <summary>
        /// Uses the IParser specified in the DiceParser variable to parse the given dice expression,
        /// roll it, and return the result.
        /// </summary>
        /// <remarks>
        /// While more convenient, parsing is computationally more expensive than evaluation. If a
        /// dice expression will be rolled many times, it is more efficient to use the Parse method
        /// once, and use the resulting IDiceExpression instance to roll the expression each time it
        /// is needed.
        /// </remarks>
        /// <param name="expression">The string dice expression to parse.</param>
        /// <param name="random">
        /// RNG to use to perform the roll. If null is specified, the default RNG is used.
        /// </param>
        /// <returns>The result of evaluating the dice expression given.</returns>
        public static int Roll(string expression, IGenerator random = null)
        {
            if (random == null) random = SingletonRandom.DefaultRNG;
            return DiceParser.Parse(expression).Roll(random);
        }
    }
}