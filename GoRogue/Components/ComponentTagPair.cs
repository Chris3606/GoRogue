using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.Components
{
    /// <summary>
    /// A component from a <see cref="IComponentCollection"/> and its associated tag.
    /// </summary>
    [DataContract]
    [PublicAPI]
    // Tuples do not resolve names properly; function is provided
    public readonly struct ComponentTagPair : IEquatable<ComponentTagPair>, IMatchable<ComponentTagPair>
    {
        /// <summary>
        /// The component.
        /// </summary>
        [DataMember] public readonly object Component;

        /// <summary>
        /// The tag associated with its component.
        /// </summary>
        [DataMember] public readonly string? Tag;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="component"/>
        /// <param name="tag"/>
        public ComponentTagPair(object component, string? tag)
        {
            Component = component;
            Tag = tag;
        }

        /// <summary>
        /// Returns a string representing the component and its tag
        /// </summary>
        /// <returns/>
        public override string ToString() => $"{Component}: {Tag ?? "null"}";

        #region Tuple Compatibility

        /// <summary>
        /// Supports C# Deconstruction syntax.
        /// </summary>
        /// <param name="component"/>
        /// <param name="tag"/>
        public void Deconstruct(out object component, out string? tag)
        {
            component = Component;
            tag = Tag;
        }

        /// <summary>
        /// Implicitly converts a ComponentTagPair to an equivalent tuple.
        /// </summary>
        /// <param name="pair"/>
        /// <returns/>
        public static implicit operator (object component, string? tag)(ComponentTagPair pair) => pair.ToTuple();

        /// <summary>
        /// Implicitly converts a tuple to its equivalent ComponentTagPair.
        /// </summary>
        /// <param name="tuple"/>
        /// <returns/>
        public static implicit operator ComponentTagPair((object component, string? tag) tuple) => FromTuple(tuple);

        /// <summary>
        /// Converts the pair to an equivalent tuple.
        /// </summary>
        /// <returns/>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (object component, string? tag) ToTuple() => (Component, Tag);

        /// <summary>
        /// Converts the tuple to an equivalent ComponentTagPair.
        /// </summary>
        /// <param name="tuple"/>
        /// <returns/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentTagPair FromTuple((object component, string? tag) tuple)
            => new ComponentTagPair(tuple.component, tuple.tag);
        #endregion

        #region EqualityComparison

        /// <summary>
        /// True if the given pair has the same component and tag; false otherwise.
        /// </summary>
        /// <param name="other"/>
        /// <returns/>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ComponentTagPair other) => Component.Equals(other.Component) && Tag == other.Tag;

        /// <summary>
        /// True if the given pair has the same component and tag; false otherwise.
        /// </summary>
        /// <param name="other"/>
        /// <returns/>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Matches(ComponentTagPair other) => Equals(other);

        /// <summary>
        /// True if the given object is a ComponentTagPair and has the same component and tag; false otherwise.
        /// </summary>
        /// <param name="obj"/>
        /// <returns/>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is ComponentTagPair pair && Equals(pair);

        /// <summary>
        /// Returns a hash code based on all of the pair's field's.
        /// </summary>
        /// <returns/>
        public override int GetHashCode()
        {
            int hash = Component.GetHashCode();
            if (Tag != null)
                hash ^= Tag.GetHashCode();

            return hash;
        }

        /// <summary>
        /// True if the given pairs have the same component and tag; false otherwise.
        /// </summary>
        /// <param name="left"/>
        /// <param name="right"/>
        /// <returns/>
        public static bool operator ==(ComponentTagPair left, ComponentTagPair right) => left.Equals(right);

        /// <summary>
        /// True if the given pairs have different components and/or tags; false otherwise.
        /// </summary>
        /// <param name="left"/>
        /// <param name="right"/>
        /// <returns/>
        public static bool operator !=(ComponentTagPair left, ComponentTagPair right) => !(left == right);
        #endregion

    }
}
