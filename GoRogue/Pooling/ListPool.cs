using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace GoRogue.Pooling
{
    /// <summary>
    /// A basic implementation of <see cref="IListPool{T}"/> which uses a simple List of lists to store the pool.
    /// </summary>
    /// <typeparam name="T">Type of items being stored in the list.</typeparam>
    [PublicAPI]
    public sealed class ListPool<T> : IListPool<T>
    {
        /// <summary>
        /// Maximum number of lists that are allowed to be in the pool at any given time.  Any lists beyond this number
        /// which are returned, are allowed to be GCed.
        /// </summary>
        public int MaxLists { get; }

        /// <summary>
        /// The maximum <see cref="List{T}.Capacity"/> allowed for lists that are returned via <see cref="Return"/>.
        /// Any lists with capacities over this value are shrunk to this value when they are returned.
        /// </summary>
        /// <remarks>
        /// This value, in combination with <see cref="MaxLists"/>, ensures that a hard limit can be imposed on the memory usage
        /// for a pool.  However, if this value is small relative to the actual capacity of lists being returned, it will
        /// result in lists that are constantly shrunk then sized back up when they are used; and as such it will reduce
        /// the effectiveness of the pool.
        /// </remarks>
        public int MaxCapacity { get; }

        private readonly List<List<T>> _lists;

        /// <summary>
        /// Constructs a new pool with the given parameters.
        /// </summary>
        /// <param name="maxLists">
        /// Maximum number of lists that are allowed to be in the pool at any given time.  Any lists beyond this number
        /// which are returned, are allowed to be GCed.
        /// </param>
        /// <param name="maxCapacity">
        /// The maximum <see cref="List{T}.Capacity"/> allowed for lists that are returned via <see cref="Return"/>.
        /// Any lists with capacities over this value are shrunk to this value when they are returned.
        /// </param>
        public ListPool(int maxLists, int maxCapacity)
        {
            MaxLists = maxLists;
            MaxCapacity = maxCapacity;

            _lists = new List<List<T>>(maxLists);
        }

        /// <inheritdoc/>
        public List<T> Rent()
        {
            int count = _lists.Count;
            if (count == 0)
                return new List<T>();

            var list = _lists[^1];
            _lists.RemoveAt(count - 1);
            return list;
        }

        /// <summary>
        /// Returns the given list to the pool.  The list will be discarded (and allowed to be queued for GC) if
        /// there are already at least <see cref="MaxLists"/> unused lists in the pool.
        /// </summary>
        /// <param name="list">The list to return.</param>
        /// <param name="clear">
        /// Whether or not to clear the list given before adding it to the pool.  Should be set to true unless you are
        /// absolutely sure the list is cleared via other means before passing it.
        /// </param>
        public void Return(List<T> list, bool clear = true)
        {
            if (_lists.Count >= MaxLists) return;

            if (clear && list.Count > 0)
                list.Clear();

            if (list.Capacity > MaxCapacity)
                list.Capacity = MaxCapacity;

            _lists.Add(list);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _lists.Clear();
    }
}
