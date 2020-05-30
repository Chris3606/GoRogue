using GoRogue.DiceNotation.Terms;
using GoRogue.Random;
using JetBrains.Annotations;
using Troschuetz.Random;

namespace GoRogue.DiceNotation
{
    /// <summary>
    /// The default class for representing a parsed dice expression.
    /// </summary>
    [PublicAPI]
    public class DiceExpression : IDiceExpression
    {
        private readonly ITerm _termToEvaluate;

        /// <summary>
        /// Constructor. Takes the last term in the dice expression (the root of the expression tree).
        /// </summary>
        /// <param name="termToEvaluate">
        /// The root of the expression tree -- by evaluating this term, all others will be evaluated recursively.
        /// </param>
        public DiceExpression(ITerm termToEvaluate) => _termToEvaluate = termToEvaluate;

        /// <summary>
        /// Returns the maximum possible result of the dice expression.
        /// </summary>
        /// <returns>The maximum possible result of the dice expression.</returns>
        public int MaxRoll() => Roll(new MaxRandom());

        /// <summary>
        /// Returns the minimum possible result of the dice expression.
        /// </summary>
        /// <returns>The minimum possible result of the dice expression.</returns>
        public int MinRoll() => Roll(new MinRandom());

        /// <summary>
        /// Rolls the expression using the RNG given, returning the result.
        /// </summary>
        /// <param name="rng">The RNG to use. If null is specified, the default RNG is used.</param>
        /// <returns>The result obtained by rolling the dice expression.</returns>
        public int Roll(IGenerator? rng = null)
        {
            rng ??= GlobalRandom.DefaultRNG;

            return _termToEvaluate.GetResult(rng);
        }

        /// <summary>
        /// Returns a parenthesized string representing the dice expression in dice notation
        /// </summary>
        /// <returns>A parenthesized string representing the expression.</returns>
        public override string ToString() => _termToEvaluate.ToString();
    }
}
