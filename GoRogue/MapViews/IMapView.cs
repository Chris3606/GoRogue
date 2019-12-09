using SadRogue.Primitives;

namespace GoRogue.MapViews
{
	/// <summary>
	/// Interface designed to act as a standardized input/output interpretation for algorithms.
	/// </summary>
	/// <remarks>
	/// Many roguelike/2D grid algorithms, such as pathfinding, FOV, and map generation, view a map
	/// as simply a 2D array of some type. In many games, however, the "value" associated with each
	/// 2D position may be dependent upon many different things. For example, pathfinding, as input,
	/// often needs a "walkability map" -- in common terms, a 2D array of boolean values, where the
	/// boolean value at each position represents whether or not that tile is passable with respect to
	/// pathing. This boolean value might be determined by a number of things - terrain type, monster
	/// positions, etc. Thus, in practice, maintaining an actual 2D array of boolean values that such
	/// an algorithm could take as input can be difficult. IMapView solves this problem by
	/// providing an interface that all such algorithms can take as input -- pathfinding, for
	/// instance, would take an <see cref="IMapView{Boolean}"/>; instance, rather than a 2D array of booleans. A
	/// user of that algorithm might create a class that implements the indexers below to check the
	/// terrain type, if there is a monster at the position, etc., and returns the correct value.
	/// This prevents the need to maintain an actual 2D array in code that pathfinding can use, if
	/// such an array does not fit with your game architecture. If you do want to store the data in an actual 2D
	/// array, IMapView works similarly -- the indexers can simply retrieve values in the array.
	/// 
	/// Although manually implementing a custom IMapView may be necessary in some cases, GoRogue provides many
	/// implementations that cater to common cases -- for example, <see cref="ArrayMap{T}"/> is an implementation
	/// that uses an actual array, while <see cref="LambdaMapView{T}"/> is a built-in implementation that uses a
	/// function to retrieve values.
	/// </remarks>
	/// <typeparam name="T">The type of value being returned by the indexer functions.</typeparam>
	public interface IMapView<out T>
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
		/// Given an X and Y value, returns the "value" associated with that location.
		/// </summary>
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
		/// Typically, this may be implemented in terms of <see cref="this[Point]"/> by using
		/// <see cref="Point.FromIndex(int, int)"/> to calculate the 2D position represented by that
		/// 1D index, and passing that position to the <see cref="this[Point]"/> indexer to determine
		/// the value associated with the position.
		/// </remarks>
		/// <param name="index1D">1D index of location to retrieve the "value" for.</param>
		/// <returns>The "value" associated with the given location.</returns>
		T this[int index1D] { get; }
	}
}
