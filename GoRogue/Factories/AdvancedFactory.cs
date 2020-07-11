using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace GoRogue.Factories
{
    /// <summary>
    /// A more advanced factory that produces a type of object based on a blueprint and a set of configuration parameters.
    /// </summary>
    /// <typeparam name="TBlueprintConfig">
    /// The type of parameter passed to the <see cref="Create(string, TBlueprintConfig)" />
    /// function each time an object is created.
    /// </typeparam>
    /// <typeparam name="TProduced">The type of object this factory creates.</typeparam>
    [PublicAPI]
    [DataContract]
    public class AdvancedFactory<TBlueprintConfig, TProduced>
        : IEnumerable<IAdvancedFactoryBlueprint<TBlueprintConfig, TProduced>>
    {
        private readonly Dictionary<string, IAdvancedFactoryBlueprint<TBlueprintConfig, TProduced>> _blueprints;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AdvancedFactory()
        {
            _blueprints = new Dictionary<string, IAdvancedFactoryBlueprint<TBlueprintConfig, TProduced>>();
        }
        /// <summary>
        /// Constructor.  Takes some initial blueprints to add.
        /// </summary>
        /// <param name="initialBlueprints">Initial blueprints to add.</param>
        public AdvancedFactory(IEnumerable<IAdvancedFactoryBlueprint<TBlueprintConfig, TProduced>> initialBlueprints)
            : this()
        {
            foreach (var blueprint in initialBlueprints)
                Add(blueprint);
        }

        /// <summary>
        /// Gets an enumerator of all of the blueprints in the factory.
        /// </summary>
        /// <returns>An enumeration of the blueprints.</returns>
        public IEnumerator<IAdvancedFactoryBlueprint<TBlueprintConfig, TProduced>> GetEnumerator()
            => _blueprints.Values.GetEnumerator();

        /// <summary>
        /// Gets an enumerator of all of the blueprints in the factory.
        /// </summary>
        /// <returns>An enumeration of the blueprints.</returns>
        IEnumerator IEnumerable.GetEnumerator() => _blueprints.Values.GetEnumerator();

        /// <summary>
        /// Adds a blueprint to the factory.
        /// </summary>
        /// <param name="blueprint">The blueprint to add.</param>
        public void Add(IAdvancedFactoryBlueprint<TBlueprintConfig, TProduced> blueprint)
            => _blueprints[blueprint.Id] = blueprint;

        /// <summary>
        /// Creates a <typeparamref name="TProduced" /> object using the blueprint with the given factory id, and the given
        /// settings object.
        /// </summary>
        /// <param name="factoryId">The factory id of a blueprint.</param>
        /// <param name="blueprintConfig">A settings object passed to the Create function of the blueprint.</param>
        /// <returns>A new object.</returns>
        public TProduced Create(string factoryId, TBlueprintConfig blueprintConfig)
        {
            if (!_blueprints.ContainsKey(factoryId))
                throw new ItemNotDefinedException(factoryId);

            var obj = _blueprints[factoryId].Create(blueprintConfig);
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
        public IAdvancedFactoryBlueprint<TBlueprintConfig, TProduced> GetBlueprint(string factoryId)
        {
            if (!_blueprints.ContainsKey(factoryId))
                throw new ItemNotDefinedException(factoryId);

            return _blueprints[factoryId];
        }
    }
}
