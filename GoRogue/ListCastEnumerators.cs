using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue
{
    /// <summary>
    /// A custom enumerator that iterates over a List and casts its objects to the given type.
    ///
    /// All objects _must_ be of the specified type, or the iterator will not function.
    /// </summary>
    /// <remarks>
    /// This type is a struct, and as such is much more efficient when used in a foreach loop than a function returning
    /// IEnumerable&lt;T&gt; or using System.LINQ extensions such as Where.
    ///
    /// Otherwise, it has basically the same characteristics that exposing a list as <see cref="IEnumerable{T}"/> would;
    /// so if you need to expose items as some type like IEnumerable, and the items are internally stored as a list, this
    /// can be a good option.  This type does implement IEnumerable, and as such can be used directly with functions that
    /// require one (for example, System.LINQ).  However, this will have reduced performance due to boxing of the iterator.
    /// </remarks>
    /// <typeparam name="TBase">Type items in the list are stored as.</typeparam>
    /// <typeparam name="TItem">Actual type of items in the list.</typeparam>
    [PublicAPI]
    public struct ListCastEnumerator<TBase, TItem> : IEnumerator<TItem>, IEnumerable<TItem>
        where TItem : TBase
    {
        private List<TBase>.Enumerator _enumerator;
        private TItem _current;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="list">List to iterate over.</param>
        public ListCastEnumerator(List<TBase> list)
        {
            _enumerator = list.GetEnumerator();
            _current = default!;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _enumerator.Dispose();
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (!_enumerator.MoveNext()) return false;

            _current = (TItem)_enumerator.Current!;
            return true;
        }

        /// <inheritdoc/>
        public TItem Current => _current;

        object? IEnumerator.Current => _current;

        void IEnumerator.Reset()
        {
            ((IEnumerator)_enumerator).Reset();
        }

        /// <summary>
        /// Returns this enumerator.
        /// </summary>
        /// <returns>This enumerator.</returns>
        public ListCastEnumerator<TBase, TItem> GetEnumerator() => this;

        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;
    }

    /// <summary>
    /// A structure similar to <see cref="ListCastEnumerator{TBase, TItem}"/>, but for <see cref="IReadOnlyList{T}"/>.  It is not quite
    /// as fast as <see cref="ListCastEnumerator{TBase, TItem}"/>, but is still faster than using the typical Enumerable implementation
    /// for IReadOnlyList.  You should only use this if you can't use <see cref="ListCastEnumerator{TBase, TItem}"/> due to the type
    /// you're working with; they share the same characteristics otherwise.
    ///
    /// All objects _must_ be of the specified type, or the iterator will not function.
    /// </summary>
    /// <typeparam name="TBase">Type items in the list are stored as.</typeparam>
    /// <typeparam name="TItem">Actual type of items in the list.</typeparam>
    [PublicAPI]
    public struct ReadOnlyListCastEnumerator<TBase, TItem> : IEnumerator<TItem>, IEnumerable<TItem>
        where TItem : TBase
    {
        private ReadOnlyListEnumerator<TBase> _enumerator;
        private TItem _current;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="list">List to iterate over.</param>
        public ReadOnlyListCastEnumerator(IReadOnlyList<TBase> list)
        {
            _enumerator = new ReadOnlyListEnumerator<TBase>(list);
            _current = default!;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _enumerator.Dispose();
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (!_enumerator.MoveNext()) return false;

            _current = (TItem)_enumerator.Current!;
            return true;
        }

        /// <inheritdoc/>
        public TItem Current => _current;

        object? IEnumerator.Current => _current;

        void IEnumerator.Reset()
        {
            ((IEnumerator)_enumerator).Reset();
        }

        /// <summary>
        /// Returns this enumerator.
        /// </summary>
        /// <returns>This enumerator.</returns>
        public ReadOnlyListCastEnumerator<TBase, TItem> GetEnumerator() => this;

        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;
    }
}
