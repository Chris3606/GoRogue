namespace GoRogue.MapViews
{
    /// <summary>
    /// Interface designed to act as a standardized input/output for algorithms that need to make
    /// modifications to data.
    /// </summary>
    /// <remarks>
    /// See IMapView documentation. Algorithms such as map generation may need to modify their
    /// inputs. Again, creating an actual 2D array can be tedious, depending on the application. As
    /// such, this interface extends IMapView to provide the capability to "set" values. /// Again,
    /// in case an actual 2D array is desired, ArrayMap implements this interface and provides a
    /// similar interface.
    /// </remarks>
    /// <typeparam name="T">The type of value being returned/set by the indexer functions.</typeparam>
    public interface ISettableMapView<T> : IMapView<T>
    {
        /// <summary>
        /// Given an X and Y value, should return/set the "value" associated with that location.
        /// </summary>
        /// <param name="x">X-value of location.</param>
        /// <param name="y">Y-value of location.</param>
        /// <returns>The "value" associated with that location.</returns>
        new T this[int x, int y] { get; set; }

        /// <summary>
        /// Given a Coord, should return/set the "value" associated with that location.
        /// </summary>
        /// <param name="pos">Location to get/set the value for.</param>
        /// <returns>The "value" associated with the provided location.</returns>
        new T this[Coord pos] { get; set; }
    }
}