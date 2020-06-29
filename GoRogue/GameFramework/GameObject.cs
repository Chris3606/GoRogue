using System;
using GoRogue.GameFramework.Components;
using GoRogue.MapViews;
using GoRogue.Random;
using GoRogue.SpatialMaps;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.GameFramework
{
    /// <summary>
    /// Base class for any object that has a grid position and can be added to a <see cref="Map" />.  Implements basic
    /// attributes generally common to all objects
    /// on a map, as well as properties/methods that the Map class needs to function.  It also implements
    /// <see cref="IHasComponents" />, which means you can attach
    /// components to it.  In cases where this class cannot be inherited from, have your class implement
    /// <see cref="IGameObject" /> via a private GameObject field.
    /// </summary>
    /// <remarks>
    /// This class is designed to serve as a base class for your own game objects in your game.  It implements basic common
    /// functionality such as
    /// walkability and transparency, provides some infrastructure that allows it to be added to instances of
    /// <see cref="Map" />, and implements the
    /// framework for GoRogue's component system.  It also implements the necessary functionality that allows GameObjects to be
    /// added to an
    /// <see cref="ISpatialMap{T}" /> implementation.
    /// Generally, you would create one or more classes (say, MyGameObject or MyTerrain), that derives from this one
    /// (GameObject), or implement
    /// <see cref="IGameObject" /> by composition using a private field of this class, and use that as
    /// the base class for your game's objects.  There is an example of doing this
    /// <a href="https://chris3606.github.io/GoRogue/articles/game-framework.html#implementing-igameobject">here</a>.
    /// If you utilize the component system, a subclass of GameObject isn't strictly necessary, as you could just construct
    /// basic GameObject instances and add
    /// components to them.  In either case, a <see cref="Map" /> instance can be used to store these objects efficiently. As
    /// well, Map provides functionality that will allow you to retrieve your
    /// objects as references of their derived type (MyGameObject or MyTerrain, in the example above), meaning that you can
    /// implement any common, game-specific functionality you need and have
    /// easy access to that information when objects are retrieved from the map.  This also means that if you prefer an
    /// inheritance-based approach, you can simply not utilize
    /// the component system, and you are still able to use these Map functions to differentiate functionality.
    /// Although the component system will accept items of any type as components, there is an optional
    /// <see cref="IGameObjectComponent" /> interface that GoRogue provides,
    /// that has a field for the parent, eg. the IGameObject that it is attached to.  This field is automatically kept up to
    /// date as you add/remove components
    /// if you implement this interface for your components.
    /// </remarks>
    [PublicAPI]
    public class GameObject : ComponentContainer, IGameObject
    {
        // Use the value of this variable instead of the "this" keyword from within GameObject
        private readonly IGameObject _parentObject;

        private bool _isWalkable;

        private Point _position;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// The idGenerator function given is used to generate an ID which is assigned to the <see cref="ID" /> field.
        /// When null is specified, the constructor simply assigns a random number in range of valid uints. This
        /// is sufficiently distinct for the purposes of placing the objects in an <see cref="ISpatialMap{T}" />
        /// implementation, however obviously does NOT guarantee true uniqueness. If uniqueness or some other
        /// implementation is required, override this function to return an appropriate ID. Keep in
        /// mind a relatively high degree of uniqueness is necessary for efficient placement in an
        /// ISpatialMap implementation.
        /// </remarks>
        /// <param name="position">Position to start the object at.</param>
        /// <param name="layer">The layer of of a <see cref="Map" /> the object is assigned to.</param>
        /// <param name="parentObject">
        /// Object holding this GameObject instance. If you are inheriting from GameObject,
        /// or using a backing field of type GameObject to implement <see cref="IGameObject" />, for example, you would pass
        /// <see langword="this" /> at construction.
        /// Otherwise, if you are simply instantiating base GameObject instances and adding them to a <see cref="Map" />, you would
        /// pass null.
        /// </param>
        /// <param name="isStatic">
        /// Whether or not the object can be moved (true if the object CANNOT be moved,
        /// false otherwise).
        /// </param>
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
        /// Most of the time, you will not need to specify this as the default implementation will be sufficient.  See constructor
        /// remarks for details.
        /// </param>
        public GameObject(Point position, int layer, IGameObject parentObject, bool isStatic = false,
                          bool isWalkable = true, bool isTransparent = true, Func<uint>? idGenerator = null)
        {
            idGenerator ??= GlobalRandom.DefaultRNG.NextUInt;

            _parentObject = parentObject ?? this;
            _position = position;
            Layer = layer;
            IsWalkable = isWalkable;
            IsTransparent = isTransparent;
            IsStatic = isStatic;

            CurrentMap = null;

            ID = idGenerator();
        }

        /// <inheritdoc />
        public virtual Point Position
        {
            get => _position;
            set
            {
                if (_position == value)
                    return; // This is OK as the position is already what was given so count it as "success".

                if (IsStatic)
                    throw new InvalidOperationException(
                        $"Tried to move a {GetType().Name} that was set as static.  Static objects cannot move.");

                if (CurrentMap != null && !CurrentMap.Contains(value))
                    throw new InvalidOperationException(
                        $"An entity's {nameof(CurrentMap)} is not synchronized with the map, as the {nameof(CurrentMap)} reports that it does not contain the entity." +
                        "This indicates either a GoRogue bug or a bug in an implementation of IGameObject.");

                if (!IsWalkable && CurrentMap != null && !CurrentMap.WalkabilityView[value])
                    throw new InvalidOperationException(
                        $"Tried to move {GetType().Name} to a square that it is not allowed to because it would collide with another non-walkable object.");

                var oldPos = _position;

                CurrentMap?.AttemptEntityMove(_parentObject, value);

                _position = value;
                Moved?.Invoke(_parentObject, new ItemMovedEventArgs<IGameObject>(_parentObject, oldPos, _position));
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
                if (_isWalkable == value) return;

                // Would violate walkability
                if (!value && CurrentMap != null && !CurrentMap.WalkabilityView[Position])
                    throw new ArgumentException(
                        "Cannot set walkability of object to false; this would violate walkability of the map the object resides on.",
                        nameof(IsWalkable));

                _isWalkable = value;
            }
        }

        /// <inheritdoc />
        public bool IsTransparent { get; set; }

        /// <inheritdoc />
        public bool IsStatic { get; }

        /// <inheritdoc />
        public uint ID { get; }

        /// <inheritdoc />
        public int Layer { get; }

        /// <inheritdoc />
        public Map? CurrentMap { get; private set; }

        /// <summary>
        /// Returns true if the GameObject can be moved to the location specified; false otherwise.
        /// </summary>
        /// <remarks>
        /// This function can return false in the following cases:
        /// 1. If the object has <see cref="IsStatic" /> set to true (in which case it cannot be moved)
        /// 2. If the object is added to the map and either:
        /// a. The position specified is not within the bounds of the map
        /// b. The object is not walkable and there is already a non-walkable item at the specified location
        /// c. The object's layer cannot support multiple items at one location and there is already an item at that location on
        /// that layer.
        /// </remarks>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the object can be moved to the specified position; false otherwise.</returns>
        public virtual bool CanMove(Point position)
        {
            if (IsStatic)
                return false;

            if (CurrentMap != null)
            {
                if (!CurrentMap.Contains(position) || !IsWalkable && !CurrentMap.WalkabilityView[position])
                    return false;

                return CurrentMap.EntityCanMove(_parentObject, position);
            }

            return true;
        }

        /// <summary>
        /// Returns true if the GameObject can move in the given direction; false otherwise.
        /// </summary>
        /// <remarks>
        /// See remarks in documentation for <see cref="CanMove(Point)" /> for details on when this function can return false.
        /// </remarks>
        /// <param name="direction">The direction of movement to check.</param>
        /// <returns>True if the object can be moved in the specified direction; false otherwise</returns>
        public bool CanMoveIn(Direction direction) => CanMove(Position + direction);

        /// <inheritdoc />
        public void OnMapChanged(Map? newMap)
        {
            if (newMap != null)
            {
                // Hack check to make sure the parentObject parameter was set properly.  The only case the add operation could have called
                // this function, but the object not be "present", is if reference equality of our parent doesn't match what is actually
                // in the map.  This can only happen if the parentObject isn't what it is supposed to be.
                if (Layer == 0) // It's terrain
                {
                    if (newMap.Terrain[Position] != _parentObject)
                        ThrowInvalidParentException();
                }
                else if (!newMap.Entities.Contains(_parentObject)) // It's an entity
                    ThrowInvalidParentException();
            }

            CurrentMap = newMap;
        }

        private void ThrowInvalidParentException()
        {
            var name = nameof(_parentObject).Replace("_", "");
            throw new Exception(
                $"{name} for an object of type {nameof(GameObject)} was set incorrectly when it was constructed.  See API documentation of {nameof(GameObject)} for details on that constructor parameter.");
        }

        #region Component Functions

        /// <inheritdoc />
        public override void AddComponent(object component, string? tag = null)
        {
            base.AddComponent(component, tag);

            // If no exception was thrown, the above add succeeded.
            if (!(component is IGameObjectComponent c)) return;

            if (c.Parent != null)
                throw new ArgumentException(
                    $"Components implementing {nameof(IGameObjectComponent)} cannot be added to multiple objects at once.");

            c.Parent = _parentObject;
        }

        /// <inheritdoc />
        public override void RemoveComponents(params object[] components)
        {
            base.RemoveComponents(components);

            // If no exception was thrown, the above remove succeeded.
            foreach (var component in components)
                if (component is IGameObjectComponent c)
                    c.Parent = null;
        }

        #endregion
    }
}
