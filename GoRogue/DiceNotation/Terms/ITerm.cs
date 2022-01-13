using JetBrains.Annotations;
using ShaiRandom.Generators;
using Troschuetz.Random;

namespace GoRogue.DiceNotation.Terms
{
    /// <summary>
    /// Interface for a term of a dice expression that can be evaluated.
    /// </summary>
    [PublicAPI]
    public interface ITerm
    {
        /// <summary>
        /// Evaluates the term and returns the result.
        /// </summary>
        /// <param name="rng">The rng to use.</param>
        /// <returns>The result of evaluating the term.</returns>
        int GetResult(IEnhancedRandom rng);
    }
}
