using GoRogue.MapViews;

namespace GoRogue.Pathing
{
	/// <summary>
	/// A version of <see cref="AStar"/> that may perform significantly faster, in exchange for not being guaranteed to always produce a shortest path.  A 
	/// valid path will still be produced, but it is not guaranteed to be the shortest possible.
	/// </summary>
	/// <remarks>
	/// This class is exactly like a regular <see cref="AStar"/> instance, but sets the heuristic by default to the <see cref="Distance.MANHATTAN"/>
	/// calculate function (with a tiebreaker). In the case that euclidean or chebyshev distance is used, this heuristic is over-estimating -- that is, it may in some cases
	/// produce a value that is greater than the actual shortest path between two points.  As such, this means that, while the algorithm will still produce
	/// valid paths, the algorithm is no longer guaranteed to produce fully shortest paths.  In exchange, however, the algorithm may perform significantly faster
	/// than an AStar instance with the default heuristic.
	/// 
	/// In practice, however, it is worth noting that the paths are often (though not always) the shortest path regardless, and when they are not, the deviation
	/// in length between the path that the algorithm returns and the actual shortest path is often very small (less than 5%).  As such, it may be viable for use
	/// in most cases.
	/// </remarks>
	public class FastAStar : AStar
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
		public FastAStar(IMapView<bool> walkabilityMap, Distance distanceMeasurement, IMapView<double> weights = null)
			: base(walkabilityMap, distanceMeasurement, null, weights)
		{
			Heuristic = (c1, c2) => Distance.MANHATTAN.Calculate(c1, c2) * this.MaxPathMultiplier;
		}
	}
}
