using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Interface designed to act as a standardized input/output interpretation for algorithms that need to make
    /// modifications to data on a map.
    /// </summary>
    /// <remarks>
    /// See <see cref="IMapView{T}"/>. Algorithms such as map generation may need to modify their
    /// inputs. Again, creating an actual 2D array can be tedious, depending on the application. As
    /// such, this interface extends IMapView to provide the capability to "set" values
    /// 
    /// Like IMapView, a number of implementations of this interface to cover common cases are provided.  For example,
    /// in case an actual array is desired, <see cref="ArrayMap{T}"/> implements this interface for you via an actual
    /// array. Similarly, <see cref="LambdaSettableMapView{T}"/> implements the interface for you via a function you
    /// pass to it.
    /// </remarks>
    /// <typeparam name="T">The type of value being returned/set by the indexer functions.</typeparam>
    [PublicAPI]
    public interface ISettableMapView<T> : IMapView<T>
    {
        /// <summary>
        /// Given an X and Y value, returns/sets the "value" associated with that location.
        /// </summary>
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
        /// Typically, this may be implemented in terms of <see cref="this[Point]"/> by using
        /// <see cref="Point.FromIndex(int, int)"/> to calculate the 2D position represented by that
        /// 1D index, and passing that position to the <see cref="this[Point]"/> indexer to get/set
        /// the value associated with the position.
        /// </remarks>
        /// <param name="index1D">1D index of location to get/set the "value" for.</param>
        /// <returns>The "value" associated with the given location.</returns>
        new T this[int index1D] { get; set; }
    }
}
