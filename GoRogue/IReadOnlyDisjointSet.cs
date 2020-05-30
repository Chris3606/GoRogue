using JetBrains.Annotations;

namespace GoRogue
{
    /// <summary>
    /// Read-only representation of <see cref="DisjointSet"/>
    /// </summary>
    [PublicAPI]
    public interface IReadOnlyDisjointSet
    {
        /// <summary>
        /// Number of distinct sets.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Returns the parent of the set containing <paramref name="objectID"/>, performing path compression as search is completed.
        /// </summary>
        /// <param name="objectID">Object to search for.</param>
        /// <returns>The parent of the object given.</returns>
        int Find(int objectID);

        /// <summary>
        /// Returns true if the two objects specified are in the same set.
        /// </summary>
        /// <param name="obj1"/>
        /// <param name="obj2"/>
        /// <returns>True if the two objects are in the same set, false otherwise.</returns>
        bool InSameSet(int obj1, int obj2);
    }
}
