using System.Collections.Generic;

namespace GoRogue
{
	/// <summary>
	/// Read-only interface of <see cref="RadiusAreaProvider"/>.
	/// </summary>
	public interface IReadOnlyRadiusAreaProvider
	{
		/// <summary>
		/// The bounds to constrain the returned locations to. Set to <see cref="Rectangle.EMPTY"/>
		/// to indicate that there are no bounds.
		/// </summary>
		Rectangle Bounds { get; }

		/// <summary>
		/// The center point of the radius.
		/// </summary>
		Coord Center { get; }

		/// <summary>
		/// The distance calculation used to determine what shape the radius has (or a type
		/// implicitly convertible to a distance method, eg. <see cref="Radius"/>).
		/// </summary>
		Distance DistanceCalc { get; }

		/// <summary>
		/// The length of the radius, eg. the number of tiles from the center point (as defined by
		/// the distance calculation/radius shape given) to which the radius extends.
		/// </summary>
		/// <remarks>
		/// When this value is changed, reallocation of an underlying array is performed, however overhead should
		/// be relatively small in most cases.
		/// </remarks>
		int Radius { get; }

		/// <summary>
		/// Calculates the new radius, and returns an IEnumerable of all unique locations within that
		/// radius and bounds specified (as applicable).
		/// </summary>
		/// <remarks>
		/// In the case that MANHATTAN/CHEBYSHEV distance, or DIAMOND/SQUARE/OCTAHEDRON/CUBE radius shapes
		/// are specified via <see cref="DistanceCalc"/>, positions returned are guaranteed to be returned
		/// in order of distance from the center, from least to greatest. This guarantee does NOT hold if
		/// EUCLIDEAN distance, or CIRCLE/SPHERE radius shapes are specified.
		/// </remarks>
		/// <returns>Enumerable of all unique positions within the radius and bounds specified.</returns>
		IEnumerable<Coord> CalculatePositions();
	}
}
