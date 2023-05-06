using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GoRogue
{
    // TODO: More generic type params for TBase?
    /// <summary>
    /// A custom enumerator that iterates over a List and casts its objects to the given type.
    /// </summary>
    /// <remarks>
    /// This type is a struct, and as such is much more efficient when used in a foreach loop than a function returning
    /// IEnumerable&lt;T&gt;.  It is also faster than iterating over a list via an IReadOnlyList reference.
    ///
    /// Otherwise, it has basically the same characteristics that exposing a list as <see cref="IEnumerable{T}"/> would;
    /// so if you need to expose items as some type like IEnumerable, and the items are internally stored as a list, this
    /// can be a good option.  This type does implement IEnumerable, and as such can be used directly with functions that
    /// require one (for example, System.LINQ).  However, this will have reduced performance due to boxing of the iterator.
    /// </remarks>
    /// <typeparam name="T">Actual type of items in the list.</typeparam>
    [PublicAPI]
    public struct ObjectListCastEnumerator<T> : IEnumerator<T>, IEnumerable<T>
    {
        private List<object>.Enumerator _enumerator;
        private T _current;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="list">List to iterate over.</param>
        public ObjectListCastEnumerator(List<object> list)
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

            _current = (T)_enumerator.Current;
            return true;
        }

        /// <inheritdoc/>
        public T Current => _current;

        object? IEnumerator.Current => _current;

        void IEnumerator.Reset()
        {
            ((IEnumerator)_enumerator).Reset();
        }

        /// <summary>
        /// Returns this enumerator.
        /// </summary>
        /// <returns>This enumerator.</returns>
        public ObjectListCastEnumerator<T> GetEnumerator() => this;

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;
    }

    ///// <summary>
    ///// A structure similar to <see cref="ListEnumerator{T}"/>, but for <see cref="IReadOnlyList{T}"/>.  It is not quite
    ///// as fast as <see cref="ListEnumerator{T}"/>, but is still faster than using the typical Enumerable implementation
    ///// for IReadOnlyList.  You should only use this if you can't use <see cref="ListEnumerator{T}"/> due to the type
    ///// you're working with; they share the same characteristics otherwise.
    ///// </summary>
    ///// <typeparam name="T">Types of elements stored in the list.</typeparam>
    //[PublicAPI]
    //public struct ReadOnlyListEnumerator<T> : IEnumerator<T>, IEnumerable<T>
    //{
    //    private readonly int _size;
    //    private readonly IReadOnlyList<T> _list;
    //    private int _index;
    //    private T _current;

    //    /// <summary>
    //    /// Constructor.
    //    /// </summary>
    //    /// <param name="list">List to iterate over.</param>
    //    public ReadOnlyListEnumerator(IReadOnlyList<T> list)
    //    {
    //        _list = list;
    //        _size = _list.Count;
    //        _index = 0;
    //        _current = default!;
    //    }

    //    /// <inheritdoc/>
    //    public void Dispose()
    //    {
    //    }

    //    /// <inheritdoc/>
    //    public bool MoveNext()
    //    {
    //        IReadOnlyList<T> localList = _list;

    //        if (((uint)_index < (uint)_size))
    //        {
    //            _current = localList[_index];
    //            _index++;
    //            return true;
    //        }
    //        return MoveNextRare();
    //    }

    //    private bool MoveNextRare()
    //    {
    //        _index = _size + 1;
    //        _current = default!;
    //        return false;
    //    }

    //    /// <inheritdoc/>
    //    public T Current => _current!;

    //    object? IEnumerator.Current
    //    {
    //        get
    //        {
    //            if (_index == 0 || _index == _size + 1)
    //            {
    //                throw new InvalidOperationException("Enumeration operation cannot happen.");
    //            }
    //            return Current;
    //        }
    //    }

    //    void IEnumerator.Reset()
    //    {
    //        _index = 0;
    //        _current = default!;
    //    }

    //    /// <summary>
    //    /// Returns this enumerator.
    //    /// </summary>
    //    /// <returns>This enumerator.</returns>
    //    public ReadOnlyListEnumerator<T> GetEnumerator() => this;

    //    IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;

    //    IEnumerator IEnumerable.GetEnumerator() => this;
    //}
}
