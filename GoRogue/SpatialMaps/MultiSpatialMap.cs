using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.SpatialMaps
{
    /// <summary>
    /// A more complex version of <see cref="MultiSpatialMap{T}" /> that does not require the items in it to implement
    /// <see cref="IHasID" />, instead requiring the specification of a custom <see cref="IEqualityComparer{T}" /> to use
    /// for hashing and comparison of items.
    /// </summary>
    /// <remarks>
    /// This class is useful for cases where you do not want to implement <see cref="IHasID" />, or if you need
    /// to use a value type in a spatial map. For simple cases, it is recommended to use <see cref="MultiSpatialMap{T}" />
    /// instead.
    /// Be mindful of the efficiency of your hashing function specified in the <see cref="IEqualityComparer{T}" /> --
    /// it will in large part determine the performance of AdvancedMultiSpatialMap!
    /// </remarks>
    /// <typeparam name="T">The type of object that will be contained by this AdvancedMultiSpatialMap.</typeparam>
    [PublicAPI]
    public class AdvancedMultiSpatialMap<T> : ISpatialMap<T>
        where T : notnull
    {
        private readonly Dictionary<T, SpatialTuple<T>> _itemMapping;
        private readonly Dictionary<Point, List<SpatialTuple<T>>> _positionMapping;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="comparer">
        /// Equality comparer to use for comparison and hashing of type T. Be especially mindful of the
        /// efficiency of its GetHashCode function, as it will determine the efficiency of many
        /// AdvancedMultiSpatialMap functions.
        /// </param>
        /// <param name="initialCapacity">
        /// The initial maximum number of elements the AdvancedMultiSpatialMap can hold before it has to
        /// internally resize data structures. Defaults to 32.
        /// </param>
        public AdvancedMultiSpatialMap(IEqualityComparer<T> comparer, int initialCapacity = 32)
        {
            _itemMapping = new Dictionary<T, SpatialTuple<T>>(initialCapacity, comparer);
            _positionMapping = new Dictionary<Point, List<SpatialTuple<T>>>(initialCapacity);
        }

        /// <inheritdoc />
        public event EventHandler<ItemEventArgs<T>>? ItemAdded;

        /// <inheritdoc />
        public event EventHandler<ItemMovedEventArgs<T>>? ItemMoved;

        /// <inheritdoc />
        public event EventHandler<ItemEventArgs<T>>? ItemRemoved;

        /// <inheritdoc />
        public int Count => _itemMapping.Count;

        /// <inheritdoc />
        public IEnumerable<T> Items
        {
            get
            {
                foreach (var item in _itemMapping.Values)
                    yield return item.Item;
            }
        }

        /// <inheritdoc />
        public IEnumerable<Point> Positions
        {
            get
            {
                foreach (var position in _positionMapping.Keys)
                    yield return position;
            }
        }

        /// <summary>
        /// Adds the given item at the given position, provided the item is not already in the
        /// spatial map. If the item is already added, throws InvalidOperationException.
        /// </summary>
        /// <param name="newItem">The item to add.</param>
        /// <param name="position">The position at which to add the new item.</param>
        public void Add(T newItem, Point position)
        {
            if (_itemMapping.ContainsKey(newItem))
                throw new InvalidOperationException($"Item added to {GetType().Name} when it has already been added.");

            var tuple = new SpatialTuple<T>(newItem, position);
            _itemMapping.Add(newItem, tuple);

            if (!_positionMapping.ContainsKey(position))
                _positionMapping.Add(position, new List<SpatialTuple<T>>());

            _positionMapping[position].Add(tuple);
            ItemAdded?.Invoke(this, new ItemEventArgs<T>(newItem, position));
        }

        /// <summary>
        /// Adds the given item at the given position, provided the item is not already in the
        /// spatial map. If the item is already added, throws InvalidOperationException.
        /// </summary>
        /// <param name="newItem">The item to add.</param>
        /// <param name="x">x-value of the position to add item to.</param>
        /// <param name="y">y-value of the position to add item to.</param>
        public void Add(T newItem, int x, int y) => Add(newItem, new Point(x, y));

        /// <inheritdoc />
        public IReadOnlySpatialMap<T> AsReadOnly() => this;

        /// <inheritdoc />
        public void Clear()
        {
            _itemMapping.Clear();
            _positionMapping.Clear();
        }

        /// <inheritdoc />
        public bool Contains(T item) => _itemMapping.ContainsKey(item);

        /// <inheritdoc />
        public bool Contains(Point position) => _positionMapping.ContainsKey(position);

        /// <inheritdoc />
        public bool Contains(int x, int y) => Contains(new Point(x, y));

        /// <summary>
        /// Used by foreach loop, so that the class will give ISpatialTuple objects when used in a
        /// foreach loop. Generally should never be called explicitly.
        /// </summary>
        /// <returns>An enumerator for the spatial map.</returns>
        public IEnumerator<ISpatialTuple<T>> GetEnumerator()
        {
            foreach (var tuple in _itemMapping.Values)
                yield return tuple;
        }

        /// <summary>
        /// Non-generic version of enumerable used by foreach loop internally.
        /// </summary>
        /// <returns>Enumerator of ISpatialTuples.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerable<T> GetItemsAt(Point position)
        {
            if (_positionMapping.ContainsKey(position))
            {
                var positionList = _positionMapping[position];

                for (var i = positionList.Count - 1; i >= 0; i--)
                    yield return positionList[i].Item;
            }
        }

        /// <inheritdoc />
        public IEnumerable<T> GetItemsAt(int x, int y) => GetItemsAt(new Point(x, y));

        /// <inheritdoc />
        public Point GetPositionOf(T item)
        {
            _itemMapping.TryGetValue(item, out var tuple);
            if (tuple == null) return Point.None;
            return tuple.Position;
        }

        /// <summary>
        /// Moves the item specified to the position specified. If the item does not exist in the
        /// spatial map or is already at the target position, the function throws InvalidOperationException.
        /// </summary>
        /// <param name="item">The item to move.</param>
        /// <param name="target">The position to move it to.</param>
        public void Move(T item, Point target)
        {
            if (!_itemMapping.ContainsKey(item))
                throw new InvalidOperationException(
                    $"Tried to move item in {GetType().Name}, but the item does not exist.");

            var movingTuple = _itemMapping[item];
            if (movingTuple.Position == target)
                throw new InvalidOperationException(
                    $"Tried to move item in {GetType().Name}, but the item was already at the target position.");

            var oldPos = movingTuple.Position;
            _positionMapping[movingTuple.Position].Remove(movingTuple);
            if (_positionMapping[movingTuple.Position].Count == 0)
                _positionMapping.Remove(movingTuple.Position);

            movingTuple.Position = target;
            if (!_positionMapping.ContainsKey(target))
                _positionMapping[target] = new List<SpatialTuple<T>>();
            _positionMapping[target].Add(movingTuple);
            ItemMoved?.Invoke(this, new ItemMovedEventArgs<T>(item, oldPos, target));
        }

        /// <summary>
        /// Moves the item specified to the position specified. If the item does not exist in the
        /// spatial map or is already at the target position, the function throws InvalidOperationException.
        /// </summary>
        /// <param name="item">The item to move.</param>
        /// <param name="targetX">X-value of the location to move it to.</param>
        /// <param name="targetY">Y-value of the location to move it to.</param>
        public void Move(T item, int targetX, int targetY) => Move(item, new Point(targetX, targetY));

        /// <inheritdoc />
        public List<T> MoveValid(Point current, Point target)
        {
            var result = new List<T>();
            if (_positionMapping.ContainsKey(current) && current != target)
            {
                if (!_positionMapping.ContainsKey(target))
                    _positionMapping.Add(target, new List<SpatialTuple<T>>());

                foreach (var tuple in _positionMapping[current])
                {
                    tuple.Position = target;
                    _positionMapping[target].Add(tuple);
                    result.Add(tuple.Item);
                }

                var list = _positionMapping[current];
                _positionMapping.Remove(current);

                if (ItemMoved != null)
                    foreach (var tuple in list)
                        ItemMoved(this, new ItemMovedEventArgs<T>(tuple.Item, current, target));
            }

            return result;
        }

        /// <inheritdoc />
        public List<T> MoveValid(int currentX, int currentY, int targetX, int targetY)
            => MoveValid(new Point(currentX, currentY), new Point(targetX, targetY));

        /// <summary>
        /// Removes the item specified, if it exists.  Throws InvalidOperationException if the item is
        /// not in the spatial map.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void Remove(T item)
        {
            if (!_itemMapping.ContainsKey(item))
                throw new InvalidOperationException(
                    $"Tried to remove an item from the {GetType().Name} that has not been added.");

            var tuple = _itemMapping[item];
            _itemMapping.Remove(item);
            _positionMapping[tuple.Position].Remove(tuple);

            if (_positionMapping[tuple.Position].Count == 0)
                _positionMapping.Remove(tuple.Position);

            ItemRemoved?.Invoke(this, new ItemEventArgs<T>(item, tuple.Position));
        }

        /// <inheritdoc />
        public List<T> Remove(Point position)
        {
            var result = new List<T>();

            if (_positionMapping.ContainsKey(position))
            {
                foreach (var tuple in _positionMapping[position])
                {
                    _itemMapping.Remove(tuple.Item);
                    result.Add(tuple.Item);
                }

                var list = _positionMapping[position];
                _positionMapping.Remove(position);
                if (ItemRemoved != null)
                    foreach (var tuple in list)
                        ItemRemoved(this, new ItemEventArgs<T>(tuple.Item, position));
            }

            return result;
        }

        /// <inheritdoc />
        public List<T> Remove(int x, int y) => Remove(new Point(x, y));

        /// <summary>
        /// Returns a string representation of the spatial map, allowing display of the
        /// spatial map's items in a specified way.
        /// </summary>
        /// <param name="itemStringifier">Function that turns an item into a string.</param>
        /// <returns>A string representation of the spatial map.</returns>
        public string ToString(Func<T, string> itemStringifier)
            => _positionMapping.ExtendToString("", valueStringifier: obj =>
                    obj.ExtendToString(elementStringifier: item => itemStringifier(item.Item)),
                kvSeparator: ": ", pairSeparator: ",\n", end: "");

        /// <summary>
        /// Returns true if the given item can be added at the given position, eg. if the item is not already in the spatial map;
        /// false otherwise.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="position">Position to add item to.</param>
        /// <returns>True if the item can be successfully added at the position given; false otherwise.</returns>
        public bool CanAdd(T newItem, Point position) => !_itemMapping.ContainsKey(newItem);

        /// <summary>
        /// Returns true if the given item can be added at the given position, eg. if the item is not already in the spatial map;
        /// false otherwise.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="x">X-value of the position to add item to.</param>
        /// <param name="y">Y-value of the position to add item to.</param>
        /// <returns>True if the item can be successfully added at the position given; false otherwise.</returns>
        public bool CanAdd(T newItem, int x, int y) => CanAdd(newItem, new Point(x, y));

        /// <summary>
        /// Returns true if the given item can be moved from its current location to the specified one,
        /// eg. the item is contained within the spatial map; false otherwise.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="target">Location to move item to.</param>
        /// <returns>true if the given item can be moved to the given position; false otherwise.</returns>
        public bool CanMove(T item, Point target) => _itemMapping.ContainsKey(item);

        /// <summary>
        /// Returns true if the given item can be moved from its current location to the specified one,
        /// eg. the item is contained within the spatial map; false otherwise.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="targetX">X-value of the location to move item to.</param>
        /// <param name="targetY">Y-value of the location to move item to.</param>
        /// <returns>true if the given item can be moved to the given position; false otherwise.</returns>
        public bool CanMove(T item, int targetX, int targetY) => CanMove(item, new Point(targetX, targetY));

        /// <inheritdoc />
        public bool CanMoveAll(Point current, Point target)
            => _positionMapping.ContainsKey(current) && current != target;

        /// <inheritdoc />
        public bool CanMoveAll(int currentX, int currentY, int targetX, int targetY)
            => CanMoveAll(new Point(currentX, currentY), new Point(targetX, targetY));

        /// <summary>
        /// Moves all items at the specified source location to the target location.  Throws InvalidOperationException if there are
        /// no items to be moved.
        /// </summary>
        /// <param name="current">Location to move items from.</param>
        /// <param name="target">Location to move items to.</param>
        public void MoveAll(Point current, Point target)
        {
            if (!_positionMapping.ContainsKey(current))
                throw new InvalidOperationException(
                    $"Tried to move all items from {current} in {GetType().Name}, but there was nothing at the that position.");

            if (current == target)
                throw new InvalidOperationException(
                    $"Tried to move all items from {current} in {GetType().Name}, but the current and target positions were the same.");

            MoveValid(current, target);
        }

        /// <summary>
        /// Moves all items at the specified source location to the target location.  Throws InvalidOperationException if there are
        /// no items to be moved.
        /// </summary>
        /// <param name="currentX">X-value of the location to move items from.</param>
        /// <param name="currentY">Y-value of the location to move items from.</param>
        /// <param name="targetX">X-value of the location to move items to.</param>
        /// <param name="targetY">Y-value of the location to move items to.</param>
        public void MoveAll(int currentX, int currentY, int targetX, int targetY)
            => MoveAll(new Point(currentX, currentY), new Point(targetX, targetY));

        /// <summary>
        /// Returns a string representation of the spatial map.
        /// </summary>
        /// <returns>A string representation of the spatial map.</returns>
        public override string ToString()
            => ToString(obj => obj.ToString() ?? "null");
    }

    /// <summary>
    /// An implementation of <see cref="ISpatialMap{T}" /> that allows multiple items to reside
    /// at any given position at the same time.  If you wish to allow only one item to reside
    /// at each location at a time, use <see cref="SpatialMap{T}" /> instead.  For a situation
    /// involving different categories or layers of items, you may want to look at
    /// <see cref="LayeredSpatialMap{T}" />.
    /// </summary>
    /// <remarks>
    /// See the <see cref="ISpatialMap{T}" /> for documentation on the practical purpose of spatial
    /// maps.
    /// The objects stored in a MultiSpatialMap must implement <see cref="IHasID" />. This is used
    /// internally to keep track of the objects, since uints are easily (and efficiently) hash-able.
    /// Although MultiSpatialMap is generally quite performant, if you know the spatial map will
    /// only have one item at any given position at a time, <see cref="SpatialMap{T}" /> may yield
    /// better performance.
    /// </remarks>
    /// <typeparam name="T">
    /// The type of items being stored in the spatial map. Must implement <see cref="IHasID" /> and be
    /// a reference-type.
    /// </typeparam>
    [PublicAPI]
    public class MultiSpatialMap<T> : AdvancedMultiSpatialMap<T> where T : class, IHasID
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialCapacity">
        /// The initial maximum number of elements the spatial map can hold before it has to
        /// internally resize data structures. Defaults to 32.
        /// </param>
        public MultiSpatialMap(int initialCapacity = 32)
            : base(new IDComparer<T>(), initialCapacity)
        { }
    }
}
