using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using ShaiRandom.Generators;
using Troschuetz.Random;

namespace GoRogue.DiceNotation.Terms
{
    /// <summary>
    /// Term representing the division operator -- divides the first term by the second.
    /// </summary>
    [PublicAPI]
    [DataContract]
    public class DivideTerm : ITerm
    {
        /// <summary>
        /// Constructor. Takes the two terms to divide.
        /// </summary>
        /// <param name="term1">The first term (left-hand side).</param>
        /// <param name="term2">The second term (right-hand side).</param>
        public DivideTerm(ITerm term1, ITerm term2)
        {
            Term1 = term1;
            Term2 = term2;
        }

        /// <summary>
        /// The first term (left-hand side).
        /// </summary>
        [DataMember] public readonly ITerm Term1;

        /// <summary>
        /// The second term (right-hand side).
        /// </summary>
        [DataMember] public readonly ITerm Term2;

        /// <summary>
        /// Divides the first term by the second, evaluating those two terms as necessary.
        /// </summary>
        /// <param name="rng">The rng to used -- passed to other terms.</param>
        /// <returns>The result of evaluating <see cref="Term1" /> / <see cref="Term2" />.</returns>
        public int GetResult(IEnhancedRandom rng) => (int)Math.Round((double)Term1.GetResult(rng) / Term2.GetResult(rng));

        /// <summary>
        /// Returns a parenthesized string representing the operation.
        /// </summary>
        /// <returns>A parenthesized string representing the operation.</returns>
        public override string ToString() => $"({Term1}/{Term2})";
    }
}
