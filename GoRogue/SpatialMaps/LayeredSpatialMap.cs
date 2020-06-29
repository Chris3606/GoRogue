using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.SpatialMaps
{
    /// <summary>
    /// A more complex version of <see cref="LayeredSpatialMap{T}" /> that does not require the items in it to implement
    /// <see cref="IHasID" />, instead requiring the specification of a custom <see cref="IEqualityComparer{T}" /> to use
    /// for hashing and comparison of items.
    /// </summary>
    /// <remarks>
    /// This class is useful for cases where you do not want to implement <see cref="IHasID" />. For simple cases, it is
    /// recommended to use <see cref="LayeredSpatialMap{T}" /> instead.
    /// Be mindful of the efficiency of your hashing function specified in the <see cref="IEqualityComparer{T}" /> --
    /// it will in large part determine the performance of AdvancedLayeredSpatialMap!
    /// </remarks>
    /// <typeparam name="T">
    /// Type of items in the layers. Type T must implement <see cref="IHasLayer" />, and its <see cref="IHasLayer.Layer" />
    /// value
    /// MUST NOT change while the item is in the AdvancedLayeredSpatialMap.
    /// </typeparam>
    [PublicAPI]
    public class AdvancedLayeredSpatialMap<T> : ISpatialMap<T>, IReadOnlyLayeredSpatialMap<T> where T : IHasLayer
    {
        // Same as above but startingLayers less layers, for actual use, since we view our layers as
        // 0 -> numberOfLayers - 1
        private readonly LayerMasker _internalLayerMasker;

        private readonly ISpatialMap<T>[] _layers;

        private readonly HashSet<Point>
            _positionCache; // Cached hash-set used for returning all positions in the LayeredSpatialMap

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
        public AdvancedLayeredSpatialMap(IEqualityComparer<T> comparer, int numberOfLayers, int startingLayer = 0,
                                         uint layersSupportingMultipleItems = 0)
        {
            if (numberOfLayers > 32 - startingLayer)
                throw new ArgumentOutOfRangeException(nameof(numberOfLayers),
                    $"More than {32 - startingLayer} layers is not supported by {nameof(AdvancedLayeredSpatialMap<T>)} starting at layer {startingLayer}");

            _layers = new ISpatialMap<T>[numberOfLayers];
            StartingLayer = startingLayer;
            _positionCache = new HashSet<Point>();

            LayerMasker = new LayerMasker(numberOfLayers + startingLayer);
            _internalLayerMasker = new LayerMasker(numberOfLayers);

            for (var i = 0; i < _layers.Length; i++)
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

        /// <inheritdoc />
        public LayerMasker LayerMasker { get; }

        /// <inheritdoc />
        public IEnumerable<IReadOnlySpatialMap<T>> Layers => _layers;

        /// <inheritdoc />
        public int NumberOfLayers => _layers.Length;

        /// <inheritdoc />
        public int StartingLayer { get; }

        /// <inheritdoc />
        public IReadOnlyLayeredSpatialMap<T> AsReadOnly() => this;

        /// <inheritdoc />
        public bool Contains(Point position, uint layerMask = uint.MaxValue)
            => Contains(position.X, position.Y, layerMask);

        /// <inheritdoc />
        public bool Contains(int x, int y, uint layerMask = uint.MaxValue)
        {
            foreach (var relativeLayerNumber in _internalLayerMasker.Layers(layerMask >> StartingLayer))
                if (_layers[relativeLayerNumber].Contains(x, y))
                    return true;

            return false;
        }

        /// <inheritdoc />
        public IEnumerable<T> GetItemsAt(Point position, uint layerMask = uint.MaxValue)
            => GetItemsAt(position.X, position.Y, layerMask);

        /// <inheritdoc />
        public IEnumerable<T> GetItemsAt(int x, int y, uint layerMask = uint.MaxValue)
        {
            foreach (var relativeLayerNumber in _internalLayerMasker.Layers(layerMask >> StartingLayer))
            foreach (var item in _layers[relativeLayerNumber].GetItemsAt(x, y))
                yield return item;
        }

        /// <inheritdoc />
        public IReadOnlySpatialMap<T> GetLayer(int layer) => _layers[layer - StartingLayer].AsReadOnly();

        /// <inheritdoc />
        public IEnumerable<IReadOnlySpatialMap<T>> GetLayersInMask(uint layerMask = uint.MaxValue)
        {
            foreach (var num in _internalLayerMasker.Layers(layerMask >> StartingLayer)
            ) // LayerMasking will ignore layers that don't actually exist
                yield return _layers[num - StartingLayer];
        }

        /// <inheritdoc />
        public bool CanMoveAll(Point current, Point target, uint layerMask = uint.MaxValue)
            => CanMoveAll(current.X, current.Y, target.X, target.Y, layerMask);

        /// <inheritdoc />
        public bool CanMoveAll(int currentX, int currentY, int targetX, int targetY, uint layerMask = uint.MaxValue)
        {
            var hasItems = false;

            foreach (var relativeLayerNumber in _internalLayerMasker.Layers(layerMask >> StartingLayer))
                if (_layers[relativeLayerNumber].Contains(currentX, currentY))
                {
                    hasItems = true;
                    if (!_layers[relativeLayerNumber].CanMoveAll(currentX, currentY, targetX, targetY))
                        return false;
                }

            return hasItems;
        }

        /// <inheritdoc />
        public event EventHandler<ItemEventArgs<T>>? ItemAdded;

        /// <inheritdoc />
        public event EventHandler<ItemMovedEventArgs<T>>? ItemMoved;

        /// <inheritdoc />
        public event EventHandler<ItemEventArgs<T>>? ItemRemoved;

        /// <inheritdoc />
        public int Count => _layers.Sum(map => map.Count);

        /// <inheritdoc />
        public IEnumerable<T> Items
        {
            get
            {
                foreach (var layer in _layers)
                foreach (var item in layer.Items)
                    yield return item;
            }
        }

        /// <inheritdoc />
        public IEnumerable<Point> Positions
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
        /// Adds the given item at the given position on the correct layer.  InvalidOperationException is thrown if the layer is
        /// invalid or
        /// the item otherwise cannot be added to its layer.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="position">Position to add item at.</param>
        public void Add(T newItem, Point position)
        {
            var relativeLayer = newItem.Layer - StartingLayer;

            if (relativeLayer < 0 || relativeLayer >= _layers.Length)
                throw new InvalidOperationException(
                    $"Tried to add item to {GetType().Name} on layer {newItem.Layer}, but no such layer exists.");

            _layers[relativeLayer].Add(newItem, position);
        }

        /// <summary>
        /// Adds the given item at the given position on the correct layer.  InvalidOperationException is thrown if the layer is
        /// invalid or
        /// the item otherwise cannot be added to its layer.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="x">X-value of position to add item at.</param>
        /// <param name="y">Y-value of position to add item at.</param>
        public void Add(T newItem, int x, int y) => Add(newItem, new Point(x, y));

        /// <inheritdoc />
        IReadOnlySpatialMap<T> IReadOnlySpatialMap<T>.AsReadOnly() => this;

        /// <inheritdoc />
        public void Clear()
        {
            foreach (var layer in _layers)
                layer.Clear();
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            var relativeLayer = item.Layer - StartingLayer;
            if (relativeLayer < 0 || relativeLayer >= _layers.Length)
                return false;

            return _layers[relativeLayer].Contains(item);
        }

        /// <inheritdoc />
        bool IReadOnlySpatialMap<T>.Contains(Point position) => Contains(position);

        /// <inheritdoc />
        bool IReadOnlySpatialMap<T>.Contains(int x, int y) => Contains(x, y);

        /// <summary>
        /// Used by foreach loop, so that the class will give ISpatialTuple objects when used in a
        /// foreach loop. Generally should never be called explicitly.
        /// </summary>
        /// <returns>An enumerator for the spatial map</returns>
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

        /// <inheritdoc />
        IEnumerable<T> IReadOnlySpatialMap<T>.GetItemsAt(Point position) => GetItemsAt(position);

        /// <inheritdoc />
        IEnumerable<T> IReadOnlySpatialMap<T>.GetItemsAt(int x, int y) => GetItemsAt(x, y);

        /// <inheritdoc />
        public Point GetPositionOf(T item)
        {
            var relativeLayer = item.Layer - StartingLayer;
            if (relativeLayer < 0 || relativeLayer >= _layers.Length)
                return Point.None;

            return _layers[relativeLayer].GetPositionOf(item);
        }

        /// <summary>
        /// Moves the given item to the given position.  Throws InvalidOperationException if either the item given
        /// isn't in the spatial map, or if the layer that the item resides on is configured to allow only one item per
        /// location at any given time and there is already an item at <paramref name="target" />.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="target">Position to move the given item to.</param>
        public void Move(T item, Point target)
        {
            var relativeLayer = item.Layer - StartingLayer;

            if (relativeLayer < 0 || relativeLayer >= _layers.Length)
                throw new InvalidOperationException(
                    $"Tried to move item in {GetType().Name} on layer {item.Layer}, but no such layer exists.");

            _layers[relativeLayer].Move(item, target);
        }

        /// <summary>
        /// Moves the given item to the given position.  Throws InvalidOperationException if either the item given
        /// isn't in the spatial map, or if the layer that the item resides on is configured to allow only one item per
        /// location at any given time and there is already an item at the target position.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="targetX">X-value of position to move the given item to.</param>
        /// <param name="targetY">Y-value of position to move the given item to.</param>
        public void Move(T item, int targetX, int targetY) => Move(item, new Point(targetX, targetY));

        /// <inheritdoc />
        IEnumerable<T> ISpatialMap<T>.MoveValid(Point current, Point target)
            => MoveValid(current.X, current.Y, target.X, target.Y);

        /// <inheritdoc />
        IEnumerable<T> ISpatialMap<T>.MoveValid(int currentX, int currentY, int targetX, int targetY)
            => MoveValid(currentX, currentY, targetX, targetY);

        /// <inheritdoc />
        public void Remove(T item)
        {
            var relativeLayer = item.Layer - StartingLayer;
            if (relativeLayer < 0 || relativeLayer >= _layers.Length)
                throw new InvalidOperationException(
                    $"Tried to remove item from {GetType().Name} on layer {item.Layer}, but no such layer exists.");

            _layers[relativeLayer].Remove(item);
        }

        /// <inheritdoc />
        IEnumerable<T> ISpatialMap<T>.Remove(Point position) => Remove(position);

        /// <inheritdoc />
        IEnumerable<T> ISpatialMap<T>.Remove(int x, int y) => Remove(x, y);

        /// <summary>
        /// Returns a string representation of each item in the spatial map, with elements
        /// displayed in the specified way.
        /// </summary>
        /// <param name="elementStringifier">
        /// A function that takes an element of type T and produces the string that should
        /// represent it in the output.
        /// </param>
        /// <returns>A string representing each layer in the spatial map, with each element displayed in the specified way.</returns>
        public string ToString(Func<T, string> elementStringifier)
        {
            var layer = StartingLayer;
            var sb = new StringBuilder();
            foreach (var map in _layers)
            {
                sb.Append(layer + ": ");
                sb.Append(map.ToString(elementStringifier) + '\n');
                layer++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns true if the given item can be added at the given position, eg. it is on a layer in the spatial map and its
        /// layer will accept it; false otherwise.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="position">Position to add item to.</param>
        /// <returns>True if the item can be successfully added at the position given; false otherwise.</returns>
        public bool CanAdd(T newItem, Point position)
        {
            var relativeLayer = newItem.Layer - StartingLayer;
            return relativeLayer >= 0 && relativeLayer < _layers.Length &&
                   _layers[relativeLayer].CanAdd(newItem, position);
        }

        /// <summary>
        /// Returns true if the given item can be added at the given position, eg. it is on a layer in the spatial map and its
        /// layer will accept it; false otherwise.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="x">X-value of the position to add item to.</param>
        /// <param name="y">Y-value of the position to add item to.</param>
        /// <returns>True if the item can be successfully added at the position given; false otherwise.</returns>
        public bool CanAdd(T newItem, int x, int y) => CanAdd(newItem, new Point(x, y));

        /// <summary>
        /// Returns true if the given item can be moved from its current location to the specified one, eg. it is in the spatial
        /// map and its layer will
        /// accept it at the new position; false otherwise.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="target">Location to move item to.</param>
        /// <returns>true if the given item can be moved to the given position; false otherwise.</returns>
        public bool CanMove(T item, Point target)
        {
            var relativeLayer = item.Layer - StartingLayer;
            return relativeLayer >= 0 && relativeLayer < _layers.Length && _layers[relativeLayer].CanMove(item, target);
        }

        /// <summary>
        /// Returns true if the given item can be moved from its current location to the specified one, eg. it is in the spatial
        /// map and its layer will
        /// accept it at the new position; false otherwise.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="targetX">X-value of the location to move item to.</param>
        /// <param name="targetY">Y-value of the location to move item to.</param>
        /// <returns>true if the given item can be moved to the given position; false otherwise.</returns>
        public bool CanMove(T item, int targetX, int targetY) => CanMove(item, new Point(targetX, targetY));

        /// <inheritdoc />
        bool IReadOnlySpatialMap<T>.CanMoveAll(Point current, Point target)
            => CanMoveAll(current.X, current.Y, target.X, target.Y);

        /// <inheritdoc />
        bool IReadOnlySpatialMap<T>.CanMoveAll(int currentX, int currentY, int targetX, int targetY)
            => CanMoveAll(currentX, currentY, targetX, targetY);

        /// <inheritdoc />
        void ISpatialMap<T>.MoveAll(Point current, Point target) => MoveAll(current, target);

        /// <inheritdoc />
        void ISpatialMap<T>.MoveAll(int currentX, int currentY, int targetX, int targetY)
            => MoveAll(currentX, currentY, targetX, targetY);

        /// <summary>
        /// Moves all items that can be moved, that are at the given position and on any layer specified by the given layer
        /// mask, to the new position. If no layer mask is specified, defaults to all layers.
        /// </summary>
        /// <param name="current">Position to move all items from.</param>
        /// <param name="target">Position to move all items to.</param>
        /// <param name="layerMask">
        /// Layer mask specifying which layers to search for items on. Defaults to all layers.
        /// </param>
        /// <returns>All items moved.</returns>
        public IEnumerable<T> MoveValid(Point current, Point target, uint layerMask = uint.MaxValue)
            => MoveValid(current.X, current.Y, target.X, target.Y, layerMask);

        /// <summary>
        /// Moves all items that can be moved, that are at the given position and on any layer specified by the given layer
        /// mask, to the new position. If no layer mask is specified, defaults to all layers.
        /// </summary>
        /// <param name="currentX">X-value of the position to move items from.</param>
        /// <param name="currentY">Y-value of the position to move items from.</param>
        /// <param name="targetX">X-value of the position to move items to.</param>
        /// <param name="targetY">Y-value of the position to move items from.</param>
        /// <param name="layerMask">
        /// Layer mask specifying which layers to search for items on. Defaults to all layers.
        /// </param>
        /// <returns>All items moved.</returns>
        public IEnumerable<T> MoveValid(int currentX, int currentY, int targetX, int targetY,
                                        uint layerMask = uint.MaxValue)
        {
            foreach (var relativeLayerNumber in _internalLayerMasker.Layers(layerMask >> StartingLayer))
            foreach (var itemMoved in _layers[relativeLayerNumber].MoveValid(currentX, currentY, targetX, targetY))
                yield return itemMoved;
        }

        /// <summary>
        /// Removes all items at the specified location that are on any layer included in the given
        /// layer mask from the spatial map. Returns any items that were removed. Defaults to searching
        /// for items on all layers.
        /// </summary>
        /// <param name="position">Position to remove items from.</param>
        /// <param name="layerMask">
        /// The layer mask indicating which layers to search for items. Defaults to all layers.
        /// </param>
        /// <returns>Any items that were removed, or nothing if no items were removed.</returns>
        public IEnumerable<T> Remove(Point position, uint layerMask = uint.MaxValue)
            => Remove(position.X, position.Y, layerMask);

        /// <summary>
        /// Removes all items at the specified location that are on any layer included in the given
        /// layer mask from the spatial map. Returns any items that were removed. Defaults to searching
        /// for items on all layers.
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
        /// Returns a string representation of each layer in the spatial map.
        /// </summary>
        /// <returns>A string representing each layer of the LayeredSpatialMap</returns>
        public override string ToString() => ToString(t => t.ToString());

        /// <summary>
        /// Moves all items that are on layers in <paramref name="layerMask" /> at the specified source location to the target
        /// location.  Throws InvalidOperationException if one or more items cannot be moved or there are
        /// no items to be moved.
        /// </summary>
        /// <param name="current">Location to move items from.</param>
        /// <param name="target">Location to move items to.</param>
        /// <param name="layerMask">The layer mask to use to find items.</param>
        public void MoveAll(Point current, Point target, uint layerMask = uint.MaxValue)
            => MoveAll(current.X, current.Y, target.X, target.Y, layerMask);

        /// <summary>
        /// Moves all items that are on layers in <paramref name="layerMask" /> at the specified source location to the target
        /// location.  Throws InvalidOperationException if one or more items cannot be moved or there are
        /// no items to be moved.
        /// </summary>
        /// <param name="currentX">X-value of the location to move items from.</param>
        /// <param name="currentY">Y-value of the location to move items from.</param>
        /// <param name="targetX">X-value of the location to move items to.</param>
        /// <param name="targetY">Y-value of the location to move items to.</param>
        /// <param name="layerMask">The layer mask to use to find items.</param>
        public void MoveAll(int currentX, int currentY, int targetX, int targetY, uint layerMask = uint.MaxValue)
        {
            var hasItems = false;

            foreach (var relativeLayerNumber in _internalLayerMasker.Layers(layerMask >> StartingLayer))
                if (_layers[relativeLayerNumber].Contains(currentX, currentY))
                {
                    hasItems = true;
                    _layers[relativeLayerNumber].MoveAll(currentX, currentY, targetX, targetY);
                }

            if (!hasItems)
                throw new InvalidOperationException(
                    $"Tried to move all items at position {new Point(currentX, currentY)} in a {GetType().Name}, but there were no items present at that location.");
        }
    }

    /// <summary>
    /// <see cref="ISpatialMap{T}" /> implementation that can be used to efficiently represent multiple
    /// "layers" of objects, with each layer represented as an <see cref="ISpatialMap{T}" /> instance.
    /// It provides the regular spatial map functionality, as well as adds layer masking functionality
    /// that allow functions to operate on specific layers only.
    /// </summary>
    /// <remarks>
    /// See the <see cref="ISpatialMap{T}" /> for documentation on the practical purpose of spatial
    /// maps.
    /// The objects stored in a LayeredSpatialMap must be reference types and implement both <see cref="IHasID" />
    /// and <see cref="IHasLayer" />.  Each object in a spatial map is presumed to have a "layer", which is assumed
    /// to remain constant once the item is added to the layer mask.
    /// </remarks>
    /// <typeparam name="T">
    /// Type of items stored in the layers. Type T must implement <see cref="IHasID" /> and <see cref="IHasLayer" />,
    /// must be a reference type, and its <see cref="IHasLayer.Layer" /> value MUST NOT change while the item is in the
    /// spatial map.
    /// </typeparam>
    [PublicAPI]
    public class LayeredSpatialMap<T> : AdvancedLayeredSpatialMap<T> where T : class, IHasLayer, IHasID
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// This class allows you to specify the starting index for layers in order to make it easy to
        /// combine with other structures in a map which may represent other layers. For example, if a
        /// <paramref name="startingLayer" /> of 0 is specified, layers in the spatial map will have numbers
        /// in range[0, numberOfLayers - 1]. If 1 is specified, layers will have numbers in range [1-numberOfLayers],
        /// and anything to do with layer 0 will be ignored. For example, If a layer-mask that includes layers 0,
        /// 2, and 3 is passed to a function, only layers 2 and 3 are considered (since they are the only ones that would
        /// be included in the spatial map.
        /// </remarks>
        /// <param name="numberOfLayers">Number of layers to include.</param>
        /// <param name="startingLayer">Index to use for the first layer.</param>
        /// <param name="layersSupportingMultipleItems">
        /// A layer mask indicating which layers should support multiple items residing at the same
        /// location on that layer. Defaults to no layers.  Generate this layer mask via <see cref="LayerMasker.DEFAULT" />.
        /// </param>
        public LayeredSpatialMap(int numberOfLayers, int startingLayer = 0, uint layersSupportingMultipleItems = 0)
            : base(new IDComparer<T>(), numberOfLayers, startingLayer, layersSupportingMultipleItems)
        { }
    }
}
