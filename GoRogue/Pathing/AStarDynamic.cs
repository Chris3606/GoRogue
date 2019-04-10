using GoRogue.MapViews;
using System;
using System.Collections.Generic;

namespace GoRogue.Pathing
{
	/// <summary>
	/// A version of <see cref="AStar"/> that will perform better than AStar when the map view's size is changing frequently.  If you map view size
	/// does not change frequently, use <see cref="AStar"/>.
	/// </summary>
	/// <remarks>
	/// This algorithm performs the best when the map view it is given is changing size frequently relative to the number of calls to shortest path.
	/// Generally, in cases where maximum performance is needed, it is recommended that the map view _not_ change size frequently (regardless of whether
	/// the underlying map is actually changing size), and to use <see cref="AStar"/> instead (or its faster variant <see cref="FastAStar"/>).  However,
	/// in cases where the map view size must change size frequently, you will get better performance out of this or its fast variant, <see cref="FastAStarDynamic"/>.
	/// </remarks>
	public class AStarDynamic
	{
		private Func<int, int, IEnumerable<Direction>> neighborFunc;

		private double[] costSoFar;
		private Coord[] cameFrom;
		private int _cachedWidth;
		private int _cachedHeight;

		/// <summary>
		/// The map view being used to determine whether or not each tile is walkable.
		/// </summary>
		public IMapView<bool> WalkabilityMap { get; }

		private Distance _distanceMeasurement;

		/// <summary>
		/// The distance calculation being used to determine distance between points. <see cref="Distance.MANHATTAN"/>
		/// implies 4-way connectivity, while <see cref="Distance.CHEBYSHEV"/> or <see cref="Distance.EUCLIDEAN"/> imply
		/// 8-way connectivity for the purpose of determining adjacent coordinates.
		/// </summary>
		public Distance DistanceMeasurement
		{
			get => _distanceMeasurement;
			set
			{
				_distanceMeasurement = value;
				if (_distanceMeasurement == Distance.MANHATTAN)
					neighborFunc = AStar.cardinalNeighbors;
				else
					neighborFunc = AStar.neighbors;
			}
		}

		/// <summary>
		/// The heuristic used to estimate distance from nodes to the end point.  If unspecified, defaults to using the distance
		/// calculation specified by <see cref="DistanceMeasurement"/>.
		/// </summary>
		public Func<Coord, Coord, double> Heuristic { get; }

		/// <summary>
		/// Weights given to each tile.  The weight is multiplied by the cost of a tile, so a tile with weight  is twice as hard to enter
		/// as a tile with weight 1.  If unspecified, all tiles have weight 1.
		/// </summary>
		public IMapView<double> Weights { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="walkabilityMap">Map view used to deterine whether or not each location can be traversed -- true indicates a tile can be traversed,
		/// and false indicates it cannot.</param>
		/// <param name="distanceMeasurement">Distance calculation used to determine whether 4-way or 8-way connectivity is used, and to determine
		/// how to calculate the distance between points.  If <paramref name="heuristic"/> is unspecified, also determines the estimation heuristic used.</param>
		/// <param name="heuristic">Function used to estimate the distance between two given points.  If unspecified, a distance calculation corresponding
		/// to <paramref name="distanceMeasurement"/> is used, which will produce guranteed shortest paths.</param>
		/// <param name="weights">A map view indicating the weights of each location (see <see cref="Weights"/>.  If unspecified, each location will default to having a weight of 1.</param>
		public AStarDynamic(IMapView<bool> walkabilityMap, Distance distanceMeasurement, Func<Coord, Coord, double> heuristic = null,
						 IMapView<double> weights = null)
		{
			Heuristic = heuristic ?? Distance.MANHATTAN.Calculate;
			if (weights == null)
			{
				var weightsMap = new ArrayMap<double>(walkabilityMap.Width, walkabilityMap.Height);
				foreach (var pos in weightsMap.Positions())
					weightsMap[pos] = 1.0;
				Weights = weightsMap;
			}
			else
				Weights = weights;

			WalkabilityMap = walkabilityMap;
			DistanceMeasurement = distanceMeasurement;

			InitializeArrays(out costSoFar, out cameFrom, WalkabilityMap.Width * WalkabilityMap.Height);
		}

		/// <summary>
		/// Finds the shortest path between the two specified points.
		/// </summary>
		/// <remarks>
		/// Returns <see langword="null"/> if there is no path between the specified points. Will still return an
		/// appropriate path object if the start point is equal to the end point.
		/// </remarks>
		/// <param name="start">The starting point of the path.</param>
		/// <param name="end">The ending point of the path.</param>
		/// <param name="assumeEndpointsWalkable">
		/// Whether or not to assume the start and end points are walkable, regardless of what the
		/// <see cref="WalkabilityMap"/> reports. Defaults to <see langword="true"/>.
		/// </param>
		/// <returns>The shortest path between the two points, or <see langword="null"/> if no valid path exists.</returns>
		public Path ShortestPath(Coord start, Coord end, bool assumeEndpointsWalkable = true)
		{
			// Can't be a path
			if (!assumeEndpointsWalkable && (!WalkabilityMap[start] || !WalkabilityMap[end]))
				return null;

			// Path is 1 long, so don't bother with allocations
			if (start == end)
				return new Path(new List<Coord> { start });

			if (_cachedWidth != WalkabilityMap.Width || _cachedHeight != WalkabilityMap.Height)
			{
				InitializeArrays(out costSoFar, out cameFrom, WalkabilityMap.Width * WalkabilityMap.Height);
				_cachedWidth = WalkabilityMap.Width;
				_cachedHeight = WalkabilityMap.Height;
			}
			else
			{
				int length = costSoFar.Length;
				Array.Clear(costSoFar, 0, length); // cameFrom isn't accessed until path found so don't need to clear it
			}

			// Calculate path
			var head = new LinkedListPriorityQueueNode<Coord>(start, Heuristic(start, end));
			var open = new LinkedListPriorityQueue<Coord>();
			open.Push(head);

			
			while (!open.IsEmpty())
			{
				var current = open.Pop().Value;

				// Path complete -- return
				if (current == end)
				{
					List<Coord> path = new List<Coord>();
					while (current != start)
					{
						path.Add(current);
						current = cameFrom[current.ToIndex(WalkabilityMap.Width)];
					}
					path.Add(start);
					return new Path(path);
				}

				// Step to neighbors
				var currentIndex = current.ToIndex(WalkabilityMap.Width);
				var initialCost = costSoFar[currentIndex];

				// Go through neighbors based on distance calculation
				foreach (var neighborDir in neighborFunc(current.X - end.X, current.Y - end.Y))
				{
					var neighbor = current + neighborDir;
					// Ignore if out of bounds or location unwalkable
					if (!WalkabilityMap.Bounds().Contains(neighbor) ||
						(!WalkabilityMap[neighbor] && (!assumeEndpointsWalkable || (!neighbor.Equals(start) && !neighbor.Equals(end)))))
						continue;

					var neighborIndex = neighbor.ToIndex(WalkabilityMap.Width);
					
					// Real cost of getting to neighbor via path passing through current
					var newCost = initialCost + _distanceMeasurement.Calculate(current, neighbor) * Weights[neighbor];
					
					var oldCost = costSoFar[neighborIndex];
					// Compare to best path to neighbor we have; 0 means no path found yet to neighbor
					if (!(oldCost <= 0) && !(newCost < oldCost))
						continue;

					// We've found a better path, so update parent and known cost
					costSoFar[neighborIndex] = newCost;
					cameFrom[neighborIndex] = current;

					// Use new distance + heuristic to compute new expected cost from neighbor to end, and update priority queue
					var expectedCost = newCost + Heuristic(neighbor, end);
					open.Push(new LinkedListPriorityQueueNode<Coord>(neighbor, expectedCost));
				}
			}

			// There is no path
			return null;
		}

		/// <summary>
		/// Finds the shortest path between the two specified points.
		/// </summary>
		/// <remarks>
		/// Returns <see langword="null"/> if there is no path between the specified points. Will still return an
		/// appropriate path object if the start point is equal to the end point.
		/// </remarks>
		/// <param name="startX">The x-coordinate of the starting point of the path.</param>
		/// <param name="startY">The y-coordinate of the starting point of the path.</param>
		/// <param name="endX">The x-coordinate of the ending point of the path.</param>
		/// <param name="endY">The y-coordinate of the ending point of the path.</param>
		/// <param name="assumeEndpointsWalkable">
		/// Whether or not to assume the start and end points are walkable, regardless of what the
		/// <see cref="WalkabilityMap"/> reports. Defaults to <see langword="true"/>.
		/// </param>
		/// <returns>The shortest path between the two points, or <see langword="null"/> if no valid path exists.</returns>
		public Path ShortestPath(int startX, int startY, int endX, int endY, bool assumeEndpointsWalkable = true)
			=> ShortestPath(new Coord(startX, startY), new Coord(endX, endY), assumeEndpointsWalkable);

		private static void InitializeArrays(out double[] costSoFar, out Coord[] cameFrom, int length)
		{
			costSoFar = new double[length];
			cameFrom = new Coord[length];
		}
	}
}
