using System;
using GoRogue.GameFramework.Components;
using GoRogue.MapViews;
using SadRogue.Primitives;

namespace GoRogue.GameFramework
{
	/// <summary>
	/// Base class for any object that has a grid position and can be added to a <see cref="Map"/>.  Implements basic attributes generally common to all objects
	/// on a map, as well as properties/methods that the Map class needs to function.  It also implements <see cref="IHasComponents"/>, which means you can attach
	/// components to it.  In cases where this class cannot be inherited from, have your class implement <see cref="IGameObject"/> via a private GameObject field.
	/// </summary>
	/// <remarks>
	/// This class is designed to serve as a base class for your own game objects in your game.  It implements basic common functionality such as
	/// walkability and transparency, provides some infrastructure that allows it to be added to instances of <see cref="Map"/>, and implements the
	/// framework for GoRogue's component system.  It also implements the necessary functionality that allows GameObjects to be added to an
	/// <see cref="ISpatialMap{T}"/> implementation.
	/// 
	/// Generally, you would create one or more classes (say, MyGameObject or MyTerrain), that derives from this one (GameObject), or implement
	/// <see cref="IGameObject"/> by composition using a private field of this class, and use that as
	/// the base class for your game's objects.  There is an example of doing this <a href="https://chris3606.github.io/GoRogue/articles/game-framework.html#implementing-igameobject">here</a>.
	/// If you utilize the component system, a subclass of GameObject isn't strictly necessary, as you could just construct basic GameObject instances and add
	/// components to them.  In either case, a <see cref="Map"/> instance can be used to store these objects efficiently. As well, Map provides functionality that will allow you to retrieve your
	/// objects as references of their derived type (MyGameObject or MyTerrain, in the example above), meaning that you can implement any common, game-specific functionality you need and have
	/// easy access to that information when objects are retrieved from the map.  This also means that if you prefer an inheritance-based approach, you can simply not utilize
	/// the component system, and you are still able to use these Map functions to differentiate functionality.
	/// 
	/// Although the component system will accept items of any type as components, there is an optional <see cref="IGameObjectComponent"/> interface that GoRogue provides,
	/// that has a field for the parent, eg. the IGameObject that it is attached to.  This field is automatically kept up to date as you add/remove components
	/// if you implement this interface for your components.
	/// </remarks>
	public class GameObject : ComponentContainer, IGameObject
	{
		// Use the value of this variable instead of the "this" keyword from within GameObject
		private IGameObject _parentObject;

		private Point _position;
		/// <summary>
		/// The position of this object on the grid. Any time this value is changed, the <see cref="Moved"/> event is fired.
		/// </summary>
		/// <remarks>
		/// This property may be overriden to implement custom functionality, however it is highly recommended
		/// that you call the base set in the overridden setter, as it performs collision detection.
		/// </remarks>
		public virtual Point Position
		{
			get => _position;
			set
			{
				if (_position == value || IsStatic || (CurrentMap != null && !CurrentMap.Contains(value)))
					return;

				if (CurrentMap == null || IsWalkable || CurrentMap.WalkabilityView[value])
				{
					var oldPos = _position;

					if (CurrentMap != null && !CurrentMap.AttemptEntityMove(_parentObject, value))
						return; // The spatial map kicked it back, so invalidate the move.  Otherwise, proceed.

					_position = value;
					Moved?.Invoke(_parentObject, new ItemMovedEventArgs<IGameObject>(_parentObject, oldPos, _position));
				}
			}
		}

		/// <summary>
		/// Event fired whenever this object's grid position is successfully changed.  Fired regardless of whether
		/// the object is part of a <see cref="Map"/>.
		/// </summary>
		public event EventHandler<ItemMovedEventArgs<IGameObject>> Moved;

		private bool _isWalkable;

		/// <summary>
		/// Whether or not the object is to be considered "walkable", eg. whether or not the square it resides
		/// on can be traversed by other, non-walkable objects on the same <see cref="Map"/>.  Effectively, whether or not this
		/// object collides.
		/// </summary>
		public virtual bool IsWalkable
		{
			get => _isWalkable;

			set
			{
				if (_isWalkable != value)
				{
					// Would violate walkability
					if (!value && CurrentMap != null && !CurrentMap.WalkabilityView[Position])
						throw new ArgumentException("Cannot set walkability of object to false; this would violate walkability of the map the object resides on.", nameof(IsWalkable));

					_isWalkable = value;
				}
			}
		}

		/// <summary>
		/// Whether or not the object is considered "transparent", eg. whether or not light passes through it
		/// for the sake of calculating the FOV of a <see cref="Map"/>.
		/// </summary>
		public virtual bool IsTransparent { get; set; }

		/// <summary>
		/// Whether or not the object is "static".  Static objects CANNOT be moved, and only static objects may
		/// be placed on layer 0 of a <see cref="Map"/>.
		/// </summary>
		public bool IsStatic { get; }

		/// <summary>
		/// ID of the object.  Used for the sake of putting instances of this class in an <see cref="ISpatialMap{T}"/> implementation,
		/// and is NOT guaranteed to be entirely unique, though this can be modified by passing a custom generation function to the
		/// GameObject constructor.
		/// </summary>
		public uint ID { get; }

		/// <summary>
		/// Layer of a <see cref="Map"/> that this object will reside on.
		/// </summary>
		public int Layer { get; }

		/// <summary>
		/// The current <see cref="Map"/> which this object resides on.  Returns null if the object has not been added to a map.
		/// A GameObject is allowed to reside on only one map.
		/// </summary>
		public Map CurrentMap { get; private set; }

        /// <summary>
        /// Function used at construction to assign an ID to the object.
        /// </summary>
        /// <remarks>
        /// The default implementation simply assigns a random number in range of valid uints. This
        /// is sufficiently distinct for the purposes of placing the objects in an <see cref="ISpatialMap{T}"/>
        /// implementation, however obviously does NOT guarantee true uniqueness. If uniqueness or some other
        /// implementation is required, override this function to return an appropriate ID. Keep in
        /// mind a relatively high degree of uniqueness is necessary for efficient placement in an
        /// ISpatialMap implementation.
        ///
        /// This function is called from within the constructor 
        /// </remarks>
        /// <returns>An ID to assign to the current object.</returns>

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// The idGenerator function given is used to generate an ID which is assigned to the <see cref="ID"/> field.
        /// When null is specified, the constructor simply assigns a random number in range of valid uints. This
        /// is sufficiently distinct for the purposes of placing the objects in an <see cref="ISpatialMap{T}"/>
        /// implementation, however obviously does NOT guarantee true uniqueness. If uniqueness or some other
        /// implementation is required, override this function to return an appropriate ID. Keep in
        /// mind a relatively high degree of uniqueness is necessary for efficient placement in an
        /// ISpatialMap implementation.
        /// </remarks>
        /// <param name="position">Position to start the object at.</param>
        /// <param name="layer">The layer of of a <see cref="Map"/> the object is assigned to.</param>
        /// <param name="parentObject">Object holding this GameObject instance. If you are inheriting from GameObject,
        /// or using a backing field of type GameObject to implement <see cref="IGameObject"/>, for example, you would pass <see langword="this"/> at construction.
        /// Otherwise, if you are simply instantiating base GameObject instances and adding them to a <see cref="Map"/>, you would pass null.</param>
        /// <param name="isStatic">Whether or not the object can be moved (true if the object CANNOT be moved,
        /// false otherwise).</param>
        /// <param name="isWalkable">Whether or not the object is to be considered "walkable", eg. whether or not the square it resides
        /// on can be traversed by other, non-walkable objects on the same <see cref="Map"/>.  Effectively, whether or not this
        /// object collides.</param>
        /// <param name="isTransparent">Whether or not the object is considered "transparent", eg. whether or not light passes through it
        /// for the sake of calculating the FOV of a <see cref="Map"/>.</param>
        /// <param name="idGenerator">The function used to generate and return an unsigned integer to use assign to the <see cref="ID"/> field.
        /// Most of the time, you will not need to specify this as the default implementation will be sufficient.  See constructor remarks for details.</param>
        public GameObject(Point position, int layer, IGameObject parentObject, bool isStatic = false, bool isWalkable = true, bool isTransparent = true, Func<uint> idGenerator = null)
		{
            idGenerator = idGenerator ?? Random.SingletonRandom.DefaultRNG.NextUInt;

            _parentObject = parentObject ?? this;
			_position = position;
			Layer = layer;
			IsWalkable = isWalkable;
			IsTransparent = isTransparent;
			IsStatic = isStatic;

			CurrentMap = null;

			ID = idGenerator();
		}

		/// <summary>
		/// Attempts to move the object in the given direction, and returns true if the object was successfully
		/// moved, false otherwise.
		/// </summary>
		/// <param name="direction">The direction in which to try to move the object.</param>
		/// <returns>True if the object was successfully moved, false otherwise.</returns>
		public bool MoveIn(Direction direction)
		{
            Point oldPos = _position;
			Position += direction;

			return _position != oldPos;
		}

		/// <summary>
		/// Internal use only, do not call manually!  Must, at minimum, update the <see cref="CurrentMap"/> field of the
		/// GameObject to reflect the change.
		/// </summary>
		/// <param name="newMap">New map to which the GameObject has been added.</param>
		public void OnMapChanged(Map newMap)
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
			throw new Exception($"{name} for an object of type {nameof(GameObject)} was set incorrectly when it was constructed.  See API documentation of {nameof(GameObject)} for details on that constructor parameter.");
		}

		#region Component Functions
		/// <summary>
		/// Adds the given object as a component.  Throws an exception if that specific instance is already attached to this GameObject.
		/// </summary>
		/// <param name="component">Component to add.</param>
		public override void AddComponent(object component)
		{
			base.AddComponent(component);

			// If no exception was thrown, the above add succeeded.
			if (component is IGameObjectComponent c)
			{
				if (c.Parent != null)
					throw new ArgumentException($"Components implementing {nameof(IGameObjectComponent)} cannot be added to multiple objects at once.");

				c.Parent = _parentObject;
			}
		}

		/// <summary>
		/// Removes the given component(s) from the GameObject.  Throws an exception if a component given is not attached to
		/// the GameObject.
		/// </summary>
		/// <param name="components">One or more component instances to remove.</param>
		public override void RemoveComponents(params object[] components)
		{
			base.RemoveComponents(components);

			// If no exception was thrown, the above remove succeeded.
			foreach (var component in components)
			{
				if (component is IGameObjectComponent c)
					c.Parent = null;
			}
		}
		#endregion
	}
}
