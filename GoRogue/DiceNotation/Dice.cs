using GoRogue.Random;
using Troschuetz.Random;

namespace GoRogue.DiceNotation
{
	/// <summary>
	/// The most important DiceNotation class -- contains functions to roll dice, and to retrieve an
	/// IDiceExpression instance representing a given expression.
	/// </summary>
	public static class Dice
	{
		/// <summary>
		/// The parser that will be used to parse dice expressions given to the Parse and Roll functions.
		/// If you want to use a custom parser, you can assign an instance to this field.
		/// </summary>
		public static IParser DiceParser = new Parser();

		/// <summary>
		/// Uses the IParser specified in the DiceParser variable to produce a IDiceExpression
		/// instance representing the given dice expression.
		/// </summary>
		/// <remarks>
		/// Generally speaking, dice-parsing via the standard Roll method is extremely fast.  However, if
		/// you are repeating a dice roll many times, in a case where maximum performance 
		/// is absolutely necessary, there is some benefit to retrieving an IDiceExpression instance instead
		/// of using the Roll function, and calling that expressions's Roll() method whenever a result is required.
		/// </remarks>
		/// <param name="expression">The string dice expression to parse.</param>
		/// <returns>An IDiceExpression instance representing the parsed string.</returns>
		public static IDiceExpression Parse(string expression) => DiceParser.Parse(expression);

		/// <summary>
		/// Uses the IParser specified in the DiceParser variable to parse the given dice expression,
		/// roll it, and return the result.  This is the standard method for rolling dice.
		/// </summary>
		/// <remarks>
		/// This method is convenient and typically very fast, however technically, parsing is computationally
		/// more expensive than evaluation. If a dice expression will be rolled many times in a situation where
		/// maximum performance is required, it is more efficient to use the Parse method
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