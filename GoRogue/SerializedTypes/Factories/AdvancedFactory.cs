using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using GoRogue.Factories;
using JetBrains.Annotations;

namespace GoRogue.SerializedTypes.Factories
{
    /// <summary>
    /// Serializable (pure-data) object representing a <see cref="AdvancedFactory{TBlueprintID, TBlueprintConfig, TProduced}"/>
    /// </summary>
    [PublicAPI]
    [DataContract]
    public struct AdvancedFactorySerialized<TBlueprintID, TBlueprintConfig, TProduced>
        where TBlueprintID : notnull
    {
        /// <summary>
        /// Blueprints in the factory.
        /// </summary>
        [DataMember] public List<IAdvancedFactoryBlueprint<TBlueprintID, TBlueprintConfig, TProduced>> Blueprints;

        /// <summary>
        /// Converts <see cref="AdvancedFactory{TBlueprintID, TBlueprintConfig, TProduced}"/> to
        /// <see cref="AdvancedFactorySerialized{TBlueprintID, TBlueprintConfig, TProduced}"/>.
        /// </summary>
        /// <param name="factory"/>
        /// <returns/>
        public static implicit operator AdvancedFactorySerialized<TBlueprintID, TBlueprintConfig, TProduced>(
            AdvancedFactory<TBlueprintID, TBlueprintConfig, TProduced> factory)
            => FromAdvancedFactory(factory);

        /// <summary>
        /// Converts <see cref="AdvancedFactorySerialized{TBlueprintID, TBlueprintConfig, TProduced}"/> to
        /// <see cref="AdvancedFactory{TBlueprintID, TBlueprintConfig, TProduced}"/>.
        /// </summary>
        /// <param name="factory"/>
        /// <returns/>
        public static implicit operator AdvancedFactory<TBlueprintID, TBlueprintConfig, TProduced>(
            AdvancedFactorySerialized<TBlueprintID, TBlueprintConfig, TProduced> factory)
            => factory.ToAdvancedFactory();

        /// <summary>
        /// Converts <see cref="AdvancedFactory{TBlueprintID, TBlueprintConfig, TProduced}"/> to
        /// <see cref="AdvancedFactorySerialized{TBlueprintID, TBlueprintConfig, TProduced}"/>.
        /// </summary>
        /// <param name="factory"/>
        /// <returns/>
        [SuppressMessage("ReSharper", "CA1000")] // Static method is required to implement implicit ops
        public static AdvancedFactorySerialized<TBlueprintID, TBlueprintConfig, TProduced> FromAdvancedFactory(
            AdvancedFactory<TBlueprintID, TBlueprintConfig, TProduced> factory)
            => new AdvancedFactorySerialized<TBlueprintID, TBlueprintConfig, TProduced> { Blueprints = factory.ToList() };

        /// <summary>
        /// Converts <see cref="AdvancedFactorySerialized{TBlueprintID, TBlueprintConfig, TProduced}"/> to
        /// <see cref="AdvancedFactory{TBlueprintID, TBlueprintConfig, TProduced}"/>.
        /// </summary>
        /// <returns/>
        public AdvancedFactory<TBlueprintID, TBlueprintConfig, TProduced> ToAdvancedFactory()
            => new AdvancedFactory<TBlueprintID, TBlueprintConfig, TProduced>(Blueprints);
    }
}
