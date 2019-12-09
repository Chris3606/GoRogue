using System;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.Connectors
{
	/// <summary>
	/// Interface for implementing an algorithm for selecting the positions to connect in order to
	/// connect two given map areas.
	/// </summary>
	public interface IAreaConnectionPointSelector
	{
		/// <summary>
		/// Implements the algorithm. Returns a tuple of two positions -- the first position is the
		/// position in <paramref name="area1"/> to use, the second position is the position in
		/// <paramref name="area2"/> to use.
		/// </summary>
		/// <param name="area1">First <see cref="Area"/> to connect.</param>
		/// <param name="area2">Second <see cref="Area"/> to connect.</param>
		/// <returns>
		/// A tuple containing the Pointinates from each <see cref="Area"/> to connect -- the first
		/// item in the tuple is the position in area1, the second is the position in area2.
		/// </returns>
		Tuple<Point, Point> SelectConnectionPoints(IReadOnlyArea area1, IReadOnlyArea area2);
	}
}
