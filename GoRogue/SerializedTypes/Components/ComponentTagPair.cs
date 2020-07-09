using System;
using System.Diagnostics.CodeAnalysis;
using GoRogue.Components;
using JetBrains.Annotations;

namespace GoRogue.SerializedTypes.Components
{
    /// <summary>
    /// Serializable (pure-data) object representing a <see cref="ComponentTagPair"/>
    /// </summary>
    [PublicAPI]
    [Serializable]
    [SuppressMessage("ReSharper", "CA1815")] // Type should only be used for serialization
    public struct ComponentTagPairSerialized
    {
        /// <summary>
        /// Component object.
        /// </summary>
        public object Component;

        /// <summary>
        /// Tag for component.
        /// </summary>
        public string? Tag;

        /// <summary>
        /// Converts <see cref="ComponentTagPair"/> to <see cref="ComponentTagPairSerialized"/>.
        /// </summary>
        /// <param name="pair"/>
        /// <returns/>
        public static implicit operator ComponentTagPairSerialized(ComponentTagPair pair) => FromComponentTagPair(pair);

        /// <summary>
        /// Converts <see cref="ComponentTagPairSerialized"/> to <see cref="ComponentTagPair"/>.
        /// </summary>
        /// <param name="pair"/>
        /// <returns/>
        public static implicit operator ComponentTagPair(ComponentTagPairSerialized pair) => pair.ToComponentTagPair();

        /// <summary>
        /// Converts <see cref="ComponentTagPair"/> to <see cref="ComponentTagPairSerialized"/>.
        /// </summary>
        /// <param name="pair"/>
        /// <returns/>
        public static ComponentTagPairSerialized FromComponentTagPair(ComponentTagPair pair)
            => new ComponentTagPairSerialized { Component = pair.Component, Tag = pair.Tag };

        /// <summary>
        /// Converts <see cref="ComponentTagPairSerialized"/> to <see cref="ComponentTagPair"/>.
        /// </summary>
        /// <returns/>
        public ComponentTagPair ToComponentTagPair() => new ComponentTagPair(Component, Tag);
    }
}
