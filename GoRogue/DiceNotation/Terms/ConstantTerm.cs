using JetBrains.Annotations;
using Troschuetz.Random;

namespace GoRogue.DiceNotation.Terms
{
    /// <summary>
    /// Base term -- represents a numerical constant.
    /// </summary>
    [PublicAPI]
    public class ConstantTerm : ITerm
    {
        private readonly int _value;

        /// <summary>
        /// Constructor. Takes the numerical constant it represents.
        /// </summary>
        /// <param name="value">The numerical value this term represents.</param>
        public ConstantTerm(int value) => _value = value;

        /// <summary>
        /// Returns the numerical constant it represents. RNG is unused.
        /// </summary>
        /// <param name="rng">(Unused) rng.</param>
        /// <returns>The numerical constant this term represents.</returns>
        public int GetResult(IGenerator rng) => _value;

        /// <summary>
        /// Returns a string representation of this constant.
        /// </summary>
        /// <returns>The numerical constant being represented, as a string.</returns>
        public override string ToString() => _value.ToString();
    }
}
