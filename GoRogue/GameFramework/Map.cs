using System.Collections.Generic;
using GoRogue.Pathing;
using GoRogue.MapViews;
using System.Linq;
using System;

namespace GoRogue.GameFramework
{
	/// <summary>
	/// Base class for a map that consists of one or more objects of base type IGameObject.  It implements basic functionality to manage and access these objects, as well as
	/// commonly needed functinonality like tile exploration, FOV, and pathfinding.  It also provides methods to easily access these objects as instances of some derived
	/// type, that can be used to access functionality you've implemented in a subclass.
	/// </summary>
	/// <remarks>
	/// A Map consists of IGameObjects on one or more layers.  These layers are numbered, from the lowest layer of 0 upward.  Each Map contains at minimum a
	/// "terrain" layer.  This is considered to be layer 0.  All objects added to this layer must have their IsStatic flag set to true, and must reside
	/// on layer 0.
	/// 
	/// A map will typically also have some other layers, for non terrain objects like monsters, items, etc.  The number of these layers present
	/// on the map, along with which of all the layers participate in collision detection, etc., can be specified in the constructor.
	/// 
	/// While this class has some flexibility, it does, unlike the rest of the library, tend to impose itself on your architecture.  In cases where this is
	/// undesireable, each component of this map class exists as a separate component (layer masking, the SpatialMap(s) storing the entity layers, FOV, and pathfinding
	/// all exist as their own (more flexible) components).  This class is not intended to cover every possible use case, but instead may act as an example or starting
	/// point in the case where you would like to use the components in a different way or within a different architecture.
	/// </remarks>
	public class Map : IMapView<IEnumerable<IGameObject>>
	{
		private ISettableMapView<IGameObject> _terrain;
		/// <summary>
		/// Terrain of the map.  Terrain at each location may be set via the SetTerrain function.
		/// </summary>
		public IMapView<IGameObject> Terrain => _terrain;

		/// <summary>
		/// Whether or not each tile is considered explored.  Tiles start off unexplored, and become explored as soon as they are within
		/// a calculated FOV.  This ArrayMap may also have values set to it, to easily allow for serialization or wizard-mode like functionality.
		/// </summary>
		public ArrayMap<bool> Explored;

		private LayeredSpatialMap<IGameObject> _entities;
		/// <summary>
		/// IReadOnlyLayeredSpatialMap of all entities (non-terrain objects) on the map.
		/// </summary>
		public IReadOnlyLayeredSpatialMap<IGameObject> Entities => _entities.AsReadOnly();

		/// <summary>
		/// LayerMasker that should be used to create layer masks for this Map.
		/// </summary>
		public LayerMasker LayerMasker => _entities.LayerMasker;

		/// <summary>
		/// Layer mask that contains only layers that block walkability.  A non-walkable BaseObject can only be added to this map if the layer it is on is contained
		/// within this layer mask.
		/// </summary>
		public uint LayersBlockingWalkability { get; }
		/// <summary>
		/// Layer mask that contains only layers that block transparency.  A non-transparent BaseObject can only be added to this map if the layer it is on is contained
		/// within this layer mask.
		/// </summary>
		public uint LayersBlockingTransparency { get; }

		/// <summary>
		/// IMapView representing transparency values for each tile.  Each location is true if the location is transparent (there are no non-transparent objects
		/// at that location), false otherwise.
		/// </summary>
		public IMapView<bool> TransparencyView { get; }
		/// <summary>
		/// IMapView representing walkability values for each tile.  Each location is true if the location is walkable (there are no non-walkable objects
		/// at that location), false otherwise.
		/// </summary>
		public IMapView<bool> WalkabilityView { get; }

		private FOV _fov;
		/// <summary>
		/// Current FOV results for the map.  Calculate FOV via the Map's CalculateFOV functions.
		/// </summary>
		public IReadOnlyFOV FOV => _fov.AsReadOnly();

		/// <summary>
		/// AStar pathfinder for the map.  By default, uses WalkabilityView to determine which locations can be reached.
		/// </summary>
		public AStar AStar { get; set;  }

		/// <summary>
		/// Distance measurement used for pathing and measuring distance on the map.
		/// </summary>
		public Distance DistanceMeasurement => AStar.DistanceMeasurement;

		/// <summary>
		/// Event that is fired whenever some object is added to the map.
		/// </summary>
		public event EventHandler<ItemEventArgs<IGameObject>> ObjectAdded;
		/// <summary>
		/// Event that is fired whenever some object is removed from the map.
		/// </summary>
		public event EventHandler<ItemEventArgs<IGameObject>> ObjectRemoved;
		/// <summary>
		/// Event that is fired whenever some object that is part of the map is successfully moved.
		/// </summary>
		public event EventHandler<ItemMovedEventArgs<IGameObject>> ObjectMoved;

		/// <summary>
		/// Height of the map, in grid spaces.
		/// </summary>
		public int Height => _terrain.Height;

		/// <summary>
		/// Width of the map, in grid spaces.
		/// </summary>
		public int Width => _terrain.Width;

		public IEnumerable<IGameObject> this[int index1D] => GetObjects(Coord.ToCoord(index1D, Width));

		/// <summary>
		/// Gets all objects at the given location, from the highest layer (layer with the highest number) down.
		/// </summary>
		/// <param name="pos">The position to retrieve objects for.</param>
		/// <returns>All objects at the given location, in order from highest layer to lowest layer.</returns>
		public IEnumerable<IGameObject> this[Coord pos] => GetObjects(pos);

		/// <summary>
		/// Gets all objects at the given location, from the highest layer (layer with the highest number) down.
		/// </summary>
		/// <param name="x">X-value of the position to retrieve objects for.</param>
		/// <param name="y">Y-value of the position to retrieve objects for.</param>
		/// <returns>All objects at the given location, in order from highest layer to lowest layer.</returns>
		public IEnumerable<IGameObject> this[int x, int y] => GetObjects(x, y);

		/// <summary>
		/// Constructor.  Constructs terrain map as ArrayMap&lt;BaseObject&gt; with the given width/height.
		/// </summary>
		/// <param name="width">Width of the map.</param>
		/// <param name="height">Height of the map.</param>
		/// <param name="numberOfEntityLayers">Number of non-terrain layers for the map.</param>
		/// <param name="distanceMeasurement">Distance measurement to use for pathing/measuring distance on the map.</param>
		/// <param name="layersBlockingWalkability">Layer mask containing those layers that should be allowed to have items that block walkability.
		/// Defaults to all layers.</param>
		/// <param name="layersBlockingTransparency">Layer mask containing those layers that should be allowed to have items that block FOV.
		/// Defaults to all layers.</param>
		/// <param name="entityLayersSupportingMultipleItems">Layer mask containing those layers that should be allowed to have multiple objects at the same
		/// location on the same layer.  Defaults to no layers.</param>
		public Map(int width, int height, int numberOfEntityLayers, Distance distanceMeasurement, uint layersBlockingWalkability = uint.MaxValue,
				   uint layersBlockingTransparency = uint.MaxValue, uint entityLayersSupportingMultipleItems = 0)
			: this(new ArrayMap<IGameObject>(width, height), numberOfEntityLayers, distanceMeasurement, layersBlockingWalkability,
				  layersBlockingTransparency, entityLayersSupportingMultipleItems)
		{ }

		/// <summary>
		/// Constructor.  Constructs map with the given terrain layer, determining width/height based on the width/height of that terrain layer.
		/// </summary>
		/// <param name="terrainLayer">The ISettableMapView that represents the terrain layer for this map.  It is intended that this be modified
		/// ONLY via the SetTerrain function -- if it is modified outside of that, the ItemAdded/Removed events are NOT guaranteed to be called!</param>
		/// <param name="numberOfEntityLayers">Number of non-terrain layers for the map.</param>
		/// <param name="distanceMeasurement">Distance measurement to use for pathing/measuring distance on the map.</param>
		/// <param name="layersBlockingWalkability">Layer mask containing those layers that should be allowed to have items that block walkability.
		/// Defaults to all layers.</param>
		/// <param name="layersBlockingTransparency">Layer mask containing those layers that should be allowed to have items that block FOV.
		/// Defaults to all layers.</param>
		/// <param name="entityLayersSupportingMultipleItems">Layer mask containing those layers that should be allowed to have multiple objects at the same
		/// location on the same layer.  Defaults to no layers.</param>
		public Map(ISettableMapView<IGameObject> terrainLayer, int numberOfEntityLayers, Distance distanceMeasurement, uint layersBlockingWalkability = uint.MaxValue,
			       uint layersBlockingTransparency = uint.MaxValue, uint entityLayersSupportingMultipleItems = 0)
		{
			_terrain = terrainLayer;
			Explored = new ArrayMap<bool>(_terrain.Width, _terrain.Height);

			_entities = new LayeredSpatialMap<IGameObject>(numberOfEntityLayers, 1, entityLayersSupportingMultipleItems);

			LayersBlockingWalkability = layersBlockingWalkability;
			LayersBlockingTransparency = layersBlockingTransparency;

			_entities.ItemAdded += (s, e) => ObjectAdded?.Invoke(this, e);
			_entities.ItemRemoved += (s, e) => ObjectRemoved?.Invoke(this, e);
			_entities.ItemMoved += (s, e) => ObjectMoved?.Invoke(this, e);

			if (layersBlockingTransparency == 1) // Only terrain so we optimize
				TransparencyView = new LambdaMapView<bool>(_terrain.Width, _terrain.Height, c => _terrain[c] == null || _terrain[c].IsTransparent);
			else
				TransparencyView = new LambdaMapView<bool>(_terrain.Width, _terrain.Height, FullIsTransparent);

			if (layersBlockingWalkability == 1) // Similar, only terrain blocks, so optimize
				WalkabilityView = new LambdaMapView<bool>(_terrain.Width, _terrain.Height, c => _terrain[c] == null || _terrain[c].IsWalkable);
			else
				WalkabilityView = new LambdaMapView<bool>(_terrain.Width, _terrain.Height, FullIsWalkable);

			_fov = new FOV(TransparencyView);
			AStar = new AStar(WalkabilityView, distanceMeasurement);
		}

		#region Terrain
		/// <summary>
		/// Gets the terrain object at the given location, or null if no terrain is set to that location.
		/// </summary>
		/// <param name="position">The position to get the terrain for.</param>
		/// <returns>The terrain at the given postion, or null if no terrain exists at that location.</returns>
		public IGameObject GetTerrain(Coord position) => _terrain[position];

		/// <summary>
		/// Gets the terrain object at the given location, as a value of type TerrainType.  Returns null if no terrain is set, or the terrain
		/// cannot be casted to the type specified.
		/// </summary>
		/// <typeparam name="TerrainType">Type to return the terrain as.</typeparam>
		/// <param name="position">The position to get the terrain for.</param>
		/// <returns>The terrain at the given postion, or null if either no terrain exists at that location or the terrain was not castable to type TerrainType.</returns>
		public TerrainType GetTerrain<TerrainType>(Coord position) where TerrainType : class, IGameObject => _terrain[position] as TerrainType;

		/// <summary>
		/// Gets the terrain object at the given location, or null if no terrain is set to that location.
		/// </summary>
		/// <param name="x">X-value of the position to get the terrain for.</param>
		/// <param name="y">Y-value of the position to get the terrain for.</param>
		/// <returns>The terrain at the given postion, or null if no terrain exists at that location.</returns>
		public IGameObject GetTerrain(int x, int y) => _terrain[x, y];

		/// <summary>
		/// Gets the terrain object at the given location, as a value of type TerrainType.  Returns null if no terrain is set, or the terrain
		/// cannot be casted to the type specified.
		/// </summary>
		/// <typeparam name="TerrainType">Type to return the terrain as.</typeparam>
		/// <param name="x">X-value of the position to get the terrain for.</param>
		/// <param name="y">Y-value of the position to get the terrain for.</param>
		/// <returns>The terrain at the given postion, or null if either no terrain exists at that location or the terrain was not castable to type TerrainType.</returns>
		public TerrainType GetTerrain<TerrainType>(int x, int y) where TerrainType : class, IGameObject => _terrain[x, y] as TerrainType;

		/// <summary>
		/// Sets the terrain at the given objects location to the given object, overwriting any terrain already present there.
		/// </summary>
		/// <param name="terrain">Terrain to replace the current terrain with.</param>
		public void SetTerrain(IGameObject terrain)
		{
			if (terrain.Layer != 0)
				throw new ArgumentException($"Terrain for Map must reside on layer 0.", nameof(terrain));

			if (!terrain.IsStatic)
				throw new ArgumentException($"Terrain for Map must be marked static via its {nameof(IGameObject.IsStatic)} flag.", nameof(terrain));

			if (terrain.CurrentMap != null)
				throw new ArgumentException($"Cannot add terrain to more than one {nameof(Map)}.", nameof(terrain));

			if (!terrain.IsWalkable)
			{
				foreach (var obj in Entities.GetItems(terrain.Position))
					if (!obj.IsWalkable)
						throw new Exception("Tried to place non-walkable terrain at a location that already has another non-walkable item.");
			}

			if (_terrain[terrain.Position] != null)
				ObjectRemoved?.Invoke(this, new ItemEventArgs<IGameObject>(terrain, terrain.Position));

			_terrain[terrain.Position] = terrain;

			terrain.OnMapChanged(this);
			ObjectAdded?.Invoke(this, new ItemEventArgs<IGameObject>(terrain, terrain.Position));
		}
		#endregion

		#region Entities
		/// <summary>
		/// Adds the given entity (non-terrain object) to its recorded location, removing it from the map it is currently a part of.  Returns true if the entity was added, 
		/// false otherwise (eg., collision detection would not allow it, etc.)
		/// </summary>
		/// <param name="entity">Entity to add.</param>
		/// <returns>True if the entity was successfully added to the map, false otherwise.</returns>
		public bool AddEntity(IGameObject entity)
		{
			if (entity.CurrentMap == this)
				return false;

			if (entity.Layer < 1)
				return false;

			if (!entity.IsWalkable && (!LayerMasker.HasLayer(LayersBlockingWalkability, entity.Layer) || !WalkabilityView[entity.Position]))
				return false;

			if (!entity.IsTransparent && !LayerMasker.HasLayer(LayersBlockingTransparency, entity.Layer))
				return false;

			if (!_entities.Add(entity, entity.Position))
				return false;

			entity.CurrentMap?.RemoveEntity(entity);
			entity.OnMapChanged(this);
			return true;
		}

		
		public EntityType GetEntity<EntityType>(Coord position, uint layerMask = uint.MaxValue) where EntityType : IGameObject
			=> GetEntities<EntityType>(position.X, position.Y, layerMask).FirstOrDefault();

		/// <summary>
		/// Gets the first (non-terrain) entity encountered at the given position that can be casted to the specified type, moving from the highest existing
		/// layer in the layer mask downward. Layer mask defaults to all layers. Null is returned if no entities of the specified type are found, or if
		/// there are no entities at the location.
		/// </summary>
		/// <typeparam name="EntityType">Type of entities to return.</typeparam>
		/// <param name="x">X-value of the position to get entity for.</param>
		/// <param name="y">Y-value of the position to get entity for.</param>
		/// <param name="layerMask">Layer mask for which layers can return an entity.  Defaults to all layers.</param>
		/// <returns>The first entity encountered, moving from the highest existing layer in the layer mask downward, or null if there are no entities of
		/// the specified type are found.</returns>
		public EntityType GetEntity<EntityType>(int x, int y, uint layerMask = uint.MaxValue) where EntityType : IGameObject
			=> GetEntities<EntityType>(x, y, layerMask).FirstOrDefault();

		/// <summary>
		/// Gets all (non-terrain) entities encountered at the given position that are castable to type EntityType, in order from the highest existing layer
		/// in the layer mask downward.  Layer mask defaults to all layers.
		/// </summary>
		/// <typeparam name="EntityType">Type of entities to return.</typeparam>
		/// <param name="position">Position to get entities for.</param>
		/// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
		/// <returns>All entities encountered at the given position that are castable to the given type, in order from the highest existing layer
		/// in the mask downward.</returns>
		public IEnumerable<EntityType> GetEntities<EntityType>(Coord position, uint layerMask = uint.MaxValue) where EntityType : IGameObject
			=> GetEntities<EntityType>(position.X, position.Y, layerMask);

		/// <summary>
		/// Gets all (non-terrain) entities encountered at the given position that are castable to type EntityType, in order from the highest existing layer
		/// in the layer mask downward.  Layer mask defaults to all layers.
		/// </summary>
		/// <typeparam name="EntityType">Type of entities to return.</typeparam>
		/// <param name="x">X-value of the position to get entities for.</param>
		/// <param name="y">Y-value of the position to get entities for.</param>
		/// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
		/// <returns>All entities encountered at the given position that are castable to the given type, in order from the highest existing layer
		/// in the mask downward.</returns>
		public IEnumerable<EntityType> GetEntities<EntityType>(int x, int y, uint layerMask = uint.MaxValue) where EntityType : IGameObject
		{
			foreach (var entity in Entities.GetItems(x, y, layerMask))
				if (entity is EntityType e)
					yield return e;
		}

		/// <summary>
		/// Removes the given entity (non-terrain object) from the map, returning true if it was successfully removed, false otherwise.
		/// </summary>
		/// <param name="entity">The entity to remove from the map.</param>
		/// <returns>True if the entity was removed successfully, false otherwise (eg, the entity was not part of this map).</returns>
		public bool RemoveEntity(IGameObject entity)
		{
			if (!_entities.Remove(entity))
				return false;

			entity.OnMapChanged(null);
			return true;
		}
		#endregion

		#region GetObjects
		/// <summary>
		/// Gets the first object encountered at the given position, moving from the highest existing layer in the layer mask downward.  Layer mask defaults to all
		/// layers.
		/// </summary>
		/// <param name="position">Position to get object for.</param>
		/// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
		/// <returns>The first object encountered, moving from the highest existing layer in the layer mask downward.</returns>
		public IGameObject GetObject(Coord position, uint layerMask = uint.MaxValue) => GetObjects(position.X, position.Y, layerMask).FirstOrDefault();

		
		/// <summary>
		/// Gets the first object encountered at the given position that can be casted to the type specified, moving from the highest existing layer in the
		/// layer mask downward. Layer mask defaults to all layers.  Null is returned if no objects of the specified type are found, or if there are no objects at the
		/// location.
		/// </summary>
		/// <typeparam name="ObjectType">Type of objects to return.</typeparam>
		/// <param name="position">Position to get object for.</param>
		/// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
		/// <returns>The first object encountered, moving from the highest existing layer in the layer mask downward, or null if there are no objects of the specified
		/// type are found.</returns>
		public ObjectType GetObject<ObjectType>(Coord position, uint layerMask = uint.MaxValue) where ObjectType : class, IGameObject
			=> GetObjects<ObjectType>(position.X, position.Y, layerMask).FirstOrDefault();

		/// <summary>
		/// Gets the first object encountered at the given position that can be casted to the specified type, moving from the highest existing layer in the layer mask
		/// downward. Layer mask defaults to all layers. Null is returned if no objects of the specified type are found, or if there are no objects at the
		/// location.
		/// </summary>
		/// <param name="x">X-value of the position to get object for.</param>
		/// <param name="y">Y-value of the position to get object for.</param>
		/// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
		/// <returns>The first object encountered, moving from the highest existing layer in the layer mask downward.</returns>
		public IGameObject GetObject(int x, int y, uint layerMask = uint.MaxValue) => GetObjects(x, y, layerMask).FirstOrDefault();

		/// <summary>
		/// Gets the first object encountered at the given position that can be casted to the specified type, moving from the highest existing layer in the layer mask
		/// downward. Layer mask defaults to all layers. Null is returned if no objects of the specified type are found, or if there are no objects at the
		/// location.
		/// </summary>
		/// <typeparam name="ObjectType">Type of objects to return.</typeparam>
		/// <param name="x">X-value of the position to get object for.</param>
		/// <param name="y">Y-value of the position to get object for.</param>
		/// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
		/// <returns>The first object encountered, moving from the highest existing layer in the layer mask downward, or null if there are no objects of the specified
		/// type are found.</returns>
		public ObjectType GetObject<ObjectType>(int x, int y, uint layerMask = uint.MaxValue) where ObjectType : class, IGameObject
			=> GetObjects<ObjectType>(x, y, layerMask).FirstOrDefault();

		/// <summary>
		/// Gets all objects encountered at the given position, in order from the highest existing layer in the layer mask downward.  Layer mask defaults to all layers.
		/// </summary>
		/// <param name="position">Position to get objects for.</param>
		/// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
		/// <returns>All objects encountered at the given position, in order from the highest existing layer in the mask downward.</returns>
		public IEnumerable<IGameObject> GetObjects(Coord position, uint layerMask = uint.MaxValue) => GetObjects(position.X, position.Y, layerMask);

		/// <summary>
		/// Gets all objects encountered at the given position that are castable to type ObjectType, in order from the highest existing layer in the layer mask downward.
		/// Layer mask defaults to all layers.
		/// </summary>
		/// <typeparam name="ObjectType">Type of objects to return.</typeparam>
		/// <param name="position">Position to get objects for.</param>
		/// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
		/// <returns>All objects encountered at the given position that are castable to the given type, in order from the highest existing layer
		/// in the mask downward.</returns>
		public IEnumerable<ObjectType> GetObjects<ObjectType>(Coord position, uint layerMask = uint.MaxValue) where ObjectType : class, IGameObject
			=> GetObjects<ObjectType>(position.X, position.Y, layerMask);

		/// <summary>
		/// Gets all objects encountered at the given position, in order from the highest existing layer in the layer mask downward.  Layer mask defaults to all layers.
		/// </summary>
		/// <param name="x">X-value of the position to get objects for.</param>
		/// <param name="y">Y-value of the position to get objects for.</param>
		/// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
		/// <returns>All objects encountered at the given position, in order from the highest existing layer in the mask downward.</returns>
		public IEnumerable<IGameObject> GetObjects(int x, int y, uint layerMask = uint.MaxValue)
		{
			foreach (var entity in _entities.GetItems(x, y, layerMask))
				yield return entity;

			if (LayerMasker.HasLayer(layerMask, 0) && _terrain[x, y] != null)
				yield return _terrain[x, y];
		}

		/// <summary>
		/// Gets all objects encountered at the given position that are castable to type ObjectType, in order from the highest existing layer in the layer mask downward.
		/// Layer mask defaults to all layers.
		/// </summary>
		/// <typeparam name="ObjectType">Type of objects to return.</typeparam>
		/// <param name="x">X-value of the position to get objects for.</param>
		/// <param name="y">Y-value of the position to get objects for.</param>
		/// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
		/// <returns>All objects encountered at the given position that are castable to the given type, in order from the highest existing layer
		/// in the mask downward.</returns>
		public IEnumerable<ObjectType> GetObjects<ObjectType>(int x, int y, uint layerMask = uint.MaxValue) where ObjectType : class, IGameObject
		{
			foreach (var entity in _entities.GetItems(x, y, layerMask))
				if (entity is ObjectType e)
					yield return e;

			if (LayerMasker.HasLayer(layerMask, 0) && _terrain[x, y] is ObjectType t)
				yield return t;
		}
		#endregion

		#region FOV
		/// <summary>
		/// Calculates FOV with the given center point and radius (of shape circle), and stores the result in the FOV property.  All tiles that are in the resulting FOV
		/// are marked as explored.
		/// </summary>
		/// <param name="position">The center point of the new FOV to calculate.</param>
		/// <param name="radius">The radius of the FOV.  Defaults to infinite.</param>
		public void CalculateFOV(Coord position, double radius = double.MaxValue) => CalculateFOV(position.X, position.Y, radius);

		/// <summary>
		/// Calculates FOV with the given center point and radius (of shape circle), and stores the result in the FOV property.  All tiles that are in the resulting FOV
		/// are marked as explored.
		/// </summary>
		/// <param name="x">X-value of the center point for the new FOV to calculate.</param>
		/// <param name="y">Y-value of the center point for the new FOV to calculate.</param>
		/// <param name="radius">The radius of the FOV.  Defaults to infinite.</param>
		public void CalculateFOV(int x, int y, double radius = double.MaxValue)
		{
			_fov.Calculate(x, y, radius);

			foreach (var pos in _fov.NewlySeen)
				Explored[pos] = true;
		}

		/// <summary>
		/// Calculates FOV with the given center point and radius, and stores the result in the FOV property.  All tiles that are in the resulting FOV
		/// are marked as explored.
		/// </summary>
		/// <param name="position">The center point of the new FOV to calculate.</param>
		/// <param name="radius">The radius of the FOV.  Defaults to infinite.</param>
		/// <param name="radiusShape">The shape of the FOV to calculate.  Can be specified as either Distance or Radius types (they are implicitly convertible).</param>
		public void CalculateFOV(Coord position, double radius, Distance radiusShape) => CalculateFOV(position.X, position.Y, radius, radiusShape);

		/// <summary>
		/// Calculates FOV with the given center point and radius, and stores the result in the FOV property.  All tiles that are in the resulting FOV
		/// are marked as explored.
		/// </summary>
		/// <param name="x">X-value of the center point for the new FOV to calculate.</param>
		/// <param name="y">Y-value of the center point for the new FOV to calculate.</param>
		/// <param name="radius">The radius of the FOV.  Defaults to infinite.</param>
		/// <param name="radiusShape">The shape of the FOV to calculate.  Can be specified as either Distance or Radius types (they are implicitly convertible).</param>
		public void CalculateFOV(int x, int y, double radius, Distance radiusShape)
		{
			_fov.Calculate(x, y, radius, radiusShape);

			foreach (var pos in _fov.NewlySeen)
				Explored[pos] = true;
		}

		/// <summary>
		/// Calculates FOV with the given center point and radius, restricted to the given angle and span, and stores the result in the FOV property.  All tiles that
		/// are in the resulting FOV are marked as explored.
		/// </summary>
		/// <param name="position">The center point of the new FOV to calculate.</param>
		/// <param name="radius">The radius of the FOV.  Defaults to infinite.</param>
		/// <param name="radiusShape">The shape of the FOV to calculate.  Can be specified as either Distance or Radius types (they are implicitly convertible).</param>
		/// <param name="angle">The angle in degrees the FOV cone faces.  0 degrees points right.</param>
		/// <param name="span">The angle in degrees specifying the full arc of the FOV cone.  span/2 degrees on either side of the given angle are included
		/// in the cone.</param>
		public void CalculateFOV(Coord position, double radius, Distance radiusShape, double angle, double span)
			=> CalculateFOV(position.X, position.Y, radius, radiusShape, angle, span);

		/// <summary>
		/// Calculates FOV with the given center point and radius, restricted to the given angle and span, and stores the result in the FOV property.  All tiles that
		/// are in the resulting FOV are marked as explored.
		/// </summary>
		/// <param name="x">X-value of the center point for the new FOV to calculate.</param>
		/// <param name="y">Y-value of the center point for the new FOV to calculate.</param>
		/// <param name="radius">The radius of the FOV.  Defaults to infinite.</param>
		/// <param name="radiusShape">The shape of the FOV to calculate.  Can be specified as either Distance or Radius types (they are implicitly convertible).</param>
		/// <param name="angle">The angle in degrees the FOV cone faces.  0 degrees points right.</param>
		/// <param name="span">The angle in degrees specifying the full arc of the FOV cone.  span/2 degrees on either side of the given angle are included
		/// in the cone.</param>
		public void CalculateFOV(int x, int y, double radius, Distance radiusShape, double angle, double span)
		{
			_fov.Calculate(x, y, radius, radiusShape, angle, span);

			foreach (var pos in _fov.NewlySeen)
				Explored[pos] = true;
		}
		#endregion

		/// <summary>
		/// Effectively a helper-constructor.  Constructs a map using an ISettableView&lt;T&gt; for the terrain map that can have its type T
		/// be of any type that implements IGameObject.  Note that a Map that is constructed using this function will throw an InvalidCastException
		/// if any IGameObject is given to SetTerrain that cannot be casted to type T.
		/// </summary>
		/// <remarks>
		/// Say you have a class MyTerrain that inherits from BaseClass and implements IGameObject.  This construction function allows things
		/// like constructing your map from an ISettableView&lt;MyTerrain&gt;, which you cannot do with the regular constructor since
		/// ISettableMapView&lt;MyTerrain&gt; does not satisfy its type requirement of ISettableMapView&lt;IGameObject&gt;.
		/// 
		/// Since this function under the hood creates a SettableTranslationMap that translates to/from IGameObject, any change made either via the SetTerrain
		/// function or by modifying the original ISettableMapView passed in will be reflected both in the map and in the original ISettableMapView.
		/// </remarks>
		/// <typeparam name="T">The type of terrain that will be stored in the created Map.  Can be any type that implements IGameObject.</typeparam>
		/// <param name="terrainLayer">The ISettableMapView that represents the terrain layer for this map.  It is intended that this be modified
		/// ONLY via the SetTerrain function -- if it is modified outside of that, the ItemAdded/Removed events are NOT guaranteed to be called!</param>
		/// <param name="numberOfEntityLayers">Number of non-terrain layers for the map.</param>
		/// <param name="distanceMeasurement">Distance measurement to use for pathing/measuring distance on the map.</param>
		/// <param name="layersBlockingWalkability">Layer mask containing those layers that should be allowed to have items that block walkability.
		/// Defaults to all layers.</param>
		/// <param name="layersBlockingTransparency">Layer mask containing those layers that should be allowed to have items that block FOV.
		/// Defaults to all layers.</param>
		/// <param name="entityLayersSupportingMultipleItems">Layer mask containing those layers that should be allowed to have multiple objects at the same
		/// location on the same layer.  Defaults to no layers.</param>
		/// <returns>A new Map whose terrain is created using the given terrainLayer, and with the given parameters.</returns>
		public static Map CreateMap<T>(ISettableMapView<T> terrainLayer, int numberOfEntityLayers, Distance distanceMeasurement, uint layersBlockingWalkability = uint.MaxValue,
				   uint layersBlockingTransparency = uint.MaxValue, uint entityLayersSupportingMultipleItems = 0) where T : IGameObject
		{
			var terrainMap = new LambdaSettableTranslationMap<T, IGameObject>(terrainLayer, t => t, g => (T)g);
			return new Map(terrainMap, numberOfEntityLayers, distanceMeasurement, layersBlockingWalkability, layersBlockingTransparency, entityLayersSupportingMultipleItems);
		}

		internal bool AttemptEntityMove(IGameObject gameObject, Coord newPosition) => _entities.Move(gameObject, newPosition);

		private bool FullIsTransparent(Coord position)
		{
			foreach (var item in GetObjects(position, LayersBlockingTransparency))
				if (!item.IsTransparent)
					return false;

			return true;
		}

		private bool FullIsWalkable(Coord position)
		{
			foreach (var item in GetObjects(position, LayersBlockingWalkability))
				if (!item.IsWalkable)
					return false;

			return true;
		}
	}
}
