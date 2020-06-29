using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GoRogue.Factories
{
    /// <summary>
    /// A simple factory that produces a type of object based on a blueprint.
    /// </summary>
    /// <typeparam name="TProduced">The type of object this factory creates.</typeparam>
    [PublicAPI]
    public class Factory<TProduced> : IEnumerable<IFactoryBlueprint<TProduced>>
    {
        private readonly Dictionary<string, IFactoryBlueprint<TProduced>> _blueprints =
            new Dictionary<string, IFactoryBlueprint<TProduced>>();

        /// <summary>
        /// Gets an enumerator of all of the blueprints in the factory.
        /// </summary>
        /// <returns>An enumeration of the blueprints.</returns>
        public IEnumerator<IFactoryBlueprint<TProduced>> GetEnumerator() => _blueprints.Values.GetEnumerator();

        /// <summary>
        /// Gets an enumerator of all of the blueprints in the factory.
        /// </summary>
        /// <returns>An enumeration of the blueprints.</returns>
        IEnumerator IEnumerable.GetEnumerator() => _blueprints.Values.GetEnumerator();

        /// <summary>
        /// Adds a blueprint to the factory.
        /// </summary>
        /// <param name="blueprint">The blueprint to add.</param>
        public void Add(IFactoryBlueprint<TProduced> blueprint) => _blueprints[blueprint.Id] = blueprint;

        /// <summary>
        /// Creates a <typeparamref name="TProduced" /> object using the blueprint with the given factory id.
        /// </summary>
        /// <param name="factoryId">The factory id of a blueprint.</param>
        /// <returns>A new object.</returns>
        public TProduced Create(string factoryId)
        {
            if (!_blueprints.ContainsKey(factoryId))
                throw new ItemNotDefinedException(factoryId);

            var obj = _blueprints[factoryId].Create();
            if (obj is IFactoryObject factoryObj)
                factoryObj.DefinitionId = factoryId;

            return obj;
        }

        /// <summary>
        /// Checks if a blueprint exists.
        /// </summary>
        /// <param name="factoryId">The blueprint to check for.</param>
        /// <returns>Returns true when the specified <paramref name="factoryId" /> exists; otherwise false.</returns>
        public bool BlueprintExists(string factoryId) => _blueprints.ContainsKey(factoryId);

        /// <summary>
        /// Gets a blueprint by identifier.
        /// </summary>
        /// <param name="factoryId">The blueprint identifier to get.</param>
        /// <returns>The blueprint of the object.</returns>
        /// <exception cref="ItemNotDefinedException">Thrown if the factory identifier does not exist.</exception>
        public IFactoryBlueprint<TProduced> GetBlueprint(string factoryId)
        {
            if (!_blueprints.ContainsKey(factoryId))
                throw new ItemNotDefinedException(factoryId);

            return _blueprints[factoryId];
        }
    }
}
