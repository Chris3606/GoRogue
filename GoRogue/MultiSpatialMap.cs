using System;
using System.Collections;
using System.Collections.Generic;

namespace GoRogue
{
	/// <summary>
	/// Advanced version of MultiSpatialMap that allows for use of a custom IEqualityComparer for
	/// hashing and comparison of type T. May be useful for cases where one does not want to
	/// implement IHasID, or if you need to use a value type in a MultiSpatialMap. For simple cases,
	/// it is recommended to use MultiSpatialMap instead.
	/// </summary>
	/// <remarks>
	/// Be mindful of the efficiency of your hashing function specified in the IEqualityComparer --
	/// it will in large part determine the performance of AdvancedMultiSpatialMap!
	/// </remarks>
	/// <typeparam name="T">The type of object that will be contained by this AdvancedMultiSpatialMap.</typeparam>
	public class AdvancedMultiSpatialMap<T> : ISpatialMap<T>
	{
		private Dictionary<T, SpatialTuple<T>> itemMapping;
		private Dictionary<Coord, List<SpatialTuple<T>>> positionMapping;

		/// <summary>
		/// Constructor. Creates an empty MultiSpatialMap.
		/// </summary>
		/// <param name="comparer">
		/// Equality comparer to use for comparison and hashing of type T. Be mindful of the
		/// efficiency of this instances GetHashCode function, as it will determine the efficiency of
		/// many AdvancedMultiSpatialMap functions.
		/// </param>
		/// <param name="initialCapacity">
		/// The initial maximum number of elements the MultiSpatialMap can hold before it has to
		/// internally resize data structures. Defaults to 32.
		/// </param>
		public AdvancedMultiSpatialMap(IEqualityComparer<T> comparer, int initialCapacity = 32)
		{
			itemMapping = new Dictionary<T, SpatialTuple<T>>(initialCapacity, comparer);
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
			if (itemMapping.ContainsKey(newItem))
				return false;

			var tuple = new SpatialTuple<T>(newItem, position);
			itemMapping.Add(newItem, tuple);

			if (!positionMapping.ContainsKey(position))
				positionMapping.Add(position, new List<SpatialTuple<T>>());

			positionMapping[position].Add(tuple);
			ItemAdded?.Invoke(this, new ItemEventArgs<T>(newItem, position));
			return true;
		}

		/// <summary>
		/// Adds the given item at the given position, provided the item is not already in the
		/// MultiSpatialMap. If the item is already contained in it, does nothing and returns false.
		/// Otherwise (if item was successfully added), returns true.
		/// </summary>
		/// <param name="newItem">The item to add.</param>
		/// <param name="x">x-value of the position to add item to.</param>
		/// <param name="y">y-value of the position to add item to.</param>
		/// <returns>True if the item was added, false if not.</returns>
		public bool Add(T newItem, int x, int y) => Add(newItem, new Coord(x, y));

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
		public bool Contains(T item) => itemMapping.ContainsKey(item);

		/// <summary>
		/// See IReadOnlySpatialMap.Contains.
		/// </summary>
		public bool Contains(Coord position) => positionMapping.ContainsKey(position);

		/// <summary>
		/// See IReadOnlySpatialMap.Contains.
		/// </summary>
		public bool Contains(int x, int y) => Contains(new Coord(x, y));

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
			{
				var positionList = positionMapping[position];

				for (int i = positionList.Count - 1; i >= 0; i--)
					yield return positionList[i].Item;
			}
		}

		/// <summary>
		/// See IReadOnlySpatialMap.GetItems.
		/// </summary>
		public IEnumerable<T> GetItems(int x, int y) => GetItems(new Coord(x, y));

		/// <summary>
		/// See IReadOnlySpatialMap.GetPosition.
		/// </summary>
		public Coord GetPosition(T item)
		{
			SpatialTuple<T> tuple;
			itemMapping.TryGetValue(item, out tuple);
			if (tuple == null) return Coord.NONE;
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
			if (!itemMapping.ContainsKey(item))
				return false;

			var movingTuple = itemMapping[item];
			if (movingTuple.Position == target)
				return false;

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
		/// Move the item specified to the position specified. Returns true if successful. If the
		/// item does not exist in the MultiSpatialMap, does nothing and returns false. Otherwise,
		/// returns true.
		/// </summary>
		/// <param name="item">The item to move.</param>
		/// <param name="targetX">X-value of the location to move it to.</param>
		/// <param name="targetY">Y-value of the location to move it to.</param>
		/// <returns>True if the item was moved, false if not.</returns>
		public bool Move(T item, int targetX, int targetY) => Move(item, new Coord(targetX, targetY));

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
			if (positionMapping.ContainsKey(current) && current != target)
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
		/// Moves everything at position current, if anything, to postion target. If something was
		/// moved, returns everything that was moved. If nothing was moved, eg. there was nothing at
		/// position current, returns nothing.
		/// </summary>
		/// <param name="currentX">X-value of the location to move items from.</param>
		/// <param name="currentY">Y-value of the location to move items from.</param>
		/// <param name="targetX">X-value of the location to move items to.</param>
		/// <param name="targetY">Y-value of the location to move items to.</param>
		/// <returns>The items moved if something was moved, or nothing if no item was moved.</returns>
		public IEnumerable<T> Move(int currentX, int currentY, int targetX, int targetY) => Move(new Coord(currentX, currentY), new Coord(targetX, targetY));

		/// <summary>
		/// Removes the item specified, if it exists, and returns true. Returns false if the item was
		/// not in the MultiSpatialMap.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		/// <returns>True if the item was removed, false if the item was not found.</returns>
		public bool Remove(T item)
		{
			if (!itemMapping.ContainsKey(item))
				return false;

			var tuple = itemMapping[item];
			itemMapping.Remove(item);
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
					itemMapping.Remove(tuple.Item);
					yield return tuple.Item;
				}

				var list = positionMapping[position];
				positionMapping.Remove(position);
				if (ItemRemoved != null)
					foreach (var tuple in list)
						ItemRemoved(this, new ItemEventArgs<T>(tuple.Item, position));
			}
		}

		/// <summary>
		/// Removes everything at the given position, if anything, and returns the items removed.
		/// Returns nothing if no item was at the position specified.
		/// </summary>
		/// <param name="x">X-value of the position to remove items from.</param>
		/// <param name="y">Y-value of the position to remove items from.</param>
		/// <returns>
		/// The items removed, if any were removed; nothing if no item was found at that position.
		/// </returns>
		public IEnumerable<T> Remove(int x, int y) => Remove(new Coord(x, y));

		/// <summary>
		/// Returns a string representation of the MultiSpatialMap, allowing display of the
		/// MultiSpatialMap's items in a specified way.
		/// </summary>
		/// <param name="itemStringifier">Function that turns an item into a string.</param>
		/// <returns>A string representation of the MultiSpatialMap.</returns>
		public string ToString(Func<T, string> itemStringifier)
			=> positionMapping.ExtendToString("", valueStringifier: (List<SpatialTuple<T>> obj) =>
																	 obj.ExtendToString(elementStringifier: (SpatialTuple<T> item) => itemStringifier(item.Item)),
											  kvSeparator: ": ", pairSeparator: ",\n", end: "");

		/// <summary>
		/// Returns a string representation of the MultiSpatialMap.
		/// </summary>
		/// <returns>A string representation of the MultiSpatialMap.</returns>
		public override string ToString()
			=> ToString((T obj) => obj.ToString());
	}

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
	/// The two implementations could also in many cases be used in combination as necessary, since
	/// they both implement the ISpatialMap interface.
	/// </remarks>
	/// <typeparam name="T">
	/// The type of items being stored. Must implement IHasID and be a reference-type.
	/// </typeparam>
	public class MultiSpatialMap<T> : AdvancedMultiSpatialMap<T> where T : class, IHasID
	{
		/// <summary>
		/// Constructor. Creates an empty MultiSpatialMap.
		/// </summary>
		/// <param name="initialCapacity">
		/// The initial maximum number of elements the SpatialMap can hold before it has to
		/// internally resize data structures. Defaults to 32.
		/// </param>
		public MultiSpatialMap(int initialCapacity = 32)
			: base(new IDComparer<T>(), initialCapacity)
		{ }
	}
}