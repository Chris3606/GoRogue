using System;
using System.Collections.Generic;
using Troschuetz.Random;

namespace GoRogue.MapGeneration
{
	/// <summary>
	/// Read-only interface for an arbitrarily-shaped area of the map.
	/// </summary>
	public interface IReadOnlyMapArea
	{
		/// <summary>
		/// Smallest possible rectangle that encompasses every position in the area.
		/// </summary>
		Rectangle Bounds { get; }

		/// <summary>
		/// Number of (unique) positions in the currently stored list of positions.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// List of all (unique) positions in the list.
		/// </summary>
		IReadOnlyList<Coord> Positions { get; }

		/// <summary>
		/// Returns whether or not the given map area is completely contained within the current one.
		/// </summary>
		/// <param name="area">Area to check.</param>
		/// <returns>
		/// True if the given area is completely contained within the current one, false otherwise.
		/// </returns>
		bool Contains(IReadOnlyMapArea area);

		/// <summary>
		/// Determines whether or not the given position is considered within the area or not.
		/// </summary>
		/// <param name="x">X-coordinate of the position to check.</param>
		/// <param name="y">Y-coordinate of the position to check.</param>
		/// <returns>True if the specified position is within the area, false otherwise.</returns>
		bool Contains(int x, int y);

		/// <summary>
		/// Determines whether or not the given position is considered within the area or not.
		/// </summary>
		/// <param name="position">The position to check.</param>
		/// <returns>True if the specified position is within the area, false otherwise.</returns>
		bool Contains(Coord position);

		/// <summary>
		/// Returns whether or not the given map area intersects the current one. If you intend to
		/// determine/use the exact intersection based on this return value, it is best to instead
		/// call <see cref="MapArea.GetIntersection(IReadOnlyMapArea, IReadOnlyMapArea)"/>, and check the number of positions in the result (0 if
		/// no intersection).
		/// </summary>
		/// <param name="area">The area to check.</param>
		/// <returns>True if the given area intersects the current one, false otherwise.</returns>
		bool Intersects(IReadOnlyMapArea area);

		/// <summary>
		/// Gets a random position from the area.
		/// </summary>
		/// <param name="rng">The rng to use. Defaults to <see cref="GoRogue.Random.SingletonRandom.DefaultRNG"/>.</param>
		/// <returns>A random position from within the map area.</returns>
		Coord RandomPosition(IGenerator rng = null);

		/// <summary>
		/// Gets a random position from the map area for which the given selector returns true. Positions
		/// are repeatedly selected until a valid one is found.
		/// </summary>
		/// <param name="selector">
		/// A function that should return true for any coordinate that is a valid selection, and
		/// false otherwise.
		/// </param>
		/// <param name="rng">The rng to use. Defaults to <see cref="GoRogue.Random.SingletonRandom.DefaultRNG"/>.</param>
		/// <returns>
		/// A random position from within the map area for which the selector given returns true.
		/// </returns>
		Coord RandomPosition(Func<Coord, bool> selector, IGenerator rng = null);
	}
}