using GoRogue.MapViews;

namespace GoRogue.Pathing
{
	/// <summary>
	/// A version of <see cref="AStarDynamic"/> that may perform significantly faster, in exchange for not being guaranteed to always produce a shortest path.  A 
	/// valid path will still be produced, but it is not guaranteed to be the shortest possible.
	/// </summary>
	/// <remarks>
	/// This class is exactly like a regular <see cref="AStarDynamic"/> instance, but sets the heuristic by default to the <see cref="Distance.MANHATTAN"/>
	/// calculate function. In the case that euclidean or chebyshev distance is used, this heuristic is over-estimating -- that is, it may in some cases
	/// produce a value that is greater than the actual shortest path between two points.  As such, this means that, while the algorithm will still produce
	/// valid paths, the algorithm is no longer guaranteed to produce fully shortest paths.  In exchange, however, the algorithm may perform significantly faster
	/// than an AStar instance with the default heuristic.
	/// 
	/// In practice, however, it is worth noting that the paths are often (though not always) the shortest path regardless, and when they are not, the deviation
	/// in length between the path that the algorithm returns and the actual shortest path is often very small (less than 5%).  As such, it may be viable for use
	/// in most cases.
	/// 
	/// This algorithm performs the best when the map view it is given is changing size frequently relative to the number of calls to shortest path.
	/// Generally, in cases where maximum performance is needed, it is recommended that the map view _not_ change size frequently (regardless of whether
	/// the underlying map is actually changing size), and to use <see cref="FastAStar"/> instead (or its variant <see cref="AStar"/>).  However,
	/// in cases where the map view size must change size frequently, you will get better performance out of this class.
	/// </remarks>
	public class FastAStarDynamic : AStar
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="walkabilityMap">Map view used to deterine whether or not each location can be traversed -- true indicates a tile can be traversed,
		/// and false indicates it cannot.</param>
		/// <param name="distanceMeasurement">Distance calculation used to determine whether 4-way or 8-way connectivity is used, and to determine
		/// how to calculate the distance between points.</param>
		/// <param name="weights">A map view indicating the weights of each location (see <see cref="AStar.Weights"/>.  If unspecified, each location will default
		/// to having a weight of 1.</param>
		public FastAStarDynamic(IMapView<bool> walkabilityMap, Distance distanceMeasurement, IMapView<double> weights = null)
			: base(walkabilityMap, distanceMeasurement, Distance.MANHATTAN.Calculate, weights) { }
	}
}
