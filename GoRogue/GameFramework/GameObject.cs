using System;
using GoRogue.Components;
using GoRogue.GameFramework.Components;
using GoRogue.Random;
using GoRogue.SpatialMaps;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.GameFramework
{
    /// <summary>
    /// Base class for any object that has a grid position and can be added to a <see cref="Map" />.  Implements basic
    /// attributes generally common to all objects on a map, as well as properties/methods that <see cref="Map"/> needs
    /// to function.  It also provides a container that you may attach arbitrary components to.
    ///
    /// In cases where you cannot inherit from GameObject, you may have your class implement <see cref="IGameObject" />
    /// via a private field of this type.
    /// </summary>
    /// <remarks>
    /// This class is designed to serve as a base class for your own game objects in your game.  It implements basic,
    /// common functionality such as walkability and transparency, provides some infrastructure that allows it to be
    /// added to instances of <see cref="Map" />, and has a collection that you may add arbitrary components to.
    /// Additionally, it implements the necessary functionality to allow it to be added to an
    /// <see cref="ISpatialMap{T}" /> implementation.
    ///
    /// Generally, you would create one or more classes (say, MyGameObject or MyTerrain), that derives from this one
    /// (GameObject), or implement
    /// <see cref="IGameObject" /> by composition using a private field of this class, and use that as
    /// the base class for your game's objects.  There is an example of doing this
    /// <a href="https://chris3606.github.io/GoRogue/articles/game-framework.html#implementing-igameobject">here</a>.
    ///
    /// If you wish to use a composition/components based approach instead, a subclass of GameObject isn't strictly
    /// necessary.  You may simply construct basic GameObject instances and add components to their
    /// <see cref="GoRogueComponents"/> collection.
    ///
    /// In either case, a <see cref="Map" /> instance can be used to store these objects efficiently.  As well, Map
    /// provides functionality that will allow you to retrieve your objects as references of their derived type
    /// (MyGameObject or MyTerrain, in the example above), meaning that if you are using an inheritance-based approach
    /// you can implement any common, game-specific functionality you need and have easy access to that information when
    /// objects are retrieved from the map.
    ///
    /// If you are using a component-based approach, the component collection will accept components of any type.
    /// If the components implement <see cref="ISortedComponent"/>, the priority system works as expected; see
    /// <see cref="ComponentCollection"/> documentation.  Additionally, your components may optionally implement
    /// <see cref="IGameObjectComponent" />.  This interface has a <see cref="IGameObjectComponent.Parent"/> property,
    /// which will automatically be updated to be set to the game object that the component has been added to.
    /// </remarks>
    [PublicAPI]
    public class GameObject : IGameObject
    {
        private bool _isWalkable;
        private Point _position;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// <paramref name="idGenerator"/> is used to generate an ID which is assigned to the <see cref="ID" />
        /// field. When null is specified, the constructor simply assigns a random number in range of valid uints. This
        /// is sufficiently distinct for the purposes of placing the objects in an <see cref="ISpatialMap{T}" />
        /// implementation, however obviously does NOT guarantee true uniqueness. If uniqueness or some other
        /// implementation is required, override this function to return an appropriate ID. Keep in mind a relatively
        /// high degree of uniqueness is necessary for efficient placement in an ISpatialMap implementation.
        /// </remarks>
        /// <param name="position">Position to start the object at.</param>
        /// <param name="layer">The layer of of a <see cref="Map" /> the object is assigned to.</param>
        /// <param name="isWalkable">
        /// Whether or not the object is to be considered "walkable", eg. whether or not the square it resides
        /// on can be traversed by other, non-walkable objects on the same <see cref="Map" />.  Effectively, whether or not this
        /// object collides.
        /// </param>
        /// <param name="isTransparent">
        /// Whether or not the object is considered "transparent", eg. whether or not light passes through it
        /// for the sake of calculating the FOV of a <see cref="Map" />.
        /// </param>
        /// <param name="idGenerator">
        /// The function used to generate and return an unsigned integer to use assign to the <see cref="ID" /> field.
        /// Most of the time, you will not need to specify this as the default implementation will be sufficient.  See
        /// the constructor remarks for details.
        /// </param>
        /// <param name="customComponentContainer">
        /// A custom component container to use for objects.  If not specified, a <see cref="ComponentCollection"/> is
        /// used.  Typically you will not need to specify this, as a ComponentCollection is sufficient for nearly all
        /// use cases.
        /// </param>
        public GameObject(Point position, int layer, bool isWalkable = true, bool isTransparent = true,
                          Func<uint>? idGenerator = null, ITaggableComponentCollection? customComponentContainer = null)
        {
            idGenerator ??= GlobalRandom.DefaultRNG.NextUInt;

            _position = position;
            Layer = layer;
            IsWalkable = isWalkable;
            IsTransparent = isTransparent;

            CurrentMap = null;

            ID = idGenerator();
            GoRogueComponents = customComponentContainer ?? new ComponentCollection();
            GoRogueComponents.ComponentAdded += On_ComponentAdded;
            GoRogueComponents.ComponentRemoved += On_ComponentRemoved;
        }

        /// <inheritdoc />
        public Point Position
        {
            get => _position;
            set
            {
                // Nothing to do; we're already at the specified position
                if (_position == value)
                    return;

                // Set position and fire event
                var oldValue = _position;
                _position = value;
                try
                {
                    Moved?.Invoke(this, new ItemMovedEventArgs<IGameObject>(this, oldValue, _position));
                }
                catch (InvalidOperationException)
                {
                    // If exception, preserve old value for future
                    _position = oldValue;
                    throw;
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler<ItemMovedEventArgs<IGameObject>>? Moved;

        /// <inheritdoc />
        public bool IsWalkable
        {
            get => _isWalkable;

            set
            {
                // Nothing to do; value has not changed.
                if (_isWalkable == value)
                    return;

                // Set walkability and fire event
                var oldValue = _isWalkable;
                _isWalkable = value;
                try
                {
                    WalkabilityChanged?.Invoke(this, new ItemWalkabilityChangedEventArgs(this, oldValue, _isWalkable));
                }
                catch (InvalidOperationException)
                {
                    // If exception, preserve old value for future
                    _isWalkable = oldValue;
                    throw;
                }
            }
        }
        /// <inheritdoc />
        public event EventHandler<ItemWalkabilityChangedEventArgs>? WalkabilityChanged;

        private bool _isTransparent;

        /// <inheritdoc />
        public bool IsTransparent
        {
            get => _isTransparent;
            set
            {
                // Nothing to do; value has not changed.
                if (_isWalkable == value)
                    return;

                // Set transparency and fire event
                var oldValue = _isTransparent;
                _isTransparent = value;
                try
                {
                    TransparencyChanged?.Invoke(this, new ItemTransparencyChangedEventArgs(this, oldValue, _isWalkable));
                }
                catch (InvalidOperationException)
                {
                    // If exception, preserve old value for future
                    _isTransparent = oldValue;
                    throw;
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler<ItemTransparencyChangedEventArgs>? TransparencyChanged;

        /// <inheritdoc />
        public uint ID { get; }

        /// <inheritdoc />
        public int Layer { get; }

        /// <inheritdoc />
        public Map? CurrentMap { get; private set; }

        /// <inheritdoc />
        public ITaggableComponentCollection GoRogueComponents { get; }

        /// <inheritdoc />
        public void OnMapChanged(Map? newMap)
        {
            CurrentMap = newMap;
        }

        #region Component Handlers
        private void On_ComponentAdded(object? s, ComponentChangedEventArgs e)
        {
            if (!(e.Component is IGameObjectComponent c))
                return;

            if (c.Parent != null)
                throw new ArgumentException(
                    $"Components implementing {nameof(IGameObjectComponent)} cannot be added to multiple objects at once.");

            c.Parent = this;
        }

        private void On_ComponentRemoved(object? s, ComponentChangedEventArgs e)
        {
            if (e.Component is IGameObjectComponent c)
                c.Parent = null;
        }
        #endregion
    }
}
