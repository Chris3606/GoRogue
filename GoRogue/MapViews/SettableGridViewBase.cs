using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// A convenient base class to inherit from when implementing <see cref="ISettableGridView{T}"/> that minimizes
    /// the number of items you must implement by implementing indexers in terms of a single indexer taking a Point.
    /// </summary>
    [PublicAPI]
    public abstract class SettableGridViewBase<T> : ISettableGridView<T>
    {
        /// <inheritdoc />
        public abstract int Height { get; }

        /// <inheritdoc />
        public abstract int Width { get; }

        /// <inheritdoc cref="ISettableGridView{T}"/>
        public abstract T this[Point pos] { get; set; }

        /// <inheritdoc />
        public int Count => Width * Height;

        /// <inheritdoc cref="ISettableGridView{T}"/>
        public T this[int x, int y]
        {
            get => this[new Point(x, y)];
            set => this[new Point(x, y)] = value;
        }

        /// <inheritdoc cref="ISettableGridView{T}"/>
        public T this[int index1D]
        {
            get => this[Point.FromIndex(index1D, Width)];
            set => this[Point.FromIndex(index1D, Width)] = value;
        }
    }
}
