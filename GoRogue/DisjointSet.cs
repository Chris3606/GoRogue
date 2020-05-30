using System;
using System.Collections.Generic;
using JetBrains.Annotations;

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
    [Serializable]
    [PublicAPI]
    public class DisjointSet : IReadOnlyDisjointSet
    {
        private readonly int[] _parents;

        private readonly int[] _sizes;

        /// <summary>
        /// Constructor. The disjoint set will contain all values in range [0, <paramref name="size"/> - 1].
        /// </summary>
        /// <param name="size">(Max) size of the disjoint set.</param>
        public DisjointSet(int size)
        {
            Count = size;
            _parents = new int[size];
            _sizes = new int[size];

            for (int i = 0; i < size; i++)
            {
                _parents[i] = i;
                _sizes[i] = 1;
            }
        }

        /// <inheritdoc/>
        public int Count { get; private set; }

        /// <summary>
        /// Returns a read-only representation of the disjoint set.
        /// </summary>
        /// <returns>A read-only representation of the disjoint set.</returns>
        public IReadOnlyDisjointSet AsReadOnly() => this;

        /// <inheritdoc/>
        public int Find(int obj)
        {
            // Find base parent, and path compress
            if (obj != _parents[obj])
                _parents[obj] = Find(_parents[obj]);

            return _parents[obj];
        }

        /// <inheritdoc/>
        public bool InSameSet(int obj1, int obj2) => Find(obj1) == Find(obj2); // In same set; same parent

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
            if (_sizes[i] <= _sizes[j])
            {
                _parents[i] = j;
                _sizes[j] += _sizes[i];
            }
            else
            {
                _parents[j] = i;
                _sizes[i] += _sizes[j];
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

            for (int i = 0; i < _parents.Length; i++)
            {
                int parentOf = FindNoCompression(i);
                if (!values.ContainsKey(parentOf))
                {
                    values[parentOf] = new List<int>
                    {
                        parentOf // Parent is the first element in each child list
                    };
                }

                if (parentOf != i) // We already added the parent, so don't double add
                    values[parentOf].Add(i);
            }

            return values.ExtendToString("", valueStringifier: obj => obj.ExtendToString(), kvSeparator: ": ", pairSeparator: "\n", end: "");
        }

        // Used to ensure ToString doesn't affect the performance of future operations
        private int FindNoCompression(int obj)
        {
            while (_parents[obj] != obj)
                obj = _parents[obj];

            return obj;
        }
    }
}
