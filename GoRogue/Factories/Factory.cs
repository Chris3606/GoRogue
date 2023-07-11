using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace GoRogue.Factories
{
    /// <summary>
    /// A simple factory that produces a type of object based on a blueprint.
    /// </summary>
    /// <typeparam name="TBlueprintID">The type used to uniquely identify blueprints.</typeparam>
    /// <typeparam name="TProduced">The type of object this factory creates.</typeparam>
    [PublicAPI]
    [DataContract]
    public class Factory<TBlueprintID, TProduced> : IEnumerable<IFactoryBlueprint<TBlueprintID, TProduced>>
        where TBlueprintID : notnull
    {
        private readonly Dictionary<TBlueprintID, IFactoryBlueprint<TBlueprintID, TProduced>> _blueprints;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Factory()
        {
            _blueprints = new Dictionary<TBlueprintID, IFactoryBlueprint<TBlueprintID, TProduced>>();
        }

        /// <summary>
        /// Constructor.  Takes initial blueprints to add.
        /// </summary>
        /// <param name="initialBlueprints">Initial blueprints to add.</param>
        public Factory(IEnumerable<IFactoryBlueprint<TBlueprintID, TProduced>> initialBlueprints)
            : this()
        {
            foreach (var blueprint in initialBlueprints)
                Add(blueprint);
        }

        /// <summary>
        /// Gets an enumerator of all of the blueprints in the factory.
        /// </summary>
        /// <returns>An enumeration of the blueprints.</returns>
        public IEnumerator<IFactoryBlueprint<TBlueprintID, TProduced>> GetEnumerator() => _blueprints.Values.GetEnumerator();

        /// <summary>
        /// Gets an enumerator of all of the blueprints in the factory.
        /// </summary>
        /// <returns>An enumeration of the blueprints.</returns>
        IEnumerator IEnumerable.GetEnumerator() => _blueprints.Values.GetEnumerator();

        /// <summary>
        /// Adds a blueprint to the factory.
        /// </summary>
        /// <param name="blueprint">The blueprint to add.</param>
        public void Add(IFactoryBlueprint<TBlueprintID, TProduced> blueprint) => _blueprints[blueprint.ID] = blueprint;

        /// <summary>
        /// Adds the given blueprints to the factory.
        /// </summary>
        /// <param name="blueprints">The blueprints to add.</param>
        public void AddRange(IEnumerable<IFactoryBlueprint<TBlueprintID, TProduced>> blueprints)
        {
            foreach (var blueprint in blueprints)
                Add(blueprint);
        }

        /// <summary>
        /// Creates a <typeparamref name="TProduced" /> object using the blueprint with the given factory id.
        /// </summary>
        /// <param name="blueprintID">The factory id of a blueprint.</param>
        /// <returns>A new object.</returns>
        public TProduced Create(TBlueprintID blueprintID)
        {
            IFactoryBlueprint<TBlueprintID, TProduced> blueprint;
            try
            {
                blueprint = _blueprints[blueprintID];
            }
            catch (KeyNotFoundException)
            {
                throw new ItemNotDefinedException<TBlueprintID>(blueprintID);
            }

            var obj = blueprint.Create();
            if (obj is IFactoryObject<TBlueprintID> factoryObj)
                factoryObj.DefinitionID = blueprintID;

            return obj;
        }

        /// <summary>
        /// Checks if a blueprint exists.
        /// </summary>
        /// <param name="blueprintID">The blueprint to check for.</param>
        /// <returns>Returns true when the specified <paramref name="blueprintID" /> exists; otherwise false.</returns>
        public bool BlueprintExists(TBlueprintID blueprintID) => _blueprints.ContainsKey(blueprintID);

        /// <summary>
        /// Gets a blueprint by identifier.
        /// </summary>
        /// <param name="blueprintID">The blueprint identifier to get.</param>
        /// <returns>The blueprint of the object.</returns>
        /// <exception cref="ItemNotDefinedException{TBlueprintID}">Thrown if the factory identifier does not exist.</exception>
        public IFactoryBlueprint<TBlueprintID, TProduced> GetBlueprint(TBlueprintID blueprintID)
        {
            try
            {
                return _blueprints[blueprintID];
            }
            catch (KeyNotFoundException)
            {
                throw new ItemNotDefinedException<TBlueprintID>(blueprintID);
            }
        }
    }
}
