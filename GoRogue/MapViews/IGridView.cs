using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Interface designed to act as a standardized input/output format that defines minimal required data
    /// for algorithms that operate on a grid of some sort.  For a concrete implementation to subclass for custom
    /// implementations, see <see cref="GridViewBase{T}"/>.
    /// </summary>
    /// <remarks>
    /// Many algorithms that operate on a grid need only some very basic information about each location in the grid
    /// to function.  For example, a basic grid-based pathfinding algorithm might only need to know whether each
    /// location can be traversed or not, which can be represented by a single boolean value per location.  A renderer
    /// might want to know simple rendering information for each location, which might be wrapped in a class or
    /// structure.
    ///
    /// One option for creating these algorithms would be to take as input arrays of the type they need.  For example,
    /// the pathing algorithm might take an array of boolean values as input.  However, this can be quite inflexible.
    /// The values that algorithms need to function might be determined based on a much more complex structure than a
    /// simple array of the precise type it needs, depending on use case.  Taking an array as input in these cases
    /// forces a user to either adapt their data structure to the one that the algorithm uses, or maintain multiple
    /// "copies" of their data in the format that the algorithms need.
    ///
    /// <see cref="IGridView{T}"/> is designed to act as a much more flexible input/output format for algorithms
    /// like this that operate on a grid.  The interface simply defines properties for width and height, and some
    /// basic abstract indexers that allow accessing the "object" at each location.  In the examples from above,
    /// the basic pathfinding algorithm might take as input an IGridView&lt;bool&gt;, whereas the renderer might take
    /// IGridView&lt;RenderingInfo&gt;.  This allows the algorithms to operate on the minimal data that they need
    /// to function, but allows a user to define where that data comes from.  For common cases, concrete implementations
    /// are provided that make the interface easier to use; for example, <see cref="ArrayView{T}"/> defines the
    /// interface such that the data comes from an array, and <see cref="LambdaGridView{T}"/> defines the interface
    /// such that an arbitrary callback is used to retrieve the data.
    /// </remarks>
    /// <typeparam name="T">The type of value being returned by the indexer functions.</typeparam>
    [PublicAPI]
    public interface IGridView<out T>
    {
        /// <summary>
        /// The height of the grid being represented.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// The width of the grid being represented.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Number of tiles in the grid; equal to <see cref="Width"/> * <see cref="Height"/>.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Given an X and Y value, returns the "value" associated with that location.
        /// </summary>
        /// <remarks>
        /// Typically, this can be implemented via <see cref="this[Point]"/>.
        /// </remarks>
        /// <param name="x">X-value of location.</param>
        /// <param name="y">Y-value of location.</param>
        /// <returns>The "value" associated with that location.</returns>
        T this[int x, int y] { get; }

        /// <summary>
        /// Given a position, returns the "value" associated with that location.
        /// </summary>
        /// <param name="pos">Location to retrieve the value for.</param>
        /// <returns>The "value" associated with the provided location.</returns>
        T this[Point pos] { get; }

        /// <summary>
        /// Given an 1-dimensional index, returns the value associated with the corresponding position
        /// in the map view.
        /// </summary>
        /// <remarks>
        /// Typically, this may be implemented in terms of <see cref="this[Point]" /> by using
        /// <see cref="Point.FromIndex(int, int)" /> to calculate the 2D position represented by that
        /// 1D index, and passing that position to the <see cref="this[Point]" /> indexer to determine
        /// the value associated with the position.
        /// </remarks>
        /// <param name="index1D">1D index of location to retrieve the "value" for.</param>
        /// <returns>The "value" associated with the given location.</returns>
        T this[int index1D] { get; }
    }
}
