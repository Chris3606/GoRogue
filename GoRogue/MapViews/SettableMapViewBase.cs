using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// A convenient base class to inherit from when implementing <see cref="ISettableMapView{T}"/> that minimizes
    /// the number of items you must implement by automatically forwarding to the interface's default implementations
    /// where possible.
    /// </summary>
    /// <remarks>
    /// All algorithms in GoRogue that operate on settable map views use <see cref="ISettableMapView{T}"/>, which this
    /// implements; but if you cannot inherit from a base class, you may still implement
    /// <see cref="ISettableMapView{T}"/> directly. This class simply serves as a convenience feature for quick
    /// implementations.
    /// </remarks>
    [PublicAPI]
    public abstract class SettableMapViewBase<T> : ISettableMapView<T>
    {
        /// <inheritdoc />
        public abstract int Height { get; }

        /// <inheritdoc />
        public abstract int Width { get; }

        /// <inheritdoc cref="ISettableMapView{T}"/>
        public abstract T this[Point pos] { get; set; }

        /// <inheritdoc />
        public int Count => Width * Height;

        /// <inheritdoc cref="ISettableMapView{T}"/>
        public T this[int x, int y]
        {
            get => this[new Point(x, y)];
            set => this[new Point(x, y)] = value;
        }

        /// <inheritdoc cref="ISettableMapView{T}"/>
        public T this[int index1D]
        {
            get => this[Point.FromIndex(index1D, Width)];
            set => this[Point.FromIndex(index1D, Width)] = value;
        }
    }
}
