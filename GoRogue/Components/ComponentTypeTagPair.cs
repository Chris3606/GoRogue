using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace GoRogue.Components
{
    /// <summary>
    /// A type of a component and the tag expected to be associated with a component of that type in a
    /// <see cref="ITaggableComponentCollection"/> or map generation step.
    /// </summary>
    [DataContract]
    [PublicAPI]
    // Tuples do not resolve names properly; function is provided
    [SuppressMessage("ReSharper", "CA2225")]
    public struct ComponentTypeTagPair : IEquatable<ComponentTypeTagPair>
    {
        /// <summary>
        /// The type of component expected.
        /// </summary>
        [DataMember] public readonly Type ComponentType;

        /// <summary>
        /// The tag expected to be associated with a component of the specified type.
        /// </summary>
        [DataMember] public readonly string? Tag;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="componentType"/>
        /// <param name="tag"/>
        public ComponentTypeTagPair(Type componentType, string? tag)
        {
            ComponentType = componentType;
            Tag = tag;
        }

        /// <summary>
        /// Returns a string representing the component type and its tag.
        /// </summary>
        /// <returns/>
        public override string ToString() => $"{ComponentType.Name}: {Tag ?? "null"}";

        #region Tuple Compatibility

        /// <summary>
        /// Supports C# Deconstruction syntax.
        /// </summary>
        /// <param name="componentType"/>
        /// <param name="tag"/>
        public void Deconstruct(out Type componentType, out string? tag)
        {
            componentType = ComponentType;
            tag = Tag;
        }

        /// <summary>
        /// Implicitly converts a ComponentTypeTagPair to an equivalent tuple.
        /// </summary>
        /// <param name="pair"/>
        /// <returns/>
        public static implicit operator (Type componentType, string? tag)(ComponentTypeTagPair pair) => pair.ToTuple();

        /// <summary>
        /// Implicitly converts a tuple to its equivalent ComponentTypeTagPair.
        /// </summary>
        /// <param name="tuple"/>
        /// <returns/>
        public static implicit operator ComponentTypeTagPair((Type componentType, string? tag) tuple)
            => FromTuple(tuple);

        /// <summary>
        /// Converts the pair to an equivalent tuple.
        /// </summary>
        /// <returns/>
        public (Type componentType, string? tag) ToTuple() => (ComponentType, Tag);

        /// <summary>
        /// Converts the tuple to an equivalent ComponentTypeTagPair.
        /// </summary>
        /// <param name="tuple"/>
        /// <returns/>
        public static ComponentTypeTagPair FromTuple((Type componentType, string? tag) tuple)
            => new ComponentTypeTagPair(tuple.componentType, tuple.tag);
        #endregion

        #region Equality Comparison

        /// <summary>
        /// True if the given pair has the same component type and tag; false otherwise.
        /// </summary>
        /// <param name="other"/>
        /// <returns/>
        public bool Equals(ComponentTypeTagPair other)
            => ComponentType == other.ComponentType && Tag == other.Tag;

        /// <summary>
        /// True if the given object is a ComponentTypeTagPair and has the same component type and tag; false otherwise.
        /// </summary>
        /// <param name="obj"/>
        /// <returns/>
        public override bool Equals(object? obj) => obj is ComponentTypeTagPair pair && Equals(pair);

        /// <summary>
        /// Returns a hash code based on all of the pair's field's.
        /// </summary>
        /// <returns/>
        public override int GetHashCode()
        {
            int hash = ComponentType.GetHashCode();
            if (Tag != null)
                hash ^= Tag.GetHashCode();

            return hash;
        }

        /// <summary>
        /// True if the given pairs have the same component type and tag; false otherwise.
        /// </summary>
        /// <param name="left"/>
        /// <param name="right"/>
        /// <returns/>
        public static bool operator ==(ComponentTypeTagPair left, ComponentTypeTagPair right) => left.Equals(right);

        /// <summary>
        /// True if the given pairs have different component types and/or tags; false otherwise.
        /// </summary>
        /// <param name="left"/>
        /// <param name="right"/>
        /// <returns/>
        public static bool operator !=(ComponentTypeTagPair left, ComponentTypeTagPair right) => !(left == right);
        #endregion

    }
}
