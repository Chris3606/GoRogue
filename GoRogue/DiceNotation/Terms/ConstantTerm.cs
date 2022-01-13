using System.Runtime.Serialization;
using JetBrains.Annotations;
using ShaiRandom.Generators;
using Troschuetz.Random;

namespace GoRogue.DiceNotation.Terms
{
    /// <summary>
    /// Base term -- represents a numerical constant.
    /// </summary>
    [PublicAPI]
    [DataContract]
    public class ConstantTerm : ITerm
    {
        /// <summary>
        /// The numerical constant represented by this term.
        /// </summary>
        [DataMember] public readonly int Value;

        /// <summary>
        /// Constructor. Takes the numerical constant it represents.
        /// </summary>
        /// <param name="value">The numerical value this term represents.</param>
        public ConstantTerm(int value) => Value = value;

        /// <summary>
        /// Returns the numerical constant it represents. RNG is unused.
        /// </summary>
        /// <param name="rng">(Unused) rng.</param>
        /// <returns>The numerical constant this term represents.</returns>
        public int GetResult(IEnhancedRandom rng) => Value;

        /// <summary>
        /// Returns a string representation of this constant.
        /// </summary>
        /// <returns>The numerical constant being represented, as a string.</returns>
        public override string ToString() => Value.ToString();
    }
}
