using System;
using System.Collections.Generic;

namespace GoRogue
{
	/// <summary>
	/// Implements the read-only interface of <see cref="ISpatialMap{T}"/>.
	/// </summary>
	/// <remarks>
	/// Simply exposes only those functions of <see cref="ISpatialMap{T}"/> that do not allow direct modification
	/// of the data (eg. adding/moving/removing items). This can allow for direct exposure of an ISpatialMap as a
	/// property of type IReadOnlySpatialMap, without allowing such an exposure to break data
	/// encapsulation principles of something like a game map.
	/// </remarks>
	public interface IReadOnlySpatialMap<T> : IEnumerable<ISpatialTuple<T>>
	{
		/// <summary>
		/// The number of items in the spatial map.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Enumerable of the items stored in the spatial map: for use to iterate over all items with a foreach loop.
		/// </summary>
		IEnumerable<T> Items { get; }

		/// <summary>
		/// Enumerable of all positions that contain items.
		/// </summary>
		IEnumerable<Coord> Positions { get; }

		/// <summary>
		/// Returns a read-only reference to the spatial map. Convenient for "safely" exposing the
		/// spatial as a property, without allowing direct modification.
		/// </summary>
		/// <returns>The current spatial map, as a "read-only" reference.</returns>
		IReadOnlySpatialMap<T> AsReadOnly();

		/// <summary>
		/// Returns whether or not the spatial map contains the given item.
		/// </summary>
		/// <param name="item">The item to check for.</param>
		/// <returns>True if the given item is contained in the spatial map, false if not.</returns>
		bool Contains(T item);

		/// <summary>
		/// Returns if there is an item in the spatial map at the given position or not.
		/// </summary>
		/// <param name="position">The position to check for.</param>
		/// <returns>True if there is some item at the given position, false if not.</returns>
		bool Contains(Coord position);

		/// <summary>
		/// Returns if there is an item in the spatial map at the given position or not.
		/// </summary>
		/// <param name="x">The x-value of the position to check for.</param>
		/// <param name="y">The y-value of the position to check for.</param>
		/// <returns>True if there is some item at the given position, false if not.</returns>
		bool Contains(int x, int y);

		/// <summary>
		/// Gets the item(s) at the given position if there are any items, or returns
		/// nothing if there is nothing at that position.
		/// </summary>
		/// <param name="position">The position to return the item(s) for.</param>
		/// <returns>
		/// The item(s) at the given position if there are any items, or nothing if there is nothing
		/// at that position.
		/// </returns>
		IEnumerable<T> GetItems(Coord position);

		/// <summary>
		/// Gets the item(s) at the given position if there are any items, or returns
		/// nothing if there is nothing at that position.
		/// </summary>
		/// <param name="x">The x-value of the position to return the item(s) for.</param>
		/// <param name="y">The y-value of the position to return the item(s) for.</param>
		/// <returns>
		/// The item(s) at the given position if there are any items, or nothing if there is nothing
		/// at that position.
		/// </returns>
		IEnumerable<T> GetItems(int x, int y);

		/// <summary>
		/// Gets the position associated with the given item in the spatial map, or null if that item is
		/// not found.
		/// </summary>
		/// <param name="item">The item to get the position for.</param>
		/// <returns>
		/// The position associated with the given item, if it exists in the spatial map, or <see cref="Coord.NONE"/>
		/// if the item does not exist.
		/// </returns>
		Coord GetPosition(T item);

		/// <summary>
		/// Returns a string representation of the IReadOnlySpatialMap, allowing display of the spatial map's
		/// items in a specified way.
		/// </summary>
		/// <param name="itemStringifier">Function that turns an item into a string.</param>
		/// <returns>A string representation of the spatial map.</returns>
		string ToString(Func<T, string> itemStringifier);
	}

	/// <summary>
	/// Interface specifying return type for item-location pairs in spatial maps.
	/// </summary>
	/// <typeparam name="T">Type of the item associated with locations.</typeparam>
	public interface ISpatialTuple<T>
	{
		/// <summary>
		/// The item associated with this pair.
		/// </summary>
		T Item { get; }

		/// <summary>
		/// The position associated with this pair.
		/// </summary>
		Coord Position { get; }
	}
}