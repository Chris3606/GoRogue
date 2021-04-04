using System;
using JetBrains.Annotations;

namespace GoRogue
{
    /// <summary>
    /// Read-only representation of <see cref="DisjointSet"/>.
    /// </summary>
    [PublicAPI]
    public interface IReadOnlyDisjointSet
    {
        /// <summary>
        /// Fired when two sets are joined into one.  The arguments give the IDs of the two sets that were joined.
        /// </summary>
        public event EventHandler<JoinedEventArgs>? SetsJoined;

        /// <summary>
        /// Number of distinct sets.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Returns the ID for the parent of the set containing the set with ID <paramref name="objectID" />,
        /// performing path compression as search is completed.
        /// </summary>
        /// <param name="objectID">ID of the object to search for.</param>
        /// <returns>The ID for the parent of the object given.</returns>
        int Find(int objectID);

        /// <summary>
        /// Returns true if the objects specified by the given IDs are in the same set.
        /// </summary>
        /// <param name="objectID1" />
        /// <param name="objectID2" />
        /// <returns>True if the the objects specified by the given IDs are in the same set, false otherwise.</returns>
        bool InSameSet(int objectID1, int objectID2);
    }

    /// <summary>
    /// Read-only representation of <see cref="DisjointSet{T}"/>.
    /// </summary>
    [PublicAPI]
    public interface IReadOnlyDisjointSet<T>
    {
        /// <summary>
        /// Fired when two sets are joined into one.  The arguments give the two sets that were joined.
        /// </summary>
        public event EventHandler<JoinedEventArgs<T>>? SetsJoined;

        /// <summary>
        /// Number of distinct sets.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Returns the parent of the set containing <paramref name="item" />, performing path compression as search is
        /// completed.
        /// </summary>
        /// <param name="item">Object to search for.</param>
        /// <returns>The parent of the object given.</returns>
        T Find(T item);

        /// <summary>
        /// Returns true if the two objects specified are in the same set.
        /// </summary>
        /// <param name="item1" />
        /// <param name="item2" />
        /// <returns>True if the two objects are in the same set, false otherwise.</returns>
        bool InSameSet(T item1, T item2);
    }
}
