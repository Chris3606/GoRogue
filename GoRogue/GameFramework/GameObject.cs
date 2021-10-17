using System;
using GoRogue.Components;
using GoRogue.Random;
using GoRogue.SpatialMaps;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.GameFramework
{
    /// <summary>
    /// Concrete implementation of <see cref="IGameObject"/> that works for most use cases.
    /// </summary>
    /// <remarks>
    /// Any component collection implementing the proper interface may be passed in at construction and used as the
    /// <see cref="GoRogueComponents"/> property value, however by default a <see cref="ComponentCollection"/> is used,
    /// and thus will accept components of any type and will properly support <see cref="ISortedComponent"/>.
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
        /// <paramref name="idGenerator" /> is used to generate an ID which is assigned to the <see cref="ID"/>
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
        /// on can be traversed by other, non-walkable objects on the same <see cref="Map" />.  Effectively, whether or
        /// not this object collides.
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
        /// <param name="customComponentCollection">
        /// A custom component collection to use for objects.  If not specified, a <see cref="ComponentCollection"/> is
        /// used.  Typically you will not need to specify this, as a ComponentCollection is sufficient for nearly all
        /// use cases.
        /// </param>
        public GameObject(Point position, int layer, bool isWalkable = true, bool isTransparent = true,
                          Func<uint>? idGenerator = null, IComponentCollection? customComponentCollection = null)
            : this(layer, isWalkable, isTransparent, idGenerator, customComponentCollection)
        {
            Position = position;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// <paramref name="idGenerator" /> is used to generate an ID which is assigned to the <see cref="ID"/>
        /// field. When null is specified, the constructor simply assigns a random number in range of valid uints. This
        /// is sufficiently distinct for the purposes of placing the objects in an <see cref="ISpatialMap{T}" />
        /// implementation, however obviously does NOT guarantee true uniqueness. If uniqueness or some other
        /// implementation is required, override this function to return an appropriate ID. Keep in mind a relatively
        /// high degree of uniqueness is necessary for efficient placement in an ISpatialMap implementation.
        /// </remarks>
        /// <param name="layer">The layer of of a <see cref="Map" /> the object is assigned to.</param>
        /// <param name="isWalkable">
        /// Whether or not the object is to be considered "walkable", eg. whether or not the square it resides
        /// on can be traversed by other, non-walkable objects on the same <see cref="Map" />.  Effectively, whether or
        /// not this object collides.
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
        /// <param name="customComponentCollection">
        /// A custom component collection to use for objects.  If not specified, a <see cref="ComponentCollection"/> is
        /// used.  Typically you will not need to specify this, as a ComponentCollection is sufficient for nearly all
        /// use cases.
        /// </param>
        public GameObject(int layer, bool isWalkable = true, bool isTransparent = true,
                          Func<uint>? idGenerator = null, IComponentCollection? customComponentCollection = null)
        {
            idGenerator ??= GlobalRandom.DefaultRNG.NextUInt;

            Layer = layer;
            IsWalkable = isWalkable;
            IsTransparent = isTransparent;

            _currentMap = null;

            ID = idGenerator();
            GoRogueComponents = customComponentCollection ?? new ComponentCollection();
            GoRogueComponents.ParentForAddedComponents = this;
        }

        /// <inheritdoc />
        public Point Position
        {
            get => _position;
            set => this.SafelySetProperty(ref _position, value, Moved);
        }

        /// <inheritdoc />
        public event EventHandler<GameObjectPropertyChanged<Point>>? Moved;

        /// <inheritdoc />
        public bool IsWalkable
        {
            get => _isWalkable;
            set => this.SafelySetProperty(ref _isWalkable, value, WalkabilityChanging, WalkabilityChanged);
        }

        /// <inheritdoc />
        public event EventHandler<GameObjectPropertyChanged<bool>>? WalkabilityChanging;

        /// <inheritdoc />
        public event EventHandler<GameObjectPropertyChanged<bool>>? WalkabilityChanged;

        private bool _isTransparent;

        /// <inheritdoc />
        public bool IsTransparent
        {
            get => _isTransparent;
            set => this.SafelySetProperty(ref _isTransparent, value, TransparencyChanging, TransparencyChanged);
        }

        /// <inheritdoc />
        public event EventHandler<GameObjectPropertyChanged<bool>>? TransparencyChanging;

        /// <inheritdoc />
        public event EventHandler<GameObjectPropertyChanged<bool>>? TransparencyChanged;

        /// <inheritdoc />
        public uint ID { get; }

        /// <inheritdoc />
        public int Layer { get; }

        private Map? _currentMap;

        /// <inheritdoc />
        public Map? CurrentMap => _currentMap;

        /// <inheritdoc />
        public event EventHandler<GameObjectCurrentMapChanged>? AddedToMap;

        /// <inheritdoc />
        public event EventHandler<GameObjectCurrentMapChanged>? RemovedFromMap;

        /// <inheritdoc />
        public IComponentCollection GoRogueComponents { get; }

        /// <inheritdoc />
        public void OnMapChanged(Map? newMap)
            => this.SafelySetCurrentMap(ref _currentMap, newMap, AddedToMap, RemovedFromMap);
    }
}
