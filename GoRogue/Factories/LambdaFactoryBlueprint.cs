using System;
using JetBrains.Annotations;

namespace GoRogue.Factories
{
    /// <summary>
    /// An implementation of <see cref="IFactoryBlueprint{TBlueprintID, TProduced}"/> that allows you to specify
    /// a Func object to use as the factory method.
    /// </summary>
    /// <remarks>
    /// This class may be useful for simple cases where your blueprint has no state, and you simply need to specify
    /// some code to run to create the object.  For more complex cases, you may want to implement your own IFactoryBlueprint.
    /// </remarks>
    /// <typeparam name="TBlueprintID">The type being used to identify what object to produce.</typeparam>
    /// <typeparam name="TProduced">The type of object being produced.</typeparam>
    [PublicAPI]
    public class LambdaFactoryBlueprint<TBlueprintID, TProduced> : IFactoryBlueprint<TBlueprintID, TProduced>
    {
        /// <inheritdoc />
        public TBlueprintID ID { get; }

        private readonly Func<TProduced> _factoryMethod;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">A unique identifier for this factory blueprint definition.</param>
        /// <param name="factoryMethod">The method to call when <see cref="Create"/> is used in order to get the object to return.</param>
        public LambdaFactoryBlueprint(TBlueprintID id, Func<TProduced> factoryMethod)
        {
            ID = id;
            _factoryMethod = factoryMethod;
        }

        /// <inheritdoc />
        public TProduced Create() => _factoryMethod();
    }
}
