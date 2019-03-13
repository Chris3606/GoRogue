using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoRogue
{
	/// <summary>
	/// A more complex version of <see cref="LayeredSpatialMap{T}"/> that does not require the items in it to implement
	/// <see cref="IHasID"/>, instead requiring the specification of a custom <see cref="IEqualityComparer{T}"/> to use
	/// for hashing and comparison of items.
	/// </summary>
	/// <remarks>
	/// This class is useful for cases where you do not want to implement <see cref="IHasID"/>. For simple cases, it is
	/// recommended to use <see cref="LayeredSpatialMap{T}"/> instead.
	/// 
	/// Be mindful of the efficiency of your hashing function specified in the <see cref="IEqualityComparer{T}"/> --
	/// it will in large part determine the performance of AdvancedLayeredSpatialMap!
	/// </remarks>
	/// <typeparam name="T">
	/// Type of items in the layers. Type T must implement <see cref="IHasLayer"/>, and its <see cref="IHasLayer.Layer"/> value
	/// MUST NOT change while the item is in the AdvancedLayeredSpatialMap.
	/// </typeparam>
	public class AdvancedLayeredSpatialMap<T> : ISpatialMap<T>, IReadOnlyLayeredSpatialMap<T> where T : IHasLayer
	{
		// Same as above but startingLayers less layers, for actual use, since we view our layers as
		// 0 - numberOfLayers - 1
		private LayerMasker _internalLayerMasker;

		private ISpatialMap<T>[] _layers;
		private HashSet<Coord> _positionCache; // Cached hash-set used for returning all positions in the LayeredSpatialMap

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="comparer">
		/// Equality comparer to use for comparison and hashing of type T. Be especially mindful of the
		/// efficiency of its GetHashCode function, as it will determine the efficiency of
		/// many AdvancedLayeredSpatialMap functions.
		/// </param>
		/// <param name="numberOfLayers">Number of layers to include.</param>
		/// <param name="startingLayer">Index to use for the first layer.</param>
		/// <param name="layersSupportingMultipleItems">
		/// A layer mask indicating which layers should support multiple items residing at the same
		/// location on that layer. Defaults to no layers.
		/// </param>
		public AdvancedLayeredSpatialMap(IEqualityComparer<T> comparer, int numberOfLayers, int startingLayer = 0, uint layersSupportingMultipleItems = 0)
		{
			if (numberOfLayers > 32 - startingLayer)
				throw new ArgumentOutOfRangeException(nameof(numberOfLayers), $"More than {32 - startingLayer} layers is not supported by {nameof(AdvancedLayeredSpatialMap<T>)} starting at layer {startingLayer}");

			_layers = new ISpatialMap<T>[numberOfLayers];
			StartingLayer = startingLayer;
			_positionCache = new HashSet<Coord>();

			LayerMasker = new LayerMasker(numberOfLayers + startingLayer);
			_internalLayerMasker = new LayerMasker(numberOfLayers);

			for (int i = 0; i < _layers.Length; i++)
				if (LayerMasker.HasLayer(layersSupportingMultipleItems, i + StartingLayer))
					_layers[i] = new AdvancedMultiSpatialMap<T>(comparer);
				else
					_layers[i] = new AdvancedSpatialMap<T>(comparer);

			foreach (var layer in _layers)
			{
				layer.ItemAdded += (_, e) => ItemAdded?.Invoke(this, e);
				layer.ItemRemoved += (_, e) => ItemRemoved?.Invoke(this, e);
				layer.ItemMoved += (_, e) => ItemMoved?.Invoke(this, e);
			}
		}

		/// <summary>
		/// See <see cref="ISpatialMap{T}.ItemAdded"/>.
		/// </summary>
		public event EventHandler<ItemEventArgs<T>> ItemAdded;

		/// <summary>
		/// See <see cref="ISpatialMap{T}.ItemMoved"/>.
		/// </summary>
		public event EventHandler<ItemMovedEventArgs<T>> ItemMoved;

		/// <summary>
		/// See <see cref="ISpatialMap{T}.ItemRemoved"/>.
		/// </summary>
		public event EventHandler<ItemEventArgs<T>> ItemRemoved;

		/// <summary>
		/// See <see cref="IReadOnlySpatialMap{T}.Count"/>
		/// </summary>
		public int Count => _layers.Sum(map => map.Count);

		/// <summary>
		/// See <see cref="IReadOnlySpatialMap{T}.Items"/>.
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
		/// Object used to get layer masks as they pertain to this spatial map.
		/// </summary>
		public LayerMasker LayerMasker { get; }

		/// <summary>
		/// Gets read-only spatial maps representing each layer. To access a specific layer, instead
		/// use <see cref="GetLayer(int)"/>.
		/// </summary>
		public IEnumerable<IReadOnlySpatialMap<T>> Layers => _layers;

		/// <summary>
		/// Gets the number of layers contained in the spatial map.
		/// </summary>
		public int NumberOfLayers => _layers.Length;

		/// <summary>
		/// Gets all positions that have items for each layer. No positions are duplicated if
		/// multiple layers have an item at a position.
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

		/// <summary>
		/// Starting index for layers contained in this spatial map.
		/// </summary>
		public int StartingLayer { get; }
		
		// TODO: Begin docs parse here!
		
		/// <summary>
		/// Adds the given item at the given position. Item is automatically added to correct layer.
		/// </summary>
		/// <param name="newItem">Item to add.</param>
		/// <param name="position">Position to add item at.</param>
		/// <returns>True if the item was successfully added -- false otherwise.</returns>
		public bool Add(T newItem, Coord position) => Add(newItem, position.X, position.Y);

		/// <summary>
		/// Adds the given item at the given position, or returns false if the item cannot be added.
		/// Item is automatically added to correct layer.
		/// </summary>
		/// <param name="newItem">Item to add.</param>
		/// <param name="x">X-value of position to add item at.</param>
		/// <param name="y">Y-value of position to add item at.</param>
		/// <returns>True if the item was successfully added, false otherwise.</returns>
		public bool Add(T newItem, int x, int y)
		{
			
			int relativeLayer = newItem.Layer - StartingLayer;

			if (relativeLayer < 0 || relativeLayer >= _layers.Length)
				return false;

			return _layers[relativeLayer].Add(newItem, x, y);
		}

		/// <summary>
		/// Returns a read-only reference to the data structure as an ISpatialMap.
		/// </summary>
		/// <returns>The current data structure, as a "read-only" reference.</returns>
		IReadOnlySpatialMap<T> IReadOnlySpatialMap<T>.AsReadOnly() => this;

		/// <summary>
		/// Returns a read-only reference to the data structure. Convenient for "safely" exposing the
		/// structure as a property.
		/// </summary>
		/// <returns>The current data structure, as a "read-only" reference.</returns>
		public IReadOnlyLayeredSpatialMap<T> AsReadOnly() => this;

		/// <summary>
		/// Clears all items from all layers.
		/// </summary>
		public void Clear()
		{
			foreach (var layer in _layers)
				layer.Clear();
		}

		/// <summary>
		/// Returns whether or not the data structure contains the given item.
		/// </summary>
		/// <param name="item">The item to check for.</param>
		/// <returns>True if the given item is in the data structure, false if not.</returns>
		public bool Contains(T item)
		{
			int relativeLayer = item.Layer - StartingLayer;
			if (relativeLayer < 0 || relativeLayer >= _layers.Length)
				return false;

			return _layers[relativeLayer].Contains(item);
		}

		/// <summary>
		/// Returns if there is an item in the data structure at the given position (on any layer) or not.
		/// </summary>
		/// <param name="position">The position to check for.</param>
		/// <returns>
		/// True if there is some item at the given position on some layer, false if not.
		/// </returns>
		bool IReadOnlySpatialMap<T>.Contains(Coord position) => Contains(position);

		/// <summary>
		/// Returns whether or not there is an item in the data structure at the given position that
		/// is on a layer included in the given layer mask. Defaults to searching on all layers.
		/// </summary>
		/// <param name="position">The position to check for.</param>
		/// <param name="layerMask">
		/// Layer mask that indicates which layers to check. Defaults to all layers.
		/// </param>
		/// <returns>
		/// True if there is some item at the given position on a layer included in the given layer
		/// mask, false if not.
		/// </returns>
		public bool Contains(Coord position, uint layerMask = uint.MaxValue) => Contains(position.X, position.Y, layerMask);

		/// <summary>
		/// Returns if there is an item in the data structure at the given position (on any layer) or not.
		/// </summary>
		/// <param name="x">X-value of the position to check for.</param>
		/// <param name="y">Y-value of the position to check for.</param>
		/// <returns>
		/// True if there is some item at the given position on some layer, false if not.
		/// </returns>
		bool IReadOnlySpatialMap<T>.Contains(int x, int y) => Contains(x, y);

		/// <summary>
		/// Returns whether or not there is an item in the data structure at the given position, that
		/// is on a layer included in the given layer mask.
		/// </summary>
		/// <param name="x">X-value of the position to check for.</param>
		/// <param name="y">Y-value of the position to check for.</param>
		/// <param name="layerMask">
		/// Layer mask that indicates which layers to check. Defaults to all layers.
		/// </param>
		/// <returns>
		/// True if there is some item at the given position on a layer included in the given layer
		/// mask, false if not.
		/// </returns>
		public bool Contains(int x, int y, uint layerMask = uint.MaxValue)
		{
			foreach (var relativeLayerNumber in _internalLayerMasker.Layers(layerMask >> StartingLayer))
				if (_layers[relativeLayerNumber].Contains(x, y))
					return true;

			return false;
		}

		/// <summary>
		/// Used by foreach loop, so that the class will give ISpatialTuple objects when used in a
		/// foreach loop. Generally should never be called explicitly.
		/// </summary>
		/// <returns>An enumerator for the SpatialMap</returns>
		public IEnumerator<ISpatialTuple<T>> GetEnumerator()
		{
			foreach (var layer in _layers)
				foreach (var tuple in layer)
					yield return tuple;
		}

		/// <summary>
		/// Generic iterator used internally by foreach loops.
		/// </summary>
		/// <returns>Enumerator to ISpatialTuple instances.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (var layer in _layers)
				foreach (var tuple in layer)
					yield return tuple;
		}

		/// <summary>
		/// Gets the item(s) associated with the given position (from all layers) if there are any
		/// items, or returns nothing if there is nothing at that position.
		/// </summary>
		/// <param name="position">The position to return the item(s) for.</param>
		/// <returns>
		/// The item(s) at the given position if there are any items, or nothing if there is nothing
		/// at that position.
		/// </returns>
		IEnumerable<T> IReadOnlySpatialMap<T>.GetItems(Coord position) => GetItems(position);

		/// <summary>
		/// Gets the item(s) associated with the given position that reside on any layer included in
		/// the given layer mask. Returns nothing if there is nothing at that position on a layer
		/// included in the given layer mask.
		/// </summary>
		/// <param name="position">The position to return the item(s) for.</param>
		/// <param name="layerMask">
		/// Layer mask that indicates which layers to check. Defaults to all layers.
		/// </param>
		/// <returns>
		/// The item(s) at the given position that reside on a layer included in the layer mask if
		/// there are any items, or nothing if there is nothing at that position.
		/// </returns>
		public IEnumerable<T> GetItems(Coord position, uint layerMask = uint.MaxValue) => GetItems(position.X, position.Y, layerMask);

		/// <summary>
		/// Gets the item(s) associated with the given position (from all layers) if there are any
		/// items, or returns nothing if there is nothing at that position.
		/// </summary>
		/// <param name="x">X-value of the position to return the item(s) for.</param>
		/// <param name="y">Y-value of the position to return the item(s) for.</param>
		/// <returns>
		/// The item(s) at the given position if there are any items, or nothing if there is nothing
		/// at that position.
		/// </returns>
		IEnumerable<T> IReadOnlySpatialMap<T>.GetItems(int x, int y) => GetItems(x, y);

		/// <summary>
		/// Gets the item(s) associated with the given position that reside on any layer included in
		/// the given layer mask. Returns nothing if there is nothing at that position on a layer
		/// included in the given layer mask.
		/// </summary>
		/// <param name="x">X-value of the position to return the item(s) for.</param>
		/// <param name="y">Y-value of the position to return the item(s) for.</param>
		/// <param name="layerMask">
		/// Layer mask that indicates which layers to check. Defaults to all layers.
		/// </param>
		/// <returns>
		/// The item(s) at the given position that reside on a layer included in the layer mask if
		/// there are any items, or nothing if there is nothing at that position.
		/// </returns>
		public IEnumerable<T> GetItems(int x, int y, uint layerMask = uint.MaxValue)
		{
			foreach (var relativeLayerNumber in _internalLayerMasker.Layers(layerMask >> StartingLayer))
				foreach (var item in _layers[relativeLayerNumber].GetItems(x, y))
					yield return item;
		}

		/// <summary>
		/// Gets a read-only spatial map representing the layer given.
		/// </summary>
		/// <param name="layer">The layer to retrieve.</param>
		/// <returns>The IReadOnlySpatialMap that represents the given layer.</returns>
		public IReadOnlySpatialMap<T> GetLayer(int layer) => _layers[layer - StartingLayer].AsReadOnly();

		/// <summary>
		/// Returns read-only spatial maps that represent each layer included in the given layer
		/// mask. Defaults to all layers.
		/// </summary>
		/// <param name="layerMask">
		/// Layer mask indicating which layers to return. Defaults to all layers.
		/// </param>
		/// <returns></returns>
		public IEnumerable<IReadOnlySpatialMap<T>> GetLayers(uint layerMask = uint.MaxValue)
		{
			foreach (var num in _internalLayerMasker.Layers(layerMask >> StartingLayer)) // LayerMasking will ignore layers that dont' actually exist
				yield return _layers[num - StartingLayer];
		}

		/// <summary>
		/// Gets the position associated with the item in the data structure, or null if that item is
		/// not found.
		/// </summary>
		/// <param name="item">The item to get the position for.</param>
		/// <returns>
		/// The position associated with the given item, if it exists in the data structure, or null
		/// if the item does not exist.
		/// </returns>
		public Coord GetPosition(T item)
		{
			int relativeLayer = item.Layer - StartingLayer;
			if (relativeLayer < 0 || relativeLayer >= _layers.Length)
				return Coord.NONE;

			return _layers[relativeLayer].GetPosition(item);
		}

		/// <summary>
		/// Moves the given item to the given position, or returns false if the item cannot be moved.
		/// </summary>
		/// <param name="item">Item to move.</param>
		/// <param name="target">Position to move the given item to.</param>
		/// <returns>True if the item was successfully moved, false otherwise.</returns>
		public bool Move(T item, Coord target) => Move(item, target.X, target.Y);

		/// <summary>
		/// Moves the given item to the given position, or returns false if the item cannot be moved.
		/// </summary>
		/// <param name="item">Item to move.</param>
		/// <param name="targetX">X-value of position to move the given item to.</param>
		/// <param name="targetY">Y-value of position to move the given item to.</param>
		/// <returns>True if the item was successfully moved, false otherwise.</returns>
		public bool Move(T item, int targetX, int targetY)
		{
			int relativeLayer = item.Layer - StartingLayer;
			
			if (relativeLayer < 0 || relativeLayer >= _layers.Length)
				return false;

			return _layers[relativeLayer].Move(item, targetX, targetY);
		}

		/// <summary>
		/// Moves all items on all layers at the given position to the new position.
		/// </summary>
		/// <param name="current">Position to move items from.</param>
		/// <param name="target">Position to move items to</param>
		/// <returns>All items moved.</returns>
		IEnumerable<T> ISpatialMap<T>.Move(Coord current, Coord target) => Move(current.X, current.Y, target.X, target.Y);

		/// <summary>
		/// Moves all items at the given position, that are on any layer specified by the given layer
		/// mask, to the new position. If no layer mask is specified, defaults to all layers.
		/// </summary>
		/// <param name="current">Position to move all items from.</param>
		/// <param name="target">Position to move all items to.</param>
		/// <param name="layerMask">
		/// Layer mask specifying which layers to search for items on. Defaults to all layers.
		/// </param>
		/// <returns>All items moved.</returns>
		public IEnumerable<T> Move(Coord current, Coord target, uint layerMask = uint.MaxValue) => Move(current.X, current.Y, target.X, target.Y, layerMask);

		/// <summary>
		/// Moves all items on all layers at the given position to the new position.
		/// </summary>
		/// <param name="currentX">X-value of the position to move items from.</param>
		/// <param name="currentY">Y-value of the position to move items from.</param>
		/// <param name="targetX">X-value of the position to move items to.</param>
		/// <param name="targetY">Y-value of the position to move itesm from.</param>
		/// <returns>All items moved.</returns>
		IEnumerable<T> ISpatialMap<T>.Move(int currentX, int currentY, int targetX, int targetY) => Move(currentX, currentY, targetX, targetY, uint.MaxValue);

		/// <summary>
		/// Moves all items at the given position, that are on any layer specified by the given layer
		/// mask, to the new position. If no layer mask is specified, defaults to all layers.
		/// </summary>
		/// <param name="currentX">X-value of the position to move items from.</param>
		/// <param name="currentY">Y-value of the position to move items from.</param>
		/// <param name="targetX">X-value of the position to move items to.</param>
		/// <param name="targetY">Y-value of the position to move itesm from.</param>
		/// <param name="layerMask">
		/// Layer mask specifying which layers to search for items on. Defaults to all layers.
		/// </param>
		/// <returns>All items moved.</returns>
		public IEnumerable<T> Move(int currentX, int currentY, int targetX, int targetY, uint layerMask = uint.MaxValue)
		{
			foreach (var relativeLayerNumber in _internalLayerMasker.Layers(layerMask >> StartingLayer))
				foreach (var itemMoved in _layers[relativeLayerNumber].Move(currentX, currentY, targetX, targetY))
					yield return itemMoved;
		}

		/// <summary>
		/// Removes the given item from the LayerdSpatialMap. Returns false if the item did not exist.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		/// <returns>True if the item was removed, false otherwise (eg. the item did not exist)</returns>
		public bool Remove(T item)
		{
			int relativeLayer = item.Layer - StartingLayer;
			if (relativeLayer < 0 || relativeLayer >= _layers.Length)
				return false;

			return _layers[relativeLayer].Remove(item);
		}

		/// <summary>
		/// Removes all items at the specified location on all layers from the data structure.
		/// Returns any items that were removed.
		/// </summary>
		/// <param name="position">Position to remove items from.</param>
		/// <returns>Any items that were removed, or nothing if no items were removed.</returns>
		IEnumerable<T> ISpatialMap<T>.Remove(Coord position) => Remove(position);

		/// <summary>
		/// Removes all items at the specified location that are on any layer included in the given
		/// layer mask from the data structure. Returns any items that were removed. Defaults to all layers.
		/// </summary>
		/// <param name="position">Position to remove items from.</param>
		/// <param name="layerMask">
		/// The layer mask indicating which layers to search for items. Defaults to all layers.
		/// </param>
		/// <returns>Any items that were removed, or nothing if no items were removed.</returns>
		public IEnumerable<T> Remove(Coord position, uint layerMask = uint.MaxValue) => Remove(position.X, position.Y, layerMask);

		/// <summary>
		/// Removes all items at the specified location on all layers from the data structure.
		/// Returns any items that were removed.
		/// </summary>
		/// <param name="x">X-value of the position to remove items from.</param>
		/// <param name="y">Y-value of the position to remove items from.</param>
		/// <returns>Any items that were removed, or nothing if no items were removed.</returns>
		IEnumerable<T> ISpatialMap<T>.Remove(int x, int y) => Remove(x, y);

		/// <summary>
		/// Removes all items at the specified location that are on any layer included in the given
		/// layer mask from the data structure. Returns any items that were removed. Defaults to all layers.
		/// </summary>
		/// <param name="x">X-value of the position to remove items from.</param>
		/// <param name="y">Y-value of the position to remove items from.</param>
		/// <param name="layerMask">
		/// The layer mask indicating which layers to search for items. Defaults to all layers.
		/// </param>
		/// <returns>Any items that were removed, or nothing if no items were removed.</returns>
		public IEnumerable<T> Remove(int x, int y, uint layerMask = uint.MaxValue)
		{
			foreach (var relativeLayerNumber in _internalLayerMasker.Layers(layerMask >> StartingLayer))
				foreach (var item in _layers[relativeLayerNumber].Remove(x, y))
					yield return item;
		}

		/// <summary>
		/// Returns a string representation of each layer in the LayeredSpatialMap.
		/// </summary>
		/// <returns>A string representing each layer of the LayeredSpatialMap</returns>
		public override string ToString() => ToString(t => t.ToString());

		/// <summary>
		/// Returns a string representation of each item in the LayeredSpatialMap, with elements
		/// displayed in the specified way.
		/// </summary>
		/// <param name="elementStringifier">A function that takes an element of type T and produces the string that should represent it in the output.</param>
		/// <returns>A string representing each layer in the LayeredSpatialMap, with each element displayed in the specified way.</returns>
		public string ToString(Func<T, string> elementStringifier)
		{
			int layer = StartingLayer;
			var sb = new StringBuilder();
			foreach (var map in _layers)
			{
				sb.Append(layer + ": ");
				sb.Append(map.ToString(elementStringifier) + '\n');
				layer++;
			}

			return sb.ToString();
		}
	}

	/// <summary>
	/// <see cref="ISpatialMap{T}"/> implementation that can be used to efficiently represent multiple
	/// "layers" of objects, with each layer represented as an <see cref="ISpatialMap{T}"/> instance.
	/// It provides the regular spatial map functionality, as well as adds layer masking functionality
	/// that allow functions to operate on specific layers only. 
	/// </summary>
	/// <remarks>
	/// See the <see cref="ISpatialMap{T}"/> for documentation on the practical purpose of spatial
	/// maps.
	/// 
	/// The objects stored in a LayeredSpatialMap must be reference types and implement both <see cref="IHasID"/>
	/// and <see cref="IHasLayer"/>.  Each object in a spatial map is presumed to have a "layer", which is assumed
	/// to remain constant once the item is added to the layer mask.
	/// </remarks>
	/// <typeparam name="T">
	/// Type of items stored in the layers. Type T must implement <see cref="IHasID"/> and <see cref="IHasLayer"/>,
	/// must be a reference type, and its <see cref="IHasLayer.Layer"/> value MUST NOT change while the item is in the
	/// LayeredSpatialMap.
	/// </typeparam>
	public class LayeredSpatialMap<T> : AdvancedLayeredSpatialMap<T> where T : class, IHasLayer, IHasID
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <remarks>
		/// This class allows you to specify the starting index for layers in order to make it easy to
		/// combine with other structures in a map which may represent other layers. For example, if a
		/// <paramref name="startingLayer"/> of 0 is specified, layers in the spatial map will have numbers
		/// in range[0, numberOfLayers - 1]. If 1 is specified, layers will have numbers in range [1-numberOfLayers],
		/// and anything to do with layer 0 will be ignored. For example, If a layer-mask that includes layers 0,
		/// 2, and 3 is passed to a function, only layers 2 and 3 are considered (since they are the only ones that would
		/// be included in the LayeredSpatialMap.
		/// </remarks>
		/// <param name="numberOfLayers">Number of layers to include.</param>
		/// <param name="startingLayer">Index to use for the first layer.</param>
		/// <param name="layersSupportingMultipleItems">
		/// A layer mask indicating which layers should support multiple items residing at the same
		/// location on that layer. Defaults to no layers.  Generate this layer mask via <see cref="LayerMasker.DEFAULT"/>.
		/// </param>
		public LayeredSpatialMap(int numberOfLayers, int startingLayer = 0, uint layersSupportingMultipleItems = 0)
			: base(new IDComparer<T>(), numberOfLayers, startingLayer, layersSupportingMultipleItems)
		{ }
	}
}