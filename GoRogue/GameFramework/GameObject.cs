using System;
using System.Collections.Generic;
using GoRogue.GameFramework.Components;

namespace GoRogue.GameFramework
{
	/// <summary>
	/// Base class for any object that has a grid position and can be added to a Map.  Implements basic attributes generally common to all objects
	/// on a map, as well as properties/methods that the Map class needs to function.  It also implements IHasComponents, which means you can attach
	/// components to it (see ComponentContainer documentation for details on those functions).  In cases where this class cannot be inherited from, have
	/// your class implement IGameObject via a private GameObject field.
	/// </summary>
	/// <remarks>
	/// This class is designed to serve as a base class for your own game objects in your game.  It implements basic common functionality such as
	/// walkability and transparency, and provides some infrastructure that allows it to be added to instances of Map, as well as implementing the
	/// framework for GoRogue's component system.  It also implements the necessary functionality that allows GameObjects to be added to an ISpatialMap
	/// implementation.
	/// 
	/// Generally, you would create one or more classes (say, MyGameObject or MyTerrain), that derives from this one (GameObject), or implement
	/// IGameObject by composition using a private field of this class, and use that as
	/// the base class for your game's objects.  If you utilize the component system, a subclass isn't even strictly necessary,
	/// as you could just construct basic GameObject instances and add components to them.  In either case, a Map instance can be used to store
	/// these objects efficiently. As well, Map provides functionality that will allow you to retrieve your objects as references of their derived
	/// type (MyGameObject or MyTerrain, in the example above), meaning that you can implement any common, game-specific functionality you need and have
	/// easy access to that information when objects are retrieved from the map (without being forced to make that functionality a component.
	/// 
	/// Although the component system will accept items of any type as components, there is an optional IGameObjectComponent class that GoRogue provides,
	/// that has a field for the parent.  This field is automatically kept up to date as you add/remove components if you use this class as the base for
	/// your components.
	/// </remarks>
	public class GameObject : IGameObject, IHasComponents
	{
		// Use the value of this variable instead of the "this" keyword from within GameObject
		private IGameObject _parentObject;

		private IHasComponents _backingComponentContainer;

		private Coord _position;
		/// <summary>
		/// The position of this object on the grid. Any time this value is changed, the Moved event is fired.
		/// </summary>
		/// <remarks>
		/// This property may be overriden to implement custom functionality, however it is highly recommended
		/// that you call the base set in the overridden setter, as it performs collision detection.
		/// </remarks>
		public virtual Coord Position
		{
			get => _position;
			set
			{
				if (_position == value || IsStatic)
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
		/// the object is part of a Map.
		/// </summary>
		public event EventHandler<ItemMovedEventArgs<IGameObject>> Moved;

		/// <summary>
		/// Whether or not the object is to be considered "walkable", eg. whether or not the square it resides
		/// on can be traversed by other, non-walkable objects on the same map.  Effectively, whether or not this
		/// object collides according to the collision layer.
		/// </summary>
		public virtual bool IsWalkable { get; set; }

		/// <summary>
		/// Whether or not the object is considered "transparent", eg. whether or not light passes through it
		/// for the sake of a Map's FOV.
		/// </summary>
		public virtual bool IsTransparent { get; set; }

		/// <summary>
		/// Whether or not the object is "static".  Static objects CANNOT be moved, and only static objects may
		/// be placed on a Map's layer 0.
		/// </summary>
		public bool IsStatic { get; }

		/// <summary>
		/// ID of the object.  Used for the sake of putting instances of this class in ISpatialMap implementations,
		/// and is NOT guaranteed to be entirely unique, though this can be modified by overriding the GenerateID
		/// function.
		/// </summary>
		public uint ID { get; }

		/// <summary>
		/// Layer of a Map that this object can reside on.
		/// </summary>
		public int Layer { get; }

		/// <summary>
		/// The current Map which this object resides on.  Null if the object has not been assigned an object.
		/// A GameObject is allowed to reside on only one map.
		/// </summary>
		public Map CurrentMap { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="position">Position to start the object at.</param>
		/// <param name="layer">The layer of of a Map the object is assigned to.</param>
		/// <param name="parentObject">Object holding this GameObject instance. If you are inheriting from GameObject,
		/// or using a backing field of type GameObject to implement IGameObject, for example, you would pass "this" at construction.
		/// Otherwise, if you are simply instantiating base GameObject instances and adding them to a Map, you would pass null.</param>
		/// <param name="isStatic">Whether or not the object can be moved (true if the object CANNOT be moved,
		/// false otherwise).</param>
		/// <param name="isWalkable">Whether or not the object is considered "transparent", eg. whether or not light passes through it
		/// for the sake of a Map's FOV.</param>
		/// <param name="isTransparent">Whether or not the object is considered "transparent", eg. whether or not light passes through it
		/// for the sake of a Map's FOV.</param>
		public GameObject(Coord position, int layer, IGameObject parentObject, bool isStatic = false, bool isWalkable = true, bool isTransparent = true)
		{
			_parentObject = parentObject ?? this;
			_backingComponentContainer = new ComponentContainer();
			_position = position;
			Layer = layer;
			IsWalkable = isWalkable;
			IsTransparent = isTransparent;
			IsStatic = isStatic;

			CurrentMap = null;

			ID = GenerateID();
		}

		/// <summary>
		/// Attempts to move the object in the given direction, and returns true if the objec was successfully
		/// moved, false otherwise.
		/// </summary>
		/// <param name="direction">The direction in which to try to move the object.</param>
		/// <returns>True if the object was successfully moved, false otherwise.</returns>
		public bool MoveIn(Direction direction)
		{
			Coord oldPos = _position;
			Position += direction;

			return _position != oldPos;
		}

		/// <summary>
		/// Function used at construction to assign an ID to the object.
		/// </summary>
		/// <remarks>
		/// The default implementation simply assigns a random number in range of valid uints. This
		/// is sufficiently distinct for the purposes of placing the objects in an ISpatialMap,
		/// however obviously does NOT guarantee true uniqueness. If uniqueness or some other
		/// implementation is required, override this function to return an appropriate ID. Bear in
		/// mind a relatively high degree of uniqueness is necessary for efficient placement in an
		/// ISpatialMap implementation.
		/// </remarks>
		/// <returns>An ID to assign to the current object.</returns>
		protected virtual uint GenerateID() => Random.SingletonRandom.DefaultRNG.NextUInt();

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
		public void AddComponent(object component)
		{
			_backingComponentContainer.AddComponent(component);

			// If no exception was thrown, the above add succeeded.
			if (component is GameObjectComponent c)
			{
				if (c.Parent != null)
					throw new ArgumentException($"Components of type {nameof(GameObjectComponent)} cannot be added to multiple components at once.");

				c.Parent = _parentObject;
			}
		}

		/// <summary>
		/// Gets the first component of type T that was attached to the GameObject, or default(T) if no component of that type has
		/// been attached.
		/// </summary>
		/// <typeparam name="T">Type of component to retrieve.</typeparam>
		/// <returns>The first component of Type T that was added to the GameObject, or default(T) if no
		/// components of the given type have been added.</returns>
		public T GetComponent<T>() => _backingComponentContainer.GetComponent<T>();

		/// <summary>
		/// Gets all components of type T that are attached to the GameObject.
		/// </summary>
		/// <typeparam name="T">Type of components to retrieve.</typeparam>
		/// <returns>All components of Type T that are attached to the GameObject.</returns>
		public IEnumerable<T> GetComponents<T>() => _backingComponentContainer.GetComponents<T>();

		/// <summary>
		/// Returns whether or not the GameObject has at least one component of the specified type.  Type may be specified
		/// by using typeof(MyComponentType).
		/// </summary>
		/// <param name="componentType">The type of component to check for.</param>
		/// <returns>True if the GameObject has at least one component of the specified type, false otherwise.</returns>
		public bool HasComponent(Type componentType) => _backingComponentContainer.HasComponent(componentType);

		/// <summary>
		/// Returns whether or not the GameObject has at least one component of type T.
		/// </summary>
		/// <typeparam name="T">Type of component to check for.</typeparam>
		/// <returns>True if the GameObject has at least one component of the specified type, false otherwise.</returns>
		public bool HasComponent<T>() => _backingComponentContainer.HasComponent<T>();

		/// <summary>
		/// Returns whether or not the GameObject has all the given types of components.  Types may be specified by
		/// using typeof(MyComponentType)
		/// </summary>
		/// <param name="componentTypes">One or more component types to check for.</param>
		/// <returns>True if the GameObject has at least one component of each specified type, false otherwise.</returns>
		public bool HasComponents(params Type[] componentTypes) => _backingComponentContainer.HasComponents(componentTypes);

		/// <summary>
		/// Removes the given component from the GameObject.  Throws an exception if the component is not attached to the GameObject.
		/// </summary>
		/// <param name="component">Component to remove.</param>
		public void RemoveComponent(object component) => RemoveComponents(component);

		/// <summary>
		/// Removes the given component(s) from the GameObject.  Throws an exception if a component given is not attached to
		/// the GameObject.
		/// </summary>
		/// <param name="components">One or more component instances to remove.</param>
		public void RemoveComponents(params object[] components)
		{
			_backingComponentContainer.RemoveComponents(components);

			foreach (var component in components)
			{
				// If no exception was thrown, the above remove succeeded.
				if (component is GameObjectComponent c)
					c.Parent = null;
			}
		}
		#endregion
	}
}
