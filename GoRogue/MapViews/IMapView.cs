namespace GoRogue.MapViews
{
    /// <summary>
    /// Interface designed to act as a standardized input/output for algorithms.
    /// </summary>
    /// <remarks>
    /// Many roguelike/2D grid algorithms, such as pathfinding, FOV, and map generation, view a map
    /// as simply a 2D array of some type. In many games, however, the "value" associated with each
    /// 2D position may be dependent upon many different things. For example, pathfinding, as input,
    /// often needs a "walkability map" -- in common terms, a 2D array of bools where the boolean
    /// value at each position represents whether or not that tile is passable with respect to
    /// pathing. This boolean value might be determined by a number of things - terrain type, monster
    /// positions, etc. Thus, in practice, maintaining an actual 2D array of boolean values that such
    /// an algorithm could take as input can be significant work.
    ///
    /// IMapView solves this problem by providing an interface that all such algorithms can take as
    /// input -- pathfinding, for instance, would take an IMapView&lt;bool&gt; instance, rather than
    /// a 2D array of booleans. A user of that algorithm might create a class that implements the
    /// indexers below to check the terrain type, if there is a monster at the position, etc., and
    /// returns the correct value. This prevents the need to maintain an actual 2D array in code that
    /// pathfinding can use, if such an array does not fit with your game architecture.
    ///
    /// If an actual 2D array is desired, a class ArrayMap is provided that implements IMapView, and
    /// acts much like an actual 2D array.
    /// </remarks>
    /// <typeparam name="T">The type of value being returned by the indexer functions.</typeparam>
    public interface IMapView<T>
    {
        /// <summary>
        /// The height of the map being represented.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// The width of the map being represented.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Given an X and Y value, should return the "value" associated with that location.
        /// </summary>
        /// <param name="x">X-value of location.</param>
        /// <param name="y">Y-value of location.</param>
        /// <returns>The "value" associated with that location.</returns>
        T this[int x, int y] { get; }

        /// <summary>
        /// Given a Coord, should return the "value" associated with that location.
        /// </summary>
        /// <param name="pos">Location to retrieve the value for.</param>
        /// <returns>The "value" associated with the provided location.</returns>
        T this[Coord pos] { get; }
    }
}