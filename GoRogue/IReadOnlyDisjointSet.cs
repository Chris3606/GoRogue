namespace GoRogue
{
    /// <summary>
    /// Basic read-only representation of a Disjoint set data structure. Assumes it is holding
    /// integers between 0 and size - 1.
    /// </summary>
    public interface IReadOnlyDisjointSet
    {
        /// <summary>
        /// Number of distinct sets.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Returns the parent of the set containing obj, performing path compression as search is completed.
        /// </summary>
        /// <param name="obj">Object to search for.</param>
        /// <returns>The parent of the obj given.</returns>
        int Find(int obj);

        /// <summary>
        /// Returns true if the two objects specified are in the same set.
        /// </summary>
        /// <param name="obj1">First object.</param>
        /// <param name="obj2">Second object.</param>
        /// <returns>True if the two objects are in the same set, false otherwise.</returns>
        bool InSameSet(int obj1, int obj2);
    }
}