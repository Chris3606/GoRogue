using GoRogue;
using GoRogue.MapViews;
using Priority_Queue;
using System;
using System.Collections.Generic;

namespace GoRogue.Pathing
{
	/// <summary>
	/// Implements an optimized AStar pathfinding algorithm. Optionally supports custom heuristics, and custom weights for each tile.
	/// </summary>
	/// <remarks>
	/// Like most GoRogue algorithms, AStar takes as a construction parameter an IMapView representing the map. 
	/// Specifically, it takes an <see cref="IMapView{Boolean}"/>, where true indicates that a tile should be
	/// considered walkable, and false indicates that a tile should be considered impassable.
	/// 
	/// For details on the map view system in general, see <see cref="IMapView{T}"/>.  As well, there is an article
	/// explaining the map view system at the GoRogue documentation page
	/// <a href="https://chris3606.github.io/GoRogue/articles">here</a>
	/// 
	/// If truly shortest paths are not strictly necessary, you may want to consider <see cref="FastAStar"/> instead.
	/// </remarks>
	public class AStar
	{
		// Node objects used under the hood for the priority queue
		private AStarNode[] nodes;

		// Stored as seperate array for performance reasons since it must be cleared at each run
		private bool[] closed;

		// Width and of the walkability map at the last path -- used to determine whether
		// reallocation of nodes array is necessary
		private int cachedHeight;
		private int cachedWidth;

		// Priority queue of the open nodes.
		private FastPriorityQueue<AStarNode> openNodes;

		/// <summary>
		/// The distance calculation being used to determine distance between points. <see cref="Distance.MANHATTAN"/>
		/// implies 4-way connectivity, while <see cref="Distance.CHEBYSHEV"/> or <see cref="Distance.EUCLIDEAN"/> imply
		/// 8-way connectivity for the purpose of determining adjacent coordinates.
		/// </summary>
		public Distance DistanceMeasurement { get; set; } // Has to be a property for default heuristic to update properly when this is changed

		/// <summary>
		/// The map view being used to determine whether or not each tile is walkable.
		/// </summary>
		public IMapView<bool> WalkabilityMap { get; private set; }

		private Func<Coord, Coord, double> _heuristic;
		/// <summary>
		/// The heuristic used to estimate distance from nodes to the end point.  If unspecified or specified as null,
		/// it defaults to using the distance calculation specified by <see cref="DistanceMeasurement"/>, with a safe/efficient
		/// tie-breaking multiplier added on.
		/// </summary>
		public Func<Coord, Coord, double> Heuristic
		{
			get => _heuristic;

			set
			{
				_heuristic = value ?? ((c1, c2) => DistanceMeasurement.Calculate(c1, c2) + (Coord.EuclideanDistanceMagnitude(c1, c2) * MaxEuclideanMultiplier));
			}
		}

		/// <summary>
		/// Weights given to each tile.  The weight is multiplied by the cost of a tile, so a tile with weight 2 is twice as hard to
		/// enter as a tile with weight 1.  If unspecified or specified as null, all tiles have weight 1.
		/// </summary>
		public IMapView<double> Weights { get; }

		// NTOE: This HAS to be a property instead of a field for default heuristic to update properly when this is changed
		/// <summary>
		/// Multiplier that is used in the tiebreaking/smoothing element of the default heuristic. This value is based on the
		/// maximum possible <see cref="Coord.EuclideanDistanceMagnitude(Coord, Coord)"/> between two points on the map.
		/// 
		/// Typically you dont' need this value unless you're creating a custom heuristic an introducing the same
		/// tiebreaking/smoothing element as the default heuristic.
		/// </summary>
		public double MaxEuclideanMultiplier { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="walkabilityMap">Map view used to deterine whether or not each location can be traversed -- true indicates a tile can be traversed,
		/// and false indicates it cannot.</param>
		/// <param name="distanceMeasurement">Distance calculation used to determine whether 4-way or 8-way connectivity is used, and to determine
		/// how to calculate the distance between points.  If <paramref name="heuristic"/> is unspecified, also determines the estimation heuristic used.</param>
		/// <param name="heuristic">Function used to estimate the distance between two given points.  If unspecified, a distance calculation corresponding
		/// to <see cref="DistanceMeasurement"/> is used along with a safe/efficient tiebreaking element, which will produce guranteed shortest paths.</param>
		/// <param name="weights">A map view indicating the weights of each location (see <see cref="Weights"/>.  If unspecified, each location will default
		/// to having a weight of 1.</param>
		public AStar(IMapView<bool> walkabilityMap, Distance distanceMeasurement, Func<Coord, Coord, double> heuristic = null,
					 IMapView<double> weights = null)
		{
			Weights = weights;

			WalkabilityMap = walkabilityMap;
			DistanceMeasurement = distanceMeasurement;
			MaxEuclideanMultiplier = 1.0 / (Coord.EuclideanDistanceMagnitude(0, 0, WalkabilityMap.Width, WalkabilityMap.Height) + 1);

			Heuristic = heuristic;

			int maxSize = walkabilityMap.Width * walkabilityMap.Height;
			nodes = new AStarNode[maxSize];
			closed = new bool[maxSize];
			cachedWidth = walkabilityMap.Width;
			cachedHeight = walkabilityMap.Height;

			openNodes = new FastPriorityQueue<AStarNode>(maxSize);
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
			// Don't waste initialization time if there is definately no path
			if (!assumeEndpointsWalkable && (!WalkabilityMap[start] || !WalkabilityMap[end]))
				return null; // There is no path

			// If the path is simply the start, don't bother with graph initialization and such
			if (start == end)
			{
				var retVal = new List<Coord> { start };
				return new Path(retVal);
			}

			// Clear nodes to beginning state
			if (cachedWidth != WalkabilityMap.Width || cachedHeight != WalkabilityMap.Height)
			{
				int length = WalkabilityMap.Width * WalkabilityMap.Height;
				nodes = new AStarNode[length];
				closed = new bool[length];
				openNodes = new FastPriorityQueue<AStarNode>(length);

				cachedWidth = WalkabilityMap.Width;
				cachedHeight = WalkabilityMap.Height;

				MaxEuclideanMultiplier = 1.0 / (Coord.EuclideanDistanceMagnitude(0, 0, WalkabilityMap.Width, WalkabilityMap.Height) + 1);
			}
			else
				Array.Clear(closed, 0, closed.Length);

			var result = new List<Coord>();
			int index = start.ToIndex(WalkabilityMap.Width);

			if (nodes[index] == null)
				nodes[index] = new AStarNode(start, null);

			nodes[index].G = 0;
			nodes[index].F = (float)Heuristic(start, end); // Completely heuristic for first node
			openNodes.Enqueue(nodes[index], nodes[index].F);

			while (openNodes.Count != 0)
			{
				var current = openNodes.Dequeue();
				var currentIndex = current.Position.ToIndex(WalkabilityMap.Width);
				closed[currentIndex] = true;

				if (current.Position == end) // We found the end, cleanup and return the path
				{
					openNodes.Clear();

					do
					{
						result.Add(current.Position);
						current = current.Parent;
					} while (current.Position != start);

					result.Add(start);
					return new Path(result);
				}

				foreach (var dir in ((AdjacencyRule)DistanceMeasurement).DirectionsOfNeighbors())
				{
					Coord neighborPos = current.Position + dir;

					// Not a valid map position, ignore
					if (neighborPos.X < 0 || neighborPos.Y < 0 || neighborPos.X >= WalkabilityMap.Width || neighborPos.Y >= WalkabilityMap.Height)
						continue;

					if (!checkWalkability(neighborPos, start, end, assumeEndpointsWalkable)) // Not part of walkable node "graph", ignore
						continue;

					int neighborIndex = neighborPos.ToIndex(WalkabilityMap.Width);
					var neighbor = nodes[neighborIndex];

					var isNeighborOpen = IsOpen(neighbor, openNodes);

					if (neighbor == null) // Can't be closed because never visited
						nodes[neighborIndex] = neighbor = new AStarNode(neighborPos, null);
					else if (closed[neighborIndex]) // This neighbor has already been evaluated at shortest possible path, don't re-add
						continue;

					float newDistance = current.G + (float)DistanceMeasurement.Calculate(current.Position, neighbor.Position) * (float)(Weights== null ? 1.0 : Weights[neighbor.Position]);
					if (isNeighborOpen && newDistance >= neighbor.G) // Not a better path
						continue;

					// We found a best path, so record and update
					neighbor.Parent = current;
					neighbor.G = newDistance; // (Known) distance to this node via shortest path
					// Heuristic distance to end (priority in queue). If it's already in the queue, update priority to new F
					neighbor.F = newDistance + (float)Heuristic(neighbor.Position, end);

					if (openNodes.Contains(neighbor))
						openNodes.UpdatePriority(neighbor, neighbor.F);
					else // Otherwise, add it with proper priority
					{
						openNodes.Enqueue(neighbor, neighbor.F);
					}
				}
			}

			openNodes.Clear();
			return null; // No path found
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

		private static bool IsOpen(AStarNode node, FastPriorityQueue<AStarNode> openSet)
		{
			return node != null && openSet.Contains(node);
		}

		private bool checkWalkability(Coord pos, Coord start, Coord end, bool assumeEndpointsWalkable)
		{
			if (!assumeEndpointsWalkable)
				return WalkabilityMap[pos];

			return WalkabilityMap[pos] || pos == start || pos == end;
		}
	}

	/// <summary>
	/// Encapsulates a path as returned by pathfinding algorithms like AStar.
	/// </summary>
	/// <remarks>
	/// Provides various functions to iterate through/access steps of the path, as well as
	/// constant-time reversing functionality.
	/// </remarks>
	public class Path
	{
		private IReadOnlyList<Coord> _steps;
		private bool inOriginalOrder;

		/// <summary>
		/// Creates a copy of the path, optionally reversing the path as it does so.
		/// </summary>
		/// <remarks>Reversing is an O(1) operation, since it does not modify the list.</remarks>
		/// <param name="pathToCopy">The path to copy.</param>
		/// <param name="reverse">Whether or not to reverse the path. Defaults to <see langword="false"/>.</param>
		public Path(Path pathToCopy, bool reverse = false)
		{
			_steps = pathToCopy._steps;
			inOriginalOrder = (reverse ? !pathToCopy.inOriginalOrder : pathToCopy.inOriginalOrder);
		}

		// Create based on internal list
		internal Path(IReadOnlyList<Coord> steps)
		{
			_steps = steps;
			inOriginalOrder = true;
		}

		/// <summary>
		/// Ending point of the path.
		/// </summary>
		public Coord End
		{
			get
			{
				if (inOriginalOrder)
					return _steps[0];

				return _steps[_steps.Count - 1];
			}
		}

		/// <summary>
		/// The length of the path, NOT including the starting point.
		/// </summary>
		public int Length { get => _steps.Count - 1; }

		/// <summary>
		/// The length of the path, INCLUDING the starting point.
		/// </summary>
		public int LengthWithStart { get => _steps.Count; }

		/// <summary>
		/// Starting point of the path.
		/// </summary>
		public Coord Start
		{
			get
			{
				if (inOriginalOrder)
					return _steps[_steps.Count - 1];

				return _steps[0];
			}
		}

		/// <summary>
		/// The coordinates that constitute the path (in order), NOT including the starting point.
		/// These are the coordinates something might walk along to follow a path.
		/// </summary>
		public IEnumerable<Coord> Steps
		{
			get
			{
				if (inOriginalOrder)
				{
					for (int i = _steps.Count - 2; i >= 0; i--)
						yield return _steps[i];
				}
				else
				{
					for (int i = 1; i < _steps.Count; i++)
						yield return _steps[i];
				}
			}
		}

		/// <summary>
		/// The coordinates that constitute the path (in order), INCLUDING the starting point.
		/// </summary>
		public IEnumerable<Coord> StepsWithStart
		{
			get
			{
				if (inOriginalOrder)
				{
					for (int i = _steps.Count - 1; i >= 0; i--)
						yield return _steps[i];
				}
				else
				{
					for (int i = 0; i < _steps.Count; i++)
						yield return _steps[i];
				}
			}
		}

		/// <summary>
		/// Gets the nth step along the path, where 0 is the step AFTER the starting point.
		/// </summary>
		/// <param name="stepNum">The (array-like index) of the step to get.</param>
		/// <returns>The coordinate consituting the step specified.</returns>
		public Coord GetStep(int stepNum)
		{
			if (inOriginalOrder)
				return _steps[(_steps.Count - 2) - stepNum];

			return _steps[stepNum + 1];
		}

		/// <summary>
		/// Gets the nth step along the path, where 0 IS the starting point.
		/// </summary>
		/// <param name="stepNum">The (array-like index) of the step to get.</param>
		/// <returns>The coordinate consituting the step specified.</returns>
		public Coord GetStepWithStart(int stepNum)
		{
			if (inOriginalOrder)
				return _steps[(_steps.Count - 1) - stepNum];

			return _steps[stepNum];
		}

		/// <summary>
		/// Reverses the path, in constant time.
		/// </summary>
		public void Reverse() => inOriginalOrder = !inOriginalOrder;

		/// <summary>
		/// Returns a string representation of all the steps in the path, including the start point,
		/// eg. [(1, 2), (3, 4), (5, 6)].
		/// </summary>
		/// <returns>A string representation of all steps in the path, including the start.</returns>
		public override string ToString() => StepsWithStart.ExtendToString();
	}

	// Node representing a grid position in AStar's priority queue
	internal class AStarNode : FastPriorityQueueNode
	{
		public readonly Coord Position;

		// Whether or not the node has been closed
		public float F;

		// (Partly estimated) distance to end point going thru this node
		public float G;

		public AStarNode Parent;
		// (Known) distance from start to this node, by shortest known path

		public AStarNode(Coord position, AStarNode parent = null)
		{
			Parent = parent;
			Position = position;
			F = G = float.MaxValue;
		}
	}
}
