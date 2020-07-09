using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GoRogue.Components;
using JetBrains.Annotations;

namespace GoRogue.SerializedTypes.Components
{
    /// <summary>
    /// Serializable (pure-data) object representing a <see cref="ComponentCollection"/>
    /// </summary>
    [PublicAPI]
    [Serializable]
    [SuppressMessage("ReSharper", "CA1815")] // Type should only be used for serialization
    public struct ComponentCollectionSerialized
    {
        /// <summary>
        /// List of components in the collection.
        /// </summary>
        public List<ComponentTagPairSerialized> Components;

        /// <summary>
        /// Converts <see cref="ComponentCollection"/> to <see cref="ComponentCollectionSerialized"/>.
        /// </summary>
        /// <param name="collection"/>
        /// <returns/>
        public static implicit operator ComponentCollectionSerialized(ComponentCollection collection)
            => FromComponentCollection(collection);

        /// <summary>
        /// Converts <see cref="ComponentCollectionSerialized"/> to <see cref="ComponentCollection"/>.
        /// </summary>
        /// <param name="collection"/>
        /// <returns/>
        public static implicit operator ComponentCollection(ComponentCollectionSerialized collection)
            => collection.ToComponentCollection();

        /// <summary>
        /// Converts <see cref="ComponentCollection"/> to <see cref="ComponentCollectionSerialized"/>.
        /// </summary>
        /// <param name="collection"/>
        /// <returns/>
        public static ComponentCollectionSerialized FromComponentCollection(ComponentCollection collection)
        {
            var expressive = new ComponentCollectionSerialized { Components = new List<ComponentTagPairSerialized>() };

            foreach (var componentTagPair in collection)
                expressive.Components.Add(componentTagPair);

            return expressive;
        }

        /// <summary>
        /// Converts <see cref="ComponentCollectionSerialized"/> to <see cref="ComponentCollection"/>.
        /// </summary>
        /// <returns/>
        public ComponentCollection ToComponentCollection()
            => new ComponentCollection(Components.Select(item => (ComponentTagPair)item));
    }
}
