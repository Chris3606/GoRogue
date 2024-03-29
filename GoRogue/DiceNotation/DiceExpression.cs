﻿using System.Runtime.Serialization;
using GoRogue.DiceNotation.Terms;
using GoRogue.Random;
using JetBrains.Annotations;
using ShaiRandom.Generators;

namespace GoRogue.DiceNotation
{
    /// <summary>
    /// The default class for representing a parsed dice expression.
    /// </summary>
    [PublicAPI]
    [DataContract]
    public class DiceExpression
    {
        /// <summary>
        /// The root term in this expression tree.
        /// </summary>
        [DataMember] public readonly ITerm RootTerm;

        /// <summary>
        /// Constructor. Takes the last term in the dice expression (the root of the expression tree).
        /// </summary>
        /// <param name="rootTerm">
        /// The root of the expression tree -- by evaluating this term, all others will be evaluated recursively.
        /// </param>
        public DiceExpression(ITerm rootTerm) => RootTerm = rootTerm;

        /// <summary>
        /// Returns the maximum possible result of the dice expression.
        /// </summary>
        /// <returns>The maximum possible result of the dice expression.</returns>
        public int MaxRoll() => Roll(MaxRandom.Instance);

        /// <summary>
        /// Returns the minimum possible result of the dice expression.
        /// </summary>
        /// <returns>The minimum possible result of the dice expression.</returns>
        public int MinRoll() => Roll(MinRandom.Instance);

        /// <summary>
        /// Rolls the expression using the RNG given, returning the result.
        /// </summary>
        /// <param name="rng">The RNG to use. If null is specified, the default RNG is used.</param>
        /// <returns>The result obtained by rolling the dice expression.</returns>
        public int Roll(IEnhancedRandom? rng = null)
        {
            rng ??= GlobalRandom.DefaultRNG;

            return RootTerm.GetResult(rng);
        }

        /// <summary>
        /// Returns a parenthesized string representing the dice expression in dice notation
        /// </summary>
        /// <returns>A parenthesized string representing the expression.</returns>
        public override string ToString() => RootTerm.ToString() ?? "null";
    }
}
