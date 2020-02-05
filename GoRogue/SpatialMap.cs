using System;
using System.Collections;
using System.Collections.Generic;

namespace GoRogue
{
	/// <summary>
	/// A more complex version of <see cref="SpatialMap{T}"/> that does not require the items in it to implement
	/// <see cref="IHasID"/>, instead requiring the specification of a custom <see cref="IEqualityComparer{T}"/>
	/// to use for hashing and comparison of items.
	/// </summary>
	/// <remarks>
	/// This class is useful for cases where you do not want to implement <see cref="IHasID"/>, or if you need
	/// to use a value type in a spatial map. For simple cases, it is recommended to use <see cref="SpatialMap{T}"/>
	/// instead.
	/// 
	/// Be mindful of the efficiency of your hashing function specified in the <see cref="IEqualityComparer{T}"/> --
	/// it will in large part determine the performance of AdvancedSpatialMap!
	/// </remarks>
	/// <typeparam name="T">The type of object that will be contained by this AdvancedSpatialMap.</typeparam>
	public class AdvancedSpatialMap<T> : ISpatialMap<T>
	{
		private Dictionary<T, SpatialTuple<T>> itemMapping;
		private Dictionary<Coord, SpatialTuple<T>> positionMapping;

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
			itemMapping = new Dictionary<T, SpatialTuple<T>>(initialCapacity, comparer);
			positionMapping = new Dictionary<Coord, SpatialTuple<T>>(initialCapacity);
		}

		/// <summary>
		/// See <see cref="IReadOnlySpatialMap{T}.ItemAdded"/>.
		/// </summary>
		public event EventHandler<ItemEventArgs<T>> ItemAdded;

		/// <summary>
		/// See <see cref="IReadOnlySpatialMap{T}.ItemMoved"/>.
		/// </summary>
		public event EventHandler<ItemMovedEventArgs<T>> ItemMoved;

		/// <summary>
		/// See <see cref="IReadOnlySpatialMap{T}.ItemRemoved"/>.
		/// </summary>
		public event EventHandler<ItemEventArgs<T>> ItemRemoved;

		/// <summary>
		/// See <see cref="IReadOnlySpatialMap{T}.Count"/>.
		/// </summary>
		public int Count => itemMapping.Count;

		/// <summary>
		/// See <see cref="IReadOnlySpatialMap{T}.Items"/>.
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
		/// See <see cref="IReadOnlySpatialMap{T}.Positions"/>.
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
		/// spatial map and the position is not already filled. If either of those are the case,
		/// returns false. Otherwise (if item was successfully added), returns true.
		/// </summary>
		/// <param name="newItem">The item to add.</param>
		/// <param name="position">The position at which to add the new item.</param>
		/// <returns>True if the item was added, false if adding the item failed.</returns>
		public bool Add(T newItem, Coord position)
		{
			if (itemMapping.ContainsKey(newItem))
				return false;

			if (positionMapping.ContainsKey(position))
				return false;

			var tuple = new SpatialTuple<T>(newItem, position);
			itemMapping.Add(newItem, tuple);
			positionMapping.Add(position, tuple);
			ItemAdded?.Invoke(this, new ItemEventArgs<T>(newItem, position));
			return true;
		}

		/// <summary>
		/// Adds the given item at the given position, provided the item is not already in the
		/// spatial map and the position is not already filled. If either of those are the case,
		/// returns false. Otherwise (if item was successfully added), returns true.
		/// </summary>
		/// <param name="newItem">The item to add.</param>
		/// <param name="x">X-value of the position to add item to.</param>
		/// <param name="y">Y-value of the position to add item to.</param>
		/// <returns>True if the item was added, false if adding the item failed.</returns>
		public bool Add(T newItem, int x, int y) => Add(newItem, new Coord(x, y));

		/// <summary>
		/// See <see cref="IReadOnlySpatialMap{T}.AsReadOnly"/>.
		/// </summary>
		public IReadOnlySpatialMap<T> AsReadOnly() => this;

		/// <summary>
		/// See <see cref="ISpatialMap{T}.Clear"/>.
		/// </summary>
		public void Clear()
		{
			itemMapping.Clear();
			positionMapping.Clear();
		}

		/// <summary>
		/// See <see cref="IReadOnlySpatialMap{T}.Contains(T)"/>.
		/// </summary>
		public bool Contains(T item) => itemMapping.ContainsKey(item);

		/// <summary>
		/// See <see cref="IReadOnlySpatialMap{T}.Contains(Coord)"/>.
		/// </summary>
		public bool Contains(Coord position) => positionMapping.ContainsKey(position);

		/// <summary>
		/// See <see cref="IReadOnlySpatialMap{T}.Contains(int, int)"/>.
		/// </summary>
		public bool Contains(int x, int y) => positionMapping.ContainsKey(new Coord(x, y));

		/// <summary>
		/// Used by foreach loop, so that the class will give ISpatialTuple objects when used in a
		/// foreach loop. Generally should never be called explicitly.
		/// </summary>
		/// <returns>An enumerator for the spatial map</returns>
		public IEnumerator<ISpatialTuple<T>> GetEnumerator()
		{
			foreach (var tuple in itemMapping.Values)
				yield return tuple;
		}

		/// <summary>
		/// Generic iterator used internally by foreach loops.
		/// </summary>
		/// <returns>Enumerator to ISpatialTuple instances.</returns>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Gets the item at the given position, or default(T) if no item exists.
		/// </summary>
		/// <remarks>
		/// Intended to be a more convenient function as compared to <see cref="GetItems(Coord)"/>, since
		/// this spatial map implementation only allows a single item to at any given location at a time.
		/// </remarks>
		/// <param name="position">The postiion to return the item for.</param>
		/// <returns>
		/// The item at the given position, or default(T) if no item exists at that location.
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
		/// Gets the item at the given position, or default(T) if no item exists.
		/// </summary>
		/// <remarks>
		/// Intended to be a more convenient function as compared to <see cref="GetItems(int, int)"/>, since
		/// this spatial map implementation only allows a single item to at any given location at a time.
		/// </remarks>
		/// <param name="x">The x-value of the position to return the item for.</param>
		/// <param name="y">The y-value of the position to return the item for.</param>
		/// <returns>
		/// The item at the given position, or default(T) if no item exists at that location.
		/// </returns>
		public T GetItem(int x, int y) => GetItem(new Coord(x, y));

		/// <summary>
		/// Gets the item at the given position as a 1-element enumerable if there is any item there,
		/// or nothing if there is nothing at that position.
		/// </summary>
		/// <remarks>
		/// Since this implementation guarantees that only one item can be at any given
		/// location at once, the return value is guaranteed to be at most one element. You may find it
		/// more convenient to use the <see cref="GetItems(Coord)"/> function when you know you are
		/// dealing with a SpatialMap/AdvancedSpatialMap instance.
		/// </remarks>
		/// <param name="position">The position to return the item for.</param>
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
		/// Gets the item at the given position as a 1-element enumerable if there is any item there,
		/// or nothing if there is nothing at that position.
		/// </summary>
		/// <remarks>
		/// Since this implementation guarantees that only one item can be at any given
		/// location at once, the return value is guaranteed to be at most one element. You may find it
		/// more convenient to use the <see cref="GetItems(int, int)"/> function when you know you are
		/// dealing with a SpatialMap/AdvancedSpatialMap instance.
		/// </remarks>
		/// <param name="x">The x-value of the position to return the item(s) for.</param>
		/// <param name="y">The y-value of the position to return the item(s) for.</param>
		/// <returns>
		/// The item at the given position as a 1-element enumerable, if there is an item there, or
		/// nothing if there is no item there.
		/// </returns>
		public IEnumerable<T> GetItems(int x, int y) => GetItems(new Coord(x, y));

		/// <summary>
		/// See <see cref="IReadOnlySpatialMap{T}.GetPosition(T)"/>.
		/// </summary>
		public Coord GetPosition(T item)
		{
			SpatialTuple<T> tuple;
			itemMapping.TryGetValue(item, out tuple);
			if (tuple == null) return Coord.NONE;
			return tuple.Position;
		}

		/// <summary>
		/// Moves the item specified to the position specified. If the item does not exist
		/// in the spatial map, or the position is already filled by some other item, the
		/// function does nothing and returns false. Otherwise, returns true.
		/// </summary>
		/// <param name="item">The item to move.</param>
		/// <param name="target">The position to move it to.</param>
		/// <returns>True if the item was moved, false if the move operation failed.</returns>
		public bool Move(T item, Coord target)
		{
			if (!itemMapping.ContainsKey(item))
				return false;

			if (positionMapping.ContainsKey(target))		
				return false;

			var movingTuple = itemMapping[item];
			Coord oldPos = movingTuple.Position;
			positionMapping.Remove(movingTuple.Position);
			movingTuple.Position = target;
			positionMapping.Add(target, movingTuple);
			ItemMoved?.Invoke(this, new ItemMovedEventArgs<T>(item, oldPos, target));
			return true;
		}

		/// <summary>
		/// Moves the item specified to the position specified. If the item does not exist
		/// in the spatial map, or the position is already filled by some other item, the
		/// function does nothing and returns false. Otherwise, returns true.
		/// </summary>
		/// <param name="item">The item to move.</param>
		/// <param name="targetX">X-value of the location to move item to.</param>
		/// <param name="targetY">Y-value of the location to move item to.</param>
		/// <returns>True if the item was moved, false if not.</returns>
		public bool Move(T item, int targetX, int targetY) => Move(item, new Coord(targetX, targetY));

		/// <summary>
		/// Moves whatever is at position current, if anything, to the target position. If something was
		/// moved, returns what was moved. If nothing was moved, eg. either there was nothing at
		/// <paramref name="current"/> or already something at <paramref name="target"/>, returns nothing.
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
		/// Moves whatever is at the "current" position specified, if anything, to the "target" position. If something was
		/// moved, returns what was moved. If nothing was moved, eg. either there was nothing at
		/// the "current" position given, or already something at the "target" position given, returns nothing.
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
		public IEnumerable<T> Move(int currentX, int currentY, int targetX, int targetY) => Move(new Coord(currentX, currentY), new Coord(targetX, targetY));

		/// <summary>
		/// Removes the item specified, if it exists, and returns true. Returns false if the item was
		/// not in the spatial map.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		/// <returns>True if the item was removed, false if the item was not found.</returns>
		public bool Remove(T item)
		{
			if (!itemMapping.ContainsKey(item))
				return false;

			var tuple = itemMapping[item];
			itemMapping.Remove(item);
			positionMapping.Remove(tuple.Position);
			ItemRemoved?.Invoke(this, new ItemEventArgs<T>(item, tuple.Position));
			return true;
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
				itemMapping.Remove(tuple.Item);
				ItemRemoved?.Invoke(this, new ItemEventArgs<T>(tuple.Item, tuple.Position));
				yield return tuple.Item;
			}
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
		public IEnumerable<T> Remove(int x, int y) => Remove(new Coord(x, y));

		/// <summary>
		/// Returns a string representation of the spatial map.
		/// </summary>
		/// <returns>A string representation of the spatial map.</returns>
		public override string ToString() => ToString((T obj) => obj.ToString());

		/// <summary>
		/// Returns a string representation of the spatial map, allowing display of the spatial map's
		/// items in a specified way.
		/// </summary>
		/// <param name="itemStringifier">Function that turns an item into a string.</param>
		/// <returns>A string representation of the spatial map.</returns>
		public string ToString(Func<T, string> itemStringifier)
			=> positionMapping.ExtendToString(valueStringifier: (SpatialTuple<T> obj) => itemStringifier(obj.Item), pairSeparator: "; ");
	}

	/// <summary>
	/// An implementation of <see cref="ISpatialMap{T}"/> that allows only one item at each position
	/// at a time.  If you need multiple items to be able to reside at one location at the same time,
	/// use <see cref="MultiSpatialMap{T}"/> or <see cref="LayeredSpatialMap{T}"/> instead.
	/// </summary>
	/// <remarks>
	/// See the <see cref="ISpatialMap{T}"/> for documentation on the practical purpose of spatial
	/// maps.
	/// 
	/// 
	/// The objects stored in a SpatialMap must implement <see cref="IHasID"/>. This is used
	/// internally to keep track of the objects, since uints are easily (and efficiently) hashable.
	/// </remarks>
	/// <typeparam name="T">
	/// The type of object that will be contained by this SpatialMap. Must implement <see cref="IHasID"/>
	/// and be a reference-type.
	/// </typeparam>
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

	internal class SpatialTuple<T> : ISpatialTuple<T>
	{
		public SpatialTuple(T item, Coord position)
		{
			Item = item;
			Position = position;
		}

		public T Item { get; set; }
		public Coord Position { get; set; }

		public string ToString(Func<T, string> itemStringifier) => Position + " : " + itemStringifier(Item);

		public override string ToString() => Position + " : " + Item;
	}
}