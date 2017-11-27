namespace GoRogue
{
    /// <summary>
    /// Basic representation of a Disjoint set data structure.  Assumes it is holding integers between 0 and size - 1.
    /// </summary>
    public class DisjointSet
    {
        /// <summary>
        /// Number of distinct sets.
        /// </summary>
        public int Count { get; private set; }

        private int[] parents;
        private int[] sizes;

        /// <summary>
        /// Constructor.  Size of disjoint set is specified -- it holds values between 0 and size - 1.
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
        /// Returns the parent of the set containing obj, performing path compression as search is completed.
        /// </summary>
        /// <param name="obj">Object to search for.</param>
        /// <returns>The parent of the obj given.</returns>
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
        /// <param name="obj1">First object.</param>
        /// <param name="obj2">Second object.</param>
        /// <returns>True if the two objects are in the same set, false otherwise.</returns>
        public bool InSameSet(int obj1, int obj2)
        {
            return Find(obj1) == Find(obj2); // In same set; same parent
        }

        /// <summary>
        /// Performs a union of the sets containing the two objects specified.  After this operation, every element
        /// in the sets containing the two objects specified will be part of one larger set.
        /// </summary>
        /// <remarks>
        /// If the two elements are already in the same set, nothing is done.
        /// </remarks>
        /// <param name="obj1">First object.</param>
        /// <param name="obj2">Second object.</param>
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
    }
}