using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GoRogue
{
	/// <summary>
	/// Contains static functions to help with constructing and interpreting layer-masking, since LayeredSpatialMap cannot implement standard enum layer-masking.
	/// </summary>
	static public class LayerMask
	{
		public const uint ALL = uint.MaxValue;
		public const uint NONE = 0;

		public static uint GetMask(params int[] layers) => GetMask((IEnumerable<int>)layers);

		public static uint GetMask(IEnumerable<int> layers)
		{
			uint mask = 0;

			foreach (var layer in layers)
				mask |= ((uint)1 << layer);

			return mask;
		}

		public static bool HasLayer(uint mask, int layer) => (mask & ((uint)1 << layer)) != 0;

		public static IEnumerable<int> LayersInMask(uint mask)
		{
			int layer = 0;
			while (mask != 0)
			{
				if ((mask & 1) != 0)
					yield return layer;

				mask >>= 1;
				layer++;
			}
		}
	}

	/// <summary>
	/// SpatialMap implementation that can be used to efficiently represent "layers" of objects, with each layer represented as a SpatialMap
	/// </summary>
	/// <remarks>
	/// This class is desinged to wrap a bunch of ISpatialMap instances together.  At creation, whether or not each layer supports
	/// multiple items at the same location is specified.  One spatial map represents one layer of a map (items, monsters, etc).  It provides
	/// read-only access to each layer, as well as functions to add/remove/move items, do item grabbing based on layers, etc.  Will not allow
	/// the same item to be added to multiple layers.
	/// </remarks>
	/// <typeparam name="T">Type of items in the layers.</typeparam>
	public class LayeredSpatialMap<T> : ISpatialMap<T>, IReadOnlyLayeredSpatialMap<T> where T : IHasID, IHasLayer
	{
		private ISpatialMap<T>[] _layers;
		private int _startingLayer;
		private HashSet<Coord> _positionCache;

		/// <summary>
		/// Gets read-only spatial maps representing each layer.  To access a specific layer, instead use GetLayer.
		/// </summary>
		public IEnumerable<IReadOnlySpatialMap<T>> Layers => _layers;

		//private bool[] _layersSupportMultipleItems;

		/// <summary>
		/// Fires whenever an item is added to any layer.
		/// </summary>
		public event EventHandler<ItemEventArgs<T>> ItemAdded;

		/// <summary>
		/// Fires whenever an item is moved on any layer.
		/// </summary>
		public event EventHandler<ItemMovedEventArgs<T>> ItemMoved;

		/// <summary>
		/// Fires whenever an item is removed from any layer.
		/// </summary>
		public event EventHandler<ItemEventArgs<T>> ItemRemoved;

		/// <summary>
		/// Gets the number of layers represented.
		/// </summary>
		public int NumberOfLayers => _layers.Length;

		/// <summary>
		/// Gets the number of entities on all layers.
		/// </summary>
		public int Count => _layers.Sum(map => map.Count);

		/// <summary>
		/// Gets all the items on all layers.
		/// </summary>
		public IEnumerable<T> Items
		{
			get
			{
				foreach (var layer in _layers)
					foreach (var item in layer.Items)
						yield return item;
			}
		}

		/// <summary>
		/// Gets all positions that have items for each layer.  No positions are duplicated if multiple layers have an item at a position.
		/// </summary>
		public IEnumerable<Coord> Positions
		{
			get
			{
				foreach (var layer in _layers)
					foreach (var pos in layer.Positions)
						if (_positionCache.Add(pos))
							yield return pos;

				_positionCache.Clear();
			}
		}
		
		public LayeredSpatialMap(int numberOfLayers, int startingLayer = 0, uint layersSupportingMultipleItems = LayerMask.NONE)
		{
			if (numberOfLayers > 32)
				throw new ArgumentOutOfRangeException(nameof(numberOfLayers), $"More than 32 layers is not supported by {nameof(LayeredSpatialMap<T>)}");

			_layers = new ISpatialMap<T>[numberOfLayers];
			_startingLayer = startingLayer;
			_positionCache = new HashSet<Coord>();

			for (int i = 0; i < _layers.Length; i++)
				if (LayerMask.HasLayer(layersSupportingMultipleItems, i + _startingLayer))
					_layers[i] = new MultiSpatialMap<T>();
				else
					_layers[i] = new SpatialMap<T>();

			foreach (var layer in _layers)
			{
				layer.ItemAdded += (_, e) => ItemAdded?.Invoke(this, e);
				layer.ItemRemoved += (_, e) => ItemRemoved?.Invoke(this, e);
				layer.ItemMoved += (_, e) => ItemMoved?.Invoke(this, e); 
			}
		}

		/// <summary>
		/// Gets a read-only spatial map representing the layer given.
		/// </summary>
		/// <param name="layer">The layer to retrieve.</param>
		/// <returns>The IReadOnlySpatialMap that represents the given layer.</returns>
		public IReadOnlySpatialMap<T> GetLayer(int layer) => _layers[layer - _startingLayer].AsReadOnly();

		public IEnumerable<IReadOnlySpatialMap<T>> GetLayers(uint mask)
		{
			foreach (var num in LayerMask.LayersInMask(mask))
				yield return _layers[num - _startingLayer];
		}

		/// <summary>
		/// Adds the given item at the given position.  Item is automatically added to correct layer.
		/// </summary>
		/// <param name="newItem">Item to add.</param>
		/// <param name="position">Position to add item at.</param>
		/// <returns></returns>
		public bool Add(T newItem, Coord position) => _layers[newItem.Layer - _startingLayer].Add(newItem, position);

		/// <summary>
		/// Adds the given item at the given position, or returns false if the item cannot be added.  Item is automatically added to correct layer.
		/// </summary>
		/// <param name="newItem">Item to add.</param>
		/// <param name="x">X-value of position to add item at.</param>
		/// <param name="y">Y-value of position to add item at.</param>
		/// <returns></returns>
		public bool Add(T newItem, int x, int y) => _layers[newItem.Layer - _startingLayer].Add(newItem, x, y);

		/// <summary>
		/// Clears all items from all layers.
		/// </summary>
		public void Clear()
		{
			foreach (var layer in _layers)
				layer.Clear();
		}

		/// <summary>
		/// Moves the given item to the given position, or returns false if the item cannot be moved.
		/// </summary>
		/// <param name="item">Item to move.</param>
		/// <param name="target">Position to move the given item to.</param>
		/// <returns>True if the item was successfully moved, false otherwise.</returns>
		public bool Move(T item, Coord target) => _layers[item.Layer - _startingLayer].Move(item, target);

		/// <summary>
		/// Moves the given item to the given position, or returns false if the item cannot be moved.
		/// </summary>
		/// <param name="item">Item to move.</param>
		/// <param name="targetX">X-value of position to move the given item to.</param>
		/// <param name="targetY">Y-value of position to move the given item to.</param>
		/// <returns></returns>
		public bool Move(T item, int targetX, int targetY) => _layers[item.Layer - _startingLayer].Move(item, targetX, targetY);

		/// <summary>
		/// Moves all items (on all layers) at the given position to the new position.  
		/// </summary>
		/// <param name="current"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public IEnumerable<T> Move(Coord current, Coord target) => Move(current.X, current.Y, target.X, target.Y);

		public IEnumerable<T> Move(int currentX, int currentY, int targetX, int targetY)
		{
			foreach (var layer in _layers)
				foreach (var itemMoved in layer.Move(currentX, currentY, targetX, targetY))
					yield return itemMoved;
		}

		public bool Remove(T item) => _layers[item.Layer - _startingLayer].Remove(item);

		public IEnumerable<T> Remove(Coord position) => Remove(position.X, position.Y);

		public IEnumerable<T> Remove(int x, int y)
		{
			foreach (var layer in _layers)
				foreach (var item in layer.Remove(x, y))
					yield return item;
		}

		IReadOnlySpatialMap<T> IReadOnlySpatialMap<T>.AsReadOnly() => this;

		public IReadOnlyLayeredSpatialMap<T> AsReadOnly() => this;

		public bool Contains(T item) => _layers[item.Layer - _startingLayer].Contains(item);

		public bool Contains(Coord position) => Contains(position.X, position.Y);

		public bool Contains(int x, int y)
		{
			foreach (var layer in _layers)
				if (layer.Contains(x, y))
					return true;

			return false;
		}

		public IEnumerable<T> GetItems(Coord position, int startingLayer) => GetItems(position.X, position.Y, startingLayer);

		// TODO: Going to start here for layer proofing, need to use layer-masking here
		public IEnumerable<T> GetItems(int x, int y, int startingLayer)
		{
			for (int i = Math.Min(_layers.Length - 1, startingLayer); i >= 0; i--)
				foreach (var item in _layers[i].GetItems(x, y))
					yield return item;
		}

		public Coord GetPosition(T item) => _layers[item.Layer].GetPosition(item);

		public IEnumerator<ISpatialTuple<T>> GetEnumerator()
		{
			foreach (var layer in _layers)
				foreach (var tuple in layer)
					yield return tuple;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (var layer in _layers)
				foreach (var tuple in layer)
					yield return tuple;
		}

		public IEnumerable<T> GetItems(Coord position) => GetItems(position.X, position.Y, _layers.Length - 1);

		public IEnumerable<T> GetItems(int x, int y) => GetItems(x, y, _layers.Length - 1);
	}
}
