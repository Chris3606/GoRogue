using System.Collections.Generic;

namespace GoRogue.Pooling
{
    /// <summary>
    /// A basic interface for classes which act as a pool of <see cref="List{T}"/> structures.  It provides functions to
    /// rent lists from the pool, return them, and limit the number of lists kept in the pool.
    /// </summary>
    /// <remarks>
    /// The API for this interface is generally inspired by <see cref="System.Buffers.ArrayPool{T}"/>, and serves a similar
    /// purpose, except for lists.  Note that, at the current time, the API is more limited and the implementation less
    /// efficient than ArrayPool.  The biggest difference is that this pool effectively assumes lists are blank, and
    /// doesn't have any way to take into account list capacity when requesting a list.
    /// </remarks>
    /// <typeparam name="T">Type of items being stored in the list.</typeparam>
    public interface IListPool<T>
    {
        /// <summary>
        /// Maximum number of lists that are allowed to be in the pool at any given time.  Any lists beyond this number
        /// which are returned, are allowed to be GCed.
        /// </summary>
        int MaxLists { get; set; }

        /// <summary>
        /// Retrieve a list from the pool, or allocate a new one if there are no lists available.
        /// </summary>
        /// <returns>A list from the pool, or a new list if no lists are available in the pool.</returns>
        public List<T> Rent();

        /// <summary>
        /// Returns the given list to the pool.  The list will be discarded (and allowed to be queued for GC) if
        /// there are already at least <see cref="MaxLists"/> unused lists in the pool.
        /// </summary>
        /// <param name="list">The list to return.</param>
        /// <param name="clear">
        /// Whether or not to clear the list given before adding it to the pool.  Should be set to true unless you are
        /// absolutely sure the list is cleared via other means before passing it.
        /// </param>
        public void Return(List<T> list, bool clear = true);
    }
}
