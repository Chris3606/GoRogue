using System.Collections.Generic;

namespace GoRogue
{
    /// <summary>
    /// See ISpatialMap documentation. Similar in principle to the C# standard
    /// IReadOnlyList/IReadOnlyCollection interface. Simply exposes only those functions of the
    /// ISpatialMap interface that do not allow direct modification of the data (eg.
    /// adding/moving/removing items). This can allow for direct exposure of an ISpatialMap as a
    /// property of type IReadOnlySpatialMap, without allowing such an exposure to break data
    /// encapsulation principles of something like a game map.
    /// </summary>
    public interface IReadOnlySpatialMap<T> : IEnumerable<ISpatialTuple<T>>
    {
        /// <summary>
        /// The number of items in the data structure.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Enumerable of the items stored in the data structure: for use to iterate over all items,
        /// eg: <example>
        /// <code>
        /// foreach (var item in mySpatialMap.Items)
        ///     // Do stuff
        /// </code>
        /// </example>
        /// </summary>
        IEnumerable<T> Items { get; }

        /// <summary>
        /// Enumerable of all positions that contain items. Likely this won't be used as much, since
        /// most objects that are contained in a spatial map implementation will record their
        /// position as a member, however in case that is not the case for a particular use, or just
        /// as a convenience if you only care about the coordinates, this is provided.
        /// </summary>
        IEnumerable<Coord> Positions { get; }

        /// <summary>
        /// Returns a read-only reference to the data structure. Convenient for "safely" exposing the
        /// structure as a property.
        /// </summary>
        /// <returns>
        /// The current data structure, as a "read-only" reference.
        /// </returns>
        IReadOnlySpatialMap<T> AsReadOnly();

        /// <summary>
        /// Returns whether or not the data structure contains the given item.
        /// </summary>
        /// <param name="item">
        /// The item to check for.
        /// </param>
        /// <returns>
        /// True if the given item is in the data structure, false if not.
        /// </returns>
        bool Contains(T item);

        /// <summary>
        /// Returns if there is an item in the data structure at the given position or not.
        /// </summary>
        /// <param name="position">
        /// The position to check for.
        /// </param>
        /// <returns>
        /// True if there is some item at the given position, false if not.
        /// </returns>
        bool Contains(Coord position);

        /// <summary>
        /// Gets the item(s) associated with the given position if there are any items, or returns
        /// nothing if there is nothing at that position.
        /// </summary>
        /// <param name="position">
        /// The position to return the item(s) for.
        /// </param>
        /// <returns>
        /// The item(s) at the given position if there are any items, or nothing if there is nothing
        /// at that position.
        /// </returns>
        IEnumerable<T> GetItems(Coord position);

        /// <summary>
        /// Gets the position associated with the item in the data structure, or null if that item is
        /// not found.
        /// </summary>
        /// <param name="item">
        /// The item to get the position for.
        /// </param>
        /// <returns>
        /// The position associated with the given item, if it exists in the data structure, or null
        /// if the item does not exist.
        /// </returns>
        Coord GetPosition(T item);
    }

    /// <summary>
    /// Interface specifying return type for item-location pairs in a spatial map implementation.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the item associated with locations.
    /// </typeparam>
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