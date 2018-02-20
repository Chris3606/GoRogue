using System;
using System.Collections;
using System.Collections.Generic;

namespace GoRogue
{
    /// <summary>
    /// See SpatialMap documentation -- similar in principle. However, this implementation allows
    /// multiple items to exist at one point in the SpatialMap, in exchange for the loss of the
    /// convenience functions like GetItem vs GetItems, as well as potential performance differences
    /// (although unless the number of objects at any given location is large, the performance is
    /// asymptotically the same).
    /// </summary>
    /// <remarks>
    /// Although SpatialMap should generally be preferred in cases where only one item is allowed at
    /// a location in the first place, this implementation may be particularly useful for situations
    /// such as inventory items, where multiple items may be desired at one location. If one is
    /// implementing something akin to "buckets", one may also subclass this implementation and
    /// provide handlers to the various events it exposes to keep track of the object on top, etc.
    ///
    /// The two implementations could also in many cases be used in combination as necessary, since
    /// they both implement the ISpatialMap interface.
    /// </remarks>
    /// <typeparam name="T">The type of items being stored.</typeparam>
    public class MultiSpatialMap<T> : ISpatialMap<T> where T : IHasID
    {
        private Dictionary<uint, SpatialTuple<T>> itemMapping;
        private Dictionary<Coord, List<SpatialTuple<T>>> positionMapping;

        /// <summary>
        /// Constructor. Creates an empty MultiSpatialMap.
        /// </summary>
        /// <param name="initialCapacity">
        /// The initial maximum number of elements the MultiSpatialMap can hold before it has to
        /// internally resize data structures. Defaults to 32.
        /// </param>
        public MultiSpatialMap(int initialCapacity = 32)
        {
            itemMapping = new Dictionary<uint, SpatialTuple<T>>(initialCapacity);
            positionMapping = new Dictionary<Coord, List<SpatialTuple<T>>>(initialCapacity);
        }

        /// <summary>
        /// See ISpatialMap.ItemAdded.
        /// </summary>
        public event EventHandler<ItemEventArgs<T>> ItemAdded;

        /// <summary>
        /// See ISpatialMap.ItemMoved.
        /// </summary>
        public event EventHandler<ItemMovedEventArgs<T>> ItemMoved;

        /// <summary>
        /// See ISpatialMap.ItemRemoved.
        /// </summary>
        public event EventHandler<ItemEventArgs<T>> ItemRemoved;

        /// <summary>
        /// See IReadOnlySpatialMap.Count.
        /// </summary>
        public int Count { get => itemMapping.Count; }

        /// <summary>
        /// See IReadOnlySpatialMap.Items.
        /// </summary>
        public IEnumerable<T> Items
        {
            get
            {
                foreach (var item in itemMapping.Values)
                    yield return item.Item;
            }
        }

        /// <summary>
        /// See IReadOnlySpatialMap.Positions.
        /// </summary>
        public IEnumerable<Coord> Positions
        {
            get
            {
                foreach (var position in positionMapping.Keys)
                    yield return position;
            }
        }

        /// <summary>
        /// Adds the given item at the given position, provided the item is not already in the
        /// MultiSpatialMap. If the item is already contained in it, does nothing and returns false.
        /// Otherwise (if item was successfully added), returns true.
        /// </summary>
        /// <param name="newItem">The item to add.</param>
        /// <param name="position">The position at which to add the new item.</param>
        /// <returns>True if the item was added, false if not.</returns>
        public bool Add(T newItem, Coord position)
        {
            if (itemMapping.ContainsKey(newItem.ID))
                return false;

            var tuple = new SpatialTuple<T>(newItem, position);
            itemMapping.Add(newItem.ID, tuple);

            if (!positionMapping.ContainsKey(position))
                positionMapping.Add(position, new List<SpatialTuple<T>>());

            positionMapping[position].Add(tuple);
            ItemAdded?.Invoke(this, new ItemEventArgs<T>(newItem, position));
            return true;
        }

        /// <summary>
        /// Returns a ReadOnly reference to the SpatialMap. Convenient for "safely" exposing a
        /// SpatialMap as a property
        /// </summary>
        /// <returns>The current SpatialMap, as a "read-only" reference.</returns>
        public IReadOnlySpatialMap<T> AsReadOnly() => (IReadOnlySpatialMap<T>)this;

        /// <summary>
        /// See IReadOnlySpatialMap.Clear.
        /// </summary>
        public void Clear()
        {
            itemMapping.Clear();
            positionMapping.Clear();
        }

        /// <summary>
        /// See IReadOnlySpatialMap.Contains.
        /// </summary>
        public bool Contains(T item) => itemMapping.ContainsKey(item.ID);

        /// <summary>
        /// See IReadOnlySpatialMap.Contains.
        /// </summary>
        public bool Contains(Coord position) => positionMapping.ContainsKey(position);

        /// <summary>
        /// Used by foreach loop, so that the class will give ISpatialTuple objects when used in a
        /// foreach loop. Generally should never be called explicitly.
        /// </summary>
        /// <returns>An enumerator for the SpatialMap</returns>
        public IEnumerator<ISpatialTuple<T>> GetEnumerator()
        {
            foreach (var tuple in itemMapping.Values)
                yield return tuple;
        }

        /// <summary>
        /// Non-generic verion of enumerable used by foreach loop internally.
        /// </summary>
        /// <returns>Enumerator of ISpatialTuples.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// See IReadOnlySpatialMap.GetItems.
        /// </summary>
        public IEnumerable<T> GetItems(Coord position)
        {
            if (positionMapping.ContainsKey(position))
                foreach (var tuple in positionMapping[position])
                    yield return tuple.Item;
        }

        /// <summary>
        /// See IReadOnlySpatialMap.GetPosition.
        /// </summary>
        public Coord GetPosition(T item)
        {
            SpatialTuple<T> tuple;
            itemMapping.TryGetValue(item.ID, out tuple);
            if (tuple == null) return null;
            return tuple.Position;
        }

        /// <summary>
        /// Move the item specified to the position specified. Returns true if successful. If the
        /// item does not exist in the MultiSpatialMap, does nothing and returns false. Otherwise,
        /// returns true.
        /// </summary>
        /// <param name="item">The item to move.</param>
        /// <param name="target">The position to move it to.</param>
        /// <returns>True if the item was moved, false if not.</returns>
        public bool Move(T item, Coord target)
        {
            if (!itemMapping.ContainsKey(item.ID))
                return false;

            var movingTuple = itemMapping[item.ID];

            Coord oldPos = movingTuple.Position;
            positionMapping[movingTuple.Position].Remove(movingTuple);
            if (positionMapping[movingTuple.Position].Count == 0)
                positionMapping.Remove(movingTuple.Position);

            movingTuple.Position = target;
            if (!positionMapping.ContainsKey(target))
                positionMapping[target] = new List<SpatialTuple<T>>();
            positionMapping[target].Add(movingTuple);
            ItemMoved?.Invoke(this, new ItemMovedEventArgs<T>(item, oldPos, target));
            return true;
        }

        /// <summary>
        /// Moves everything at position current, if anything, to postion target. If something was
        /// moved, returns everything that was moved. If nothing was moved, eg. there was nothing at
        /// position current, returns nothing.
        /// </summary>
        /// <param name="current">The position of the items to move.</param>
        /// <param name="target">The position to move the item to.</param>
        /// <returns>The items moved if something was moved, or nothing if no item was moved.</returns>
        public IEnumerable<T> Move(Coord current, Coord target)
        {
            if (positionMapping.ContainsKey(current))
            {
                if (!positionMapping.ContainsKey(target))
                    positionMapping.Add(target, new List<SpatialTuple<T>>());

                var movingTuple = positionMapping[current];
                foreach (var tuple in positionMapping[current])
                {
                    tuple.Position = target;
                    positionMapping[target].Add(tuple);
                    yield return tuple.Item;
                }

                var list = positionMapping[current];
                positionMapping.Remove(current);

                if (ItemMoved != null)
                {
                    foreach (var tuple in list)
                        ItemMoved(this, new ItemMovedEventArgs<T>(tuple.Item, current, target));
                }
            }
        }

        /// <summary>
        /// Removes the item specified, if it exists, and returns true. Returns false if the item was
        /// not in the MultiSpatialMap.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the item was removed, false if the item was not found.</returns>
        public bool Remove(T item)
        {
            if (!itemMapping.ContainsKey(item.ID))
                return false;

            var tuple = itemMapping[item.ID];
            itemMapping.Remove(item.ID);
            positionMapping[tuple.Position].Remove(tuple);

            if (positionMapping[tuple.Position].Count == 0)
                positionMapping.Remove(tuple.Position);

            ItemRemoved?.Invoke(this, new ItemEventArgs<T>(item, tuple.Position));
            return true;
        }

        /// <summary>
        /// Removes everything at the given position, if anything, and returns the items removed.
        /// Returns nothing if no item was at the position specified.
        /// </summary>
        /// <param name="position">The position of the item to remove.</param>
        /// <returns>
        /// The items removed, if any were removed; nothing if no item was found at that position.
        /// </returns>
        public IEnumerable<T> Remove(Coord position)
        {
            if (positionMapping.ContainsKey(position))
            {
                foreach (var tuple in positionMapping[position])
                {
                    itemMapping.Remove(tuple.Item.ID);
                    yield return tuple.Item;
                }

                var list = positionMapping[position];
                positionMapping.Remove(position);
                if (ItemRemoved != null)
                    foreach (var tuple in list)
                        ItemRemoved(this, new ItemEventArgs<T>(tuple.Item, position));
            }
        }
    }
}