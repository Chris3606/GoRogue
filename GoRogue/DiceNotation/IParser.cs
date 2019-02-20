namespace GoRogue.DiceNotation
{
	/// <summary>
	/// Interface for a class that parses a string representing a dice expression into a
	/// IDiceExpression instance.  You might implement this if you need to implement a custom
	/// dice parser.
	/// </summary>
	public interface IParser
	{
		/// <summary>
		/// Parses the dice expression spcified into an IDiceExpression instance.
		/// </summary>
		/// <param name="expression">The expression to parse.</param>
		/// <returns>
		/// An IDiceExpression representing the given expression, that can "roll" the expression on command.
		/// </returns>
		IDiceExpression Parse(string expression);
	}
}