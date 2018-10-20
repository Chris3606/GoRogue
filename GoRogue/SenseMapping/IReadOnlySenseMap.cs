using GoRogue.MapViews;
using System.Collections.Generic;

namespace GoRogue.SenseMapping
{
	/// <summary>
	/// Read-only interface of a SenseMap.
	/// </summary>
	public interface IReadOnlySenseMap : IEnumerable<double>, IMapView<double>
	{
		/// <summary>
		/// IEnumerable of only positions currently "in" the sense map, eg. all positions that have a
		/// value other than 0.0.
		/// </summary>
		IEnumerable<Coord> CurrentSenseMap { get; }

		/// <summary>
		/// IEnumerable of positions that DO have a non-zero value in the sense map as of the most
		/// current Calculate call, but DID NOT have a non-zero value after the previous time
		/// Calculate was called.
		/// </summary>
		IEnumerable<Coord> NewlyInSenseMap { get; }

		/// <summary>
		/// IEnumerable of positions that DO NOT have a non-zero value in the sense map as of the
		/// most current Calculate call, but DID have a non-zero value after the previous time
		/// Calculate was called.
		/// </summary>
		IEnumerable<Coord> NewlyOutOfSenseMap { get; }

		/// <summary>
		/// Read-only list of all sources currently taken into account. Some may have their enabled
		/// flag set to false, so all of these may or may not be counted when Calculate is called.
		/// </summary>
		IReadOnlyList<SenseSource> SenseSources { get; }
	}
}