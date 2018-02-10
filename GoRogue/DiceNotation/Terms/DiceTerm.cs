using GoRogue.Random;
using System.Collections.Generic;

namespace GoRogue.DiceNotation.Terms
{
    /// <summary>
    /// Represents a dice term, eg 1d4 or 2d6.
    /// </summary>
    public class DiceTerm : ITerm
    {
        /// <summary>
        /// Term representing the number of dice being rolled -- 2d6 has multiplicity 2.
        /// </summary>
        public ITerm Multiplicity { get; private set; }
        /// <summary>
        /// Term representing the number of sides the dice have -- 2d6 has 6 sides.
        /// </summary>
        public ITerm Sides { get; private set; }

        /// <summary>
        /// The result of evaluating the Multiplicity term that was used during the last call to GetResult.
        /// </summary>
        public int LastMultiplicity { get; private set; }
        /// <summary>
        /// The result of evaluating the Sides term that was used during the last call to GetResult.
        /// </summary>
        public int LastSidedness { get; private set; }

        private List<int> _diceResults;
        /// <summary>
        /// An enumerable of integers representing the result of each dice roll.  The expression 2d6 rolls 2 dice, and as such this enumerable
        /// would be of length 2 and contain the result of each individual die.
        /// </summary>
        public IEnumerable<int> DiceResults { get => _diceResults; }

        /// <summary>
        /// Constructor. Takes the terms representing multiplicity and number of sides.
        /// </summary>
        /// <param name="multiplicity">Term representing the number of dice being rolled -- 2d6 has multiplicity 2.</param>
        /// <param name="sides">Term representing the number of sides the dice have -- 2d6 has 6 sides.</param>
        public DiceTerm(ITerm multiplicity, ITerm sides)
        {
            Multiplicity = multiplicity;
            Sides = sides;

            _diceResults = new List<int>();
        }

        /// <summary>
        /// Rolls the dice, returning the sum.
        /// </summary>
        /// <param name="rng">The RNG to use for rolling,</param>
        /// <returns>The sum of the roll.</returns>
        public int GetResult(IRandom rng)
        {
            _diceResults.Clear();
            int sum = 0;
            LastMultiplicity = Multiplicity.GetResult(rng);
            LastSidedness = Sides.GetResult(rng);

            if (LastMultiplicity < 0)
                throw new Exceptions.InvalidMultiplicityException();

            if (LastSidedness <= 0)
                throw new Exceptions.ImpossibleDieException();


            for (int i = 0; i < LastMultiplicity; i++)
            {
                int diceVal = rng.Next(1, LastSidedness);
                sum += diceVal;
                _diceResults.Add(diceVal);
            }

            return sum;
        }

        /// <summary>
        /// Gets a parenthesized string representation of the dice term, eg (2d6).
        /// </summary>
        /// <returns>A parenthesized representation of the term.</returns>
        public override string ToString()
        {
            return "(" + Multiplicity + "d" + Sides + ")";
        }
    }
}
