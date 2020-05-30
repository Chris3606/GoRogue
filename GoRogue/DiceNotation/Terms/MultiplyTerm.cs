using JetBrains.Annotations;
using Troschuetz.Random;

namespace GoRogue.DiceNotation.Terms
{
    /// <summary>
    /// Term representing the multiplication operator -- multiplies <see cref="Term1"/> and <see cref="Term2"/>.
    /// </summary>
    [PublicAPI]
    public class MultiplyTerm : ITerm
    {
        /// <summary>
        /// Constructor. Takes the terms that will be multiplied.
        /// </summary>
        /// <param name="term1">The first term (left-hand side).</param>
        /// <param name="term2">The second term (left-hand side).</param>
        public MultiplyTerm(ITerm term1, ITerm term2)
        {
            Term1 = term1;
            Term2 = term2;
        }

        /// <summary>
        /// The first term (left-hand side).
        /// </summary>
        public ITerm Term1 { get; private set; }

        /// <summary>
        /// The second term (right-hand side).
        /// </summary>
        public ITerm Term2 { get; private set; }

        /// <summary>
        /// Multiplies the first term by the second, evaluating those two terms as necessary.
        /// </summary>
        /// <param name="rng">The rng to used -- passed to other terms.</param>
        /// <returns>The result of evaluating <see cref="Term1"/> * <see cref="Term2"/>.</returns>
        public int GetResult(IGenerator rng) => Term1.GetResult(rng) * Term2.GetResult(rng);

        /// <summary>
        /// Returns a parenthesized string representing the term.
        /// </summary>
        /// <returns>A parenthesized string representing the term.</returns>
        public override string ToString() => $"({Term1}*{Term2})";
    }
}
