namespace GoRogue
{
	/// <summary>
	/// Read-only representation of <see cref="DisjointSet"/>
	/// </summary>
	public interface IReadOnlyDisjointSet
	{
		/// <summary>
		/// Number of distinct sets.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Returns the parent of the set containing <paramref name="obj"/>, performing path compression as search is completed.
		/// </summary>
		/// <param name="obj">Object to search for.</param>
		/// <returns>The parent of the object given.</returns>
		int Find(int obj);

		/// <summary>
		/// Returns true if the two objects specified are in the same set.
		/// </summary>
		/// <param name="obj1"/>
		/// <param name="obj2"/>
		/// <returns>True if the two objects are in the same set, false otherwise.</returns>
		bool InSameSet(int obj1, int obj2);
	}
}
