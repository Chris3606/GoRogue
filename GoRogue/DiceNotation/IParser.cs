using JetBrains.Annotations;

namespace GoRogue.DiceNotation
{
    /// <summary>
    /// Interface for a class that parses a string representing a dice expression into an
    /// <see cref="DiceExpression" /> instance.  You might implement this if you need to implement a custom
    /// dice parser.
    /// </summary>
    [PublicAPI]
    public interface IParser
    {
        /// <summary>
        /// Parses the dice expression specified into a <see cref="DiceExpression" /> instance.
        /// </summary>
        /// <param name="expression">The expression to parse.</param>
        /// <returns>
        /// An <see cref="DiceExpression" /> representing the given expression, that can "roll" the expression on command.
        /// </returns>
        DiceExpression Parse(string expression);
    }
}
