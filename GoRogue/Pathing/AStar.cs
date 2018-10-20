using GoRogue.MapViews;
using Priority_Queue;
using System;
using System.Collections.Generic;

namespace GoRogue.Pathing
{
	/// <summary>
	/// Implements basic AStar pathing. Distance specified determins the heuristic and connectivity
	/// (4-way vs. 8-way) assumed.
	/// </summary>
	public class AStar
	{
		private Distance _distanceMeasurement;

		private Func<int, int, IEnumerable<Direction>> neighborFunc;

		// Used to calculate neighbors of a given cell
		private AStarNode[] nodes;

		private int nodesHeight;

		// Width and of the walkability map at the last path -- used to determine whether
		// reallocation of nodes array is necessary
		private int nodesWidth;

		// Node objects used under the hood for the priority queue
		private FastPriorityQueue<AStarNode> openNodes;

		// Priority queue of the open nodes.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="walkabilityMap">
		/// Map used to determine whether or not a given location can be traversed -- true indicates
		/// walkable, false unwalkable.
		/// </param>
		/// <param name="distanceMeasurement">
		/// Distance measurement used to determine the method of measuring distance between two
		/// points, the heuristic AStar uses when pathfinding, and whether locations are connected in
		/// a 4-way or 8-way manner.
		/// </param>
		public AStar(IMapView<bool> walkabilityMap, Distance distanceMeasurement)
		{
			WalkabilityMap = walkabilityMap;
			DistanceMeasurement = distanceMeasurement;

			int maxSize = walkabilityMap.Width * walkabilityMap.Height;
			nodes = new AStarNode[maxSize];
			for (int i = 0; i < maxSize; i++)
				nodes[i] = new AStarNode(Coord.ToCoord(i, walkabilityMap.Width), null);
			nodesWidth = walkabilityMap.Width;
			nodesHeight = walkabilityMap.Height;

			openNodes = new FastPriorityQueue<AStarNode>(maxSize);
		}

		/// <summary>
		/// The distance calculation being used to determine distance between points. MANHATTAN
		/// implies 4-way connectivity, while CHEBYSHEV or EUCLIDEAN imply 8-way connectivity for the
		/// purpose of determining adjacent coordinates.
		/// </summary>
		public Distance DistanceMeasurement
		{
			get => _distanceMeasurement;
			set
			{
				_distanceMeasurement = value;
				if (_distanceMeasurement == Distance.MANHATTAN)
					neighborFunc = cardinalNeighbors;
				else
					neighborFunc = neighbors;
			}
		}

		/// <summary>
		/// The map being used as the source for whether or not each tile is walkable.
		/// </summary>
		public IMapView<bool> WalkabilityMap { get; private set; }

		/// <summary>
		/// Finds the shortest path between the two specified points.
		/// </summary>
		/// <remarks>
		/// Returns null if there is no path between the specified points. Will still return an
		/// appropriate path object if the start point is equal to the end point.
		/// </remarks>
		/// <param name="start">The starting point of the path.</param>
		/// <param name="end">The ending point of the path.</param>
		/// <param name="assumeEndpointsWalkable">
		/// Whether or not to assume the start and end points are walkable, regardless of what the
		/// walkability map reports. Defaults to true.
		/// </param>
		/// <returns>The shortest path between the two points, or null if no valid path exists.</returns>
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
			if (nodesWidth != WalkabilityMap.Width || nodesHeight != WalkabilityMap.Height)
			{
				int length = WalkabilityMap.Width * WalkabilityMap.Height;
				nodes = new AStarNode[length];
				openNodes = new FastPriorityQueue<AStarNode>(length);
				for (int i = 0; i < length; i++)
					nodes[i] = new AStarNode(Coord.ToCoord(i, WalkabilityMap.Width), null);

				nodesWidth = WalkabilityMap.Width;
				nodesHeight = WalkabilityMap.Height;
			}
			else
			{
				foreach (var node in nodes)
				{
					node.Parent = null;
					node.Closed = false;
					node.F = node.G = float.MaxValue;
				}
			}

			var result = new List<Coord>();
			int index = start.ToIndex(WalkabilityMap.Width);
			nodes[index].G = 0;
			nodes[index].F = (float)_distanceMeasurement.Calculate(start, end); // Completely heuristic for first node
			openNodes.Enqueue(nodes[index], nodes[index].F);

			while (openNodes.Count != 0)
			{
				var current = openNodes.Dequeue();
				current.Closed = true; // We are evaluating this node, no need for it to ever end up in open nodes queue again
				if (current.Position == end) // We found the end, cleanup and return the path
				{
					openNodes.Clear();

					do
					{
						result.Add(current.Position);
						current = current.Parent;
					} while (current != null);

					return new Path(result);
				}

				foreach (var dir in neighborFunc(end.X - current.Position.X, end.Y - current.Position.Y))
				{
					Coord neighborPos = current.Position + dir;

					// Not a valid map position, ignore
					if (neighborPos.X < 0 || neighborPos.Y < 0 || neighborPos.X >= WalkabilityMap.Width || neighborPos.Y >= WalkabilityMap.Height)
						continue;

					if (!checkWalkability(neighborPos, start, end, assumeEndpointsWalkable)) // Not part of walkable node "graph", ignore
						continue;

					int neighborIndex = neighborPos.ToIndex(WalkabilityMap.Width);
					var neighbor = nodes[neighborIndex];

					if (neighbor.Closed) // This neighbor has already been evaluated at shortest possible path, don't re-add
						continue;

					float newDistance = current.G + (float)_distanceMeasurement.Calculate(current.Position, neighbor.Position);
					if (newDistance >= neighbor.G) // Not a better path
						continue;

					// We found a best path, so record and update
					neighbor.Parent = current;
					neighbor.G = newDistance; // (Known) distance to this node via shortest path
					neighbor.F = newDistance + (float)_distanceMeasurement.Calculate(neighbor.Position, end); // Heuristic distance to end (priority in queue)
																											  // If
																											  // it's
																											  // already
																											  // in
																											  // the
																											  // queue,
																											  // update
																											  // priority
																											  // to
																											  // new F
					if (openNodes.Contains(neighbor))
						openNodes.UpdatePriority(neighbor, neighbor.F);
					else // Otherwise, add it with proper priority
						openNodes.Enqueue(neighbor, neighbor.F);
				}
			}

			openNodes.Clear();
			return null; // No path found
		}

		/// <summary>
		/// Finds the shortest path between the two specified points.
		/// </summary>
		/// <remarks>
		/// Returns null if there is no path between the specified points. Will still return an
		/// appropriate path object if the start point is equal to the end point.
		/// </remarks>
		/// <param name="startX">The x-coordinate of the starting point of the path.</param>
		/// <param name="startY">The y-coordinate of the starting point of the path.</param>
		/// <param name="endX">The x-coordinate of the ending point of the path.</param>
		/// <param name="endY">The y-coordinate of the ending point of the path.</param>
		/// <param name="assumeEndpointsWalkable">
		/// Whether or not to assume the start and end points are walkable, regardless of what the
		/// walkability map reports. Defaults to true.
		/// </param>
		/// <returns>The shortest path between the two points, or null if no valid path exists.</returns>
		public Path ShortestPath(int startX, int startY, int endX, int endY, bool assumeEndpointsWalkable = true)
			=> ShortestPath(Coord.Get(startX, startY), Coord.Get(endX, endY), assumeEndpointsWalkable);

		// These neighbor functions are special in that they return (approximately) the closest
		// directions to the end goal first. This is intended to "prioritize" more direct-looking
		// paths, in the case that one or more paths are equally short
		private static IEnumerable<Direction> cardinalNeighbors(int dx, int dy)
		{
			Direction left, right;
			// Intentional inversion of dx and dy sign, because of the order in which our priority
			// queue returns values (we want the direction that we ideally use, eg. the one closest
			// to the specified line, to be last-in, not first-in)
			left = right = Direction.GetCardinalDirection(-dx, -dy);
			yield return right; // Return first direction

			left -= 2;
			right += 2;
			yield return left;
			yield return right;

			// Return last direction
			right += 2;
			yield return right;
		}

		private static IEnumerable<Direction> neighbors(int dx, int dy)
		{
			Direction left, right;
			// Intentional inversion of dx and dy sign, because of the order in which our priority
			// queue returns values
			left = right = Direction.GetDirection(-dx, -dy);
			yield return right; // Return first direction

			for (int i = 0; i < 3; i++)
			{
				left--;
				right++;

				yield return left;
				yield return right;
			}

			// Return last direction
			right++;
			yield return right;
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
		/// <param name="reverse">Whether or not to reverse the path. Defaults to false.</param>
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

	// Node for AStar, stores all values and integrates with priority queue implementation
	internal class AStarNode : FastPriorityQueueNode
	{
		public readonly Coord Position;
		public bool Closed;

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
			Closed = false;
			F = G = float.MaxValue;
		}
	}
}