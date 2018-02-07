using GoRogue.Random;
using System.Collections.Generic;

namespace GoRogue.DiceNotation.Terms
{
    /// <summary>
    /// Can be implemented to create a new term type for a dice expression.
    /// </summary>
    public interface IDiceExpressionTerm
    {
        /// <summary>
        /// Gets the TermResults for the implementation.
        /// </summary>
        /// <param name="random">
        /// IRandom RNG used to perform the Roll. If null is specified, uses the default rng.
        /// </param>
        /// <returns>
        /// An IEnumerable of TermResult which will have one item per result.
        /// </returns>
        IEnumerable<TermResult> GetResults(IRandom random = null);
    }
}