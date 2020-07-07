using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// A convenient base class to inherit from when implementing <see cref="IMapView{T}"/> that minimizes
    /// the number of items you must implement by implementing indexers in terms of a single indexer taking Point.
    /// </summary>
    /// <remarks>
    /// All algorithms in GoRogue that operate on map views use <see cref="IMapView{T}"/>, which this implements;
    /// but if you cannot inherit from a base class, you may still implement <see cref="IMapView{T}"/> directly.
    /// This class simply serves as a convenience feature for quick implementations.
    /// </remarks>
    [PublicAPI]
    public abstract class MapViewBase<T> : IMapView<T>
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
