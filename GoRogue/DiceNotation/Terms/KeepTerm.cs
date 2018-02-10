using System.Linq;
using GoRogue.Random;

namespace GoRogue.DiceNotation.Terms
{
    /// <summary>
    /// Term represnting the keep operator -- keeping only the n highest dice from a dice term.
    /// </summary>
    public class KeepTerm : ITerm
    {
        private DiceTerm diceTerm;
        private ITerm keep;

        /// <summary>
        /// Constructor.  Takes a term representing the number of dice to keep, and the dice term to operate on.
        /// </summary>
        /// <param name="keep">Term representing the number of dice to keep.</param>
        /// <param name="diceTerm">The dice term to operate on.</param>
        public KeepTerm(ITerm keep, DiceTerm diceTerm)
        {
            this.diceTerm = diceTerm;
            this.keep = keep;
        }

        /// <summary>
        /// Evaluates the term (as well as the dice expression), returning the sum of the highest n rolls in the dice term.
        /// </summary>
        /// <param name="rng">The rng to use -- passed to the dice term being operated on.</param>
        /// <returns>The sum of the highest n rolls of the dice term being operated on, where n is equal to the value of the keep variable taken in the constructor.</returns>
        public int GetResult(IRandom rng)
        {
            int keepVal = keep.GetResult(rng);

            if (keepVal < 0)
                throw new Exceptions.InvalidChooseException();

            diceTerm.GetResult(rng); // Roll so we can check chooses

            if (keepVal > diceTerm.LastMultiplicity)
                throw new Exceptions.InvalidChooseException();

            return diceTerm.DiceResults.OrderByDescending(value => value).Take(keepVal).Sum();
        }

        /// <summary>
        /// Returns a parenthesized string representing the term -- eg (4d6k3) or (2d6k2)
        /// </summary>
        /// <returns>A parenthesized string representing the term</returns>
        public override string ToString()
        {
            return "(" + diceTerm + "k" + keep + ")";
        }
    }
}
