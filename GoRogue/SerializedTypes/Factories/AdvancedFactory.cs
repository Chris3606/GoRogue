using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using GoRogue.Factories;
using JetBrains.Annotations;

namespace GoRogue.SerializedTypes.Factories
{
    /// <summary>
    /// Serializable (pure-data) object representing a <see cref="AdvancedFactory{TBlueprintConfig, TProduced}"/>
    /// </summary>
    [PublicAPI]
    [DataContract]
    [SuppressMessage("ReSharper", "CA1815")] // Type should only be used for serialization
    public struct AdvancedFactorySerialized<TBlueprintConfig, TProduced>
    {
        /// <summary>
        /// Blueprints in the factory.
        /// </summary>
        [DataMember] public List<IAdvancedFactoryBlueprint<TBlueprintConfig, TProduced>> Blueprints;

        /// <summary>
        /// Converts <see cref="AdvancedFactory{TBlueprintConfig, TProduced}"/> to
        /// <see cref="AdvancedFactorySerialized{TBlueprintConfig, TProduced}"/>.
        /// </summary>
        /// <param name="factory"/>
        /// <returns/>
        public static implicit operator AdvancedFactorySerialized<TBlueprintConfig, TProduced>(
            AdvancedFactory<TBlueprintConfig, TProduced> factory)
            => FromAdvancedFactory(factory);

        /// <summary>
        /// Converts <see cref="AdvancedFactorySerialized{TBlueprintConfig, TProduced}"/> to
        /// <see cref="AdvancedFactory{TBlueprintConfig, TProduced}"/>.
        /// </summary>
        /// <param name="factory"/>
        /// <returns/>
        public static implicit operator AdvancedFactory<TBlueprintConfig, TProduced>(
            AdvancedFactorySerialized<TBlueprintConfig, TProduced> factory)
            => factory.ToAdvancedFactory();

        /// <summary>
        /// Converts <see cref="AdvancedFactory{TBlueprintConfig, TProduced}"/> to
        /// <see cref="AdvancedFactorySerialized{TBlueprintConfig, TProduced}"/>.
        /// </summary>
        /// <param name="factory"/>
        /// <returns/>
        [SuppressMessage("ReSharper", "CA1000")] // Static method is required to implement implicit ops
        public static AdvancedFactorySerialized<TBlueprintConfig, TProduced> FromAdvancedFactory(
            AdvancedFactory<TBlueprintConfig, TProduced> factory)
            => new AdvancedFactorySerialized<TBlueprintConfig, TProduced> { Blueprints = factory.ToList() };

        /// <summary>
        /// Converts <see cref="AdvancedFactorySerialized{TBlueprintConfig, TProduced}"/> to
        /// <see cref="AdvancedFactory{TBlueprintConfig, TProduced}"/>.
        /// </summary>
        /// <returns/>
        public AdvancedFactory<TBlueprintConfig, TProduced> ToAdvancedFactory()
            => new AdvancedFactory<TBlueprintConfig, TProduced>(Blueprints);
    }
}
