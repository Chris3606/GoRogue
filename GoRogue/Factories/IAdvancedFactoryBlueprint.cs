using JetBrains.Annotations;

namespace GoRogue.Factories
{
    /// <summary>
    /// Defines how to create a <typeparamref name="TProduced"/> object for use in an <see cref="AdvancedFactory{TBlueprintConfig, TProduced}"/>.
    /// </summary>
    /// <typeparam name="TBlueprintConfig">The type of the parameter to pass to the <see cref="Create(TBlueprintConfig)"/> function.</typeparam>
    /// <typeparam name="TProduced">The type of object to create.</typeparam>
    [PublicAPI]
    public interface IAdvancedFactoryBlueprint<in TBlueprintConfig, out TProduced>
    {
        /// <summary>
        /// A unique identifier of this factory definition.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Creates a <typeparamref name="TProduced"/> object.
        /// </summary>
        /// <param name="config">Configuration parameters used to create the object.</param>
        /// <returns>The created object.</returns>
        TProduced Create(TBlueprintConfig config);
    }
}
