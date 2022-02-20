using System;
using System.Collections;
using System.Collections.Generic;
using GoRogue.Pooling;
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
        private readonly Dictionary<T, Point> _itemMapping;
        private readonly Dictionary<Point, List<T>> _positionMapping;

        private readonly IListPool<T> _itemListPool;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="itemComparer">
        /// Equality comparer to use for comparison and hashing of type T. Be especially mindful of the
        /// efficiency of its GetHashCode function, as it will determine the efficiency of many AdvancedMultiSpatialMap
        /// functions.
        /// </param>
        /// <param name="pointComparer">
        /// Equality comparer to use for comparison and hashing of points, as object are added to/removed from/moved
        /// around the spatial map.  Be especially mindful of the efficiency of its GetHashCode function, as it will
        /// determine the efficiency of many AdvancedMultiSpatialMap functions.  Defaults to the default equality
        /// comparer for Point, which uses a fairly efficient generalized hashing algorithm.
        /// </param>
        /// <param name="initialCapacity">
        /// The initial maximum number of elements the AdvancedMultiSpatialMap can hold before it has to
        /// internally resize data structures. Defaults to 32.
        /// </param>
        public AdvancedMultiSpatialMap(IEqualityComparer<T> itemComparer, IEqualityComparer<Point>? pointComparer = null,
                                       int initialCapacity = 32)
            : this(itemComparer, new ListPool<T>(50, 16), pointComparer, initialCapacity)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="itemComparer">
        /// Equality comparer to use for comparison and hashing of type T. Be especially mindful of the
        /// efficiency of its GetHashCode function, as it will determine the efficiency of many AdvancedMultiSpatialMap
        /// functions.
        /// </param>
        /// <param name="listPool">
        /// The list pool implementation to use.  Specify <see cref="NoPoolingListPool{T}"/> to disable pooling entirely.
        /// This implementation _may_ be shared with other spatial maps if you wish, however be aware that no thread safety is implemented
        /// by the default list pool implementations or the spatial map itself.
        /// </param>
        /// <param name="pointComparer">
        /// Equality comparer to use for comparison and hashing of points, as object are added to/removed from/moved
        /// around the spatial map.  Be especially mindful of the efficiency of its GetHashCode function, as it will
        /// determine the efficiency of many AdvancedMultiSpatialMap functions.  Defaults to the default equality
        /// comparer for Point, which uses a fairly efficient generalized hashing algorithm.
        /// </param>
        /// <param name="initialCapacity">
        /// The initial maximum number of elements the AdvancedMultiSpatialMap can hold before it has to
        /// internally resize data structures. Defaults to 32.
        /// </param>
        public AdvancedMultiSpatialMap(IEqualityComparer<T> itemComparer, IListPool<T> listPool,
                                       IEqualityComparer<Point>? pointComparer = null,
                                       int initialCapacity = 32)
        {
            _itemMapping = new Dictionary<T, Point>(initialCapacity, itemComparer);
            _positionMapping = new Dictionary<Point, List<T>>(initialCapacity, pointComparer ?? EqualityComparer<Point>.Default);

            _itemListPool = listPool;
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
                foreach (var item in _itemMapping.Keys)
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
        /// Adds the given item at the given position, provided the item is not already in the
        /// spatial map. If the item is already added, throws ArgumentException.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="position">The position at which to add the new item.</param>
        public void Add(T item, Point position)
        {
            try
            {
                _itemMapping.Add(item, position);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(
                    $"Item added to {GetType().Name} when it has already been added.",
                    nameof(item));
            }

            if (!_positionMapping.TryGetValue(position, out List<T>? positionList))
                _positionMapping[position] = positionList = _itemListPool.Rent();

            positionList.Add(item);
            ItemAdded?.Invoke(this, new ItemEventArgs<T>(item, position));
        }

        /// <summary>
        /// Adds the given item at the given position, provided the item is not already in the
        /// spatial map. If the item is already added, throws ArgumentException.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="x">x-value of the position to add item to.</param>
        /// <param name="y">y-value of the position to add item to.</param>
        public void Add(T item, int x, int y) => Add(item, new Point(x, y));

        /// <summary>
        /// Adds the given item at the given position, provided the item is not already in the
        /// spatial map. If the item is already added, returns false.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="position">The position at which to add the new item.</param>
        /// <returns>True if the item was successfully added; false otherwise.</returns>
        public bool TryAdd(T item, Point position)
        {
            if (!_itemMapping.TryAdd(item, position))
                return false;

            if (!_positionMapping.TryGetValue(position, out List<T>? positionList))
                _positionMapping[position] = positionList = _itemListPool.Rent();

            positionList.Add(item);
            ItemAdded?.Invoke(this, new ItemEventArgs<T>(item, position));

            return true;
        }

        /// <summary>
        /// Adds the given item at the given position, provided the item is not already in the
        /// spatial map. If the item is already added, returns false.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="x">x-value of the position to add item to.</param>
        /// <param name="y">y-value of the position to add item to.</param>
        /// <returns>True if the item was successfully added; false otherwise.</returns>
        public bool TryAdd(T item, int x, int y) => TryAdd(item, new Point(x, y));

        /// <inheritdoc />
        public IReadOnlySpatialMap<T> AsReadOnly() => this;

        /// <inheritdoc />
        public void Clear()
        {
            _itemMapping.Clear();

            foreach (var val in _positionMapping.Values)
                _itemListPool.Return(val);
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
        public IEnumerator<ItemPositionPair<T>> GetEnumerator()
        {
            foreach (var (item, pos) in _itemMapping)
                yield return (item, pos);
        }

        /// <summary>
        /// Non-generic version of enumerable used by foreach loop internally.
        /// </summary>
        /// <returns>Enumerator of ISpatialTuples.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerable<T> GetItemsAt(Point position)
        {
            if (!_positionMapping.TryGetValue(position, out var positionList))
                yield break;

            for (var i = positionList.Count - 1; i >= 0; i--)
                yield return positionList[i];
        }

        /// <inheritdoc />
        public IEnumerable<T> GetItemsAt(int x, int y) => GetItemsAt(new Point(x, y));

        /// <inheritdoc />
        public Point? GetPositionOfOrNull(T item)
        {
            if (_itemMapping.TryGetValue(item, out var pos))
                return pos;

            return null;
        }

        /// <inheritdoc />
        public bool TryGetPositionOf(T item, out Point position) => _itemMapping.TryGetValue(item, out position);

        /// <inheritdoc />
        public Point GetPositionOf(T item)
        {
            if (_itemMapping.TryGetValue(item, out var pos))
                return pos;

            throw new ArgumentException("Item position requested for an item that was not in the spatial map.", nameof(item));
        }

        /// <summary>
        /// Moves the item specified to the position specified. If the item does not exist in the
        /// spatial map or is already at the target position, the function throws ArgumentException.
        /// </summary>
        /// <param name="item">The item to move.</param>
        /// <param name="target">The position to move it to.</param>
        public void Move(T item, Point target)
        {
            Point oldPos;
            try
            {
                oldPos = _itemMapping[item];
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(
                    $"Tried to move item in {GetType().Name}, but the item does not exist.",
                    nameof(item));
            }

            if (oldPos == target)
                throw new ArgumentException(
                    $"Tried to move item in {GetType().Name}, but the item was already at the target position.",
                    nameof(target));

            // Key guaranteed to exist due to state invariant of spatial map (oldPos existed in the other map)
            var oldPosList = _positionMapping[oldPos];

            // We'll get the target list now as well, since we can do some special case shortcutting if the target doesn't
            // exist and the source has only one element.  C# doesn't offer a nice Get-Or-Insert type function, so this
            // will have to do.  This at least keeps it to two lookups max.
            if (!_positionMapping.TryGetValue(target, out var targetList))
            {
                // If the existing list has only the item we're moving, and the target doesn't exist, we'll just
                // switch the list over to avoid any removing and interacting with the pool.  This also handles a special case
                // where no list exists in the pool, but the one for the old position is about to be freed.  This ensures
                // that, in this case, the list will simply be hot-swapped over instead of a new one allocated then the
                // old one added to the pool after.
                if (oldPosList.Count == 1)
                {
                    _positionMapping[target] = oldPosList;
                    _positionMapping.Remove(oldPos);
                    _itemMapping[item] = target;
                    ItemMoved?.Invoke(this, new ItemMovedEventArgs<T>(item, oldPos, target));
                    return;
                }

                // Otherwise, we'll have to get a new list.
                _positionMapping[target] = targetList = _itemListPool.Rent();
            }

            // Add item to target list
            targetList.Add(item);

            // Remove the old one, and if it was the last item, return the list to the pool.  It could be the last
            // item if and only if the target list already existed (so the above code does not return)
            oldPosList.Remove(item);
            if (oldPosList.Count == 0)
            {
                _itemListPool.Return(oldPosList, false);
                _positionMapping.Remove(oldPos);
            }

            // Switch position of item in spatial map, and fire moved event.
            _itemMapping[item] = target;
            ItemMoved?.Invoke(this, new ItemMovedEventArgs<T>(item, oldPos, target));
        }

        /// <summary>
        /// Moves the item specified to the position specified. If the item does not exist in the
        /// spatial map or is already at the target position, the function throws ArgumentException.
        /// </summary>
        /// <param name="item">The item to move.</param>
        /// <param name="targetX">X-value of the location to move it to.</param>
        /// <param name="targetY">Y-value of the location to move it to.</param>
        public void Move(T item, int targetX, int targetY) => Move(item, new Point(targetX, targetY));

        /// <inheritdoc />
        public bool TryMove(T item, Point target)
        {
            if (!_itemMapping.TryGetValue(item, out Point oldPos))
                return false;

            if (oldPos == target)
                return false;

            // Key guaranteed to exist due to state invariant of spatial map (oldPos existed in the other map)
            var oldPosList = _positionMapping[oldPos];

            // We'll get the target list now as well, since we can do some special case shortcutting if the target doesn't
            // exist and the source has only one element.  C# doesn't offer a nice Get-Or-Insert type function, so this
            // will have to do.  This at least keeps it to two lookups max.
            if (!_positionMapping.TryGetValue(target, out var targetList))
            {
                // If the existing list has only the item we're moving, and the target doesn't exist, we'll just
                // switch the list over to avoid any removing and interacting with the pool.  This also handles a special case
                // where no list exists in the pool, but the one for the old position is about to be freed.  This ensures
                // that, in this case, the list will simply be hot-swapped over instead of a new one allocated then the
                // old one added to the pool after.
                if (oldPosList.Count == 1)
                {
                    _positionMapping[target] = oldPosList;
                    _positionMapping.Remove(oldPos);
                    _itemMapping[item] = target;
                    ItemMoved?.Invoke(this, new ItemMovedEventArgs<T>(item, oldPos, target));
                    return true;
                }

                // Otherwise, we'll have to get a new list.
                _positionMapping[target] = targetList = _itemListPool.Rent();
            }

            // Add item to target list
            targetList.Add(item);

            // Remove the old one, and if it was the last item, return the list to the pool.  It could be the last
            // item if and only if the target list already existed (so the above code does not return)
            oldPosList.Remove(item);
            if (oldPosList.Count == 0)
            {
                _itemListPool.Return(oldPosList, false);
                _positionMapping.Remove(oldPos);
            }

            // Switch position of item in spatial map, and fire moved event.
            _itemMapping[item] = target;
            ItemMoved?.Invoke(this, new ItemMovedEventArgs<T>(item, oldPos, target));

            return true;
        }

        /// <inheritdoc />
        public bool TryMove(T item, int targetX, int targetY) => TryMove(item, new Point(targetX, targetY));

        /// <inheritdoc />
        public List<T> MoveValid(Point current, Point target)
        {
            // Nothing to move in these cases
            if (current == target)
                return new List<T>();

            if (!_positionMapping.TryGetValue(current, out var currentList))
                return new List<T>();

            // Anything will successfully move to anywhere; so we know at this point that everything will move to target.
            // So, we'll just bulk copy the list over and remove it from the old position
            var result = new List<T>(currentList);
            _positionMapping.Remove(current);

            // C# doesn't offer a nice Get-Or-Insert type function, so this will have to do.
            // This at least keeps it to two lookups max.
            if (!_positionMapping.TryGetValue(target, out var targetList))
            {
                // If there isn't a target list, we can just switch the old list over to the new one to avoid
                // any allocations or unnecessary interaction with the list pool.
                _positionMapping[target] = currentList;
            }
            else
            {
                // When the target list already exists, we'll have to append to it, and put the original list back
                // in the pool.  One alternative would be to use the currentList as the return value, which could
                // avoid the allocation of a new one; however it would affect the number of lists in the pool, so
                // in theory, long-term, it should be more beneficial to keep the two list pool separate (because it
                // should make future move operations faster)
                targetList.AddRange(currentList);
                _itemListPool.Return(currentList);
            }

            // Shift the item-to-position mappings for everything that was just moved
            int count = result.Count;
            for (int i = 0; i < count; i++)
                _itemMapping[result[i]] = target;

            // Fire moved events as needed for what was just moved and return; making sure we do this _after_ the actual
            // data structure state has been updated for the entire move operation
            if (ItemMoved != null)
            {
                for (int i = 0; i < count; i++)
                    ItemMoved(this, new ItemMovedEventArgs<T>(result[i], current, target));
            }

            return result;
        }

        /// <inheritdoc />
        public List<T> MoveValid(int currentX, int currentY, int targetX, int targetY)
            => MoveValid(new Point(currentX, currentY), new Point(targetX, targetY));

        /// <summary>
        /// Removes the item specified, if it exists.  Throws ArgumentException if the item is
        /// not in the spatial map.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void Remove(T item)
        {
            Point pos;
            try
            {
                pos = _itemMapping[item];
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(
                    $"Tried to remove an item from the {GetType().Name} that has not been added.",
                    nameof(item));
            }

            _itemMapping.Remove(item);

            // Key guaranteed to exist due to state invariant of spatial map (oldPos existed in the other map)
            var posList = _positionMapping[pos];
            posList.Remove(item);
            if (posList.Count == 0)
            {
                _itemListPool.Return(posList, false);
                _positionMapping.Remove(pos);
            }

            ItemRemoved?.Invoke(this, new ItemEventArgs<T>(item, pos));
        }

        /// <summary>
        /// Removes the item specified, if it exists.  If the item is not in the spatial map, returns false.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the item was successfully removed; false otherwise.</returns>
        public bool TryRemove(T item)
        {
            if (!_itemMapping.TryGetValue(item, out var pos))
                return false;

            _itemMapping.Remove(item);

            // Key guaranteed to exist due to state invariant of spatial map (oldPos existed in the other map)
            var posList = _positionMapping[pos];
            posList.Remove(item);
            if (posList.Count == 0)
            {
                _itemListPool.Return(posList, false);
                _positionMapping.Remove(pos);
            }

            ItemRemoved?.Invoke(this, new ItemEventArgs<T>(item, pos));

            return true;
        }

        /// <inheritdoc />
        public List<T> Remove(Point position)
        {
            if (!_positionMapping.TryGetValue(position, out var posList))
                return new List<T>();

            // We'll create a new list to return.  We could just return the one that
            // is in the _positionMapping, but we'll return it to the list pool instead
            // in order to ensure it's available for a potential future Add operation
            // (which could often be paired with remove)
            var result = new List<T>(posList);
            _positionMapping.Remove(position);

            // Remove each object that we're removing from the item mapping
            int count = posList.Count;
            for (int i = 0; i < count; i++)
                _itemMapping.Remove(posList[i]);

            // Fire ItemRemoved event for each item we're removing
            if (ItemRemoved != null)
                foreach (var item in posList)
                    ItemRemoved(this, new ItemEventArgs<T>(item, position));

            // Return list to pool and return result
            _itemListPool.Return(posList);

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
                    obj.ExtendToString(elementStringifier: itemStringifier),
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
        /// Moves all items at the specified source location to the target location.  Throws ArgumentException if there are
        /// no items to be moved.
        /// </summary>
        /// <param name="current">Location to move items from.</param>
        /// <param name="target">Location to move items to.</param>
        public void MoveAll(Point current, Point target)
        {
            if (current == target)
                throw new ArgumentException(
                    $"Tried to move all items from {current} in {GetType().Name}, but the current and target positions were the same.",
                    nameof(target));

            List<T> currentList;
            try
            {
                currentList = _positionMapping[current];
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(
                    $"Tried to move all items from {current} in {GetType().Name}, but there was nothing at that position.",
                    nameof(current));
            }

            // We know the move will succeed, since they don't fail in MultiSpatialMap; so we can go ahead and remove
            // the old position list now.
            _positionMapping.Remove(current);

            // C# doesn't offer a nice Get-Or-Insert type function, so this will have to do.
            // This at least keeps it to two lookups max.
            if (!_positionMapping.TryGetValue(target, out var targetList))
            {
                // If there isn't a target list, we can just switch the old list over to the new one to avoid
                // any allocations or unnecessary interaction with the list pool.
                _positionMapping[target] = currentList;
            }
            else
            {
                // When the target list already exists, we'll have to append to it.
                targetList.AddRange(currentList);
            }

            // Shift the item-to-position mappings for everything that was just moved
            int count = currentList.Count;
            for (int i = 0; i < count; i++)
                _itemMapping[currentList[i]] = target;

            // Fire moved events as needed for what was just moved and return; making sure we do this _after_ the actual
            // data structure state has been updated for the entire move operation
            if (ItemMoved != null)
            {
                for (int i = 0; i < count; i++)
                    ItemMoved(this, new ItemMovedEventArgs<T>(currentList[i], current, target));
            }

            // Add the currentList to the pool if we didn't re-use it as targetList
            if (targetList != null)
                _itemListPool.Return(currentList);
        }

        /// <summary>
        /// Moves all items at the specified source location to the target location.  Throws ArgumentException if there are
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
    public sealed class MultiSpatialMap<T> : AdvancedMultiSpatialMap<T> where T : class, IHasID
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listPool">
        /// The list pool implementation to use.  Specify <see cref="NoPoolingListPool{T}"/> to disable pooling entirely.
        /// This implementation _may_ be shared with other spatial maps if you wish, however be aware that no thread safety is implemented
        /// by the default list pool implementations or the spatial map itself.
        /// </param>
        /// <param name="pointComparer">
        /// Equality comparer to use for comparison and hashing of points, as object are added to/removed from/moved
        /// around the spatial map.  Be especially mindful of the efficiency of its GetHashCode function, as it will
        /// determine the efficiency of many MultiSpatialMap functions.  Defaults to the default equality
        /// comparer for Point, which uses a fairly efficient generalized hashing algorithm.
        /// </param>
        /// <param name="initialCapacity">
        /// The initial maximum number of elements the spatial map can hold before it has to
        /// internally resize data structures. Defaults to 32.
        /// </param>
        public MultiSpatialMap(IListPool<T> listPool, IEqualityComparer<Point>? pointComparer = null, int initialCapacity = 32)
            : base(new IDComparer<T>(), listPool, pointComparer, initialCapacity)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pointComparer">
        /// Equality comparer to use for comparison and hashing of points, as object are added to/removed from/moved
        /// around the spatial map.  Be especially mindful of the efficiency of its GetHashCode function, as it will
        /// determine the efficiency of many MultiSpatialMap functions.  Defaults to the default equality
        /// comparer for Point, which uses a fairly efficient generalized hashing algorithm.
        /// </param>
        /// <param name="initialCapacity">
        /// The initial maximum number of elements the spatial map can hold before it has to
        /// internally resize data structures. Defaults to 32.
        /// </param>
        public MultiSpatialMap(IEqualityComparer<Point>? pointComparer = null, int initialCapacity = 32)
            : base(new IDComparer<T>(), pointComparer, initialCapacity)
        { }
    }
}
