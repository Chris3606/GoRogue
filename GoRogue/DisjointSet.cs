﻿using System.Collections.Generic;

namespace GoRogue
{
	/// <summary>
	/// Basic representation of a disjoint set data structure.
	/// </summary>
	/// <remarks>
	/// For reasons pertaining to optimization, this disjoint set implementation does not use
	/// generics, and instead holds integer values, which MUST be exactly all integer values in range
	/// [0, num_items_in_set - 1].  Thus, you will need to assign appropriate IDs to objects you intend
	/// to add and map them appropriately.
	/// </remarks>
	public class DisjointSet : IReadOnlyDisjointSet
	{
		private int[] parents;

		private int[] sizes;

		/// <summary>
		/// Constructor. The disjoint set will contain all values in range [0, <paramref name="size"/> - 1].
		/// </summary>
		/// <param name="size">(Max) size of the disjoint set.</param>
		public DisjointSet(int size)
		{
			Count = size;
			parents = new int[size];
			sizes = new int[size];

			for (int i = 0; i < size; i++)
			{
				parents[i] = i;
				sizes[i] = 1;
			}
		}

		/// <summary>
		/// Number of distinct sets.
		/// </summary>
		public int Count { get; private set; }

		/// <summary>
		/// Returns a read-only representation of the disjoint set.
		/// </summary>
		/// <returns>A read-only representation of the disjoint set.</returns>
		public IReadOnlyDisjointSet AsReadOnly() => this;

		/// <summary>
		/// Returns the parent of the set containing <paramref name="obj"/>, performing path compression
		/// as the search is completed.
		/// </summary>
		/// <param name="obj">Object to search for.</param>
		/// <returns>The parent of the object given.</returns>
		public int Find(int obj)
		{
			// Find base parent, and path compress
			if (obj != parents[obj])
				parents[obj] = Find(parents[obj]);

			return parents[obj];
		}

		/// <summary>
		/// Returns true if the two objects specified are in the same set.
		/// </summary>
		/// <param name="obj1"/>
		/// <param name="obj2"/>
		/// <returns>True if the two objects are in the same set, false otherwise.</returns>
		public bool InSameSet(int obj1, int obj2)
		{
			return Find(obj1) == Find(obj2); // In same set; same parent
		}

		/// <summary>
		/// Performs a union of the sets containing the two objects specified. After this operation,
		/// every element in the sets containing the two objects specified will be part of one larger set.
		/// </summary>
		/// <remarks>If the two elements are already in the same set, nothing is done.</remarks>
		/// <param name="obj1"/>
		/// <param name="obj2"/>
		public void MakeUnion(int obj1, int obj2)
		{
			int i = Find(obj1);
			int j = Find(obj2);

			if (i == j) return; // Two elements are already in same set; same parent

			// Always append smaller set to larger set
			if (sizes[i] <= sizes[j])
			{
				parents[i] = j;
				sizes[j] += sizes[i];
			}
			else
			{
				parents[j] = i;
				sizes[i] += sizes[j];
			}
			Count--;
		}

		/// <summary>
		/// Returns a string representation of the DisjointSet, showing parents and all elements in
		/// their set.
		/// </summary>
		/// <returns>A string representation of the DisjointSet.</returns>
		public override string ToString()
		{
			var values = new Dictionary<int, List<int>>();

			for (int i = 0; i < parents.Length; i++)
			{
				int parentOf = findNoCompression(i);
				if (!values.ContainsKey(parentOf))
				{
					values[parentOf] = new List<int>();
					values[parentOf].Add(parentOf); // Parent is the first element in each child list
				}

				if (parentOf != i) // We already added the parent, so don't double add
					values[parentOf].Add(i);
			}

			return values.ExtendToString("", valueStringifier: (List<int> obj) => obj.ExtendToString(), kvSeparator: ": ", pairSeparator: "\n", end: "");
		}

		// Used to ensure ToString doesn't affect the performance of future operations
		private int findNoCompression(int obj)
		{
			while (parents[obj] != obj)
				obj = parents[obj];

			return obj;
		}
	}
}
