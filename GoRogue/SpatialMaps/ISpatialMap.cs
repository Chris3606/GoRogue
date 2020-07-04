using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.SpatialMaps
{
    /// <summary>
    /// Interface common to spatial map implementations provided with GoRogue, which are designed
    /// to be a convenient way to store items that reside on a map.  TLDR; If you're about
    /// to use a List to store all the objects on a map, consider using a (GoRogue-provided) ISpatialMap
    /// implementation instead.
    /// </summary>
    /// <remarks>
    /// Typical roguelikes will use a 2D array (or 1D array accessed as a 2D array), for terrain, and
    /// lists of objects for things like entities, items, etc. This is simple but ultimately not
    /// efficient; for example, in that implementation, determining if there is an object at a
    /// location takes an amount of time proportional to the number of objects in this list. However,
    /// the other simple option is to use an array with size equal to the size of the map (as many do
    /// for terrain) for all object lists. This is even less ideal, as in that case, the time to
    /// iterate over all objects becomes proportional to the size of the map (since one has to do
    /// that for rendering, that's bad!), which is typically much larger than the number of objects in a
    /// list. This is the problem spatial map implementations are designed to solve. They provide fast,
    /// near-constant-time operations for getting the item(s) at a location, adding items, and removing
    /// items.  As well, they allow you to iterate through all objects in the spatial map in time
    /// proportional to the number of objects in it (the best possible). Effectively, it is a more efficient
    /// list for objects that have a position associated with them.  Spatial maps also provide events for when
    /// things are added, removed, etc., to allow you to conveniently respond to those types of actions.
    /// Spatial maps have to keep track of the position of each item in them in order to provide
    /// their fast-lookup functionality.  Spatial maps can be used as the primary authority for what an item's
    /// position is in some cases -- however, in many cases, this may be undesirable, particularly when interfacing
    /// with more traditional infrastructures from other libraries, which likely record each item's position as a field
    /// or property of the item itself.  In these cases, where the item itself records its position, you will need to
    /// call the <see cref="Move(T, Point)" /> function (or a similar overload) whenever an object moves, to keep the
    /// spatial map's position for that item in sync with its actual position.
    /// It is also worthy of note, that some implementations of ISpatialMap (such as <see cref="SpatialMap{T}" />) have
    /// implemented
    /// <see cref="Move(T, Point)" /> in such a way that it could fail in some cases.  Move will return false in cases where it
    /// fails, so if you are using an implementation where that may happen, you may need to check this to avoid desync issues.
    /// The Move function documentation for each implementation clearly states in what cases a call to Move can fail.
    /// </remarks>
    /// <typeparam name="T">The type of object that will be contained by the spatial map.</typeparam>
    [PublicAPI]
    public interface ISpatialMap<T> : IReadOnlySpatialMap<T>
    {
        /// <summary>
        /// Tries to add the given item at the given position, and throws InvalidOperationException if the item cannot be added.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="position">Position to add item to.</param>
        void Add(T newItem, Point position);

        /// <summary>
        /// Tries to add the given item at the given position, and throws InvalidOperationException if the item cannot be added.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        /// <param name="x">X-value of the position to add item to.</param>
        /// <param name="y">Y-value of the position to add item to.</param>
        void Add(T newItem, int x, int y);

        /// <summary>
        /// Clears all items out of the spatial map.
        /// </summary>
        void Clear();

        /// <summary>
        /// Moves the given item from its current location to the specified one. Throws InvalidOperationException if the item
        /// cannot be moved.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="target">Location to move item to.</param>
        void Move(T item, Point target);

        /// <summary>
        /// Moves the given item from its current location to the specified one. Throws InvalidOperationException if the item
        /// cannot be moved.
        /// </summary>
        /// <param name="item">Item to move</param>
        /// <param name="targetX">X-value of the location to move item to.</param>
        /// <param name="targetY">Y-value of the location to move item to.</param>
        void Move(T item, int targetX, int targetY);

        /// <summary>
        /// Moves all items at the specified source location to the target location.  Throws InvalidOperationException if one or
        /// more items cannot be moved or there are
        /// no items to be moved.
        /// </summary>
        /// <param name="current">Location to move items from.</param>
        /// <param name="target">Location to move items to.</param>
        void MoveAll(Point current, Point target);

        /// <summary>
        /// Moves all items at the specified source location to the target location.  Throws InvalidOperationException if one or
        /// more items cannot be moved or there are no items
        /// to be moved.
        /// </summary>
        /// <param name="currentX">X-value of the location to move items from.</param>
        /// <param name="currentY">Y-value of the location to move items from.</param>
        /// <param name="targetX">X-value of the location to move items to.</param>
        /// <param name="targetY">Y-value of the location to move items to.</param>
        void MoveAll(int currentX, int currentY, int targetX, int targetY);

        /// <summary>
        /// Moves all items at the specified source location that can be moved to the target location. Returns all items that were
        /// moved.
        /// </summary>
        /// <param name="current">Location to move items from.</param>
        /// <param name="target">Location to move items to.</param>
        /// <returns>All items that were moved, or nothing if no items were moved.</returns>
        List<T> MoveValid(Point current, Point target);

        /// <summary>
        /// Moves all items at the specified location that can be moved to the target one. Returns all items that were moved.
        /// </summary>
        /// <param name="currentX">X-value of the location to move items from.</param>
        /// <param name="currentY">Y-value of the location to move items from.</param>
        /// <param name="targetX">X-value of the location to move items to.</param>
        /// <param name="targetY">Y-value of the location to move items to.</param>
        /// <returns>All items that were moved, or nothing if no items were moved.</returns>
        List<T> MoveValid(int currentX, int currentY, int targetX, int targetY);

        /// <summary>
        /// Removes the given item from the spatial map.  Throws InvalidOperationException if the item cannot be removed.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        void Remove(T item);

        /// <summary>
        /// Removes all items at the specified location from the spatial map. Returns all items
        /// that were removed.
        /// </summary>
        /// <param name="position">Position to remove items from.</param>
        /// <returns>All items that were removed, or nothing if no items were removed.</returns>
        List<T> Remove(Point position);

        /// <summary>
        /// Removes all items at the specified location from the spatial map. Returns all items
        /// that were removed.
        /// </summary>
        /// <param name="x">X-value of the position to remove items from.</param>
        /// <param name="y">Y-value of the position to remove items from.</param>
        /// <returns>All items that were removed, or nothing if no items were removed.</returns>
        List<T> Remove(int x, int y);
    }

    /// <summary>
    /// Event arguments for spatial map events pertaining to an item (<see cref="IReadOnlySpatialMap{T}.ItemAdded" />,
    /// <see cref="IReadOnlySpatialMap{T}.ItemRemoved" />, etc.)
    /// </summary>
    /// <typeparam name="T">Type of item.</typeparam>
    [PublicAPI]
    public class ItemEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item">Item being represented.</param>
        /// <param name="position">Current position of the item.</param>
        public ItemEventArgs(T item, Point position)
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
            : this(item, new Point(x, y))
        { }

        /// <summary>
        /// Item being represented.
        /// </summary>
        public T Item { get; private set; }

        /// <summary>
        /// Current position of that item at time of event.
        /// </summary>
        public Point Position { get; private set; }
    }

    /// <summary>
    /// Event arguments for spatial maps <see cref="IReadOnlySpatialMap{T}.ItemMoved" /> event.
    /// </summary>
    /// <typeparam name="T">Type of item being stored.</typeparam>
    [PublicAPI]
    public class ItemMovedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item">Item being represented.</param>
        /// <param name="oldPosition">Position of item before it was moved.</param>
        /// <param name="newPosition">Position of item after it has been moved.</param>
        public ItemMovedEventArgs(T item, Point oldPosition, Point newPosition)
        {
            Item = item;
            OldPosition = oldPosition;
            NewPosition = newPosition;
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
            : this(item, new Point(oldPositionX, oldPositionY), new Point(newPositionX, newPositionY))
        { }

        /// <summary>
        /// Item being represented.
        /// </summary>
        public T Item { get; private set; }

        /// <summary>
        /// Position of item after it has been moved.
        /// </summary>
        public Point NewPosition { get; private set; }

        /// <summary>
        /// Position of item before it was moved.
        /// </summary>
        public Point OldPosition { get; private set; }
    }

    // Class for dictionary-hashing of things that implement IHasID
    /// <summary>
    /// Class intended for comparing/hashing objects that implement <see cref="IHasID" />. Type T must be a
    /// reference type.
    /// </summary>
    /// <typeparam name="T">
    /// Type of object being compared. Type T must be a reference type that implements <see cref="IHasID" />.
    /// </typeparam>
    internal class IDComparer<T> : IEqualityComparer<T> where T : class, IHasID
    {
        /// <summary>
        /// Equality comparison. Performs comparison via the object's <see cref="object.ReferenceEquals(object, object)" />
        /// function.
        /// </summary>
        /// <param name="x">First object to compare.</param>
        /// <param name="y">Second object to compare.</param>
        /// <returns>True if the objects are considered equal, false otherwise.</returns>
        public bool Equals(T x, T y) => ReferenceEquals(x, y);

        /// <summary>
        /// Generates a hash based on the object's ID.GetHashCode() function.
        /// </summary>
        /// <param name="obj">Object to generate the hash for.</param>
        /// <returns>The hash of the object, based on its ID.</returns>
        public int GetHashCode(T obj) => obj.ID.GetHashCode();
    }
}
