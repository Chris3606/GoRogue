using JetBrains.Annotations;

namespace GoRogue.Factories
{
    /// <summary>
    /// Defines how to create a <typeparamref name="TProduced" /> object for use in an <see cref="Factory{TBlueprintID, TProduced}" />.
    /// </summary>
    /// <typeparam name="TBlueprintID">The type used to uniquely identify blueprints.</typeparam>
    /// <typeparam name="TProduced">The type of object to create.</typeparam>
    [PublicAPI]
    public interface IFactoryBlueprint<out TBlueprintID, out TProduced>
    {
        /// <summary>
        /// A unique identifier of this factory definition.
        /// </summary>
        TBlueprintID Id { get; }

        /// <summary>
        /// Creates an object of the type specified by TProduced.
        /// </summary>
        /// <returns>The created object.</returns>
        TProduced Create();
    }
}
