using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using GoRogue.Pooling;
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
    public class AdvancedLayeredSpatialMap<T> : ISpatialMap<T>, IReadOnlyLayeredSpatialMap<T>
        where T : IHasLayer
    {
        // Same as LayerMasker property, but startingLayers less layers, for actual use, since we view our layers as
        // 0 -> numberOfLayers - 1
        private readonly LayerMasker _internalLayerMasker;

        private readonly ISpatialMap<T>[] _layers;

        // Cached hash-set used for returning all positions in the LayeredSpatialMap
        private readonly HashSet<Point> _positionCache;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="itemComparer">
        /// Equality comparer to use for comparison and hashing of type T. Be especially mindful of the
        /// efficiency of its GetHashCode function, as it will determine the efficiency of
        /// many AdvancedLayeredSpatialMap functions.
        /// </param>
        /// <param name="numberOfLayers">Number of layers to include.</param>
        /// <param name="customListPoolCreator">
        /// A function used to determine the list pool implementation used for the spatial maps which support multiple
        /// items in a location (if any).  The function takes the layer it is creating the pool for as a parameter.
        /// If no custom creator is specified, a ListPool is used.
        /// </param>
        /// <param name="pointComparer">
        /// Equality comparer to use for comparison and hashing of points, as object are added to/removed from/moved
        /// around the spatial map.  Be especially mindful of the efficiency of its GetHashCode function, as it will
        /// determine the efficiency of many AdvancedLayeredSpatialMap functions.  Defaults to the default equality
        /// comparer for Point, which uses a fairly efficient generalized hashing algorithm.
        /// </param>
        /// <param name="startingLayer">Index to use for the first layer.</param>
        /// <param name="layersSupportingMultipleItems">
        /// A layer mask indicating which layers should support multiple items residing at the same
        /// location on that layer. Defaults to no layers.
        /// </param>
        public AdvancedLayeredSpatialMap(IEqualityComparer<T> itemComparer,
                                         int numberOfLayers, Func<int, IListPool<T>>? customListPoolCreator = null,
                                         IEqualityComparer<Point>? pointComparer = null, int startingLayer = 0,
                                         uint layersSupportingMultipleItems = 0)
        {
            if (numberOfLayers > 32 - startingLayer)
                throw new ArgumentOutOfRangeException(nameof(numberOfLayers),
                    $"More than {32 - startingLayer} layers is not supported by {nameof(AdvancedLayeredSpatialMap<T>)} starting at layer {startingLayer}");

            customListPoolCreator ??= _ => new ListPool<T>(50, 16);
            pointComparer ??= EqualityComparer<Point>.Default;

            _layers = new ISpatialMap<T>[numberOfLayers];
            StartingLayer = startingLayer;
            _positionCache = new HashSet<Point>(pointComparer);

            LayerMasker = new LayerMasker(numberOfLayers + startingLayer);
            _internalLayerMasker = new LayerMasker(numberOfLayers);

            // Create layers based on mask parameters
            for (int i = 0; i < _layers.Length; i++)
            {
                var layerNum = i + startingLayer;
                if (LayerMasker.HasLayer(layersSupportingMultipleItems, layerNum))
                    _layers[i] = new AdvancedMultiSpatialMap<T>(itemComparer, customListPoolCreator(layerNum), pointComparer);
                else
                    _layers[i] = new AdvancedSpatialMap<T>(itemComparer, pointComparer);
            }

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
            // LayerMasking will ignore layers that don't actually exist
            foreach (var num in _internalLayerMasker.Layers(layerMask >> StartingLayer))
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
        /// Adds the given item at the given position on the correct layer.  ArgumentException is thrown if the layer is
        /// invalid or the item otherwise cannot be added to its layer.
        /// </summary>
        /// <param name="item">Item to add.</param>
        /// <param name="position">Position to add item at.</param>
        public void Add(T item, Point position)
        {
            int relativeLayer = item.Layer - StartingLayer;
            if (relativeLayer < 0 || relativeLayer >= _layers.Length)
                throw new ArgumentException(
                    $"Tried to add item to {GetType().Name} on layer {item.Layer}, but no such layer exists.",
                    nameof(item));

            _layers[relativeLayer].Add(item, position);
        }

        /// <summary>
        /// Adds the given item at the given position on the correct layer.  ArgumentException is thrown if the layer is
        /// invalid or
        /// the item otherwise cannot be added to its layer.
        /// </summary>
        /// <param name="item">Item to add.</param>
        /// <param name="x">X-value of position to add item at.</param>
        /// <param name="y">Y-value of position to add item at.</param>
        public void Add(T item, int x, int y) => Add(item, new Point(x, y));

        /// <summary>
        /// Adds the given item at the given position on the correct layer.  If the layer is
        /// invalid or the item otherwise cannot be added to its layer, does nothing and returns false.
        /// </summary>
        /// <param name="item">Item to add.</param>
        /// <param name="position">Position to add item at.</param>
        /// <returns>True if the item was added, false otherwise.</returns>
        public bool TryAdd(T item, Point position)
        {
            int relativeLayer = item.Layer - StartingLayer;
            if (relativeLayer < 0 || relativeLayer >= _layers.Length)
                return false;

            return _layers[relativeLayer].TryAdd(item, position);
        }

        /// <summary>
        /// Adds the given item at the given position on the correct layer.  If the layer is
        /// invalid or the item otherwise cannot be added to its layer, does nothing and returns false.
        /// </summary>
        /// <param name="item">Item to add.</param>
        /// <param name="x">X-value of position to add item at.</param>
        /// <param name="y">Y-value of position to add item at.</param>
        /// <returns>True if the item was added, false otherwise.</returns>
        public bool TryAdd(T item, int x, int y) => TryAdd(item, new Point(x, y));

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
            int relativeLayer = item.Layer - StartingLayer;
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
        public IEnumerator<ItemPositionPair<T>> GetEnumerator()
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
        public Point? GetPositionOfOrNull(T item)
        {
            int relativeLayer = item.Layer - StartingLayer;
            if (relativeLayer < 0 || relativeLayer >= _layers.Length)
                return null;

            return _layers[relativeLayer].GetPositionOfOrNull(item);
        }

        /// <inheritdoc />
        public bool TryGetPositionOf(T item, out Point position)
        {
            int relativeLayer = item.Layer - StartingLayer;
            if (relativeLayer < 0 || relativeLayer >= _layers.Length)
            {
                position = default;
                return false;
            }

            return _layers[relativeLayer].TryGetPositionOf(item, out position);
        }

        /// <inheritdoc />
        public Point GetPositionOf(T item)
        {
            int relativeLayer = item.Layer - StartingLayer;
            if (relativeLayer < 0 || relativeLayer >= _layers.Length)
                throw new ArgumentException("Tried to retrieve the position of an item with an invalid layer.",
                    nameof(item));

            return _layers[relativeLayer].GetPositionOf(item);
        }

        /// <summary>
        /// Moves the given item to the given position.  Throws ArgumentException if either the item given
        /// isn't in the spatial map, or if the layer that the item resides on is configured to allow only one item per
        /// location at any given time and there is already an item at <paramref name="target" />.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="target">Position to move the given item to.</param>
        public void Move(T item, Point target)
        {
            int relativeLayer = item.Layer - StartingLayer;
            if (relativeLayer < 0 || relativeLayer >= _layers.Length)
                throw new ArgumentException(
                    $"Tried to move item in {GetType().Name} on layer {item.Layer}, but no such layer exists.",
                    nameof(item));

            _layers[relativeLayer].Move(item, target);
        }

        /// <summary>
        /// Moves the given item to the given position.  Throws ArgumentException if either the item given
        /// isn't in the spatial map, or if the layer that the item resides on is configured to allow only one item per
        /// location at any given time and there is already an item at the target position.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="targetX">X-value of position to move the given item to.</param>
        /// <param name="targetY">Y-value of position to move the given item to.</param>
        public void Move(T item, int targetX, int targetY) => Move(item, new Point(targetX, targetY));

        /// <inheritdoc />
        public bool TryMove(T item, Point target)
        {
            int relativeLayer = item.Layer - StartingLayer;
            if (relativeLayer < 0 || relativeLayer >= _layers.Length)
                return false;

            return _layers[relativeLayer].TryMove(item, target);
        }

        /// <inheritdoc />
        public bool TryMove(T item, int targetX, int targetY) => TryMove(item, new Point(targetX, targetY));

        /// <inheritdoc />
        List<T> ISpatialMap<T>.MoveValid(Point current, Point target)
            => MoveValid(current.X, current.Y, target.X, target.Y);

        /// <inheritdoc />
        void ISpatialMap<T>.MoveValid(Point current, Point target, List<T> itemsMovedOutput)
            => MoveValid(current, target, itemsMovedOutput);

        /// <inheritdoc />
        List<T> ISpatialMap<T>.MoveValid(int currentX, int currentY, int targetX, int targetY)
            => MoveValid(currentX, currentY, targetX, targetY);

        /// <inheritdoc />
        void ISpatialMap<T>.MoveValid(int currentX, int currentY, int targetX, int targetY, List<T> itemsMovedOutput)
            => MoveValid(currentX, currentY, targetX, targetY, itemsMovedOutput);

        /// <inheritdoc />
        public void Remove(T item)
        {
            int relativeLayer = item.Layer - StartingLayer;
            if (relativeLayer < 0 || relativeLayer >= _layers.Length)
                throw new ArgumentException(
                    $"Tried to remove item from {GetType().Name} on layer {item.Layer}, but no such layer exists.",
                    nameof(item));

            _layers[relativeLayer].Remove(item);
        }

        /// <inheritdoc />
        public bool TryRemove(T item)
        {
            int relativeLayer = item.Layer - StartingLayer;
            if (relativeLayer < 0 || relativeLayer >= _layers.Length)
                return false;

            return _layers[relativeLayer].TryRemove(item);
        }

        /// <inheritdoc />
        List<T> ISpatialMap<T>.Remove(Point position) => Remove(position);

        /// <inheritdoc />
        bool ISpatialMap<T>.TryRemove(Point position) => TryRemove(position);

        /// <inheritdoc />
        List<T> ISpatialMap<T>.Remove(int x, int y) => Remove(x, y);

        /// <inheritdoc />
        bool ISpatialMap<T>.TryRemove(int x, int y) => TryRemove(x, y);

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

        /// <summary>
        /// Returns true if the given item can be added at the given position, eg. it is on a layer in the spatial map and its
        /// layer will accept it; false otherwise.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="position">Position to add item to.</param>
        /// <returns>True if the item can be successfully added at the position given; false otherwise.</returns>
        public bool CanAdd(T newItem, Point position)
        {
            int relativeLayer = newItem.Layer - StartingLayer;
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
        bool ISpatialMap<T>.TryMoveAll(Point current, Point target) => TryMoveAll(current, target);

        /// <inheritdoc />
        bool ISpatialMap<T>.TryMoveAll(int currentX, int currentY, int targetX, int targetY)
            => TryMoveAll(currentX, currentY, targetX, targetY);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> MoveValid(Point current, Point target, uint layerMask = uint.MaxValue)
        {
            var result = new List<T>(0);
            MoveValid(current, target, result, layerMask);
            return result;
        }

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
        public List<T> MoveValid(int currentX, int currentY, int targetX, int targetY, uint layerMask = uint.MaxValue)
            => MoveValid(new Point(currentX, currentY), new Point(targetX, targetY), layerMask);

        /// <summary>
        /// Moves all items that can be moved, that are at the given position and on any layer specified by the given layer
        /// mask, to the new position. If no layer mask is specified, defaults to all layers.
        /// </summary>
        /// <param name="current">Position to move all items from.</param>
        /// <param name="target">Position to move all items to.</param>
        /// <param name="itemsMovedOutput">List in which to place all moved items.</param>
        /// <param name="layerMask">
        /// Layer mask specifying which layers to search for items on. Defaults to all layers.
        /// </param>
        public void MoveValid(Point current, Point target, List<T> itemsMovedOutput, uint layerMask = uint.MaxValue)
        {
            if (current == target)
                return;

            foreach (var relativeLayerNumber in _internalLayerMasker.Layers(layerMask >> StartingLayer))
                _layers[relativeLayerNumber].MoveValid(current, target, itemsMovedOutput);
        }

        /// <summary>
        /// Moves all items that can be moved, that are at the given position and on any layer specified by the given layer
        /// mask, to the new position. If no layer mask is specified, defaults to all layers.
        /// </summary>
        /// <param name="currentX">X-value of the position to move items from.</param>
        /// <param name="currentY">Y-value of the position to move items from.</param>
        /// <param name="targetX">X-value of the position to move items to.</param>
        /// <param name="targetY">Y-value of the position to move items from.</param>
        /// <param name="itemsMovedOutput">List in which to place all moved items.</param>
        /// <param name="layerMask">
        /// Layer mask specifying which layers to search for items on. Defaults to all layers.
        /// </param>
        public void MoveValid(int currentX, int currentY, int targetX, int targetY, List<T> itemsMovedOutput,
                              uint layerMask = uint.MaxValue)
            => MoveValid(new Point(currentX, currentY), new Point(targetX, targetY), itemsMovedOutput, layerMask);

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
        public List<T> Remove(Point position, uint layerMask = uint.MaxValue)
        {
            var list = new List<T>();
            foreach (var relativeLayerNumber in _internalLayerMasker.Layers(layerMask >> StartingLayer))
                list.AddRange(_layers[relativeLayerNumber].Remove(position));

            return list;
        }

        /// <summary>
        /// Attempts to remove all items at the specified location that are on any layer included in the given
        /// layer mask from the spatial map. Returns true if the items were successfully removed; false if one or more
        /// failed.
        /// </summary>
        /// <param name="position">Position to remove items from.</param>
        /// <param name="layerMask">
        /// The layer mask indicating which layers to search for items. Defaults to all layers.
        /// </param>
        /// <returns>True if the items were successfully removed; false otherwise</returns>
        public bool TryRemove(Point position, uint layerMask = uint.MaxValue)
        {
            bool result = true;
            foreach (var relativeLayerNumber in _internalLayerMasker.Layers(layerMask >> StartingLayer))
            {
                var layer = _layers[relativeLayerNumber];
                if (layer.Contains(position))
                    result = result && _layers[relativeLayerNumber].TryRemove(position);
            }

            return result;
        }

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
        public List<T> Remove(int x, int y, uint layerMask = uint.MaxValue)
            => Remove(new Point(x, y), layerMask);

        /// <summary>
        /// Attempts to remove all items at the specified location that are on any layer included in the given
        /// layer mask from the spatial map. Returns true if the items were successfully removed; false if one or more
        /// failed.
        /// </summary>
        /// <param name="x">X-value of the position to remove items from.</param>
        /// <param name="y">Y-value of the position to remove items from.</param>
        /// <param name="layerMask">
        /// The layer mask indicating which layers to search for items. Defaults to all layers.
        /// </param>
        /// <returns>True if the items were successfully removed; false otherwise</returns>
        public bool TryRemove(int x, int y, uint layerMask = uint.MaxValue)
            => TryRemove(new Point(x, y), layerMask);

        /// <summary>
        /// Returns a string representation of each layer in the spatial map.
        /// </summary>
        /// <returns>A string representing each layer of the LayeredSpatialMap</returns>
        public override string ToString() => ToString(t => t.ToString() ?? "null");

        /// <summary>
        /// Moves all items that are on layers in <paramref name="layerMask" /> at the specified source location to the target
        /// location.  Throws ArgumentException if one or more items cannot be moved or there are
        /// no items to be moved.
        /// </summary>
        /// <param name="current">Location to move items from.</param>
        /// <param name="target">Location to move items to.</param>
        /// <param name="layerMask">The layer mask to use to find items.</param>
        public void MoveAll(Point current, Point target, uint layerMask = uint.MaxValue)
        {
            uint mask = layerMask >> StartingLayer;
            bool hasItems = false;

            // For each operation, we'll check to ensure there are actually items to move first.  This is an extra
            // dictionary lookup per layer, but ensures that MoveAll doesn't throw exceptions if one layer (of many)
            // doesn't have an item.  It's only an exception for LayeredSpatialMap if no layers have any items.
            //
            // Note: This will _not_ preserve the state of the LayeredSpatialMap on exception; however doing so
            // would drastically slow down this function, and recovering to original state is not typically a desired
            // outcome.
            foreach (int relativeLayerNumber in _internalLayerMasker.Layers(mask))
                if (_layers[relativeLayerNumber].Contains(current))
                {
                    hasItems = true;
                    _layers[relativeLayerNumber].MoveAll(current, target);
                }

            // No items is an error
            if (!hasItems)
                throw new ArgumentException(
                    $"Tried to move all items at position {current} in a {GetType().Name}, but there were no items present at that location.");
        }

        /// <summary>
        /// Moves all items that are on layers in <paramref name="layerMask" /> at the specified source location to the target
        /// location.  Throws ArgumentException if one or more items cannot be moved or there are
        /// no items to be moved.
        /// </summary>
        /// <param name="currentX">X-value of the location to move items from.</param>
        /// <param name="currentY">Y-value of the location to move items from.</param>
        /// <param name="targetX">X-value of the location to move items to.</param>
        /// <param name="targetY">Y-value of the location to move items to.</param>
        /// <param name="layerMask">The layer mask to use to find items.</param>
        public void MoveAll(int currentX, int currentY, int targetX, int targetY, uint layerMask = uint.MaxValue)
            => MoveAll(new Point(currentX, currentY), new Point(targetX, targetY), layerMask);

        /// <summary>
        /// Moves all items that are on layers in <paramref name="layerMask" /> at the specified source location to the
        /// target location.  Returns false if one or more items cannot be moved or there are no items to be moved.
        /// </summary>
        /// <param name="current">Location to move items from.</param>
        /// <param name="target">Location to move items to.</param>
        /// <param name="layerMask">The layer mask to use to find items.</param>
        /// <returns>
        /// True if all items at <paramref name="current"/> on layers within the mask given were moved to
        /// <paramref name="target"/>; false otherwise.
        /// </returns>
        public bool TryMoveAll(Point current, Point target, uint layerMask = uint.MaxValue)
        {
            uint mask = layerMask >> StartingLayer;
            bool result = true;

            foreach (int relativeLayerNumber in _internalLayerMasker.Layers(mask))
            {
                var layer = _layers[relativeLayerNumber];
                if (layer.Contains(current))
                    result = layer.TryMoveAll(current, target) && result;
            }

            // No items should return false
            return result;
        }

        /// <summary>
        /// Moves all items that are on layers in <paramref name="layerMask" /> at the specified source location to the
        /// target location.  Returns false if one or more items cannot be moved or there are no items to be moved.
        /// </summary>
        /// <param name="currentX">X-value of the location to move items from.</param>
        /// <param name="currentY">Y-value of the location to move items from.</param>
        /// <param name="targetX">X-value of the location to move items to.</param>
        /// <param name="targetY">Y-value of the location to move items to.</param>
        /// <param name="layerMask">The layer mask to use to find items.</param>
        /// <returns>
        /// True if all items at the current position on layers within the mask given were moved to
        /// the target position; false otherwise.
        /// </returns>
        public bool TryMoveAll(int currentX, int currentY, int targetX, int targetY, uint layerMask = uint.MaxValue)
            => TryMoveAll(new Point(currentX, currentY), new Point(targetX, targetY));
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
    public sealed class LayeredSpatialMap<T> : AdvancedLayeredSpatialMap<T> where T : class, IHasLayer, IHasID
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
        /// <param name="customListPoolCreator">
        /// A function used to determine the list pool implementation used for the spatial maps which support multiple
        /// items in a location (if any).  The function takes the layer it is creating the pool for as a parameter.
        /// If no custom creator is specified, a ListPool is used.
        /// </param>
        /// <param name="pointComparer">
        /// Equality comparer to use for comparison and hashing of points, as object are added to/removed from/moved
        /// around the spatial map.  Be especially mindful of the efficiency of its GetHashCode function, as it will
        /// determine the efficiency of many LayeredSpatialMap functions.  Defaults to the default equality
        /// comparer for Point, which uses a fairly efficient generalized hashing algorithm.
        /// </param>
        /// <param name="startingLayer">Index to use for the first layer.</param>
        /// <param name="layersSupportingMultipleItems">
        /// A layer mask indicating which layers should support multiple items residing at the same
        /// location on that layer. Defaults to no layers.  Generate this layer mask via <see cref="LayerMasker.Default" />.
        /// </param>
        public LayeredSpatialMap(int numberOfLayers, Func<int, IListPool<T>>? customListPoolCreator = null,
                                 IEqualityComparer<Point>? pointComparer = null, int startingLayer = 0,
                                 uint layersSupportingMultipleItems = 0)
            : base(new IDComparer<T>(), numberOfLayers, customListPoolCreator, pointComparer, startingLayer, layersSupportingMultipleItems)
        { }
    }
}
