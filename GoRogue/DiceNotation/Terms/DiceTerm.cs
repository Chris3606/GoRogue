﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using GoRogue.DiceNotation.Exceptions;
using JetBrains.Annotations;
using ShaiRandom.Generators;

namespace GoRogue.DiceNotation.Terms
{
    /// <summary>
    /// Represents a dice term, eg 1d4 or 2d6.
    /// </summary>
    [PublicAPI]
    [DataContract]
    public class DiceTerm : ITerm
    {
        private readonly List<int> _diceResults;

        /// <summary>
        /// Constructor. Takes the terms representing multiplicity and number of sides.
        /// </summary>
        /// <param name="multiplicity">
        /// Term representing the number of dice being rolled -- 2d6 has multiplicity 2.
        /// </param>
        /// <param name="sides">
        /// Term representing the number of sides the dice have -- 2d6 has 6 sides.
        /// </param>
        public DiceTerm(ITerm multiplicity, ITerm sides)
        {
            Multiplicity = multiplicity;
            Sides = sides;

            _diceResults = new List<int>();
        }

        /// <summary>
        /// An enumerable of integers representing the result of each dice roll. The expression 2d6
        /// rolls 2 dice, and as such this enumerable would be of length 2 and contain the result of
        /// each individual die.
        /// </summary>
        public IEnumerable<int> DiceResults => _diceResults;

        /// <summary>
        /// The result of evaluating the <see cref="Multiplicity" /> term that was used during the last call to
        /// <see cref="GetResult(IEnhancedRandom)" />.
        /// </summary>
        public int LastMultiplicity { get; private set; }

        /// <summary>
        /// The result of evaluating the <see cref="Sides" /> term that was used during the last call to
        /// <see cref="GetResult(IEnhancedRandom)" />.
        /// </summary>
        public int LastSidedness { get; private set; }

        /// <summary>
        /// Term representing the number of dice being rolled -- 2d6 has multiplicity 2.
        /// </summary>
        [DataMember] public readonly ITerm Multiplicity;

        /// <summary>
        /// Term representing the number of sides the dice have -- 2d6 has 6 sides.
        /// </summary>
        [DataMember] public readonly ITerm Sides;

        /// <summary>
        /// Rolls the dice, returning the sum.
        /// </summary>
        /// <param name="rng">The RNG to use for rolling,</param>
        /// <returns>The sum of the roll.</returns>
        public int GetResult(IEnhancedRandom rng)
        {
            _diceResults.Clear();
            var sum = 0;
            LastMultiplicity = Multiplicity.GetResult(rng);
            LastSidedness = Sides.GetResult(rng);

            if (LastMultiplicity < 0)
                throw new InvalidMultiplicityException();

            if (LastSidedness <= 0)
                throw new ImpossibleDieException();

            for (var i = 0; i < LastMultiplicity; i++)
            {
                var diceVal = rng.NextInt(1, LastSidedness + 1);
                sum += diceVal;
                _diceResults.Add(diceVal);
            }

            return sum;
        }

        /// <summary>
        /// Gets a parenthesized string representation of the dice term, eg (2d6).
        /// </summary>
        /// <returns>A parenthesized representation of the term.</returns>
        public override string ToString() => $"({Multiplicity}d{Sides})";
    }
}
