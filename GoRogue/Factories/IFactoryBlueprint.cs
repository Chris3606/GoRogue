using JetBrains.Annotations;

namespace GoRogue.Factories
{
    /// <summary>
    /// Defines how to create a <typeparamref name="TProduced"/> object for use in an <see cref="Factory{TProduced}"/>.
    /// </summary>
    /// <typeparam name="TProduced">The type of object to create.</typeparam>
    [PublicAPI]
    public interface IFactoryBlueprint<out TProduced>
    {
        /// <summary>
        /// A unique identifier of this factory definition.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Creates a <typeparamref name="TProduced"/> object.
        /// </summary>
        /// <returns>The created object.</returns>
        TProduced Create();
    }
}
