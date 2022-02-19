using System.Collections.Generic;
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
        /// <inheritdoc/>
        public int MaxLists { get; set; }

        private List<List<T>> _lists;

        /// <summary>
        /// Constructs a new pool with the given parameters.
        /// </summary>
        /// <param name="maxLists">
        /// Maximum number of lists that are allowed to be in the pool at any given time.  Any lists beyond this number
        /// which are returned, are allowed to be GCed.
        /// </param>
        public ListPool(int maxLists)
        {
            MaxLists = maxLists;

            _lists = new List<List<T>>();
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

        /// <inheritdoc/>
        public void Return(List<T> list, bool clear = true)
        {
            if (_lists.Count >= MaxLists) return;

            if (clear && list.Count > 0)
                list.Clear();

            _lists.Add(list);
        }
    }
}
