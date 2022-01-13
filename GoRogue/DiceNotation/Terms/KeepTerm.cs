using System.Linq;
using System.Runtime.Serialization;
using GoRogue.DiceNotation.Exceptions;
using JetBrains.Annotations;
using ShaiRandom.Generators;
using Troschuetz.Random;

namespace GoRogue.DiceNotation.Terms
{
    /// <summary>
    /// Term representing the keep operator -- keeping only the n highest dice from a dice term.
    /// </summary>
    [DataContract]
    [PublicAPI]
    public class KeepTerm : ITerm
    {
        /// <summary>
        /// The dice term to operate on.
        /// </summary>
        [DataMember] public readonly DiceTerm DiceTerm;

        /// <summary>
        /// The number of dice to keep.
        /// </summary>
        [DataMember] public readonly ITerm Keep;

        /// <summary>
        /// Constructor. Takes a term representing the number of dice to keep, and the dice term to
        /// operate on.
        /// </summary>
        /// <param name="keep">Term representing the number of dice to keep.</param>
        /// <param name="diceTerm">The dice term to operate on.</param>
        public KeepTerm(ITerm keep, DiceTerm diceTerm)
        {
            DiceTerm = diceTerm;
            Keep = keep;
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
        public int GetResult(IEnhancedRandom rng)
        {
            var keepVal = Keep.GetResult(rng);

            if (keepVal < 0)
                throw new InvalidChooseException();

            DiceTerm.GetResult(rng); // Roll so we can check chooses

            if (keepVal > DiceTerm.LastMultiplicity)
                throw new InvalidChooseException();

            return DiceTerm.DiceResults.OrderByDescending(value => value).Take(keepVal).Sum();
        }

        /// <summary>
        /// Returns a parenthesized string representing the term -- eg (4d6k3) or (2d6k2)
        /// </summary>
        /// <returns>A parenthesized string representing the term</returns>
        public override string ToString() => $"({DiceTerm}k{Keep})";
    }
}
