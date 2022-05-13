using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace GoRogue
{
    /// <summary>
    /// Event arguments for the <see cref="DisjointSet.SetsJoined"/> event.
    /// </summary>
    [PublicAPI]
    public class JoinedEventArgs : EventArgs
    {
        /// <summary>
        /// The larger of the two sets that were joined; becomes the new parent set.
        /// </summary>
        public readonly int LargerSetID;

        /// <summary>
        /// The smaller of the two sets that were joined; becomes the new child set.
        /// </summary>
        public readonly int SmallerSetID;

        /// <summary>
        ///
        /// </summary>
        /// <param name="largerSetID">The larger of the two sets that were joined; becomes the new parent set.</param>
        /// <param name="smallerSetID">The smaller of the two sets that were joined; becomes the new child set.</param>
        public JoinedEventArgs(int largerSetID, int smallerSetID)
        {
            LargerSetID = largerSetID;
            SmallerSetID = smallerSetID;
        }
    }

    /// <summary>
    /// Event arguments for the <see cref="DisjointSet{T}.SetsJoined"/> event.
    /// </summary>
    [PublicAPI]
    public class JoinedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// The larger of the two sets that were joined; becomes the new parent set.
        /// </summary>
        public readonly T LargerSet;

        /// <summary>
        /// The smaller of the two sets that were joined; becomes the new child set.
        /// </summary>
        public readonly T SmallerSet;

        /// <summary>
        ///
        /// </summary>
        /// <param name="largerSet">The larger of the two sets that were joined; becomes the new parent set.</param>
        /// <param name="smallerSet">The smaller of the two sets that were joined; becomes the new child set.</param>
        public JoinedEventArgs(T largerSet, T smallerSet)
        {
            LargerSet = largerSet;
            SmallerSet = smallerSet;
        }
    }

    /// <summary>
    /// Basic representation of a disjoint set data structure.
    /// </summary>
    /// <remarks>
    /// For reasons pertaining to optimization, this disjoint set implementation does not use
    /// generics, and instead holds integer values, which will be exactly all integer values in range
    /// [0, num_items_in_set - 1].  Thus, you will need to assign appropriate IDs to objects you intend
    /// to add and map them appropriately.
    /// </remarks>
    [Serializable]
    [PublicAPI]
    public class DisjointSet : IReadOnlyDisjointSet
    {
        private readonly int[] _parents;
        private readonly int[] _sizes;

        /// <inheritdoc />
        public event EventHandler<JoinedEventArgs>? SetsJoined;

        /// <summary>
        /// Constructor. The disjoint set will contain all values in range [0, <paramref name="size" /> - 1].
        /// </summary>
        /// <param name="size">(Max) size of the disjoint set.</param>
        public DisjointSet(int size)
        {
            Count = size;
            _parents = new int[size];
            _sizes = new int[size];

            for (var i = 0; i < size; i++)
            {
                _parents[i] = i;
                _sizes[i] = 1;
            }
        }

        /// <inheritdoc />
        public int Count { get; private set; }

        /// <inheritdoc />
        public int Find(int obj)
        {
            // Find base parent, and path compress
            if (obj != _parents[obj])
                _parents[obj] = Find(_parents[obj]);

            return _parents[obj];
        }

        /// <inheritdoc />
        public bool InSameSet(int obj1, int obj2) => Find(obj1) == Find(obj2); // In same set; same parent

        /// <summary>
        /// Returns a read-only representation of the disjoint set.
        /// </summary>
        /// <returns>A read-only representation of the disjoint set.</returns>
        public IReadOnlyDisjointSet AsReadOnly() => this;

        /// <summary>
        /// Performs a union of the sets containing the objects specified by the two IDs. After this operation,
        /// every element in the sets containing the two objects specified will be part of one larger set.
        /// </summary>
        /// <remarks>If the two elements are already in the same set, nothing is done.</remarks>
        /// <param name="obj1" />
        /// <param name="obj2" />
        public void MakeUnion(int obj1, int obj2)
        {
            var i = Find(obj1);
            var j = Find(obj2);

            if (i == j) return; // Two elements are already in same set; same parent

            // Always append smaller set to larger set
            if (_sizes[i] <= _sizes[j])
            {
                _parents[i] = j;
                _sizes[j] += _sizes[i];
                SetsJoined?.Invoke(this, new JoinedEventArgs(j, i));
            }
            else
            {
                _parents[j] = i;
                _sizes[i] += _sizes[j];
                SetsJoined?.Invoke(this, new JoinedEventArgs(i, j));
            }

            Count--;
        }

        /// <summary>
        /// Returns a string representation of the DisjointSet, showing IDs of parents and all elements in
        /// their set.
        /// </summary>
        /// <returns>A string representation of the DisjointSet.</returns>
        public override string ToString() => ExtendToString(i => i.ToString());

        /// <summary>
        /// Returns a string representation of the DisjointSet, showing parents and all elements in
        /// their set.  The given function is used to produce the string for each element.
        /// </summary>
        /// <returns>A string representation of the DisjointSet.</returns>
        public string ExtendToString(Func<int, string> elementStringifier)
        {
            var values = new Dictionary<int, List<int>>();

            for (var i = 0; i < _parents.Length; i++)
            {
                var parentOf = FindNoCompression(i);
                if (!values.ContainsKey(parentOf))
                    values[parentOf] = new List<int>
                    {
                        parentOf // Parent is the first element in each child list
                    };

                if (parentOf != i) // We already added the parent, so don't double add
                    values[parentOf].Add(i);
            }

            return values.ExtendToString("", valueStringifier: obj => obj.ExtendToString(elementStringifier: elementStringifier), kvSeparator: ": ",
                pairSeparator: "\n", end: "");
        }

        // Used to ensure ToString doesn't affect the performance of future operations
        private int FindNoCompression(int obj)
        {
            while (_parents[obj] != obj)
                obj = _parents[obj];

            return obj;
        }
    }

    /// <summary>
    /// An easier-to-use (but less efficient) variant of <see cref="DisjointSet"/>.  This version takes actual objects
    /// of type T, and manages IDs for you automatically.
    /// </summary>
    /// <remarks>
    /// This set structure is effectively exactly like <see cref="DisjointSet"/>, however it takes type T instead
    /// of IDs.  For the sake of efficiency, it still requires the number of elements to be known when the set is
    /// created.
    /// </remarks>
    /// <typeparam name="T">Type of elements in the set.</typeparam>
    [PublicAPI]
    public class DisjointSet<T> : IReadOnlyDisjointSet<T>
        where T : notnull
    {
        private DisjointSet _idSet;
        private Dictionary<T, int> _indices;
        private T[] _items;

        /// <inheritdoc />
        public event EventHandler<JoinedEventArgs<T>>? SetsJoined;

        /// <inheritdoc />
        public int Count => _idSet.Count;

        /// <summary>
        /// Creates a new disjoint set that is composed of the given items.  Each item will be its own
        /// unique set.
        /// </summary>
        /// <param name="items">Items to place in the disjoint set.</param>
        public DisjointSet(IEnumerable<T> items)
        {
            _items = items.ToArray();
            _indices = new Dictionary<T, int>(_items.Length);
            _idSet = new DisjointSet(_items.Length);

            _idSet.SetsJoined += IDSetOnSetsJoined;
        }

        /// <inheritdoc />
        public T Find(T item)
        {
            int parentID = _idSet.Find(_indices[item]);
            return _items[parentID];
        }

        /// <inheritdoc />
        public bool InSameSet(T item1, T item2)
            => _idSet.InSameSet(_indices[item1], _indices[item2]);

        /// <summary>
        /// Returns a read-only representation of the disjoint set.
        /// </summary>
        /// <returns>A read-only representation of the disjoint set.</returns>
        public IReadOnlyDisjointSet<T> AsReadOnly() => this;

        /// <summary>
        /// Performs a union of the sets containing the two objects specified. After this operation,
        /// every element in the sets containing the two objects specified will be part of one larger set.
        /// </summary>
        /// <remarks>If the two elements are already in the same set, nothing is done.</remarks>
        /// <param name="item1" />
        /// <param name="item2" />
        public void MakeUnion(T item1, T item2)
            => _idSet.MakeUnion(_indices[item1], _indices[item2]);

        /// <summary>
        /// Returns a string representation of the DisjointSet, parents and all elements in
        /// their set.  The element's default ToString method is used to produce the string.
        /// </summary>
        /// <returns>A string representation of the DisjointSet.</returns>
        public override string ToString()
            => _idSet.ExtendToString(i => _items[i].ToString() ?? "null");

        /// <summary>
        /// Returns a string representation of the DisjointSet, showing parents and all elements in
        /// their set.  The given function is used to produce the string for each element.
        /// </summary>
        /// <returns>A string representation of the DisjointSet.</returns>
        public string ExtendToString(Func<T, string> elementStringifier)
            => _idSet.ExtendToString(i => elementStringifier(_items[i]));

        #region Event Synchronization
        // When the internal event is fired, fire the public one by finding the objects corresponding to the IDs given.
        private void IDSetOnSetsJoined(object? sender, JoinedEventArgs e)
            => SetsJoined?.Invoke(this, new JoinedEventArgs<T>(_items[e.LargerSetID], _items[e.SmallerSetID]));
        #endregion
    }
}
