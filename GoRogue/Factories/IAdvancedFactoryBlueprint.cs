using JetBrains.Annotations;

namespace GoRogue.Factories
{
    /// <summary>
    /// Defines how to create a <typeparamref name="TProduced" /> object for use in an
    /// <see cref="AdvancedFactory{TBlueprintID, TBlueprintConfig, TProduced}" />.
    /// </summary>
    /// <typeparam name="TBlueprintID">The type used to uniquely identify blueprints.</typeparam>
    /// <typeparam name="TBlueprintConfig">
    /// The type of the parameter to pass to the <see cref="Create(TBlueprintConfig)" />
    /// function.
    /// </typeparam>
    /// <typeparam name="TProduced">The type of object to create.</typeparam>
    [PublicAPI]
    public interface IAdvancedFactoryBlueprint<out TBlueprintID, in TBlueprintConfig, out TProduced>
    {
        /// <summary>
        /// A unique identifier of this factory definition.
        /// </summary>
        TBlueprintID ID { get; }

        /// <summary>
        /// Creates an object of the type specified by TProduced using the given configuration parameters.
        /// </summary>
        /// <param name="config">Configuration parameters used to create the object.</param>
        /// <returns>The created object.</returns>
        TProduced Create(TBlueprintConfig config);
    }
}
