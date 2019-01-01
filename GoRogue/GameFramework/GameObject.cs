using System;

namespace GoRogue.GameFramework
{
	/// <summary>
	/// Base class for any object that has a grid position and can be added to a Map.  BaseSubclass must be the
	/// type that is deriving from this types, eg. class MyDerivingGameObject : GameObject&lt;MyDerivingGameObject&gt;
	/// </summary>
	/// <remarks>
	/// This class is designed to serve as a base class for your own game objects in your game.  It implements
	/// basic common functionality such as walkability and transparency, and provides some infrastructure
	/// that allows it to be added to instances of Map&lt;GameObject&lt;BaseSubclass&gt;&gt;.  It also implements
	/// the necessary functionality that allows GameObjects to be added to an ISpatialMap implementation.
	/// 
	/// It is intended that you create a class (say, MyGameObject), that derives from this one (GameObject&lt;MyGameObject&gt;),
	/// and use this as the base class for your game's objects.  This way, a Map&lt;MyGameObject&gt; class will return
	/// its objects as type MyGameObject, meaning you can implement any common, game-specific functionality you need 
	/// and have easy access to that information when objects are retrieved from the map.
	/// </remarks>
	/// <typeparam name="BaseSubclass">Type of the class that is deriving from this one.</typeparam>
	public class GameObject<BaseSubclass> : IHasID, IHasLayer where BaseSubclass : GameObject<BaseSubclass>
	{
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
					_position = value;
					Moved?.Invoke(this, new ItemMovedEventArgs<BaseSubclass>((BaseSubclass)this, oldPos, _position));
				}
			}
		}

		/// <summary>
		/// Event fired whenever this object's grid position is successfully changed.  Fired regardless of whether
		/// the object is part of a Map.
		/// </summary>
		public event EventHandler<ItemMovedEventArgs<BaseSubclass>> Moved;

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
		public Map<BaseSubclass> CurrentMap { get; internal set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="position">Position to start the object at.</param>
		/// <param name="layer">The layer of of a Map the object is assigned to.</param>
		/// <param name="isStatic">Whether or not the object can be moved (true if the object CANNOT be moved,
		/// false otherwise).</param>
		/// <param name="isWalkable">Whether or not the object is considered "transparent", eg. whether or not light passes through it
		/// for the sake of a Map's FOV.</param>
		/// <param name="isTransparent">Whether or not the object is considered "transparent", eg. whether or not light passes through it
		/// for the sake of a Map's FOV.</param>
		public GameObject(Coord position, int layer, bool isStatic = false, bool isWalkable = true, bool isTransparent = true)
		{
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
		/// The default implementation simply assigns a random number in range of valid uints.  This is sufficiently distinct for the purposes of placing
		/// the objects in an ISpatialMap, however obviously does NOT guarantee true uniqueness.  If uniqueness or some other implementation is required,
		/// override this function to return an appropriate ID.  Bear in mind a relatively high degree of uniqueness is necessary for efficient placement
		/// in an ISpatialMap implementation.
		/// </remarks>
		/// <returns>An ID to assign to the current object.</returns>
		protected virtual uint GenerateID() => Random.SingletonRandom.DefaultRNG.NextUInt();
	}
}
