namespace GoRogue.DiceNotation
{
    /// <summary>
    /// Interface for a class that parses a string representing a dice expression into a
    /// IDiceExpression instance.
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