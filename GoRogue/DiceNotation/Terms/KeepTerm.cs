using System.Linq;
using Troschuetz.Random;

namespace GoRogue.DiceNotation.Terms
{
    /// <summary>
    /// Term represnting the keep operator -- keeping only the n highest dice from a dice term.
    /// </summary>
    public class KeepTerm : ITerm
    {
        private readonly DiceTerm _diceTerm;
        private readonly ITerm _keep;

        /// <summary>
        /// Constructor. Takes a term representing the number of dice to keep, and the dice term to
        /// operate on.
        /// </summary>
        /// <param name="keep">Term representing the number of dice to keep.</param>
        /// <param name="diceTerm">The dice term to operate on.</param>
        public KeepTerm(ITerm keep, DiceTerm diceTerm)
        {
            _diceTerm = diceTerm;
            _keep = keep;
        }

        /// <summary>
        /// Evaluates the term (as well as the dice expression), returning the sum of the highest n
        /// rolls in the dice term.
        /// </summary>
        /// <param name="rng">The rng to use -- passed to the dice term being operated on.</param>
        /// <returns>
        /// The sum of the highest n rolls of the dice term being operated on, where n is equal to
        /// the value of the keep variable taken in the constructor.
        /// </returns>
        public int GetResult(IGenerator rng)
        {
            int keepVal = _keep.GetResult(rng);

            if (keepVal < 0)
                throw new Exceptions.InvalidChooseException();

            _diceTerm.GetResult(rng); // Roll so we can check chooses

            if (keepVal > _diceTerm.LastMultiplicity)
                throw new Exceptions.InvalidChooseException();

            return _diceTerm.DiceResults.OrderByDescending(value => value).Take(keepVal).Sum();
        }

        /// <summary>
        /// Returns a parenthesized string representing the term -- eg (4d6k3) or (2d6k2)
        /// </summary>
        /// <returns>A parenthesized string representing the term</returns>
        public override string ToString() => $"({_diceTerm}k{_keep})";
    }
}
