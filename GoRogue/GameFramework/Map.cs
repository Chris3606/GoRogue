using System.Collections.Generic;
using GoRogue.Pathing;
using GoRogue.MapViews;
using System.Linq;

namespace GoRogue.GameFramework
{
	public class Map
	{
		private ArrayMap<GameObject> _terrain;
		public IMapView<GameObject> Terrain => _terrain;

		private ArrayMap<bool> _explored;
		public IMapView<bool> Explored => _explored;

		// TODO: Rename to EntityLayers?
		private LayeredSpatialMap<GameObject> _entities;
		public IReadOnlyLayeredSpatialMap<GameObject> Entities => _entities.AsReadOnly();

		public LayerMasker LayerMasker => _entities.LayerMasker;

		private uint LayersBlockingWalkability { get; }
		private uint LayersBlockingTransparency { get; }

		public IMapView<bool> TransparencyView { get; }
		public IMapView<bool> WalkabilityView { get; }

		private FOV _fov;
		public IReadOnlyFOV FOV => _fov.AsReadOnly();

		public AStar AStar { get; }
		Distance DistanceMeasurement => AStar.DistanceMeasurement;

		public Map(int width, int height, int numberOfEntityLayers, Distance distanceMeasurement, uint layersBlockingWalkability = uint.MaxValue,
			       uint layersBlockingTransparency = uint.MaxValue, uint entityLayersSupportingMultipleItems = 0)
		{
			_terrain = new ArrayMap<GameObject>(width, height);
			_explored = new ArrayMap<bool>(width, height);

			_entities = new LayeredSpatialMap<GameObject>(numberOfEntityLayers, 1, entityLayersSupportingMultipleItems);

			LayersBlockingWalkability = layersBlockingWalkability;
			LayersBlockingTransparency = layersBlockingTransparency;

			if (layersBlockingTransparency == 1) // Only terrain so we optimize
				TransparencyView = new LambdaMapView<bool>(width, height, c => _terrain[c] == null || _terrain[c].IsTransparent);
			else
				TransparencyView = new LambdaMapView<bool>(width, height, FullIsTransparent);

			if (layersBlockingWalkability == 1) // Similar, only terrain blocks, so optimize
				WalkabilityView = new LambdaMapView<bool>(width, height, c => _terrain[c] == null || _terrain[c].IsWalkable);
			else
				WalkabilityView = new LambdaMapView<bool>(width, height, FullIsWalkable);

			_fov = new FOV(TransparencyView);
			AStar = new AStar(WalkabilityView, distanceMeasurement);
		}

		#region Terrain
		public GameObject GetTerrain(Coord position) => _terrain[position];

		public GameObject GetTerrain(int x, int y) => _terrain[x, y];

		public void SetTerrain(Coord position, GameObject terrain) => SetTerrain(position.X, position.Y, terrain);
		public void SetTerrain(int x, int y, GameObject terrain)
		{
			if (!terrain.IsStatic)
				throw new System.ArgumentException($"Terrain for Map must be marked static via its {nameof(GameObject.IsStatic)} flag.", nameof(terrain));
		}
		#endregion

		#region Entities
		public bool AddEntity(GameObject entity)
		{
			if (entity.CurrentMap == this)
				return false;


			if (!entity.IsWalkable && (!LayerMasker.HasLayer(LayersBlockingWalkability, entity.Layer) || !WalkabilityView[entity.Position]))
				return false;

			if (!entity.IsTransparent && !LayerMasker.HasLayer(LayersBlockingTransparency, entity.Layer))
				return false;			

			if (!_entities.Add(entity, entity.Position))
				return false;

			entity.CurrentMap?.RemoveEntity(entity);
			entity.Moved += OnEntityMoved;
			return true;
		}

		public bool RemoveEntity(GameObject entity)
		{
			if (!_entities.Remove(entity))
				return false;

			entity.Moved -= OnEntityMoved;
			return true;
		}
		#endregion

		#region GetObjects
		public GameObject GetObject(Coord position, uint layerMask = uint.MaxValue) => GetObjects(position.X, position.Y, layerMask).FirstOrDefault();
		public GameObject GetObject(int x, int y, uint layerMask = uint.MaxValue) => GetObjects(x, y, layerMask).FirstOrDefault();

		public IEnumerable<GameObject> GetObjects(Coord position, uint layerMask = uint.MaxValue) => GetObjects(position.X, position.Y, layerMask);
		public IEnumerable<GameObject> GetObjects(int x, int y, uint layerMask = uint.MaxValue)
		{
			foreach (var entity in _entities.GetItems(x, y, layerMask))
				yield return entity;

			if (LayerMasker.HasLayer(layerMask, 0) && _terrain[x, y] != null)
				yield return _terrain[x, y];
		}
		#endregion

		#region FOV
		public void CalculateFOV(Coord position, double radius = double.MaxValue) => CalculateFOV(position.X, position.Y, radius);
		public void CalculateFOV(int x, int y, double radius = double.MaxValue)
		{
			_fov.Calculate(x, y, radius);

			foreach (var pos in _fov.NewlySeen)
				_explored[pos] = true;
		}

		public void CalculateFOV(Coord position, double radius, Distance radiusShape) => CalculateFOV(position.X, position.Y, radius, radiusShape);
		public void CalculateFOV(int x, int y, double radius, Distance radiusShape)
		{
			_fov.Calculate(x, y, radius, radiusShape);

			foreach (var pos in _fov.NewlySeen)
				_explored[pos] = true;
		}

		public void CalculateFOV(Coord position, double radius, Distance radiusShape, double angle, double span)
			=> CalculateFOV(position.X, position.Y, radius, radiusShape, angle, span);

		public void CalculateFOV(int x, int y, double radius, Distance radiusShape, double angle, double span)
		{
			_fov.Calculate(x, y, radius, radiusShape, angle, span);

			foreach (var pos in _fov.NewlySeen)
				_explored[pos] = true;
		}
		#endregion

		private void OnEntityMoved(object s, ItemMovedEventArgs<GameObject> e)
		{
			_entities.Move(e.Item, e.NewPosition);
		}

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
