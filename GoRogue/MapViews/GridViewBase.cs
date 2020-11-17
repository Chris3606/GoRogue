using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// A convenient base class to inherit from when implementing <see cref="IGridView{T}"/> that minimizes
    /// the number of items you must implement by implementing indexers in terms of a single indexer taking a Point.
    /// </summary>
    [PublicAPI]
    public abstract class GridViewBase<T> : IGridView<T>
    {
        /// <inheritdoc />
        public abstract int Height { get; }
        /// <inheritdoc />
        public abstract int Width { get; }

        /// <inheritdoc />
        public abstract T this[Point pos] { get; }

        /// <inheritdoc />
        public int Count => Width * Height;

        /// <inheritdoc />
        public T this[int x, int y] => this[new Point(x, y)];

        /// <inheritdoc />
        public T this[int index1D] => this[Point.FromIndex(index1D, Width)];
    }
}
