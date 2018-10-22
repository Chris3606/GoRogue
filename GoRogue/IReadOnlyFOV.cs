using GoRogue.MapViews;
using System.Collections.Generic;

namespace GoRogue
{
	/// <summary>
	/// Read-only interface of the FOV class.
	/// </summary>
	public interface IReadOnlyFOV : IMapView<double>
	{
		/// <summary>
		/// IEnumerable of only positions currently in FOV.
		/// </summary>
		IEnumerable<Coord> CurrentFOV { get; }

		/// <summary>
		/// IEnumerable of positions that are in FOV as of the most current Calculate call, but were
		/// NOT in FOV afterthe previous time Calculate was called.
		/// </summary>
		IEnumerable<Coord> NewlySeen { get; }

		/// <summary>
		/// IEnumerable of positions that are NOT in FOV as of the most current Calculate call, but
		/// WERE in FOV after the previous time Calculate was called.
		/// </summary>
		IEnumerable<Coord> NewlyUnseen { get; }

		/// <summary>
		/// A view of the FOV results in boolean form, where true indicates a location is in FOV, and false indicates it is not.
		/// </summary>
		IMapView<bool> BooleanFOV { get; }
	}
}