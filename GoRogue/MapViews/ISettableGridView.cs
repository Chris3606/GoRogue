using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Interface designed to act as a standardized input/output format that defines minimal required data
    /// for algorithms that operate and make changes to a grid of some sort.  For a concrete implementation to subclass
    /// for custom implementations, see <see cref="SettableGridViewBase{T}"/>.
    /// </summary>
    /// <remarks>
    /// See <see cref="IGridView{T}" />. This interface serves the same purpose, but for cases when it is also
    /// necessary for an algorithm to be able to change the value at each location.
    ///
    /// Like IGridView, a number of implementations of this interface to cover common needs are provided.  For example,
    /// <see cref="ArrayView{T}"/> defines the interface such that the data is retrieved from and set to an array, and
    /// <see cref="LambdaSettableGridView{T}"/> defines the interface such that arbitrary callbacks are used to retrieve
    /// and set the data.
    /// </remarks>
    /// <typeparam name="T">The type of value being returned/set by the indexer functions.</typeparam>
    [PublicAPI]
    public interface ISettableGridView<T> : IGridView<T>
    {
        /// <summary>
        /// Given an X and Y value, returns/sets the "value" associated with that location.
        /// </summary>
        /// <remarks>
        /// Typically, this can be implemented via <see cref="this[Point]"/>.
        /// </remarks>
        /// <param name="x">X-value of location.</param>
        /// <param name="y">Y-value of location.</param>
        /// <returns>The "value" associated with that location.</returns>
        new T this[int x, int y] { get; set; }

        /// <summary>
        /// Given a position, returns/sets the "value" associated with that location.
        /// </summary>
        /// <param name="pos">Location to get/set the value for.</param>
        /// <returns>The "value" associated with the provided location.</returns>
        new T this[Point pos] { get; set; }

        /// <summary>
        /// Given an 1-dimensional index, returns/sets the value associated with the corresponding position
        /// in the map view.
        /// </summary>
        /// <remarks>
        /// Typically, this may be implemented in terms of <see cref="this[Point]" /> by using
        /// <see cref="Point.FromIndex(int, int)" /> to calculate the 2D position represented by that
        /// 1D index, and passing that position to the <see cref="this[Point]" /> indexer to get/set
        /// the value associated with the position.
        /// </remarks>
        /// <param name="index1D">1D index of location to get/set the "value" for.</param>
        /// <returns>The "value" associated with the given location.</returns>
        new T this[int index1D] { get; set; }
    }
}
