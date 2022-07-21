using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Pathing
{
    /// <summary>
    /// A goal map paired with a weight, designed to be used with <see cref="WeightedGoalMap"/>.
    /// </summary>
    [PublicAPI]
    public readonly struct GoalMapWeightPair : IEquatable<GoalMapWeightPair>, IMatchable<GoalMapWeightPair>
    {
        /// <summary>
        /// The goal map.
        /// </summary>
        public readonly IGridView<double?> GoalMap;

        /// <summary>
        /// The weight of the specified goal map.
        /// </summary>
        public readonly double Weight;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="goalMap">The goal map.</param>
        /// <param name="weight">Weight for the specified goal map.</param>
        public GoalMapWeightPair(IGridView<double?> goalMap, double weight)
        {
            GoalMap = goalMap;
            Weight = weight;
        }

        #region Tuple Compatibility

        /// <summary>
        /// Supports C# Deconstruction syntax.
        /// </summary>
        /// <param name="goalMap"/>
        /// <param name="weight"/>
        public void Deconstruct(out IGridView<double?> goalMap, out double weight)
        {
            goalMap = GoalMap;
            weight = Weight;
        }

        /// <summary>
        /// Implicitly converts a GoalMapWeightPair to an equivalent tuple.
        /// </summary>
        /// <param name="pair"/>
        /// <returns/>
        public static implicit operator (IGridView<double?> goalMap, double weight)(GoalMapWeightPair pair) => pair.ToTuple();

        /// <summary>
        /// Implicitly converts a tuple to its equivalent GoalMapWeightPair.
        /// </summary>
        /// <param name="tuple"/>
        /// <returns/>
        public static implicit operator GoalMapWeightPair((IGridView<double?> goalMap, double weight) tuple)
            => FromTuple(tuple);

        /// <summary>
        /// Converts the pair to an equivalent tuple.
        /// </summary>
        /// <returns/>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (IGridView<double?> goalMap, double weight) ToTuple() => (GoalMap, Weight);

        /// <summary>
        /// Converts the tuple to an equivalent ComponentTypeTagPair.
        /// </summary>
        /// <param name="tuple"/>
        /// <returns/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GoalMapWeightPair FromTuple((IGridView<double?> goalMap, double weight) tuple)
            => new GoalMapWeightPair(tuple.goalMap, tuple.weight);
        #endregion

        #region Equality Comparison

        /// <summary>
        /// True if the given pair has the same component type and tag; false otherwise.
        /// </summary>
        /// <param name="other"/>
        /// <returns/>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(GoalMapWeightPair other)
            => GoalMap == other.GoalMap && Math.Abs(Weight - other.Weight) < 0.0000000000001;

        /// <summary>
        /// True if the given pair has the same goal map and weight; false otherwise.
        /// </summary>
        /// <param name="other"/>
        /// <returns/>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Matches(GoalMapWeightPair other) => Equals(other);

        /// <summary>
        /// True if the given object is a GoalMapWeightPair and has the same goal map and weight; false otherwise.
        /// </summary>
        /// <param name="obj"/>
        /// <returns/>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is GoalMapWeightPair pair && Equals(pair);

        /// <summary>
        /// Returns a hash code based on all of the pair's field's.
        /// </summary>
        /// <returns/>
        public override int GetHashCode()
        {
            int hash = GoalMap.GetHashCode();
            hash ^= Weight.GetHashCode();

            return hash;
        }

        /// <summary>
        /// True if the given pairs have the same goal map and weight; false otherwise.
        /// </summary>
        /// <param name="left"/>
        /// <param name="right"/>
        /// <returns/>
        public static bool operator ==(GoalMapWeightPair left, GoalMapWeightPair right) => left.Equals(right);

        /// <summary>
        /// True if the given pairs have different goal maps and/or weights; false otherwise.
        /// </summary>
        /// <param name="left"/>
        /// <param name="right"/>
        /// <returns/>
        public static bool operator !=(GoalMapWeightPair left, GoalMapWeightPair right) => !(left == right);
        #endregion
    }
}
