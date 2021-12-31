using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.SpatialMaps
{
    /// <summary>
    /// Implements the read-only interface of <see cref="ISpatialMap{T}" />.
    /// </summary>
    /// <remarks>
    /// Simply exposes only those functions of <see cref="ISpatialMap{T}" /> that do not allow direct modification
    /// of the data (eg. adding/moving/removing items). This can allow for direct exposure of an ISpatialMap as a
    /// property of type IReadOnlySpatialMap, without allowing such an exposure to break data
    /// encapsulation principles of something like a game map.
    /// </remarks>
    [PublicAPI]
    public interface IReadOnlySpatialMap<T> : IEnumerable<ItemPositionPair<T>>
        where T : notnull
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
        IEnumerable<Point> Positions { get; }

        /// <summary>
        /// Event that is fired directly after an item has been added to the spatial map.
        /// </summary>
        event EventHandler<ItemEventArgs<T>> ItemAdded;

        /// <summary>
        /// Event that is fired directly after an item in the spatial map has been moved.
        /// </summary>
        event EventHandler<ItemMovedEventArgs<T>> ItemMoved;

        /// <summary>
        /// Event that is fired directly after an item has been removed from the spatial map.
        /// </summary>
        event EventHandler<ItemEventArgs<T>> ItemRemoved;

        /// <summary>
        /// Returns a read-only reference to the spatial map. Convenient for "safely" exposing the
        /// spatial as a property, without allowing direct modification.
        /// </summary>
        /// <returns>The current spatial map, as a "read-only" reference.</returns>
        IReadOnlySpatialMap<T> AsReadOnly();

        /// <summary>
        /// Returns true if the given item can be added at the given position; false otherwise.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="position">Position to add item to.</param>
        /// <returns>True if the item can be successfully added at the position given; false otherwise.</returns>
        bool CanAdd(T newItem, Point position);

        /// <summary>
        /// Returns true if the given item can be added at the given position; false otherwise.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="x">X-value of the position to add item to.</param>
        /// <param name="y">Y-value of the position to add item to.</param>
        /// <returns>True if the item can be successfully added at the position given; false otherwise.</returns>
        bool CanAdd(T newItem, int x, int y);

        /// <summary>
        /// Returns true if the given item can be moved from its current location to the specified one; false otherwise.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="target">Location to move item to.</param>
        /// <returns>true if the given item can be moved to the given position; false otherwise.</returns>
        bool CanMove(T item, Point target);

        /// <summary>
        /// Returns true if the given item can be moved from its current location to the specified one; false otherwise.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="targetX">X-value of the location to move item to.</param>
        /// <param name="targetY">Y-value of the location to move item to.</param>
        /// <returns>true if the given item can be moved to the given position; false otherwise.</returns>
        bool CanMove(T item, int targetX, int targetY);

        /// <summary>
        /// Returns true if there are items at <paramref name="current" /> and all items at that position
        /// can be moved to <paramref name="target" />; false otherwise.
        /// </summary>
        /// <param name="current">Location to move items from.</param>
        /// <param name="target">Location to move items to.</param>
        /// <returns>
        /// true if all items at the position current can be moved to the position target; false if one or more items
        /// cannot be moved or there are no items to move.
        /// </returns>
        bool CanMoveAll(Point current, Point target);

        /// <summary>
        /// Returns true if there are items at the current position specified, and all items at that position
        /// can be moved to the target position; false otherwise.
        /// </summary>
        /// <param name="currentX">X-value of the location to move items from.</param>
        /// <param name="currentY">Y-value of the location to move items from.</param>
        /// <param name="targetX">X-value of the location to move items to.</param>
        /// <param name="targetY">Y-value of the location to move items to.</param>
        /// <returns>
        /// true if all items at the position current can be moved to the position target; false if one or more items
        /// cannot be moved or there are no items to move.
        /// </returns>
        bool CanMoveAll(int currentX, int currentY, int targetX, int targetY);

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
        bool Contains(Point position);

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
        IEnumerable<T> GetItemsAt(Point position);

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
        IEnumerable<T> GetItemsAt(int x, int y);

        /// <summary>
        /// Gets the position associated with the given item in the spatial map, or null if that item is
        /// not found.
        /// </summary>
        /// <param name="item">The item to get the position for.</param>
        /// <returns>
        /// The position associated with the given item, if it exists in the spatial map, or null
        /// if the item does not exist.
        /// </returns>
        Point? GetPositionOfOrNull(T item);

        /// <summary>
        /// Attempts to get the position of the given item in the spatial map.  If successful, the function will return
        /// true and the position will be stored in the <paramref name="position"/> parameter.  If the item was not found,
        /// <paramref name="position"/> will have the default value for Point, and the function will return false.
        /// </summary>
        /// <param name="item">Item to retrieve the position of.</param>
        /// <param name="position">The position of the item if found, or default(Point) if not.</param>
        /// <returns>True if the item was found in the spatial map, false otherwise.</returns>
        bool TryGetPositionOf(T item, out Point position);

        /// <summary>
        /// Gets the position associated with the given item in the spatial map.  If the item does not exist in the
        /// spatial map, throws ArgumentException.
        /// </summary>
        /// <param name="item">The item to get the position for.</param>
        /// <returns>
        /// The position associated with the given item.
        /// </returns>
        Point GetPositionOf(T item);

        /// <summary>
        /// Returns a string representation of the IReadOnlySpatialMap, allowing display of the spatial map's
        /// items in a specified way.
        /// </summary>
        /// <param name="itemStringifier">Function that turns an item into a string.</param>
        /// <returns>A string representation of the spatial map.</returns>
        string ToString(Func<T, string> itemStringifier);
    }
}
