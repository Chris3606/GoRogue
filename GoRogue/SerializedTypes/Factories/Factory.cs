using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GoRogue.Factories;
using JetBrains.Annotations;

namespace GoRogue.SerializedTypes.Factories
{
    /// <summary>
    /// Serializable (pure-data) object representing a <see cref="Factory{TProduced}"/>
    /// </summary>
    [PublicAPI]
    [Serializable]
    [SuppressMessage("ReSharper", "CA1815")] // Type should only be used for serialization
    public struct FactorySerialized<TProduced>
    {
        /// <summary>
        /// Blueprints in the factory.
        /// </summary>
        public List<IFactoryBlueprint<TProduced>> Blueprints;

        /// <summary>
        /// Converts <see cref="Factory{TProduced}"/> to <see cref="FactorySerialized{TProduced}"/>.
        /// </summary>
        /// <param name="factory"/>
        /// <returns/>
        public static implicit operator FactorySerialized<TProduced>(Factory<TProduced> factory)
            => FromFactory(factory);

        /// <summary>
        /// Converts <see cref="FactorySerialized{TProduced}"/> to <see cref="Factory{TProduced}"/>.
        /// </summary>
        /// <param name="factory"/>
        /// <returns/>
        public static implicit operator Factory<TProduced>(FactorySerialized<TProduced> factory)
            => factory.ToFactory();

        /// <summary>
        /// Converts <see cref="Factory{TProduced}"/> to <see cref="FactorySerialized{TProduced}"/>.
        /// </summary>
        /// <param name="factory"/>
        /// <returns/>
        [SuppressMessage("ReSharper", "CA1000")] // Static method is required to implement implicit ops
        public static FactorySerialized<TProduced> FromFactory(Factory<TProduced> factory)
            => new FactorySerialized<TProduced> { Blueprints = factory.ToList() };

        /// <summary>
        /// Converts <see cref="FactorySerialized{TProduced}"/> to <see cref="Factory{TProduced}"/>.
        /// </summary>
        /// <returns/>
        public Factory<TProduced> ToFactory() => new Factory<TProduced>(Blueprints);
    }
}
