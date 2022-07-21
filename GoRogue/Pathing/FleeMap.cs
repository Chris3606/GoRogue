using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Priority_Queue;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Pathing
{
    /// <summary>
    /// Implements the concept of a "safety map", also known as "flee map", as described in the
    /// <a href="http://www.roguebasin.com/index.php?title=The_Incredible_Power_of_Dijkstra_Maps">this article</a>.
    /// </summary>
    /// <remarks>
    /// Takes a goal map, wherein any goals are treated as "threats" to be avoided. Automatically
    /// recalculated when the underlying goal map is recalculated. Implements IDisposable, so ensure
    /// that it is disposed of properly after use.
    /// </remarks>
    [PublicAPI]
    public class FleeMap : GridViewBase<double?>, IDisposable
    {
        private readonly GoalMap _baseMap;
        private readonly ArrayView<double?> _goalMap;
        private readonly GenericPriorityQueue<PositionNode, double> _openSet;
        private readonly Queue<Point> _edgeSet;
        private readonly BitArray _closedSet;

        // Nodes for the priority queue used in Update.
        private readonly ArrayView<PositionNode> _nodes;

        /// <summary>
        /// Constructor. Takes a goal map where in all goals are treated as threats to be avoided,
        /// and a magnitude to use (defaulting to 1.2).
        /// </summary>
        /// <param name="baseMap">The underlying goal map to use.</param>
        /// <param name="magnitude">Magnitude to multiply by during calculation.</param>
        public FleeMap(GoalMap baseMap, double magnitude = 1.2)
        {
            _baseMap = baseMap ?? throw new ArgumentNullException(nameof(baseMap));
            Magnitude = magnitude;
            _goalMap = new ArrayView<double?>(baseMap.Width, baseMap.Height);
            _nodes = new ArrayView<PositionNode>(baseMap.Width, baseMap.Height);
            _openSet = new GenericPriorityQueue<PositionNode, double>(baseMap.Width * baseMap.Height);
            _edgeSet = new Queue<Point>();
            _closedSet = new BitArray(baseMap.Width * baseMap.Height);
            foreach (var pos in _nodes.Positions())
                _nodes[pos] = new PositionNode(pos);

            _baseMap.Updated += Update;

            // Necessary to ensure the FleeMap is valid immediately after construction
            Update();
        }

        /// <summary>
        /// The degree to which entities following this flee-map will prefer global safety to local
        /// safety. Higher values will make entities try to move past an approaching "threat" from
        /// farther away.
        /// </summary>
        public double Magnitude { get; set; }

        /// <summary>
        /// Height of the flee map.
        /// </summary>
        public override int Height => _goalMap.Height;

        /// <summary>
        /// Width of the flee map.
        /// </summary>
        public override int Width => _goalMap.Width;

        /// <summary>
        /// Returns the flee-map value for the given position.
        /// </summary>
        /// <param name="pos">The position to return the value for.</param>
        /// <returns>The flee-map value for the given location.</returns>
        public override double? this[Point pos] => _goalMap[pos];

        /// <summary>
        /// Gets the direction of the neighbor with the minimum flee-map value from the given position.
        /// </summary>
        /// <param name="position">The position to get the minimum value for.</param>
        /// <returns>
        /// The direction that has the minimum value in the goal-map, or <see cref="SadRogue.Primitives.Direction.None" /> if the
        /// neighbors are all obstacles.
        /// </returns>
        public Direction GetDirectionOfMinValue(Point position)
            => _goalMap.GetDirectionOfMinValue(position, _baseMap.DistanceMeasurement);

        /// <summary>
        /// Gets the direction of the neighbor with the minimum flee-map value from the given position.
        /// </summary>
        /// <param name="positionX">The x-value of the position to get the minimum value for.</param>
        /// <param name="positionY">The y-value of the position to get the minimum value for.</param>
        /// <returns>
        /// The direction that has the minimum value in the goal-map, or <see cref="SadRogue.Primitives.Direction.None" /> if the
        /// neighbors are all obstacles.
        /// </returns>
        public Direction GetDirectionOfMinValue(int positionX, int positionY)
            => _goalMap.GetDirectionOfMinValue(positionX, positionY, _baseMap.DistanceMeasurement);

        /// <summary>
        /// Returns the flee-map values represented as a 2D grid-style string.
        /// </summary>
        /// <returns>A string representing the flee map values.</returns>
        public override string ToString() =>
            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            _goalMap.ToString(val => val.HasValue ? val.Value.ToString() : "null");

        /// <summary>
        /// Returns the flee-map values represented as a 2D-grid-style string, where any value that
        /// isn't null is formatted as per the specified format string.
        /// </summary>
        /// <param name="formatString">Format string to use for non-null values.</param>
        /// <returns>A string representing the flee-map values.</returns>
        public string ToString(string formatString) =>
            _goalMap.ToString(val => val.HasValue ? val.Value.ToString(formatString) : "null");

        /// <summary>
        /// Returns the flee-map values represented as a 2D-grid-style string, with the given field size.
        /// </summary>
        /// <param name="fieldSize">Number of characters allocated to each value in the string.</param>
        /// <returns>A string representing the flee-map values.</returns>
        public string ToString(int fieldSize)
            => _goalMap.ToString(fieldSize);

        /// <summary>
        /// Returns the flee-map values represented as a 2D-grid-style string, with the given field
        /// size, and any non-null values formatted using the given format string.
        /// </summary>
        /// <param name="fieldSize">Number of characters allocated to each value in the string.</param>
        /// <param name="formatString">Format string to use for non-null values.</param>
        /// <returns>A string representing the flee-map values.</returns>
        public string ToString(int fieldSize, string formatString)
            => _goalMap.ToString(fieldSize, val => val.HasValue ? val.Value.ToString(formatString) : "null");

        private void Update()
        {
            int width = Width;
            AdjacencyRule adjacencyRule = _baseMap.DistanceMeasurement;

            var mapBounds = _goalMap.Bounds();

            var walkable = _baseMap.Walkable;
            for (int i = 0; i < walkable.Count; i++)
            {
                var point = walkable[i];

                // Value won't be null as null only happens for non-walkable squares
                var newPoint = _baseMap[point]!.Value * -Magnitude;
                _goalMap[point] = newPoint;

                _openSet.Enqueue(_nodes[point], newPoint);
            }

            _edgeSet.Clear();
            _closedSet.SetAll(false);

            while (_openSet.Count > 0) // Multiple runs are needed to deal with islands
            {
                var minNode = _openSet.Dequeue();
                _closedSet[minNode.Position.ToIndex(width)] = true;

                for (int i = 0; i < adjacencyRule.DirectionsOfNeighborsCache.Length; i++)
                {
                    var openPoint = minNode.Position + adjacencyRule.DirectionsOfNeighborsCache[i];
                    if (!mapBounds.Contains(openPoint)) continue;

                    if (!_closedSet[openPoint.ToIndex(width)] && _baseMap.BaseMap[openPoint] != GoalState.Obstacle)
                        _edgeSet.Enqueue(openPoint);
                }

                while (_edgeSet.Count > 0)
                {
                    var point = _edgeSet.Dequeue();
                    var pointIndex = point.ToIndex(width);
                    if (!mapBounds.Contains(point) || _closedSet[pointIndex]) continue;

                    var current = _goalMap[point]!.Value; // Never added non-nulls so this is fine

                    for (int j = 0; j < adjacencyRule.DirectionsOfNeighborsCache.Length; j++)
                    {
                        var openPoint = point + adjacencyRule.DirectionsOfNeighborsCache[j];
                        if (!mapBounds.Contains(openPoint)) continue;
                        if (_closedSet[openPoint.ToIndex(width)] || _baseMap.BaseMap[openPoint] == GoalState.Obstacle)
                            continue;

                        var neighborValue = _goalMap[openPoint]!.Value; // Never added non-nulls so this is fine
                        var newValue = current + _baseMap.DistanceMeasurement.Calculate(point, openPoint);
                        if (newValue < neighborValue)
                        {
                            _goalMap[openPoint] = newValue;
                            _openSet.UpdatePriority(_nodes[openPoint], newValue);
                            _edgeSet.Enqueue(openPoint);
                        }
                    }

                    _closedSet[pointIndex] = true;
                    _openSet.Remove(_nodes[point]);
                }
            }
        }

        #region IDisposable Support

        private bool _disposed;

        /// <summary>
        /// Destructor for IDisposable implementation.
        /// </summary>
        ~FleeMap()
        {
            Dispose();
        }

        /// <summary>
        /// Function called to dispose of the class, automatically un-linking it from its goal map.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _baseMap.Updated -= Update;
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        #endregion IDisposable Support
    }

    // Priority queue node for Points
    internal class PositionNode : GenericPriorityQueueNode<double>
    {
        public PositionNode(Point position) => Position = position;

        // Position being represented.
        public Point Position { get; }
    }
}
