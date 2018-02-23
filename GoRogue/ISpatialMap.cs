using System;
using System.Collections.Generic;

namespace GoRogue
{
    /// <summary>
    /// General interface for a data structure that records objects on a map.
    /// </summary>
    /// <remarks>
    /// Two different implementations of this interface are provided by to the library, each
    /// providing somewhat different implementations and constraints, however both provide more
    /// efficient implementations of many of these operations than the standard "list of monsters"
    /// that is often used in roguelike map design.
    /// </remarks>
    /// <typeparam name="T">The type of object that will be contained by the data structure.</typeparam>
    public interface ISpatialMap<T> : IReadOnlySpatialMap<T>
    {
        /// <summary>
        /// Fired directly after an item has been added to the data structure.
        /// </summary>
        event EventHandler<ItemEventArgs<T>> ItemAdded;

        /// <summary>
        /// Fired directly after an item in the data structure has been moved.
        /// </summary>
        event EventHandler<ItemMovedEventArgs<T>> ItemMoved;

        /// <summary>
        /// Fired directly after an item has been removed from the data structure.
        /// </summary>
        event EventHandler<ItemEventArgs<T>> ItemRemoved;

        /// <summary>
        /// Adds the given item at the given position, and returns true if the item was successfully
        /// added. If the item could not be added, returns false.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="position">Position to add item to.</param>
        /// <returns>True if item was successfully added, false otherwise.</returns>
        bool Add(T newItem, Coord position);

        /// <summary>
        /// Adds the given item at the given position, and returns true if the item was successfully
        /// added. If the item could not be added, returns false.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="x">X-value of the position to add item to.</param>
        /// <param name="y">Y-value of the position to add item to.</param>
        /// <returns>True if item was successfully added, false otherwise.</returns>
        bool Add(T newItem, int x, int y);

        /// <summary>
        /// Clears all items out of the data structure.
        /// </summary>
        void Clear();

        /// <summary>
        /// Moves the given item from its current location to the specified one. Returns true if the
        /// item was successfully moved, false otherwise.
        /// </summary>
        /// <param name="item">Item to move</param>
        /// <param name="target">Location to move item to.</param>
        /// <returns>True if item was successfully moved, false otherwise.</returns>
        bool Move(T item, Coord target);

        /// <summary>
        /// Moves the given item from its current location to the specified one. Returns true if the
        /// item was successfully moved, false otherwise.
        /// </summary>
        /// <param name="item">Item to move</param>
        /// <param name="targetX">X-value of the location to move item to.</param>
        /// <param name="targetY">Y-value of the location to move item to.</param>
        /// <returns>True if item was successfully moved, false otherwise.</returns>
        bool Move(T item, int targetX, int targetY);

        /// <summary>
        /// Moves any items at the specified location to the target one. Returns any items that were moved.
        /// </summary>
        /// <param name="current">Location to move items from.</param>
        /// <param name="target">Location to move items to.</param>
        /// <returns>Any items that were moved, nothing if no items were moved.</returns>
        IEnumerable<T> Move(Coord current, Coord target);

        /// <summary>
        /// Moves any items at the specified location to the target one. Returns any items that were moved.
        /// </summary>
        /// <param name="currentX">X-value of the location to move items from.</param>
        /// <param name="currentY">Y-value of the location to move items from.</param>
        /// <param name="targetX">X-value of the location to move items to.</param>
        /// <param name="targetY">Y-value of the location to move items to.</param>
        /// <returns>Any items that were moved, nothing if no items were moved.</returns>
        IEnumerable<T> Move(int currentX, int currentY, int targetX, int targetY);

        /// <summary>
        /// Removes the given item from the data structure, returning true if the item was removed or
        /// false otherwise.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if item was removed, false otherwise.</returns>
        bool Remove(T item);

        /// <summary>
        /// Removes any items at the specified location from the data structure. Returns any items
        /// that were removed.
        /// </summary>
        /// <param name="position">Position to remove items from.</param>
        /// <returns>Any items that were removed, or nothing if no items were removed.</returns>
        IEnumerable<T> Remove(Coord position);

        /// <summary>
        /// Removes any items at the specified location from the data structure. Returns any items
        /// that were removed.
        /// </summary>
        /// <param name="x">X-value of the position to remove items from.</param>
        /// <param name="y">Y-value of the position to remove items from.</param>
        /// <returns>Any items that were removed, or nothing if no items were removed.</returns>
        IEnumerable<T> Remove(int x, int y);
    }

    /// <summary>
    /// Event args for spatial map events pertaining to an item (item added, item removed, etc.)
    /// </summary>
    /// <typeparam name="T">Type of item.</typeparam>
    public class ItemEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item">Item being represented.</param>
        /// <param name="position">Current position of the item.</param>
        public ItemEventArgs(T item, Coord position)
        {
            Item = item;
            Position = position;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item">Item being represented.</param>
        /// <param name="x">X-value of the current position of the item.</param>
        /// <param name="y">Y-value of the current position of the item.</param>
        public ItemEventArgs(T item, int x, int y)
            : this(item, Coord.Get(x, y)) { }

        /// <summary>
        /// Item being represented.
        /// </summary>
        public T Item { get; private set; }

        /// <summary>
        /// Current position of that item at time of event.
        /// </summary>
        public Coord Position { get; private set; }
    }

    /// <summary>
    /// Event args for SpatialMap's ItemMoved event.
    /// </summary>
    /// <typeparam name="T">Type of item being stored.</typeparam>
    public class ItemMovedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item">Item being represented.</param>
        /// <param name="oldPosition">Position of item before it was moved.</param>
        /// <param name="newPosition">Position of item after it has been moved.</param>
        public ItemMovedEventArgs(T item, Coord oldPosition, Coord newPosition)
        {
            Item = item;
            oldPosition = OldPosition;
            newPosition = NewPosition;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item">Item being represented.</param>
        /// <param name="oldPositionX">X-value of the position of item before it was moved.</param>
        /// <param name="oldPositionY">Y-value of the position of item before it was moved.</param>
        /// <param name="newPositionX">X-value of the position of item after it has been moved.</param>
        /// <param name="newPositionY">Y-value of the position of item after it has been moved.</param>
        public ItemMovedEventArgs(T item, int oldPositionX, int oldPositionY, int newPositionX, int newPositionY)
            : this(item, Coord.Get(oldPositionX, oldPositionY), Coord.Get(newPositionX, newPositionY)) { }

        /// <summary>
        /// Item being represented.
        /// </summary>
        public T Item { get; private set; }

        /// <summary>
        /// Position of item after it has been moved.
        /// </summary>
        public Coord NewPosition { get; private set; }

        /// <summary>
        /// Position of item before it was moved.
        /// </summary>
        public Coord OldPosition { get; private set; }
    }
}