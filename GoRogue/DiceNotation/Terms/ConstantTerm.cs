using Troschuetz.Random;

namespace GoRogue.DiceNotation.Terms
{
    /// <summary>
    /// Base term -- represents a numerical constant.
    /// </summary>
    public class ConstantTerm : ITerm
    {
        private int value;

        /// <summary>
        /// Constructor. Takes the numerical constant it represents.
        /// </summary>
        /// <param name="value">The numerical value this term represents.</param>
        public ConstantTerm(int value)
        {
            this.value = value;
        }

        /// <summary>
        /// Returns the numerical constant it represents. RNG is unused.
        /// </summary>
        /// <param name="rng">(Unused) rng.</param>
        /// <returns>The numerical constant this term represents.</returns>
        public int GetResult(IGenerator rng) => value;

        /// <summary>
        /// Returns a string representation of this constant.
        /// </summary>
        /// <returns>The numerical constant being represented, as a string.</returns>
        public override string ToString()
        {
            return value.ToString();
        }
    }
}