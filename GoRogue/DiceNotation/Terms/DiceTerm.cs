using GoRogue.DiceNotation.Exceptions;
using GoRogue.Random;
using System.Collections.Generic;
using System.Linq;

namespace GoRogue.DiceNotation.Terms
{
    /// <summary>
    /// The DiceTerm class represents a single "d" term in a DiceExpression. For example, in the
    /// expression "2d6+5", the term "2d6" is a DiceTerm
    /// </summary>
    public class DiceTerm : IDiceExpressionTerm
    {
        /// <summary>
        /// Construct a new instance of the DiceTerm class using the specified values.
        /// </summary>
        /// <param name="multiplicity">
        /// The number of dice.
        /// </param>
        /// <param name="sides">
        /// The number of sides per die.
        /// </param>
        /// <param name="scalar">
        /// The amount to multiply the final sum of the dice by.
        /// </param>
        public DiceTerm(int multiplicity, int sides, int scalar)
           : this(multiplicity, sides, multiplicity, scalar)
        { }

        /// <summary>
        /// Construct a new instance of the DiceTerm class using the specified values.
        /// </summary>
        /// <param name="multiplicity">
        /// The number of dice.
        /// </param>
        /// <param name="sides">
        /// The number of sides per die.
        /// </param>
        /// <param name="choose">
        /// The top valued dice up to this number are kept. If this is equal to multiplicity, keeps
        /// all dice.
        /// </param>
        /// <param name="scalar">
        /// The amount to multiply the final sum of the dice by.
        /// </param>
        public DiceTerm(int multiplicity, int sides, int choose, int scalar)
        {
            if (sides <= 0)
            {
                throw new ImpossibleDieException(string.Format("Cannot construct a die with {0} sides", sides));
            }
            if (multiplicity < 0)
            {
                throw new InvalidMultiplicityException(string.Format("Cannot roll {0} dice; this quantity is less than 0", multiplicity));
            }
            if (choose < 0)
            {
                throw new InvalidChooseException("Cannot choose {0} of the dice; it is less than 0");
            }
            if (choose > multiplicity)
            {
                throw new InvalidChooseException(string.Format("Cannot choose {0} dice, only {1} were rolled", choose, multiplicity));
            }

            Sides = sides;
            Multiplicity = multiplicity;
            Scalar = scalar;
            Choose = choose;
        }

        /// <summary>
        /// The number of dice.
        /// </summary>
        public int Multiplicity { get; private set; }

        /// <summary>
        /// The amount to multiply the final sum of the dice by.
        /// </summary>
        public int Scalar { get; private set; }

        /// <summary>
        /// The number of sides per die.
        /// </summary>
        public int Sides { get; private set; }

        /// <summary>
        /// Used in "choose" operations. The highest dice, up to this number of dice, will be summed
        /// for the final result. If Choose is 5, the 5 highest dice are taken.
        /// </summary>
        protected int Choose { get; private set; }

        /// <summary>
        /// Gets the TermResult for this DiceTerm which will include the random value rolled. If null
        /// is specified, the default RNG is used.
        /// </summary>
        /// <param name="random">
        /// IRandom RNG used to perform the Roll.
        /// </param>
        /// <returns>
        /// An IEnumerable of TermResult which will have one item per die rolled
        /// </returns>
        public IEnumerable<TermResult> GetResults(IRandom random = null)
        {
            if (random == null) random = SingletonRandom.DefaultRNG;
            IEnumerable<TermResult> results =
                from i in Enumerable.Range(0, Multiplicity)
                select new TermResult
                {
                    Scalar = Scalar,
                    Value = random.Next(1, Sides),
                    TermType = "d" + Sides
                };
            return results.OrderByDescending(d => d.Value).Take(Choose);
        }

        /// <summary>
        /// Returns a string that represents this DiceTerm
        /// </summary>
        /// <returns>
        /// A string representing this DiceTerm
        /// </returns>
        public override string ToString()
        {
            string choose = Choose == Multiplicity ? "" : "k" + Choose;
            return Scalar == 1
                ? string.Format("{0}d{1}{2}", Multiplicity, Sides, choose)
                : string.Format("{0}*{1}d{2}{3}", Scalar, Multiplicity, Sides, choose);
        }
    }
}