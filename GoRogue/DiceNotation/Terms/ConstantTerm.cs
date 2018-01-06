using GoRogue.Random;
using System.Collections.Generic;

namespace GoRogue.DiceNotation.Terms
{
    /// <summary>
    /// The ConstantTerm class represents a constant value in a DiceExpression.  For example, in the expression "2d6+5", the integer "5" is a ConstantTerm.
    /// </summary>
    public class ConstantTerm : IDiceExpressionTerm
    {
        private readonly int constant;

        /// <summary>
        /// Constructs a new instance of the ConstantTerm class using the specified integer.
        /// </summary>
        /// <param name="constant">An integer representing the constant term.</param>
        public ConstantTerm(int constant)
        {
            this.constant = constant;
        }

        /// <summary>
        /// Gets the TermResult for this ConstantTerm, which will always be a single result with a scalar of 1, and a value of the constant.
        /// </summary>
        /// <param name="random">Not used for this implementation of IDiceExpressionTerm</param>
        /// <returns>An IEnumerable of TermResult, which will always have a single result with a scalar of 1, and a value of the constant.</returns>
        public IEnumerable<TermResult> GetResults(IRandom random = null) => new[] { new TermResult { Scalar = 1, Value = constant, TermType = "constant" } };

        /// <summary>
        /// Returns a string that represents this ConstantTerm.
        /// </summary>
        /// <returns>A string representing this ConstantTerm.</returns>
        public override string ToString() => constant.ToString();
    }
}