using GoRogue.MapViews;
using System.Collections.Generic;
using SadRogue.Primitives;

namespace GoRogue
{
	/// <summary>
	/// Read-only interface of the <see cref="FOV"/> class.
	/// </summary>
	public interface IReadOnlyFOV : IMapView<double>
	{
		/// <summary>
		/// A view of the calculation results in boolean form, where true indicates a location is in
		/// field of view, and false indicates it is not.
		/// </summary>
		IMapView<bool> BooleanFOV { get; }

		/// <summary>
		/// IEnumerable of only positions currently in the field of view.
		/// </summary>
		IEnumerable<Point> CurrentFOV { get; }

		/// <summary>
		/// IEnumerable of positions that ARE in field of view as of the most current Calculate call, but were
		/// NOT in field of view after the previous time Calculate was called.
		/// </summary>
		IEnumerable<Point> NewlySeen { get; }

		/// <summary>
		/// IEnumerable of positions that are NOT in field of view as of the most current Calculate call, but
		/// WERE in field of view after the previous time Calculate was called.
		/// </summary>
		IEnumerable<Point> NewlyUnseen { get; }
	}
}
