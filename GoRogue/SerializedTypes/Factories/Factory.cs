using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using GoRogue.Factories;
using JetBrains.Annotations;

namespace GoRogue.SerializedTypes.Factories
{
    /// <summary>
    /// Serializable (pure-data) object representing a <see cref="Factory{TBlueprintID, TProduced}"/>
    /// </summary>
    [PublicAPI]
    [DataContract]
    public struct FactorySerialized<TBlueprintID, TProduced>
        where TBlueprintID : notnull
    {
        /// <summary>
        /// Blueprints in the factory.
        /// </summary>
        [DataMember] public List<IFactoryBlueprint<TBlueprintID, TProduced>> Blueprints;

        /// <summary>
        /// Converts <see cref="Factory{TBlueprintID, TProduced}"/> to <see cref="FactorySerialized{TBlueprintID, TProduced}"/>.
        /// </summary>
        /// <param name="factory"/>
        /// <returns/>
        public static implicit operator FactorySerialized<TBlueprintID, TProduced>(Factory<TBlueprintID, TProduced> factory)
            => FromFactory(factory);

        /// <summary>
        /// Converts <see cref="FactorySerialized{TBlueprintID, TProduced}"/> to <see cref="Factory{TBlueprintID, TProduced}"/>.
        /// </summary>
        /// <param name="factory"/>
        /// <returns/>
        public static implicit operator Factory<TBlueprintID, TProduced>(FactorySerialized<TBlueprintID, TProduced> factory)
            => factory.ToFactory();

        /// <summary>
        /// Converts <see cref="Factory{TBlueprintID, TProduced}"/> to <see cref="FactorySerialized{TBlueprintID, TProduced}"/>.
        /// </summary>
        /// <param name="factory"/>
        /// <returns/>
        [SuppressMessage("ReSharper", "CA1000")] // Static method is required to implement implicit ops
        public static FactorySerialized<TBlueprintID, TProduced> FromFactory(Factory<TBlueprintID, TProduced> factory)
            => new FactorySerialized<TBlueprintID, TProduced> { Blueprints = factory.ToList() };

        /// <summary>
        /// Converts <see cref="FactorySerialized{TBlueprintID, TProduced}"/> to <see cref="Factory{TBlueprintID, TProduced}"/>.
        /// </summary>
        /// <returns/>
        public Factory<TBlueprintID, TProduced> ToFactory() => new Factory<TBlueprintID, TProduced>(Blueprints);
    }
}
