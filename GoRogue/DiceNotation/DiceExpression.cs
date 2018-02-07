using GoRogue.DiceNotation.Terms;
using GoRogue.Random;
using System.Collections.Generic;
using System.Linq;

namespace GoRogue.DiceNotation
{
    /// <summary>
    /// A class representing a dice expression that can be rolled. Typically you will not construct
    /// these yourself, rather you might use this if you had a roll you needed to do often; you can
    /// use the Dice class's Parse function to retrieve one of these from your string expression, and
    /// then use this class's Roll function whenever you need to roll it. This ensures that the
    /// parsing portion, the part that uses (relatively expensive) regex pattern-matching, is done
    /// only once.
    /// </summary>
    public class DiceExpression
    {
        private readonly IList<IDiceExpressionTerm> terms;

        /// <summary>
        /// Construct a new DiceExpression class with an empty list of terms.
        /// </summary>
        public DiceExpression()
           : this(new IDiceExpressionTerm[] { })
        { }

        private DiceExpression(IEnumerable<IDiceExpressionTerm> diceTerms)
        {
            terms = diceTerms.ToList();
        }

        /// <summary>
        /// Add a constant to this DiceExpression with the specified integer value.
        /// </summary>
        /// <param name="value">
        /// An integer constant to add to this DiceExpression.
        /// </param>
        /// <returns>
        /// A DiceExpression representing the previous terms in this DiceExpression, plus this newly
        /// added Constant.
        /// </returns>
        public DiceExpression Constant(int value)
        {
            terms.Add(new ConstantTerm(value));
            return this;
        }

        /// <summary>
        /// Add multiple Dice to this DiceExpression with the specified parameters.
        /// </summary>
        /// <param name="multiplicity">
        /// The number of Dice.
        /// </param>
        /// <param name="sides">
        /// The number of sides per Die.
        /// </param>
        /// <returns>
        /// A DiceExpression representing the previous terms in this DiceExpression, plus these newly
        /// added Dice.
        /// </returns>
        public DiceExpression Dice(int multiplicity, int sides) => Dice(multiplicity, sides, 1, null);

        /// <summary>
        /// Add multiple Dice to this DiceExpression with the specified parameters.
        /// </summary>
        /// <param name="multiplicity">
        /// The number of Dice.
        /// </param>
        /// <param name="sides">
        /// The number of sides per Die.
        /// </param>
        /// <param name="scalar">
        /// The value to multiply the result of the Roll of these Dice by.
        /// </param>
        /// <returns>
        /// A DiceExpression representing the previous terms in this DiceExpression, plus these newly
        /// added Dice.
        /// </returns>
        public DiceExpression Dice(int multiplicity, int sides, int scalar) => Dice(multiplicity, sides, scalar, null);

        /// <summary>
        /// Add multiple Dice to this DiceExpression with the specified parameters.
        /// </summary>
        /// <param name="multiplicity">
        /// The number of Dice.
        /// </param>
        /// <param name="sides">
        /// The number of sides per Die.
        /// </param>
        /// <param name="scalar">
        /// The value to multiply the result of the Roll of these Dice by.
        /// </param>
        /// <param name="choose">
        /// Optional number of dice to choose out of the total rolled. The highest rolled Dice will
        /// be choosen.
        /// </param>
        /// <returns>
        /// A DiceExpression representing the previous terms in this DiceExpression, plus these newly
        /// added Dice.
        /// </returns>
        public DiceExpression Dice(int multiplicity, int sides, int scalar, int? choose)
        {
            terms.Add(new DiceTerm(multiplicity, sides, choose ?? multiplicity, scalar));
            return this;
        }

        /// <summary>
        /// Add a single Die to this DiceExpression, with the specified number of sides and given scalar.
        /// </summary>
        /// <param name="sides">
        /// The number of sides on the Die to add to this DiceExpression.
        /// </param>
        /// <param name="scalar">
        /// The value to multiply the result of the Roll of this Die by.
        /// </param>
        /// <returns>
        /// A DiceExpression representing the previous terms in this DiceExpression, plus this newly
        /// added Die.
        /// </returns>
        public DiceExpression Die(int sides, int scalar) => Dice(1, sides, scalar);

        /// <summary>
        /// Add a single Die to this DiceExpression with the specified number of sides.
        /// </summary>
        /// <param name="sides">
        /// The number of sides on the Die to add to this DiceExpression.
        /// </param>
        /// <returns>
        /// A DiceExpression representing the previous terms in this DiceExpression, plus this newly
        /// added Die.
        /// </returns>
        public DiceExpression Die(int sides) => Dice(1, sides);

        /// <summary>
        /// Roll all of the Dice that are part of this DiceExpression, but force all of the rolls to
        /// be the highest possible result.
        /// </summary>
        /// <returns>
        /// A DiceResult representing the results of this Roll. All dice should have rolled their
        /// maximum values.
        /// </returns>
        public DiceResult MaxRoll() => Roll(new MaxRandom());

        /// <summary>
        /// Roll all of the Dice that are part of this DiceExpression, but force all of the rolls to
        /// be the lowest possible result.
        /// </summary>
        /// <returns>
        /// A DiceResult representing the results of this Roll. All dice should have rolled their
        /// minimum values.
        /// </returns>
        public DiceResult MinRoll() => Roll(new MinRandom());

        /// <summary>
        /// Roll all of the Dice that are part of this DiceExpression using the given RNG, or the
        /// default RNG if null is specified.
        /// </summary>
        /// <param name="random">
        /// IRandom RNG used to perform the Roll.
        /// </param>
        /// <returns>
        /// A DiceResult representing the results of this Roll.
        /// </returns>
        public DiceResult Roll(IRandom random = null)
        {
            if (random == null) random = SingletonRandom.DefaultRNG;
            IEnumerable<TermResult> termResults = terms.SelectMany(t => t.GetResults(random)).ToList();
            return new DiceResult(termResults, random);
        }

        /// <summary>
        /// Returns a string that represents this DiceExpression.
        /// </summary>
        /// <returns>
        /// A string representing this DiceExpression.
        /// </returns>
        public override string ToString() => string.Join(" + ", terms);
    }
}