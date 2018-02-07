using System;
using System.Collections;
using System.Collections.Generic;

namespace GoRogue
{
    /// <summary>
    /// Designed as an more efficient data structure for recording objects on a map. The simple
    /// version: if you're about to use a List to store a bunch of objects in your map, consider
    /// using this or MultiSpatialMap instead!
    ///
    /// More detail: typical roguelikes will use a 2D array (or 1D array accessed as a 2D array), for
    /// terrain, and lists of objects for things like entities, items, etc. This is simple but
    /// ultimately not efficient; for example, in that implementation, determining if there is an
    /// object at a location takes an amount of time proportional to the number of objects in this
    /// list. However, the other simple option is to use an array with size equal to the size of the
    /// map (as many do for terrain) for all object lists. This is even less ideal, as in that case,
    /// the time to iterate over all objects becomes proportional to the size of the map (since one
    /// has to do that for rendering, ouch!), which is typically much larger than the number of
    /// objects in a list. This is the problem SpatialMap is designed to solve. It provides fast,
    /// near-constant-time operations for getting the object at a location, adding entities, removing
    /// entities, and will allow you to iterate through all objects in the SpatialMap in time
    /// proportional to the number of objects in it (the best possible).
    ///
    /// Effectively, it is a more efficient list for objects that have a position associated with
    /// them. This implementation can only allow one item at a given location at a time -- for an
    /// implementation that allows multiple items, see MultiSpatialMap.
    ///
    /// The objects stored in a SpatialMap must implement the IHasID (see that interface's
    /// documentation for an easy implementation example). This is used internally to keep track of
    /// the objects, since uints are easily hashable.
    /// </summary>
    /// <typeparam name="T">
    /// The type of object that will be contained by this SpatialMap.
    /// </typeparam>
    public class SpatialMap<T> : ISpatialMap<T> where T : IHasID
    {
        private Dictionary<uint, SpatialTuple<T>> itemMapping;
        private Dictionary<Coord, SpatialTuple<T>> positionMapping;

        /// <summary>
        /// Constructor. Creates an empty SpatialMap.
        /// </summary>
        /// <param name="initialCapacity">
        /// The initial maximum number of elements the SpatialMap can hold before it has to
        /// internally resize data structures. Defaults to 32.
        /// </param>
        public SpatialMap(int initialCapacity = 32)
        {
            itemMapping = new Dictionary<uint, SpatialTuple<T>>(initialCapacity);
            positionMapping = new Dictionary<Coord, SpatialTuple<T>>(initialCapacity);
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
        public int Count => itemMapping.Count;

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
        /// SpatialMap and the position is not already filled. If either of those are the case,
        /// returns false. Otherwise (if item was successfully added), returns true.
        /// </summary>
        /// <param name="newItem">
        /// The item to add.
        /// </param>
        /// <param name="position">
        /// The position at which to add the new item.
        /// </param>
        /// <returns>
        /// True if the item was added, false if not.
        /// </returns>
        public bool Add(T newItem, Coord position)
        {
            if (itemMapping.ContainsKey(newItem.ID))
                return false;

            if (positionMapping.ContainsKey(position))
                return false;

            var tuple = new SpatialTuple<T>(newItem, position);
            itemMapping.Add(newItem.ID, tuple);
            positionMapping.Add(position, tuple);
            ItemAdded?.Invoke(this, new ItemEventArgs<T>(newItem, position));
            return true;
        }

        /// <summary>
        /// See IReadOnlySpatialMap.AsReadOnly.
        /// </summary>
        public IReadOnlySpatialMap<T> AsReadOnly() => this;

        /// <summary>
        /// See ISpatialMap.Clear.
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
        /// <returns>
        /// An enumerator for the SpatialMap
        /// </returns>
        public IEnumerator<ISpatialTuple<T>> GetEnumerator()
        {
            foreach (var tuple in itemMapping.Values)
                yield return tuple;
        }

        /// <summary>
        /// Generic iterator used internally by foreach loops.
        /// </summary>
        /// <returns>
        /// Enumerator to ISpatialTuple instances.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets the item at the given position, or null/equivalent if no item exists.
        /// </summary>
        /// <remarks>
        /// Intended to be a more convenient function as compared to GetItems, for times when you are
        /// dealing exclusively with SpatialMap instances.
        /// </remarks>
        /// <param name="position">
        /// The postiion to return the item for.
        /// </param>
        /// <returns>
        /// The item at the given position, or null/equivalent if no item exists at that location.
        /// </returns>
        public T GetItem(Coord position)
        {
            SpatialTuple<T> tuple;
            positionMapping.TryGetValue(position, out tuple);
            if (tuple != null)
                return tuple.Item;

            return default(T);
        }

        /// <summary>
        /// Gets the item at the given position as a 1-element enumerable if there is any item there,
        /// or nothing if there is nothing at that position.
        /// </summary>
        /// <remarks>
        /// Again, since this implementation guarantees that only one item can be at any given
        /// location at once, the return value is guaranteed to be at most one element. For times
        /// when you are know you are dealing exclusively with SpatialMap instances, this specific
        /// class also provides a GetItem function that returns a more intuitive null if no item was
        /// found, or the item at the location as applicable.
        /// </remarks>
        /// <param name="position">
        /// The position to return the item for.
        /// </param>
        /// <returns>
        /// The item at the given position as a 1-element enumerable, if there is an item there, or
        /// nothing if there is no item there.
        /// </returns>
        public IEnumerable<T> GetItems(Coord position)
        {
            SpatialTuple<T> tuple;
            positionMapping.TryGetValue(position, out tuple);
            if (tuple != null)
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
        /// item does not exist in the SpatialMap, or the position is already filled by something,
        /// does nothing and returns false. Otherwise, returns true.
        /// </summary>
        /// <param name="item">
        /// The item to move.
        /// </param>
        /// <param name="target">
        /// The position to move it to.
        /// </param>
        /// <returns>
        /// True if the item was moved, false if not.
        /// </returns>
        public bool Move(T item, Coord target)
        {
            if (!itemMapping.ContainsKey(item.ID))
                return false;

            if (positionMapping.ContainsKey(target))
                return false;

            var movingTuple = itemMapping[item.ID];
            Coord oldPos = movingTuple.Position;
            positionMapping.Remove(movingTuple.Position);
            movingTuple.Position = target;
            positionMapping.Add(target, movingTuple);
            ItemMoved?.Invoke(this, new ItemMovedEventArgs<T>(item, oldPos, target));
            return true;
        }

        /// <summary>
        /// Moves whatever is at position current, if anything, to postion target. If something was
        /// moved, returns what was moved. If nothing was moved, eg. either there was nothing at
        /// position current or already something at position target, returns nothing.
        /// </summary>
        /// <remarks>
        /// Since this implementation of ISpatialMap guarantees that only one item may be at any
        /// given location at a time, the returned values will either be none, or a single value.
        /// </remarks>
        /// <param name="current">
        /// The position of the item to move.
        /// </param>
        /// <param name="target">
        /// The position to move the item to.
        /// </param>
        /// <returns>
        /// The item moved as a 1-element IEnumerable if something was moved, or nothing if no item
        /// was moved.
        /// </returns>
        public IEnumerable<T> Move(Coord current, Coord target)
        {
            if (positionMapping.ContainsKey(current) && !positionMapping.ContainsKey(target))
            {
                var movingTuple = positionMapping[current];
                positionMapping.Remove(current);
                movingTuple.Position = target;
                positionMapping.Add(target, movingTuple);
                yield return movingTuple.Item;

                ItemMoved?.Invoke(this, new ItemMovedEventArgs<T>(movingTuple.Item, current, target));
            }
        }

        /// <summary>
        /// Removes the item specified, if it exists, and returns true. Returns false if the item was
        /// not in the SpatialMap.
        /// </summary>
        /// <param name="item">
        /// The item to remove.
        /// </param>
        /// <returns>
        /// True if the item was removed, false if the item was not found.
        /// </returns>
        public bool Remove(T item)
        {
            if (!itemMapping.ContainsKey(item.ID))
                return false;

            var tuple = itemMapping[item.ID];
            itemMapping.Remove(item.ID);
            positionMapping.Remove(tuple.Position);
            ItemRemoved?.Invoke(this, new ItemEventArgs<T>(item, tuple.Position));
            return true;
        }

        /// <summary>
        /// Removes whatever is at the given position, if anything, and returns the item removed as a
        /// 1-element IEnumerable. Returns nothing if no item was at the position specified.
        /// </summary>
        /// <remarks>
        /// Again, since this implementation guarantees that only one item can be at any given
        /// location at a time, the returned value is guaranteed to be either nothing or a single element.
        /// </remarks>
        /// <param name="position">
        /// The position of the item to remove.
        /// </param>
        /// <returns>
        /// The item removed as a 1-element IEnumerable, if something was removed; nothing if no item
        /// was found at that position.
        /// </returns>
        public IEnumerable<T> Remove(Coord position)
        {
            SpatialTuple<T> tuple;
            positionMapping.TryGetValue(position, out tuple);
            if (tuple != null)
            {
                positionMapping.Remove(position);
                itemMapping.Remove(tuple.Item.ID);
                ItemRemoved?.Invoke(this, new ItemEventArgs<T>(tuple.Item, tuple.Position));
                yield return tuple.Item;
            }
        }
    }

    internal class SpatialTuple<T> : ISpatialTuple<T> where T : IHasID
    {
        public SpatialTuple(T item, Coord position)
        {
            Item = item;
            Position = position;
        }

        public T Item { get; set; }
        public Coord Position { get; set; }
    }
}