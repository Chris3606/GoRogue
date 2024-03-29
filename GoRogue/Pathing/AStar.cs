﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Priority_Queue;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Pathing
{
    /// <summary>
    /// Implements an optimized AStar pathfinding algorithm. Optionally supports custom heuristics, and custom weights for each
    /// tile.
    /// </summary>
    /// <remarks>
    /// Like most GoRogue algorithms, AStar takes as a construction parameter an IGridView representing the map.
    /// Specifically, it takes an <see cref="SadRogue.Primitives.GridViews.IGridView{T}" />, where true indicates that a tile should be
    /// considered walkable, and false indicates that a tile should be considered impassable.
    /// For details on the map view system in general, see <see cref="SadRogue.Primitives.GridViews.IGridView{T}" />.  As well, there is an article
    /// explaining the map view system at the GoRogue documentation page
    /// <a href="https://chris3606.github.io/GoRogue/articles">here</a>
    /// If truly shortest paths are not strictly necessary, you may want to consider <see cref="FastAStar" /> instead.
    /// </remarks>
    [PublicAPI]
    public class AStar
    {
        // Width and of the walkability map at the last path -- used to determine whether
        // reallocation of nodes array is necessary
        private int _cachedHeight;

        private double _cachedMinWeight;
        private int _cachedWidth;

        // Stored as separate array for performance reasons since it must be cleared at each run
        private BitArray _closed;

        private Func<Point, Point, double> _heuristic;

        // Node objects used under the hood for the priority queue
        private AStarNode?[] _nodes;

        // Priority queue of the open nodes.
        private FastPriorityQueue<AStarNode> _openNodes;

        /// <summary>
        /// The minimum value that is allowed to occur in the <see cref="Weights" /> map view.  This value is only used with the
        /// default heuristic
        /// for AStar and <see cref="FastAStar" />, so if a custom heuristic is used, the value is also ignored.  Must be greater
        /// than 0.0 and less
        /// than or equal to the minimum value in the <see cref="Weights" /> map view.  Defaults to 1.0 in cases where the default
        /// heuristic is used.
        /// </summary>
        public double MinimumWeight;

        /// <summary>
        /// Constructor.  Uses a default heuristic corresponding to the distance calculation given, along with a safe/efficient
        /// tie-breaking/smoothing element which will produce guaranteed shortest paths.
        /// </summary>
        /// <param name="walkabilityView">
        /// Map view used to determine whether or not each location can be traversed -- true indicates a tile can be traversed,
        /// and false indicates it cannot.
        /// </param>
        /// <param name="distanceMeasurement">
        /// Distance calculation used to determine whether 4-way or 8-way connectivity is used, and to determine
        /// how to calculate the distance between points.
        /// </param>
        public AStar(IGridView<bool> walkabilityView, Distance distanceMeasurement)
            : this(walkabilityView, distanceMeasurement, null, null, 1.0)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="walkabilityView">
        /// Map view used to determine whether or not each location can be traversed -- true indicates a tile can be traversed,
        /// and false indicates it cannot.
        /// </param>
        /// <param name="distanceMeasurement">
        /// Distance calculation used to determine whether 4-way or 8-way connectivity is used, and to determine
        /// how to calculate the distance between points.
        /// </param>
        /// <param name="heuristic">Function used to estimate the distance between two given points.</param>
        public AStar(IGridView<bool> walkabilityView, Distance distanceMeasurement, Func<Point, Point, double> heuristic)
            : this(walkabilityView, distanceMeasurement, heuristic, null, -1.0)
        { }

        /// <summary>
        /// Constructor.  Uses a default heuristic corresponding to the distance calculation given, along with a safe/efficient
        /// tie-breaking/smoothing element which will produce guaranteed shortest paths, provided <paramref name="minimumWeight" />
        /// is correct.
        /// </summary>
        /// <param name="walkabilityView">
        /// Map view used to determine whether or not each location can be traversed -- true indicates a tile can be traversed,
        /// and false indicates it cannot.
        /// </param>
        /// <param name="distanceMeasurement">
        /// Distance calculation used to determine whether 4-way or 8-way connectivity is used, and to determine
        /// how to calculate the distance between points.
        /// </param>
        /// <param name="weights">A map view indicating the weights of each location (see <see cref="Weights" />.</param>
        /// <param name="minimumWeight">
        /// The minimum value that will be present in <paramref name="weights" />.  It must be greater than 0.0 and
        /// must be less than or equal to the minimum value present in the weights view -- the algorithm may not produce truly
        /// shortest paths if
        /// this condition is not met.  If this minimum changes after construction, it may be updated via the
        /// <see cref="AStar.MinimumWeight" /> property.
        /// </param>
        public AStar(IGridView<bool> walkabilityView, Distance distanceMeasurement, IGridView<double> weights,
                     double minimumWeight)
            : this(walkabilityView, distanceMeasurement, null, weights, minimumWeight)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="walkabilityView">
        /// Map view used to determine whether or not each location can be traversed -- true indicates a tile can be traversed,
        /// and false indicates it cannot.
        /// </param>
        /// <param name="distanceMeasurement">
        /// Distance calculation used to determine whether 4-way or 8-way connectivity is used, and to determine
        /// how to calculate the distance between points.
        /// </param>
        /// <param name="heuristic">Function used to estimate the distance between two given points.</param>
        /// <param name="weights">A map view indicating the weights of each location (see <see cref="Weights" />.</param>
        public AStar(IGridView<bool> walkabilityView, Distance distanceMeasurement, Func<Point, Point, double> heuristic,
                     IGridView<double> weights)
            : this(walkabilityView, distanceMeasurement, heuristic, weights, -1.0)
        { }

        // Private constructor that does work of others
#pragma warning disable CS8618 // _heuristic is initialized via Heuristic so ignore erroneous warning
        private AStar(IGridView<bool> walkabilityView, Distance distanceMeasurement,
                      Func<Point, Point, double>? heuristic = null, IGridView<double>? weights = null,
                      double minimumWeight = 1.0)
#pragma warning restore CS8618
        {
            Weights = weights;

            WalkabilityView = walkabilityView;
            DistanceMeasurement = distanceMeasurement;
            MinimumWeight = minimumWeight;
            _cachedMinWeight = minimumWeight;
            MaxEuclideanMultiplier = MinimumWeight / Point.EuclideanDistanceMagnitude(new Point(0, 0),
                new Point(WalkabilityView.Width, WalkabilityView.Height));

            Heuristic = heuristic!; // Handles null and exposes as non-nullable since it will never allow null

            var maxSize = walkabilityView.Width * walkabilityView.Height;
            _nodes = new AStarNode?[maxSize];
            _closed = new BitArray(maxSize);
            _cachedWidth = walkabilityView.Width;
            _cachedHeight = walkabilityView.Height;

            _openNodes = new FastPriorityQueue<AStarNode>(maxSize);
        }

        /// <summary>
        /// The distance calculation being used to determine distance between points. <see cref="SadRogue.Primitives.Distance.Manhattan" />
        /// implies 4-way connectivity, while <see cref="SadRogue.Primitives.Distance.Chebyshev" /> or <see cref="SadRogue.Primitives.Distance.Euclidean" /> imply
        /// 8-way connectivity for the purpose of determining adjacent coordinates.
        /// </summary>
        public Distance DistanceMeasurement { get; set; }

        /// <summary>
        /// The map view being used to determine whether or not each tile is walkable.
        /// </summary>
        public IGridView<bool> WalkabilityView { get; private set; }

        /// <summary>
        /// The heuristic used to estimate distance from nodes to the end point.  If unspecified or specified as null,
        /// it defaults to using the distance calculation specified by <see cref="DistanceMeasurement" />, with a safe/efficient
        /// tie-breaking multiplier added on.
        /// </summary>
        [AllowNull]
        public Func<Point, Point, double> Heuristic
        {
            get => _heuristic;

            // Warning should not be present since AllowNull is set
            // ReSharper disable once ConstantNullCoalescingCondition
            set => _heuristic = value ?? ((c1, c2)
                => DistanceMeasurement.Calculate(c1, c2) +
                   Point.EuclideanDistanceMagnitude(c1, c2) * MaxEuclideanMultiplier);
        }

        /// <summary>
        /// Weights given to each tile.  The weight is multiplied by the cost of a tile, so a tile with weight 2 is twice as hard
        /// to
        /// enter as a tile with weight 1.  If unspecified or specified as null, all tiles have weight 1.
        /// </summary>
        public IGridView<double>? Weights { get; }

        // NOTE: This HAS to be a property instead of a field for default heuristic to update properly when this is changed
        /// <summary>
        /// Multiplier that is used in the tie-breaking/smoothing element of the default heuristic. This value is based on the
        /// maximum possible <see cref="SadRogue.Primitives.Point.EuclideanDistanceMagnitude(Point, Point)" /> between two points on the map.
        /// Typically you don't need this value unless you're creating a custom heuristic an introducing the same
        /// tie-breaking/smoothing element as the default heuristic.
        /// </summary>
        public double MaxEuclideanMultiplier { get; private set; }

        /// <summary>
        /// Finds the shortest path between the two specified points.
        /// </summary>
        /// <remarks>
        /// Returns <see langword="null" /> if there is no path between the specified points. Will still return an
        /// appropriate path object if the start point is equal to the end point.
        /// </remarks>
        /// <param name="start">The starting point of the path.</param>
        /// <param name="end">The ending point of the path.</param>
        /// <param name="assumeEndpointsWalkable">
        /// Whether or not to assume the start and end points are walkable, regardless of what the
        /// <see cref="WalkabilityView" /> reports. Defaults to <see langword="true" />.
        /// </param>
        /// <returns>The shortest path between the two points, or <see langword="null" /> if no valid path exists.</returns>
        public Path? ShortestPath(Point start, Point end, bool assumeEndpointsWalkable = true)
        {
            var adjacencyRule = (AdjacencyRule)DistanceMeasurement;

            // Don't waste initialization time if there is definitely no path
            if (!assumeEndpointsWalkable && (!WalkabilityView[start] || !WalkabilityView[end]))
                return null; // There is no path

            // If the path is simply the start, don't bother with graph initialization and such
            if (start == end)
            {
                var retVal = new List<Point> { start };
                return new Path(retVal);
            }

            // Update min weight if it has changed
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (MinimumWeight != _cachedMinWeight)
            {
                _cachedMinWeight = MinimumWeight;
                MaxEuclideanMultiplier = MinimumWeight / Point.EuclideanDistanceMagnitude(new Point(0, 0),
                    new Point(WalkabilityView.Width, WalkabilityView.Height));
            }

            // Update width/height dependent values if map width/height has changed
            if (_cachedWidth != WalkabilityView.Width || _cachedHeight != WalkabilityView.Height)
            {
                var length = WalkabilityView.Width * WalkabilityView.Height;
                _nodes = new AStarNode[length];
                _closed = new BitArray(length);
                _openNodes = new FastPriorityQueue<AStarNode>(length);

                _cachedWidth = WalkabilityView.Width;
                _cachedHeight = WalkabilityView.Height;

                MaxEuclideanMultiplier = MinimumWeight / Point.EuclideanDistanceMagnitude(new Point(0, 0),
                    new Point(WalkabilityView.Width, WalkabilityView.Height));
            }
            else
                _closed.SetAll(false);

            var result = new List<Point>();
            var index = start.ToIndex(WalkabilityView.Width);

            _nodes[index] ??= new AStarNode(start);

            // _nodes[index] is known to be not null because it was set to a value directly above this if it was
            _nodes[index]!.G = 0;
            _nodes[index]!.F = (float)Heuristic(start, end); // Completely heuristic for first node
            _openNodes.Enqueue(_nodes[index]!, _nodes[index]!.F);

            while (_openNodes.Count != 0)
            {
                var current = _openNodes.Dequeue();
                var currentIndex = current.Position.ToIndex(WalkabilityView.Width);
                _closed[currentIndex] = true;

                if (current.Position == end) // We found the end, cleanup and return the path
                {
                    _openNodes.Clear();

                    do
                    {
                        result.Add(current.Position);
                        // Overriding null because we know Parent won't be null because we'll hit the start and
                        // exit the loop first
                        current = current.Parent!;
                    } while (current.Position != start);

                    result.Add(start);
                    return new Path(result);
                }

                for (int i = 0; i < adjacencyRule.DirectionsOfNeighborsCache.Length; i++)
                {
                    var neighborPos = current.Position + adjacencyRule.DirectionsOfNeighborsCache[i];

                    // Not a valid map position, ignore
                    if (neighborPos.X < 0 || neighborPos.Y < 0 || neighborPos.X >= WalkabilityView.Width ||
                        neighborPos.Y >= WalkabilityView.Height)
                        continue;

                    if (!CheckWalkability(neighborPos, start, end, assumeEndpointsWalkable)
                    ) // Not part of walkable node "graph", ignore
                        continue;

                    var neighborIndex = neighborPos.ToIndex(WalkabilityView.Width);
                    var neighbor = _nodes[neighborIndex];

                    var isNeighborOpen = IsOpen(neighbor, _openNodes);

                    if (neighbor == null) // Can't be closed because never visited
                        _nodes[neighborIndex] = neighbor = new AStarNode(neighborPos);
                    else if (_closed[neighborIndex]
                    ) // This neighbor has already been evaluated at shortest possible path, don't re-add
                        continue;

                    var newDistance =
                        current.G + (float)DistanceMeasurement.Calculate(current.Position, neighbor.Position) *
                        (float)(Weights?[neighbor.Position] ?? 1.0);
                    if (isNeighborOpen && newDistance >= neighbor.G) // Not a better path
                        continue;

                    // We found a best path, so record and update
                    neighbor.Parent = current;
                    neighbor.G = newDistance; // (Known) distance to this node via shortest path
                    // Heuristic distance to end (priority in queue). If it's already in the queue, update priority to new F
                    neighbor.F = newDistance + (float)Heuristic(neighbor.Position, end);

                    if (_openNodes.Contains(neighbor))
                        _openNodes.UpdatePriority(neighbor, neighbor.F);
                    else // Otherwise, add it with proper priority
                        _openNodes.Enqueue(neighbor, neighbor.F);
                }
            }

            _openNodes.Clear();
            return null; // No path found
        }

        /// <summary>
        /// Finds the shortest path between the two specified points.
        /// </summary>
        /// <remarks>
        /// Returns <see langword="null" /> if there is no path between the specified points. Will still return an
        /// appropriate path object if the start point is equal to the end point.
        /// </remarks>
        /// <param name="startX">The x-coordinate of the starting point of the path.</param>
        /// <param name="startY">The y-coordinate of the starting point of the path.</param>
        /// <param name="endX">The x-coordinate of the ending point of the path.</param>
        /// <param name="endY">The y-coordinate of the ending point of the path.</param>
        /// <param name="assumeEndpointsWalkable">
        /// Whether or not to assume the start and end points are walkable, regardless of what the
        /// <see cref="WalkabilityView" /> reports. Defaults to <see langword="true" />.
        /// </param>
        /// <returns>The shortest path between the two points, or <see langword="null" /> if no valid path exists.</returns>
        public Path? ShortestPath(int startX, int startY, int endX, int endY, bool assumeEndpointsWalkable = true)
            => ShortestPath(new Point(startX, startY), new Point(endX, endY), assumeEndpointsWalkable);

        private static bool IsOpen(AStarNode? node, FastPriorityQueue<AStarNode> openSet)
            => node != null && openSet.Contains(node);

        private bool CheckWalkability(Point pos, Point start, Point end, bool assumeEndpointsWalkable)
        {
            if (!assumeEndpointsWalkable)
                return WalkabilityView[pos];

            return WalkabilityView[pos] || pos == start || pos == end;
        }
    }

    /// <summary>
    /// Encapsulates a path as returned by pathfinding algorithms like AStar.
    /// </summary>
    /// <remarks>
    /// Provides various functions to iterate through/access steps of the path, as well as
    /// constant-time reversing functionality.
    /// </remarks>
    [PublicAPI]
    public class Path
    {
        private readonly IReadOnlyList<Point> _steps;
        private bool _inOriginalOrder;

        /// <summary>
        /// Creates a copy of the path, optionally reversing the path as it does so.
        /// </summary>
        /// <remarks>Reversing is an O(1) operation, since it does not modify the list.</remarks>
        /// <param name="pathToCopy">The path to copy.</param>
        /// <param name="reverse">Whether or not to reverse the path. Defaults to <see langword="false" />.</param>
        public Path(Path pathToCopy, bool reverse = false)
        {
            _steps = pathToCopy._steps;
            _inOriginalOrder = reverse ? !pathToCopy._inOriginalOrder : pathToCopy._inOriginalOrder;
        }

        // Create based on internal list
        internal Path(IReadOnlyList<Point> steps)
        {
            _steps = steps;
            _inOriginalOrder = true;
        }

        /// <summary>
        /// Ending point of the path.
        /// </summary>
        public Point End => _inOriginalOrder ? _steps[0] : _steps[^1];

        /// <summary>
        /// The length of the path, NOT including the starting point.
        /// </summary>
        public int Length => _steps.Count - 1;

        /// <summary>
        /// The length of the path, INCLUDING the starting point.
        /// </summary>
        public int LengthWithStart => _steps.Count;

        /// <summary>
        /// Starting point of the path.
        /// </summary>
        public Point Start => _inOriginalOrder ? _steps[^1] : _steps[0];


        /// <summary>
        /// The coordinates that constitute the path (in order), NOT including the starting point.
        /// These are the coordinates something might walk along to follow a path.
        /// </summary>
        public IEnumerable<Point> Steps
        {
            get
            {
                if (_inOriginalOrder)
                    for (var i = _steps.Count - 2; i >= 0; i--)
                        yield return _steps[i];
                else
                    for (var i = 1; i < _steps.Count; i++)
                        yield return _steps[i];
            }
        }

        /// <summary>
        /// The coordinates that constitute the path (in order), INCLUDING the starting point.
        /// </summary>
        public IEnumerable<Point> StepsWithStart
        {
            get
            {
                if (_inOriginalOrder)
                    for (var i = _steps.Count - 1; i >= 0; i--)
                        yield return _steps[i];
                else
                    foreach (var step in _steps)
                        yield return step;
            }
        }

        /// <summary>
        /// Gets the nth step along the path, where 0 is the step AFTER the starting point.
        /// </summary>
        /// <param name="stepNum">The (array-like index) of the step to get.</param>
        /// <returns>The coordinate constituting the step specified.</returns>
        public Point GetStep(int stepNum)
        {
            if (_inOriginalOrder)
                return _steps[_steps.Count - 2 - stepNum];

            return _steps[stepNum + 1];
        }

        /// <summary>
        /// Gets the nth step along the path, where 0 IS the starting point.
        /// </summary>
        /// <param name="stepNum">The (array-like index) of the step to get.</param>
        /// <returns>The coordinate constituting the step specified.</returns>
        public Point GetStepWithStart(int stepNum) =>
            // TODO: Revisit array-from-end syntax here
            _inOriginalOrder ? _steps[_steps.Count - 1 - stepNum] : _steps[stepNum];

        /// <summary>
        /// Reverses the path, in constant time.
        /// </summary>
        public void Reverse() => _inOriginalOrder = !_inOriginalOrder;

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
        public readonly Point Position;

        // Whether or not the node has been closed
        public float F;

        // (Partly estimated) distance to end point going thru this node
        public float G;

        public AStarNode? Parent;
        // (Known) distance from start to this node, by shortest known path

        public AStarNode(Point position, AStarNode? parent = null)
        {
            Parent = parent;
            Position = position;
            F = G = float.MaxValue;
        }
    }
}
