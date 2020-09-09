using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;
using GoRogue.Pathing;
using GoRogue.SpatialMaps;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.GameFramework
{
    /// <summary>
    /// Base class for a map that consists of one or more objects of base type <see cref="IGameObject" />.  It
    /// implements basic functionality to manage and access these objects, as well as commonly needed functionality like
    /// tile exploration, FOV, and pathfinding.  It also provides methods to easily access these objects as instances of
    /// some derived type.  This can be used to easily access functionality you've implemented in a subclass.
    /// </summary>
    /// <remarks>
    /// A Map consists of <see cref="IGameObject" /> instances on one or more layers.  These layers are numbered, from
    /// the lowest layer of 0 upward.  Each Map contains at minimum a layer 0, which is considered the "terrain" layer.
    /// All objects added to this layer cannot move while they are added to a map; though they can move when they aren't
    /// a part of any map.
    ///
    /// A map will typically also have some other layers, for non-terrain objects like monsters, items, etc.  The number
    /// of these layers present on the map, along with which of all the layers participate in collision detection, etc.,
    /// can be specified in the constructor.
    ///
    /// While this class has some flexibility, it does, unlike the rest of the library, tend to impose itself on your
    /// architecture to some degree.  In cases where this is undesirable, each component of this map class exists as a
    /// separate component; layer masking, the SpatialMap(s) storing the entity layers, FOV, and pathfinding all exist
    /// as their own (more flexible) components).  This class is not intended to cover every possible use case, but
    /// may act as an example or starting point in the case where you would like to use the components in a different
    /// way or within a different architecture.
    /// </remarks>
    [PublicAPI]
    public class Map : MapViewBase<IEnumerable<IGameObject>>
    {
        private readonly LayeredSpatialMap<IGameObject> _entities;

        private readonly FOV _fov;
        private readonly ISettableMapView<IGameObject?> _terrain;

        /// <summary>
        /// Whether or not each tile is considered explored.  Tiles start off unexplored, and become explored as soon as they are
        /// within
        /// a calculated FOV.  This ArrayMap may also have values set to it, to easily allow for serialization or wizard-mode like
        /// functionality.
        /// </summary>
        public ArrayMap<bool> Explored;

        /// <summary>
        /// Constructor.  Constructs terrain map as <see cref="ArrayMap{IGameObject}" />; with the given width/height.
        /// </summary>
        /// <param name="width">Width of the map.</param>
        /// <param name="height">Height of the map.</param>
        /// <param name="numberOfEntityLayers">Number of non-terrain layers for the map.</param>
        /// <param name="distanceMeasurement">
        /// <see cref="Distance" /> measurement to use for pathfinding/measuring distance on the
        /// map.
        /// </param>
        /// <param name="layersBlockingWalkability">
        /// Layer mask containing those layers that should be allowed to have items that block walkability.
        /// Defaults to all layers.
        /// </param>
        /// <param name="layersBlockingTransparency">
        /// Layer mask containing those layers that should be allowed to have items that block FOV.
        /// Defaults to all layers.
        /// </param>
        /// <param name="entityLayersSupportingMultipleItems">
        /// Layer mask containing those layers that should be allowed to have multiple objects at the same
        /// location on the same layer.  Defaults to no layers.
        /// </param>
        public Map(int width, int height, int numberOfEntityLayers, Distance distanceMeasurement,
                   uint layersBlockingWalkability = uint.MaxValue,
                   uint layersBlockingTransparency = uint.MaxValue, uint entityLayersSupportingMultipleItems = 0)
            : this(new ArrayMap<IGameObject?>(width, height), numberOfEntityLayers, distanceMeasurement,
                layersBlockingWalkability,
                layersBlockingTransparency, entityLayersSupportingMultipleItems)
        { }

        /// <summary>
        /// Constructor.  Constructs map with the given terrain layer, determining width/height based on the width/height of that
        /// terrain layer.
        /// </summary>
        /// <remarks>
        /// Because of the way polymorphism works for custom classes in C#, the <paramref name="terrainLayer" /> parameter MUST be
        /// of type
        /// <see cref="ISettableMapView{IGameObject}" />, rather than <see cref="ISettableMapView{T}" /> where T is a type that
        /// derives from or implements
        /// <see cref="IGameObject" />.  If you need to use a map view storing type T rather than IGameObject, use the
        /// <see cref="CreateMap{T}(ISettableMapView{T}, int, Distance, uint, uint, uint)" /> function to create the map.
        /// </remarks>
        /// <param name="terrainLayer">
        /// The <see cref="ISettableMapView{IGameObject}" /> that represents the terrain layer for this map.  After the
        /// map has been created, you should use the <see cref="SetTerrain(IGameObject)" /> function to modify the values in this
        /// map view, rather
        /// than setting the values via the map view itself -- if you re-assign the value at a location via the map view, the
        /// <see cref="ObjectAdded" />/<see cref="ObjectRemoved" /> events are NOT guaranteed to be called, and many invariants of
        /// map may not be properly
        /// enforced.
        /// </param>
        /// <param name="numberOfEntityLayers">Number of non-terrain layers for the map.</param>
        /// <param name="distanceMeasurement">
        /// <see cref="Distance" /> measurement to use for pathfinding/measuring distance on the
        /// map.
        /// </param>
        /// <param name="layersBlockingWalkability">
        /// Layer mask containing those layers that should be allowed to have items that block walkability.
        /// Defaults to all layers.
        /// </param>
        /// <param name="layersBlockingTransparency">
        /// Layer mask containing those layers that should be allowed to have items that block FOV.
        /// Defaults to all layers.
        /// </param>
        /// <param name="entityLayersSupportingMultipleItems">
        /// Layer mask containing those layers that should be allowed to have multiple objects at the same
        /// location on the same layer.  Defaults to no layers.
        /// </param>
        public Map(ISettableMapView<IGameObject?> terrainLayer, int numberOfEntityLayers, Distance distanceMeasurement,
                   uint layersBlockingWalkability = uint.MaxValue,
                   uint layersBlockingTransparency = uint.MaxValue, uint entityLayersSupportingMultipleItems = 0)
        {
            _terrain = terrainLayer;
            Explored = new ArrayMap<bool>(_terrain.Width, _terrain.Height);

            _entities = new LayeredSpatialMap<IGameObject>(numberOfEntityLayers, 1,
                entityLayersSupportingMultipleItems);

            LayersBlockingWalkability = layersBlockingWalkability;
            LayersBlockingTransparency = layersBlockingTransparency;

            _entities.ItemAdded += (s, e) => ObjectAdded?.Invoke(this, e);
            _entities.ItemRemoved += (s, e) => ObjectRemoved?.Invoke(this, e);
            _entities.ItemMoved += (s, e) => ObjectMoved?.Invoke(this, e);

            TransparencyView = layersBlockingTransparency == 1
                ? new LambdaMapView<bool>(_terrain.Width, _terrain.Height, c => _terrain[c]?.IsTransparent ?? true)
                : new LambdaMapView<bool>(_terrain.Width, _terrain.Height, FullIsTransparent);

            WalkabilityView = layersBlockingWalkability == 1
                ? new LambdaMapView<bool>(_terrain.Width, _terrain.Height, c => _terrain[c]?.IsWalkable ?? true)
                : new LambdaMapView<bool>(_terrain.Width, _terrain.Height, FullIsWalkable);

            _fov = new FOV(TransparencyView);
            AStar = new AStar(WalkabilityView, distanceMeasurement);
        }

        /// <summary>
        /// Terrain of the map.  Terrain at each location may be set via the <see cref="SetTerrain(IGameObject)" /> function.
        /// </summary>
        public IMapView<IGameObject?> Terrain => _terrain;

        /// <summary>
        /// <see cref="IReadOnlyLayeredSpatialMap{IGameObject}" /> of all entities (non-terrain objects) on the map.
        /// </summary>
        public IReadOnlyLayeredSpatialMap<IGameObject> Entities => _entities.AsReadOnly();

        /// <summary>
        /// <see cref="LayerMasker" /> that should be used to create layer masks for this Map.
        /// </summary>
        public LayerMasker LayerMasker => _entities.LayerMasker;

        /// <summary>
        /// Layer mask that contains only layers that block walkability.  A non-walkable <see cref="IGameObject" /> can only be
        /// added to this
        /// map if the layer it resides on is contained within this layer mask.
        /// </summary>
        public uint LayersBlockingWalkability { get; }

        /// <summary>
        /// Layer mask that contains only layers that block transparency.  A non-transparent <see cref="IGameObject" /> can only be
        /// added to this
        /// map if the layer it is on is contained within this layer mask.
        /// </summary>
        public uint LayersBlockingTransparency { get; }

        /// <summary>
        /// <see cref="IMapView{Boolean}" /> representing transparency values for each tile.  Each location returns true if the
        /// location is transparent
        /// (there are no non-transparent objects at that location), and false otherwise.
        /// </summary>
        public IMapView<bool> TransparencyView { get; }

        /// <summary>
        /// <see cref="IMapView{Boolean}" /> representing walkability values for each tile.  Each location is true if the location
        /// is walkable (there are
        /// no non-walkable objects at that location), and false otherwise.
        /// </summary>
        public IMapView<bool> WalkabilityView { get; }

        /// <summary>
        /// Current FOV results for the map.  Calculate FOV via the Map's CalculateFOV functions.
        /// </summary>
        public IReadOnlyFOV FOV => _fov.AsReadOnly();

        /// <summary>
        /// A* pathfinder for the map.  By default, uses <see cref="WalkabilityView" /> to determine which locations can be
        /// reached,
        /// and calculates distance based on the <see cref="Distance" /> passed to the Map in the constructor.
        /// </summary>
        public AStar AStar { get; set; }

        /// <summary>
        /// <see cref="Distance" /> measurement used for pathfinding and measuring distance on the map.
        /// </summary>
        public Distance DistanceMeasurement => AStar.DistanceMeasurement;

        /// <summary>
        /// Height of the map, in grid spaces.
        /// </summary>
        public override int Height => _terrain.Height;

        /// <summary>
        /// Width of the map, in grid spaces.
        /// </summary>
        public override int Width => _terrain.Width;

        /// <summary>
        /// Gets all objects at the given location, from the highest layer (layer with the highest number) down.
        /// </summary>
        /// <param name="pos">The position to retrieve objects for.</param>
        /// <returns>All objects at the given location, in order from highest layer to lowest layer.</returns>
        public override IEnumerable<IGameObject> this[Point pos] => GetObjectsAt(pos);

        /// <summary>
        /// Event that is fired whenever some object is added to the map.
        /// </summary>
        public event EventHandler<ItemEventArgs<IGameObject>>? ObjectAdded;

        /// <summary>
        /// Event that is fired whenever some object is removed from the map.
        /// </summary>
        public event EventHandler<ItemEventArgs<IGameObject>>? ObjectRemoved;

        /// <summary>
        /// Event that is fired whenever some object that is part of the map is successfully moved.
        /// </summary>
        public event EventHandler<ItemMovedEventArgs<IGameObject>>? ObjectMoved;

        /// <summary>
        /// Effectively a helper-constructor.  Constructs a map using an <see cref="ISettableMapView{T}" /> for the terrain map,
        /// where type T can
        /// be any type that implements <see cref="IGameObject" />.  Note that a Map that is constructed using this function will
        /// throw an
        /// <see cref="InvalidCastException" /> if any IGameObject is given to <see cref="SetTerrain(IGameObject)" /> that cannot
        /// be cast to type T.
        /// </summary>
        /// <remarks>
        /// Suppose you have a class MyTerrain that inherits from BaseClass and implements <see cref="IGameObject" />.  This
        /// construction function allows
        /// you to construct your map using an <see cref="ISettableMapView{MyTerrain}" /> instance as the terrain map, which you
        /// cannot do with the regular
        /// constructor since <see cref="ISettableMapView{MyTerrain}" /> does not satisfy the constructor's type requirement of
        /// <see cref="ISettableMapView{IGameObject}" />.
        /// Since this function under the hood creates a <see cref="SettableTranslationMap{T, IGameObject}" /> that translates
        /// to/from IGameObject as needed,
        /// any change made using the map's <see cref="SetTerrain(IGameObject)" /> function will be reflected both in the map and
        /// in the original
        /// ISettableMapView.
        /// </remarks>
        /// <typeparam name="T">
        /// The type of terrain that will be stored in the created Map.  Can be any type that implements
        /// <see cref="IGameObject" />.
        /// </typeparam>
        /// <param name="terrainLayer">
        /// The <see cref="ISettableMapView{T}" /> that represents the terrain layer for this map.  After the
        /// map has been created, you should use the <see cref="SetTerrain(IGameObject)" /> function to modify the values in this
        /// map view, rather
        /// than setting the values via the map view itself -- if you re-assign the value at a location via the map view, the
        /// <see cref="ObjectAdded" />/<see cref="ObjectRemoved" /> events are NOT guaranteed to be called, and many invariants of
        /// map may not be properly
        /// enforced.
        /// </param>
        /// <param name="numberOfEntityLayers">Number of non-terrain layers for the map.</param>
        /// <param name="distanceMeasurement">
        /// <see cref="Distance" /> measurement to use for pathfinding/measuring distance on the
        /// map.
        /// </param>
        /// <param name="layersBlockingWalkability">
        /// Layer mask containing those layers that should be allowed to have items that block walkability.
        /// Defaults to all layers.
        /// </param>
        /// <param name="layersBlockingTransparency">
        /// Layer mask containing those layers that should be allowed to have items that block FOV.
        /// Defaults to all layers.
        /// </param>
        /// <param name="entityLayersSupportingMultipleItems">
        /// Layer mask containing those layers that should be allowed to have multiple objects at the same
        /// location on the same layer.  Defaults to no layers.
        /// </param>
        /// <returns>A new Map whose terrain is created using the given terrainLayer, and with the given parameters.</returns>
        public static Map CreateMap<T>(ISettableMapView<T?> terrainLayer, int numberOfEntityLayers,
                                       Distance distanceMeasurement, uint layersBlockingWalkability = uint.MaxValue,
                                       uint layersBlockingTransparency = uint.MaxValue,
                                       uint entityLayersSupportingMultipleItems = 0) where T : class, IGameObject
        {
            var terrainMap =
                new LambdaSettableTranslationMap<T?, IGameObject?>(terrainLayer, t => t,
                    g => (T?)g); // Assignment is fine here
            return new Map(terrainMap, numberOfEntityLayers, distanceMeasurement, layersBlockingWalkability,
                layersBlockingTransparency, entityLayersSupportingMultipleItems);
        }

        internal void AttemptEntityMove(IGameObject gameObject, Point newPosition)
            => _entities.Move(gameObject, newPosition);

        internal bool EntityCanMove(IGameObject gameObject, Point newPosition)
            => _entities.CanMove(gameObject, newPosition);

        private bool FullIsTransparent(Point position)
        {
            foreach (var item in GetObjectsAt(position, LayersBlockingTransparency))
                if (!item.IsTransparent)
                    return false;

            return true;
        }

        private bool FullIsWalkable(Point position)
        {
            foreach (var item in GetObjectsAt(position, LayersBlockingWalkability))
                if (!item.IsWalkable)
                    return false;

            return true;
        }

        #region Terrain

        /// <summary>
        /// Gets the terrain object at the given location, or null if no terrain is set to that location.
        /// </summary>
        /// <param name="position">The position to get the terrain for.</param>
        /// <returns>The terrain at the given position, or null if no terrain exists at that location.</returns>
        public IGameObject? GetTerrainAt(Point position) => _terrain[position];

        /// <summary>
        /// Gets the terrain object at the given location, as a value of type TerrainType.  Returns null if no terrain is set, or
        /// the terrain
        /// cannot be cast to the type specified.
        /// </summary>
        /// <typeparam name="TTerrain">Type to check for/return the terrain as.</typeparam>
        /// <param name="position">The position to get the terrain for.</param>
        /// <returns>
        /// The terrain at the given position, or null if either no terrain exists at that location or the terrain was not castable
        /// to type
        /// TerrainType.
        /// </returns>
        public TTerrain? GetTerrainAt<TTerrain>(Point position) where TTerrain : class, IGameObject
            => _terrain[position] as TTerrain;

        /// <summary>
        /// Gets the terrain object at the given location, or null if no terrain is set to that location.
        /// </summary>
        /// <param name="x">X-value of the position to get the terrain for.</param>
        /// <param name="y">Y-value of the position to get the terrain for.</param>
        /// <returns>The terrain at the given position, or null if no terrain exists at that location.</returns>
        public IGameObject? GetTerrainAt(int x, int y) => _terrain[x, y];

        /// <summary>
        /// Gets the terrain object at the given location, as a value of type TerrainType.  Returns null if no terrain is set, or
        /// the terrain
        /// cannot be cast to the type specified.
        /// </summary>
        /// <typeparam name="TTerrain">Type to return the terrain as.</typeparam>
        /// <param name="x">X-value of the position to get the terrain for.</param>
        /// <param name="y">Y-value of the position to get the terrain for.</param>
        /// <returns>
        /// The terrain at the given position, or null if either no terrain exists at that location or the terrain was
        /// not castable to <typeparamref name="TTerrain"/>.
        /// </returns>
        public TTerrain? GetTerrainAt<TTerrain>(int x, int y) where TTerrain : class, IGameObject
            => _terrain[x, y] as TTerrain;

        /// <summary>
        /// Sets the terrain at the given objects location to the given object, overwriting any terrain already present
        /// there.
        /// </summary>
        /// <remarks>
        /// A GameObject that is added as Terrain must have a Layer of 0, must not be part of a map currently, and
        /// must have a position within the bounds of the map.
        /// </remarks>
        /// <param name="terrain">
        /// Terrain to replace the current terrain with. <paramref name="terrain" /> must have its
        /// <see cref="IHasLayer.Layer" /> must be 0, or an exception will be thrown.
        /// </param>
        public void SetTerrain(IGameObject terrain)
        {
            if (terrain.Layer != 0)
                throw new ArgumentException("Terrain for Map must reside on layer 0.", nameof(terrain));

            if (terrain.CurrentMap != null)
                throw new ArgumentException($"Cannot add terrain to more than one {nameof(Map)}.", nameof(terrain));

            if (!this.Contains(terrain.Position))
                throw new ArgumentException("Terrain added to map must be within the bounds of that map.");

            if (!terrain.IsWalkable)
                foreach (var obj in Entities.GetItemsAt(terrain.Position))
                    if (!obj.IsWalkable)
                        throw new Exception(
                            "Tried to place non-walkable terrain at a location that already has another non-walkable item.");

            var oldTerrain = _terrain[terrain.Position];
            if (oldTerrain != null)
            {
                ObjectRemoved?.Invoke(this, new ItemEventArgs<IGameObject>(oldTerrain, oldTerrain.Position));
                oldTerrain.OnMapChanged(null);
            }

            _terrain[terrain.Position] = terrain;

            terrain.OnMapChanged(this);
            ObjectAdded?.Invoke(this, new ItemEventArgs<IGameObject>(terrain, terrain.Position));
        }

        /// <summary>
        /// Removes the given terrain object from the map.  If the given terrain object is not a part of the map,
        /// throws <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="terrain">Terrain to remove.</param>
        public void RemoveTerrain(IGameObject terrain)
        {
            if (terrain.CurrentMap != this)
                throw new ArgumentException("A terrain object was removed from the map that had not been added",
                    nameof(terrain));

            _terrain[terrain.Position] = null;

            ObjectRemoved?.Invoke(this, new ItemEventArgs<IGameObject>(terrain, terrain.Position));
            terrain.OnMapChanged(null);
        }

        /// <summary>
        /// Removes the terrain at the given location and returns it.  Throws <see cref="ArgumentException"/> if no
        /// terrain object has been added at that location.
        /// </summary>
        /// <param name="position">Position to remove terrain from.</param>
        /// <returns>Terrain that was at the position given.</returns>
        public IGameObject RemoveTerrainAt(Point position)
        {
            var terrain = _terrain[position];

            if (terrain == null)
                throw new ArgumentException("Cannot remove terrain at given location because no terrain at that location has been added",
                    nameof(position));

            ObjectRemoved?.Invoke(this, new ItemEventArgs<IGameObject>(terrain, terrain.Position));
            terrain.OnMapChanged(null);

            return terrain;
        }

        /// <summary>
        /// Removes the terrain at the given location and returns it.  Throws <see cref="ArgumentException"/> if no
        /// terrain object has been added at that location.
        /// </summary>
        /// <param name="x">X-value of the position to remove terrain from.</param>
        /// <param name="y">Y-value of the position to remove terrain from.</param>
        /// <returns>Terrain that was at the position given.</returns>
        public IGameObject RemoveTerrainAt(int x, int y) => RemoveTerrainAt(new Point(x, y));

        /// <summary>
        /// Sets all terrain on the map to the result of running the given translator on the value in
        /// <paramref name="overlay" /> at the corresponding position.  Useful, for example, for applying the map view
        /// resulting from map generation to a Map as terrain.
        /// </summary>
        /// <typeparam name="T">
        /// Type of values exposed by map view to translate.  Generally inferred by the compiler.
        /// </typeparam>
        /// <param name="overlay">Map view to translate.</param>
        /// <param name="translator">
        /// Function that translates values of the type that <paramref name="overlay" /> exposes to values
        /// of type IGameObject.
        /// </param>
        public void ApplyTerrainOverlay<T>(IMapView<T> overlay, Func<Point, T, IGameObject> translator)
        {
            var terrainOverlay = new LambdaTranslationMap<T, IGameObject>(overlay, translator);
            ApplyTerrainOverlay(terrainOverlay);
        }

        /// <summary>
        /// Sets all terrain on the current map to be equal to the corresponding values from the map view you pass in.
        /// All terrain will in the map view will be removed from its current Map, if any, and its position edited
        /// to what it is in the <paramref name="overlay"/>, before it is added to the map.
        /// </summary>
        /// <remarks>
        /// If translation between the overlay and IGameObject is required, see the overloads of this function that
        /// take a translation function.
        /// </remarks>
        /// <param name="overlay">
        /// Map view specifying the terrain apply to the map. Must have identical dimensions to the current map.
        /// </param>
        public void ApplyTerrainOverlay(IMapView<IGameObject> overlay)
        {
            if (Height != overlay.Height || Width != overlay.Width)
                throw new ArgumentException("Overlay size must match current map size.");

            foreach (var pos in _terrain.Positions())
            {
                var terrain = overlay[pos];
                if (terrain == null)
                    continue;

                terrain.CurrentMap?.RemoveTerrain(terrain);
                terrain.Position = pos;

                SetTerrain(terrain);
            }
        }

        #endregion

        #region Entities

        /// <summary>
        /// Adds the given entity (non-terrain object) to its recorded location, removing it from the map it is currently a part
        /// of.  Throws InvalidOperationException if the entity could not be added
        /// (eg., collision detection would not allow it, etc.)
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        public void AddEntity(IGameObject entity)
        {
            if (entity.CurrentMap == this)
                throw new InvalidOperationException(
                    $"Tried to add entity to a {GetType().Name} that was already a part of that map.");

            if (entity.Layer < 1)
                throw new InvalidOperationException(
                    $"Tried to add entity to a {GetType().Name} that had layer < 1.  Non-terrain items must have a layer >= 1.");

            if (!this.Contains(entity.Position))
                throw new InvalidOperationException(
                    $"Tried to add entity to a {GetType().Name}, but that entity's position was not within the map.");

            if (!entity.IsWalkable)
            {
                if (!LayerMasker.HasLayer(LayersBlockingWalkability, entity.Layer))
                    throw new InvalidOperationException(
                        $"Tried to add a non-walkable entity to a {GetType().Name}, but the entity's layer is not in the map's layer-mask of layers that can block walkability.");
                if (!WalkabilityView[entity.Position])
                    throw new InvalidOperationException(
                        $"Tried to add a non-walkable entity to a {GetType().Name}, but that map already has a non-walkable object at the entity's location.");
            }

            if (!entity.IsTransparent && !LayerMasker.HasLayer(LayersBlockingTransparency, entity.Layer))
                throw new InvalidOperationException(
                    $"Tried to add a non-transparent entity to a {GetType().Name}, but the entity's layer is not in the map's layer-mask of layers that can block transparency.");

            _entities.Add(entity, entity.Position);

            entity.CurrentMap?.RemoveEntity(entity);
            entity.OnMapChanged(this);
        }

        /// <summary>
        /// Gets the first (non-terrain) entity encountered at the given position that can be cast to the specified type, moving
        /// from the highest existing
        /// layer in the layer mask downward. Layer mask defaults to all layers. null is returned if no entities of the specified
        /// type are found, or if
        /// there are no entities at the location.
        /// </summary>
        /// <typeparam name="TEntity">Type of entities to return.</typeparam>
        /// <param name="position">Position to check get entity for.</param>
        /// <param name="layerMask">Layer mask for which layers can return an entity.  Defaults to all layers.</param>
        /// <returns>
        /// The first entity encountered, moving from the highest existing layer in the layer mask downward, or null if there are
        /// no entities of
        /// the specified type are found.
        /// </returns>
        public TEntity? GetEntityAt<TEntity>(Point position, uint layerMask = uint.MaxValue)
            where TEntity : class, IGameObject
            => GetEntitiesAt<TEntity>(position.X, position.Y, layerMask).FirstOrDefault();

        /// <summary>
        /// Gets the first (non-terrain) entity encountered at the given position that can be cast to the specified type, moving
        /// from the highest existing
        /// layer in the layer mask downward. Layer mask defaults to all layers. null is returned if no entities of the specified
        /// type are found, or if
        /// there are no entities at the location.
        /// </summary>
        /// <typeparam name="TEntity">Type of entities to return.</typeparam>
        /// <param name="x">X-value of the position to get entity for.</param>
        /// <param name="y">Y-value of the position to get entity for.</param>
        /// <param name="layerMask">Layer mask for which layers can return an entity.  Defaults to all layers.</param>
        /// <returns>
        /// The first entity encountered, moving from the highest existing layer in the layer mask downward, or null if there are
        /// no entities of
        /// the specified type are found.
        /// </returns>
        public TEntity? GetEntityAt<TEntity>(int x, int y, uint layerMask = uint.MaxValue)
            where TEntity : class, IGameObject
            => GetEntitiesAt<TEntity>(x, y, layerMask).FirstOrDefault();

        /// <summary>
        /// Gets all (non-terrain) entities encountered at the given position that are castable to type EntityType, in order from
        /// the highest existing layer
        /// in the layer mask downward.  Layer mask defaults to all layers.
        /// </summary>
        /// <typeparam name="TEntity">Type of entities to return.</typeparam>
        /// <param name="position">Position to get entities for.</param>
        /// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
        /// <returns>
        /// All entities encountered at the given position that are castable to the given type, in order from the highest existing
        /// layer
        /// in the mask downward.
        /// </returns>
        public IEnumerable<TEntity> GetEntitiesAt<TEntity>(Point position, uint layerMask = uint.MaxValue)
            where TEntity : IGameObject
            => GetEntitiesAt<TEntity>(position.X, position.Y, layerMask);

        /// <summary>
        /// Gets all (non-terrain) entities encountered at the given position that are castable to type EntityType, in order from
        /// the highest existing layer
        /// in the layer mask downward.  Layer mask defaults to all layers.
        /// </summary>
        /// <typeparam name="TEntity">Type of entities to return.</typeparam>
        /// <param name="x">X-value of the position to get entities for.</param>
        /// <param name="y">Y-value of the position to get entities for.</param>
        /// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
        /// <returns>
        /// All entities encountered at the given position that are castable to the given type, in order from the highest existing
        /// layer
        /// in the mask downward.
        /// </returns>
        public IEnumerable<TEntity> GetEntitiesAt<TEntity>(int x, int y, uint layerMask = uint.MaxValue)
            where TEntity : IGameObject
        {
            foreach (var entity in Entities.GetItemsAt(x, y, layerMask))
                if (entity is TEntity e)
                    yield return e;
        }

        /// <summary>
        /// Removes the given entity (non-terrain object) from the map.  Throws InvalidOperationException if the entity was not
        /// part of this map.
        /// </summary>
        /// <param name="entity">The entity to remove from the map.</param>
        public void RemoveEntity(IGameObject entity)
        {
            _entities.Remove(entity);
            entity.OnMapChanged(null);
        }

        #endregion

        #region GetObjects

        /// <summary>
        /// Gets the first object encountered at the given position, moving from the highest existing layer in the layer mask
        /// downward.  Layer mask defaults
        /// to all layers.
        /// </summary>
        /// <param name="position">Position to get object for.</param>
        /// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
        /// <returns>The first object encountered, moving from the highest existing layer in the layer mask downward.</returns>
        public IGameObject? GetObjectAt(Point position, uint layerMask = uint.MaxValue)
            => GetObjectsAt(position.X, position.Y, layerMask).FirstOrDefault();


        /// <summary>
        /// Gets the first object encountered at the given position that can be cast to the type specified, moving from the highest
        /// existing layer in the
        /// layer mask downward. Layer mask defaults to all layers.  null is returned if no objects of the specified type are
        /// found, or if there are no
        /// objects at the location.
        /// </summary>
        /// <typeparam name="TObject">Type of objects to return.</typeparam>
        /// <param name="position">Position to get object for.</param>
        /// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
        /// <returns>
        /// The first object encountered, moving from the highest existing layer in the layer mask downward, or null if there are
        /// no objects of
        /// the specified type are found.
        /// </returns>
        public TObject? GetObjectAt<TObject>(Point position, uint layerMask = uint.MaxValue)
            where TObject : class, IGameObject
            => GetObjectsAt<TObject>(position.X, position.Y, layerMask).FirstOrDefault();

        /// <summary>
        /// Gets the first object encountered at the given position that can be cast to the specified type, moving from the highest
        /// existing layer in the
        /// layer mask downward. Layer mask defaults to all layers. null is returned if no objects of the specified type are found,
        /// or if there are no
        /// objects at the location.
        /// </summary>
        /// <param name="x">X-value of the position to get object for.</param>
        /// <param name="y">Y-value of the position to get object for.</param>
        /// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
        /// <returns>The first object encountered, moving from the highest existing layer in the layer mask downward.</returns>
        public IGameObject? GetObjectAt(int x, int y, uint layerMask = uint.MaxValue)
            => GetObjectsAt(x, y, layerMask).FirstOrDefault();

        /// <summary>
        /// Gets the first object encountered at the given position that can be cast to the specified type, moving from the highest
        /// existing layer in the
        /// layer mask downward. Layer mask defaults to all layers. null is returned if no objects of the specified type are found,
        /// or if there are no
        /// objects at the location.
        /// </summary>
        /// <typeparam name="TObject">Type of objects to return.</typeparam>
        /// <param name="x">X-value of the position to get object for.</param>
        /// <param name="y">Y-value of the position to get object for.</param>
        /// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
        /// <returns>
        /// The first object encountered, moving from the highest existing layer in the layer mask downward, or null if there are
        /// no objects of
        /// the specified type are found.
        /// </returns>
        public TObject? GetObjectAt<TObject>(int x, int y, uint layerMask = uint.MaxValue)
            where TObject : class, IGameObject
            => GetObjectsAt<TObject>(x, y, layerMask).FirstOrDefault();

        /// <summary>
        /// Gets all objects encountered at the given position, in order from the highest existing layer in the layer mask
        /// downward.  Layer mask defaults
        /// to all layers.
        /// </summary>
        /// <param name="position">Position to get objects for.</param>
        /// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
        /// <returns>All objects encountered at the given position, in order from the highest existing layer in the mask downward.</returns>
        public IEnumerable<IGameObject> GetObjectsAt(Point position, uint layerMask = uint.MaxValue)
            => GetObjectsAt(position.X, position.Y, layerMask);

        /// <summary>
        /// Gets all objects encountered at the given position that are castable to type ObjectType, in order from the highest
        /// existing layer in the layer
        /// mask downward. Layer mask defaults to all layers.
        /// </summary>
        /// <typeparam name="TObject">Type of objects to return.</typeparam>
        /// <param name="position">Position to get objects for.</param>
        /// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
        /// <returns>
        /// All objects encountered at the given position that are castable to the given type, in order from the highest existing
        /// layer
        /// in the mask downward.
        /// </returns>
        public IEnumerable<TObject> GetObjectsAt<TObject>(Point position, uint layerMask = uint.MaxValue)
            where TObject : class, IGameObject
            => GetObjectsAt<TObject>(position.X, position.Y, layerMask);

        /// <summary>
        /// Gets all objects encountered at the given position, in order from the highest existing layer in the layer mask
        /// downward.  Layer mask defaults
        /// to all layers.
        /// </summary>
        /// <param name="x">X-value of the position to get objects for.</param>
        /// <param name="y">Y-value of the position to get objects for.</param>
        /// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
        /// <returns>All objects encountered at the given position, in order from the highest existing layer in the mask downward.</returns>
        public IEnumerable<IGameObject> GetObjectsAt(int x, int y, uint layerMask = uint.MaxValue)
        {
            foreach (var entity in _entities.GetItemsAt(x, y, layerMask))
                yield return entity;

            if (LayerMasker.HasLayer(layerMask, 0) && _terrain[x, y] != null)
                yield return _terrain[x, y]!; // Null-checked above
        }

        /// <summary>
        /// Gets all objects encountered at the given position that are castable to type ObjectType, in order from the highest
        /// existing layer in the layer
        /// mask downward. Layer mask defaults to all layers.
        /// </summary>
        /// <typeparam name="TObject">Type of objects to return.</typeparam>
        /// <param name="x">X-value of the position to get objects for.</param>
        /// <param name="y">Y-value of the position to get objects for.</param>
        /// <param name="layerMask">Layer mask for which layers can return an object.  Defaults to all layers.</param>
        /// <returns>
        /// All objects encountered at the given position that are castable to the given type, in order from the highest existing
        /// layer
        /// in the mask downward.
        /// </returns>
        public IEnumerable<TObject> GetObjectsAt<TObject>(int x, int y, uint layerMask = uint.MaxValue)
            where TObject : class, IGameObject
        {
            foreach (var entity in _entities.GetItemsAt(x, y, layerMask))
                if (entity is TObject e)
                    yield return e;

            if (LayerMasker.HasLayer(layerMask, 0) && _terrain[x, y] is TObject t)
                yield return t;
        }

        #endregion

        #region FOV

        /// <summary>
        /// Calculates FOV with the given center point and radius (of shape circle), and stores the result in the
        /// <see cref="FOV" /> property.  All tiles
        /// that are in the resulting FOV are marked as explored.  This function calls the virtual overload
        /// <see cref="CalculateFOV(int, int, double, Distance)" />, so if you need to override this functionality, override that
        /// overload instead.
        /// </summary>
        /// <param name="position">The center point of the new FOV to calculate.</param>
        /// <param name="radius">The radius of the FOV.  Defaults to infinite.</param>
        public void CalculateFOV(Point position, double radius = double.MaxValue)
            => CalculateFOV(position.X, position.Y, radius, Radius.Circle);

        /// <summary>
        /// Calculates FOV with the given center point and radius (of shape circle), and stores the result in the
        /// <see cref="FOV" /> property.  All tiles
        /// that are in the resulting FOV are marked as explored.  This function calls the virtual overload
        /// <see cref="CalculateFOV(int, int, double, Distance)" />, so if you need to override this functionality, override that
        /// overload instead.
        /// </summary>
        /// <param name="x">X-value of the center point for the new FOV to calculate.</param>
        /// <param name="y">Y-value of the center point for the new FOV to calculate.</param>
        /// <param name="radius">The radius of the FOV.  Defaults to infinite.</param>
        public void CalculateFOV(int x, int y, double radius = double.MaxValue)
            => CalculateFOV(x, y, radius, Radius.Circle);

        /// <summary>
        /// Calculates FOV with the given center point and radius, and stores the result in the <see cref="FOV" /> property.  All
        /// tiles that are in the
        /// resulting FOV are marked as explored.  This function calls the virtual overload
        /// <see cref="CalculateFOV(int, int, double, Distance)" />, so if you need to override this functionality, override that
        /// overload instead.
        /// </summary>
        /// <param name="position">The center point of the new FOV to calculate.</param>
        /// <param name="radius">The radius of the FOV.  Defaults to infinite.</param>
        /// <param name="radiusShape">
        /// The shape of the FOV to calculate.  Can be specified as either <see cref="Distance" /> or <see cref="Radius" /> types
        /// (they are implicitly convertible).
        /// </param>
        public void CalculateFOV(Point position, double radius, Distance radiusShape)
            => CalculateFOV(position.X, position.Y, radius, radiusShape);

        /// <summary>
        /// Calculates FOV with the given center point and radius, and stores the result in the <see cref="FOV" /> property.  All
        /// tiles that are in the
        /// resulting FOV are marked as explored.  Other non-angle-based overloads call this one, so if you need to override
        /// functionality, override
        /// this function.
        /// </summary>
        /// <param name="x">X-value of the center point for the new FOV to calculate.</param>
        /// <param name="y">Y-value of the center point for the new FOV to calculate.</param>
        /// <param name="radius">The radius of the FOV.  Defaults to infinite.</param>
        /// <param name="radiusShape">
        /// The shape of the FOV to calculate.  Can be specified as either <see cref="Distance" /> or <see cref="Radius" /> types
        /// (they are implicitly convertible).
        /// </param>
        public virtual void CalculateFOV(int x, int y, double radius, Distance radiusShape)
        {
            _fov.Calculate(x, y, radius, radiusShape);

            foreach (var pos in _fov.NewlySeen)
                Explored[pos] = true;
        }

        /// <summary>
        /// Calculates FOV with the given center point and radius, restricted to the given angle and span, and stores the result in
        /// the <see cref="FOV" />
        /// property. All tiles that are in the resulting FOV are marked as explored.  This function calls the virtual overload
        /// <see cref="CalculateFOV(int, int, double, Distance, double, double)" />, so if you need to override this functionality,
        /// override that
        /// overload instead.
        /// </summary>
        /// <param name="position">The center point of the new FOV to calculate.</param>
        /// <param name="radius">The radius of the FOV.  Defaults to infinite.</param>
        /// <param name="radiusShape">
        /// The shape of the FOV to calculate.  Can be specified as either <see cref="Distance" /> or <see cref="Radius" /> types
        /// (they are implicitly convertible).
        /// </param>
        /// <param name="angle">The angle in degrees the FOV cone faces.  0 degrees points right.</param>
        /// <param name="span">
        /// The angle in degrees specifying the full arc of the FOV cone.  span/2 degrees on either side of the given angle are
        /// included
        /// in the cone.
        /// </param>
        public void CalculateFOV(Point position, double radius, Distance radiusShape, double angle, double span)
            => CalculateFOV(position.X, position.Y, radius, radiusShape, angle, span);

        /// <summary>
        /// Calculates FOV with the given center point and radius, restricted to the given angle and span, and stores the result in
        /// the <see cref="FOV" />
        /// property.  All tiles that are in the resulting FOV are marked as explored.  Other angle-based overloads call this one,
        /// so if you need to
        /// override functionality, override this function.
        /// </summary>
        /// <param name="x">X-value of the center point for the new FOV to calculate.</param>
        /// <param name="y">Y-value of the center point for the new FOV to calculate.</param>
        /// <param name="radius">The radius of the FOV.  Defaults to infinite.</param>
        /// <param name="radiusShape">
        /// The shape of the FOV to calculate.  Can be specified as either <see cref="Distance" /> or <see cref="Radius" /> types
        /// (they are implicitly convertible).
        /// </param>
        /// <param name="angle">The angle in degrees the FOV cone faces.  0 degrees points right.</param>
        /// <param name="span">
        /// The angle in degrees specifying the full arc of the FOV cone.  span/2 degrees on either side of the given angle are
        /// included
        /// in the cone.
        /// </param>
        public virtual void CalculateFOV(int x, int y, double radius, Distance radiusShape, double angle, double span)
        {
            _fov.Calculate(x, y, radius, radiusShape, angle, span);

            foreach (var pos in _fov.NewlySeen)
                Explored[pos] = true;
        }

        #endregion
    }
}
