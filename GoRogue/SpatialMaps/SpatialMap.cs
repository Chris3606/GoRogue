using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.SpatialMaps
{
    /// <summary>
    /// A more complex version of <see cref="SpatialMap{T}" /> that does not require the items in it to implement
    /// <see cref="IHasID" />, instead requiring the specification of a custom <see cref="IEqualityComparer{T}" />
    /// to use for hashing and comparison of items.
    /// </summary>
    /// <remarks>
    /// This class is useful for cases where you do not want to implement <see cref="IHasID" />, or if you need
    /// to use a value type in a spatial map. For simple cases, it is recommended to use <see cref="SpatialMap{T}" />
    /// instead.
    /// Be mindful of the efficiency of your hashing function specified in the <see cref="IEqualityComparer{T}" /> --
    /// it will in large part determine the performance of AdvancedSpatialMap!
    /// </remarks>
    /// <typeparam name="T">The type of object that will be contained by this AdvancedSpatialMap.</typeparam>
    [PublicAPI]
    public class AdvancedSpatialMap<T> : ISpatialMap<T>
        where T : notnull
    {
        private readonly Dictionary<T, Point> _itemMapping;
        private readonly Dictionary<Point, T> _positionMapping;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="comparer">
        /// Equality comparer to use for comparison and hashing of type T. Be especially mindful of the
        /// efficiency of its GetHashCode function, as it will determine the efficiency of many AdvancedSpatialMap
        /// functions.
        /// </param>
        /// <param name="initialCapacity">
        /// The initial maximum number of elements the AdvancedSpatialMap can hold before it has to
        /// internally resize data structures. Defaults to 32.
        /// </param>
        public AdvancedSpatialMap(IEqualityComparer<T> comparer, int initialCapacity = 32)
        {
            _itemMapping = new Dictionary<T, Point>(initialCapacity, comparer);
            _positionMapping = new Dictionary<Point, T>(initialCapacity);
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
                foreach (T item in _itemMapping.Keys)
                    yield return item;
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
        /// Tries to add the given item at the given position, provided the item is not already in the
        /// spatial map and the position is not already filled. If either of those are the case,
        /// throws InvalidOperationException.
        /// </summary>
        /// <param name="item">Item to add.</param>
        /// <param name="position">Position to add item to.</param>
        public void Add(T item, Point position)
        {
            if (_itemMapping.ContainsKey(item))
                throw new InvalidOperationException($"Item added to {GetType().Name} when it has already been added.");

            if (_positionMapping.ContainsKey(position))
                throw new InvalidOperationException(
                    $"Item added to {GetType().Name} at a position already occupied by another item.");

            _itemMapping.Add(item, position);
            _positionMapping.Add(position, item);
            ItemAdded?.Invoke(this, new ItemEventArgs<T>(item, position));
        }

        /// <summary>
        /// Tries to add the given item at the given position, provided the item is not already in the
        /// spatial map and the position is not already filled. If either of those are the case,
        /// throws InvalidOperationException.
        /// </summary>
        /// <param name="item">Item to add.</param>
        /// <param name="x">X-value of the position to add item to.</param>
        /// <param name="y">Y-value of the position to add item to.</param>
        public void Add(T item, int x, int y) => Add(item, new Point(x, y));

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
        public bool Contains(int x, int y) => _positionMapping.ContainsKey(new Point(x, y));

        /// <summary>
        /// Used by foreach loop, so that the class will give ISpatialTuple objects when used in a
        /// foreach loop. Generally should never be called explicitly.
        /// </summary>
        /// <returns>An enumerator for the spatial map</returns>
        public IEnumerator<ItemPositionPair<T>> GetEnumerator()
        {
            foreach (var (item, pos) in _itemMapping)
                yield return (item, pos);
        }

        /// <summary>
        /// Generic iterator used internally by foreach loops.
        /// </summary>
        /// <returns>Enumerator to ISpatialTuple instances.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets the item at the given position as a 1-element enumerable if there is any item there,
        /// or nothing if there is nothing at that position.
        /// </summary>
        /// <remarks>
        /// Since this implementation guarantees that only one item can be at any given
        /// location at once, the return value is guaranteed to be at most one element. You may find it
        /// more convenient to use the <see cref="GetItemsAt(Point)" /> function when you know you are
        /// dealing with a SpatialMap/AdvancedSpatialMap instance.
        /// </remarks>
        /// <param name="position">The position to return the item for.</param>
        /// <returns>
        /// The item at the given position as a 1-element enumerable, if there is an item there, or
        /// nothing if there is no item there.
        /// </returns>
        public IEnumerable<T> GetItemsAt(Point position)
        {
            _positionMapping.TryGetValue(position, out var item);
            if (item != null)
                yield return item;
        }

        /// <summary>
        /// Gets the item at the given position as a 1-element enumerable if there is any item there,
        /// or nothing if there is nothing at that position.
        /// </summary>
        /// <remarks>
        /// Since this implementation guarantees that only one item can be at any given
        /// location at once, the return value is guaranteed to be at most one element. You may find it
        /// more convenient to use the <see cref="GetItemsAt(int, int)" /> function when you know you are
        /// dealing with a SpatialMap/AdvancedSpatialMap instance.
        /// </remarks>
        /// <param name="x">The x-value of the position to return the item(s) for.</param>
        /// <param name="y">The y-value of the position to return the item(s) for.</param>
        /// <returns>
        /// The item at the given position as a 1-element enumerable, if there is an item there, or
        /// nothing if there is no item there.
        /// </returns>
        public IEnumerable<T> GetItemsAt(int x, int y) => GetItemsAt(new Point(x, y));

        /// <inheritdoc />
        public Point GetPositionOf(T item)
        {
            _itemMapping.TryGetValue(item, out var pos);
            return pos;
        }

        /// <summary>
        /// Moves the item specified to the position specified. Throws InvalidOperationException if the item
        /// does not exist in the spatial map or if the position is already filled by some other item.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="target">Location to move item to.</param>
        public void Move(T item, Point target)
        {
            if (!_itemMapping.ContainsKey(item))
                throw new InvalidOperationException(
                    $"Tried to move item in {GetType().Name}, but the item does not exist.");

            if (_positionMapping.ContainsKey(target))
                throw new InvalidOperationException(
                    $"Tried to move item in {GetType().Name}, but the target position already contains an item.");

            var oldPos = _itemMapping[item];
            _positionMapping.Remove(oldPos);
            _positionMapping.Add(target, item);
            _itemMapping[item] = target;
            ItemMoved?.Invoke(this, new ItemMovedEventArgs<T>(item, oldPos, target));
        }

        /// <summary>
        /// Moves the item specified to the position specified. Throws InvalidOperationException if the item
        /// does not exist in the spatial map or if the position is already filled by some other item.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="targetX">X-value of the location to move item to.</param>
        /// <param name="targetY">Y-value of the location to move item to.</param>
        public void Move(T item, int targetX, int targetY) => Move(item, new Point(targetX, targetY));

        /// <summary>
        /// Moves whatever is at position current, if anything, to the target position, if it is a valid move.
        /// If something was moved, it returns what was moved. If nothing was moved, eg. either there was nothing at
        /// <paramref name="current" /> or already something at <paramref name="target" />, returns nothing.
        /// </summary>
        /// <remarks>
        /// Since this implementation of ISpatialMap guarantees that only one item may be at any
        /// given location at a time, the returned values will either be none, or a single value.
        /// </remarks>
        /// <param name="current">The position of the item to move.</param>
        /// <param name="target">The position to move the item to.</param>
        /// <returns>
        /// The item moved as a 1-element IEnumerable if something was moved, or nothing if no item
        /// was moved.
        /// </returns>
        public List<T> MoveValid(Point current, Point target)
        {
            var result = new List<T>();

            if (_positionMapping.ContainsKey(current) && !_positionMapping.ContainsKey(target))
            {
                var item = _positionMapping[current];
                _positionMapping.Remove(current);
                _positionMapping.Add(target, item);
                _itemMapping[item] = target;
                result.Add(item);

                ItemMoved?.Invoke(this, new ItemMovedEventArgs<T>(item, current, target));
            }

            return result;
        }

        /// <summary>
        /// Moves whatever is at the "current" position specified, if anything, to the "target" position, if
        /// it is a valid move. If something was moved, it returns what was moved. If nothing was moved, eg.
        /// either there was nothing at the "current" position given, or already something at the "target" position
        /// given, it returns nothing.
        /// </summary>
        /// <remarks>
        /// Since this implementation of ISpatialMap guarantees that only one item may be at any
        /// given location at a time, the returned values will either be none, or a single value.
        /// </remarks>
        /// <param name="currentX">X-value of the location to move item from.</param>
        /// <param name="currentY">Y-value of the location to move item from.</param>
        /// <param name="targetX">X-value of the location to move item to.</param>
        /// <param name="targetY">Y-value of the location to move item to.</param>
        /// <returns>
        /// The item moved as a 1-element IEnumerable if something was moved, or nothing if no item
        /// was moved.
        /// </returns>
        public List<T> MoveValid(int currentX, int currentY, int targetX, int targetY)
            => MoveValid(new Point(currentX, currentY), new Point(targetX, targetY));

        /// <summary>
        /// Removes the item specified. Throws InvalidOperationException if the item specified was
        /// not in the spatial map.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void Remove(T item)
        {
            if (!_itemMapping.ContainsKey(item))
                throw new InvalidOperationException(
                    $"Tried to remove an item from the {GetType().Name} that has not been added.");

            var itemPos = _itemMapping[item];
            _itemMapping.Remove(item);
            _positionMapping.Remove(itemPos);
            ItemRemoved?.Invoke(this, new ItemEventArgs<T>(item, itemPos));
        }

        /// <summary>
        /// Removes whatever is at the given position, if anything, and returns the item removed as a
        /// 1-element IEnumerable. Returns nothing if no item was at the position specified.
        /// </summary>
        /// <remarks>
        /// Since this implementation of ISpatialMap guarantees that only one item can be at any given
        /// location at a time, the returned value is guaranteed to be either nothing or a single element.
        /// </remarks>
        /// <param name="position">The position of the item to remove.</param>
        /// <returns>
        /// The item removed as a 1-element list, if something was removed; an empty list if no item
        /// was found at that position.
        /// </returns>
        public List<T> Remove(Point position)
        {
            var result = new List<T>();

            _positionMapping.TryGetValue(position, out var item);
            if (item == null)
                return result;

            _positionMapping.Remove(position);
            _itemMapping.Remove(item);
            ItemRemoved?.Invoke(this, new ItemEventArgs<T>(item, position));
            result.Add(item);

            return result;
        }

        /// <summary>
        /// Removes whatever is at the given position, if anything, and returns the item removed as a
        /// 1-element IEnumerable. Returns nothing if no item was at the position specified.
        /// </summary>
        /// <remarks>
        /// Since this implementation guarantees that only one item can be at any given
        /// location at a time, the returned value is guaranteed to be either nothing or a single element.
        /// </remarks>
        /// <param name="x">X-value of the position to remove item from.</param>
        /// <param name="y">Y-value of the position to remove item from.</param>
        /// <returns>
        /// The item removed as a 1-element IEnumerable, if something was removed; nothing if no item
        /// was found at that position.
        /// </returns>
        public List<T> Remove(int x, int y) => Remove(new Point(x, y));

        /// <summary>
        /// Returns a string representation of the spatial map, allowing display of the spatial map's
        /// items in a specified way.
        /// </summary>
        /// <param name="itemStringifier">Function that turns an item into a string.</param>
        /// <returns>A string representation of the spatial map.</returns>
        public string ToString(Func<T, string> itemStringifier)
            => _positionMapping.ExtendToString(valueStringifier: itemStringifier, pairSeparator: "; ");

        /// <summary>
        /// Returns true if the given item can be added at the given position, eg. the item is not already in the
        /// spatial map and the position is not already filled; false otherwise.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="position">Position to add item to.</param>
        /// <returns>True if the item can be successfully added at the position given; false otherwise.</returns>
        public bool CanAdd(T newItem, Point position)
        {
            if (_itemMapping.ContainsKey(newItem) || _positionMapping.ContainsKey(position))
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if the given item can be added at the given position, eg. the item is not already in the
        /// spatial map and the position is not already filled; false otherwise.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="x">X-value of the position to add item to.</param>
        /// <param name="y">Y-value of the position to add item to.</param>
        /// <returns>True if the item can be successfully added at the position given; false otherwise.</returns>
        public bool CanAdd(T newItem, int x, int y) => CanAdd(newItem, new Point(x, y));

        /// <summary>
        /// Returns true if the given item can be moved from its current location to the specified one, eg. if the item
        /// does exists in the spatial map and if the new position is not already filled by some other item; false otherwise.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="target">Location to move item to.</param>
        /// <returns>true if the given item can be moved to the given position; false otherwise.</returns>
        public bool CanMove(T item, Point target)
            => _itemMapping.ContainsKey(item) && !_positionMapping.ContainsKey(target);

        /// <summary>
        /// Returns true if the given item can be moved from its current location to the specified one, eg. if the item
        /// exists in the spatial map and if the new position is not already filled by some other item; false otherwise.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="targetX">X-value of the location to move item to.</param>
        /// <param name="targetY">Y-value of the location to move item to.</param>
        /// <returns>true if the given item can be moved to the given position; false otherwise.</returns>
        public bool CanMove(T item, int targetX, int targetY) => CanMove(item, new Point(targetX, targetY));

        /// <summary>
        /// Returns true if the item at the current position specified can be moved to the target position, eg. if an item exists
        /// at the current
        /// position and the new position is not already filled by some other item; false otherwise.
        /// </summary>
        /// <param name="current">Location to move items from.</param>
        /// <param name="target">Location to move items to.</param>
        /// <returns>
        /// true if all items at the position current can be moved to the position target; false if one or more items
        /// cannot be moved.
        /// </returns>
        public bool CanMoveAll(Point current, Point target)
            => _positionMapping.ContainsKey(current) && !_positionMapping.ContainsKey(target);

        /// <summary>
        /// Returns true if the item at the current position specified can be moved to the target position, eg. if an item exists
        /// at the current
        /// position and the new position is not already filled by some other item; false otherwise.
        /// </summary>
        /// <param name="currentX">X-value of the location to move items from.</param>
        /// <param name="currentY">Y-value of the location to move items from.</param>
        /// <param name="targetX">X-value of the location to move items to.</param>
        /// <param name="targetY">Y-value of the location to move items to.</param>
        /// <returns>
        /// true if all items at the position current can be moved to the position target; false if one or more items
        /// cannot be moved.
        /// </returns>
        public bool CanMoveAll(int currentX, int currentY, int targetX, int targetY)
            => CanMoveAll(new Point(currentX, currentY), new Point(targetX, targetY));

        /// <summary>
        /// Moves the item at the specified source location to the target location.  Throws InvalidOperationException if one or
        /// more items cannot be moved, eg.
        /// if no item exists at the current position or the new position is already filled by some other item.
        /// </summary>
        /// <param name="current">Location to move items from.</param>
        /// <param name="target">Location to move items to.</param>
        public void MoveAll(Point current, Point target)
        {
            if (!_positionMapping.ContainsKey(current))
                throw new InvalidOperationException(
                    $"Tried to move item from {current} in {GetType().Name}, but there was nothing at the that position.");

            if (current == target)
                throw new InvalidOperationException(
                    $"Tried to move all items from {current} in {GetType().Name}, but the current and target positions were the same.");

            if (_positionMapping.ContainsKey(target))
                throw new InvalidOperationException(
                    $"Tried to move item at a location in {GetType().Name}, but the target position already contains an item.");

            MoveValid(current, target);
        }

        /// <summary>
        /// Moves the item at the specified source location to the target location.  Throws InvalidOperationException if one or
        /// more items cannot be moved, eg.
        /// if no item exists at the current position or the new position is already filled by some other item.
        /// </summary>
        /// <param name="currentX">X-value of the location to move items from.</param>
        /// <param name="currentY">Y-value of the location to move items from.</param>
        /// <param name="targetX">X-value of the location to move items to.</param>
        /// <param name="targetY">Y-value of the location to move items to.</param>
        public void MoveAll(int currentX, int currentY, int targetX, int targetY)
            => MoveAll(new Point(currentX, currentY), new Point(targetX, targetY));

        /// <summary>
        /// Gets the item at the given position, or default(T) if no item exists.
        /// </summary>
        /// <remarks>
        /// Intended to be a more convenient function as compared to <see cref="GetItemsAt(Point)" />, since
        /// this spatial map implementation only allows a single item to at any given location at a time.
        /// </remarks>
        /// <param name="position">The position to return the item for.</param>
        /// <returns>
        /// The item at the given position, or default(T) if no item exists at that location.
        /// </returns>
        [return: MaybeNull]
        public T GetItem(Point position)
        {
            _positionMapping.TryGetValue(position, out var item);
            return item;
        }

        /// <summary>
        /// Gets the item at the given position, or default(T) if no item exists.
        /// </summary>
        /// <remarks>
        /// Intended to be a more convenient function as compared to <see cref="GetItemsAt(int, int)" />, since
        /// this spatial map implementation only allows a single item to at any given location at a time.
        /// </remarks>
        /// <param name="x">The x-value of the position to return the item for.</param>
        /// <param name="y">The y-value of the position to return the item for.</param>
        /// <returns>
        /// The item at the given position, or default(T) if no item exists at that location.
        /// </returns>
        [return: MaybeNull]
        public T GetItem(int x, int y) => GetItem(new Point(x, y));

        /// <summary>
        /// Returns a string representation of the spatial map.
        /// </summary>
        /// <returns>A string representation of the spatial map.</returns>
        public override string ToString() => ToString(obj => obj.ToString() ?? "null");
    }

    /// <summary>
    /// An implementation of <see cref="ISpatialMap{T}" /> that allows only one item at each position
    /// at a time.  If you need multiple items to be able to reside at one location at the same time,
    /// use <see cref="MultiSpatialMap{T}" /> or <see cref="LayeredSpatialMap{T}" /> instead.
    /// </summary>
    /// <remarks>
    /// See the <see cref="ISpatialMap{T}" /> for documentation on the practical purpose of spatial
    /// maps.
    /// The objects stored in a SpatialMap must implement <see cref="IHasID" />. This is used
    /// internally to keep track of the objects, since uints are easily (and efficiently) hash-able.
    /// </remarks>
    /// <typeparam name="T">
    /// The type of object that will be contained by this SpatialMap. Must implement <see cref="IHasID" />
    /// and be a reference-type.
    /// </typeparam>
    [PublicAPI]
    public class SpatialMap<T> : AdvancedSpatialMap<T> where T : class, IHasID
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialCapacity">
        /// The initial maximum number of elements the SpatialMap can hold before it has to
        /// internally resize data structures. Defaults to 32.
        /// </param>
        public SpatialMap(int initialCapacity = 32)
            : base(new IDComparer<T>(), initialCapacity)
        { }
    }
}
