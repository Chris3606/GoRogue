using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;
using JetBrains.Annotations;
using Priority_Queue;
using SadRogue.Primitives;

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
#pragma warning disable CA1063 // We aren't really freeing unmanaged resources so the typical Dispose(false) call is irrelevant
    [PublicAPI]
    public class FleeMap : IMapView<double?>, IDisposable
#pragma warning restore CA1063
    {
        private readonly GoalMap _baseMap;
        private readonly ArrayMap<double?> _goalMap;

        // Nodes for the priority queue used in Update.
        private readonly ArrayMap<PositionNode> _nodes;

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
            _goalMap = new ArrayMap<double?>(baseMap.Width, baseMap.Height);
            _nodes = new ArrayMap<PositionNode>(baseMap.Width, baseMap.Height);
            foreach (var pos in _nodes.Positions())
                _nodes[pos] = new PositionNode(pos);

            _baseMap.Updated += Update;
        }

        /// <summary>
        /// Height of the flee map.
        /// </summary>
        public int Height => _goalMap.Height;

        /// <summary>
        /// The degree to which entities following this flee-map will prefer global safety to local
        /// safety. Higher values will make entities try to move past an approaching "threat" from
        /// farther away.
        /// </summary>
        public double Magnitude { get; set; }

        /// <summary>
        /// Width of the flee map.
        /// </summary>
        public int Width => _goalMap.Width;

        /// <summary>
        /// Returns the flee-map value for the given position.
        /// </summary>
        /// <param name="index1D">The position to return the value for, as a 1D-array-style index.</param>
        /// <returns>The flee-map value for the given location.</returns>
        public double? this[int index1D] => _goalMap[index1D];

        /// <summary>
        /// Returns the flee-map value for the given position.
        /// </summary>
        /// <param name="pos">The position to return the value for.</param>
        /// <returns>The flee-map value for the given location.</returns>
        public double? this[Point pos] => _goalMap[pos];

        /// <summary>
        /// Returns the goal-map value for the given position.
        /// </summary>
        /// <param name="x">The x-value of the position to return the value for.</param>
        /// <param name="y">The y-value of the position to return the value for.</param>
        /// <returns>The flee-map value for the given location.</returns>
        public double? this[int x, int y] => _goalMap[x, y];

        /// <summary>
        /// Gets the direction of the neighbor with the minimum flee-map value from the given position.
        /// </summary>
        /// <param name="position">The position to get the minimum value for.</param>
        /// <returns>
        /// The direction that has the minimum value in the goal-map, or <see cref="Direction.None"/> if the
        /// neighbors are all obstacles.
        /// </returns>
        public Direction GetDirectionOfMinValue(Point position) => _goalMap.GetDirectionOfMinValue(position, _baseMap.DistanceMeasurement);

        /// <summary>
        /// Gets the direction of the neighbor with the minimum flee-map value from the given position.
        /// </summary>
        /// <param name="positionX">The x-value of the position to get the minimum value for.</param>
        /// <param name="positionY">The y-value of the position to get the minimum value for.</param>
        /// <returns>
        /// The direction that has the minimum value in the goal-map, or <see cref="Direction.None"/> if the
        /// neighbors are all obstacles.
        /// </returns>
        public Direction GetDirectionOfMinValue(int positionX, int positionY) => _goalMap.GetDirectionOfMinValue(positionX, positionY, _baseMap.DistanceMeasurement);

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
            var adjacencyRule = (AdjacencyRule)_baseMap.DistanceMeasurement;
            var openSet = new GenericPriorityQueue<PositionNode, double>(Width * Height);

            foreach (var point in _baseMap.Walkable)
            {
                var newPoint = _baseMap[point]!.Value * -Magnitude; // Value won't be null as null only happens for non-walkable squares
                _goalMap[point] = newPoint;

                openSet.Enqueue(_nodes[point], newPoint);
            }
            var edgeSet = new HashSet<Point>();
            var closedSet = new HashSet<Point>();

            while (openSet.Count > 0) //multiple runs are needed to deal with islands
            {
                var minNode = openSet.Dequeue();
                closedSet.Add(minNode.Position);

                foreach (var openPoint in adjacencyRule.Neighbors(minNode.Position))
                {
                    if ((!closedSet.Contains(openPoint)) && _baseMap.BaseMap[openPoint] != GoalState.Obstacle)
                        edgeSet.Add(openPoint);
                }
                while (edgeSet.Count > 0)
                {
                    foreach (var point in edgeSet.ToArray())
                    {
                        var current = _goalMap[point]!.Value; // Never added non-nulls so this is fine
                        foreach (var openPoint in adjacencyRule.Neighbors(point))
                        {
                            if (closedSet.Contains(openPoint) || _baseMap.BaseMap[openPoint] == GoalState.Obstacle)
                                continue;
                            var neighborValue = _goalMap[openPoint]!.Value; // Never added non-nulls so this is fine
                            var newValue = current + _baseMap.DistanceMeasurement.Calculate(point, openPoint);
                            if (newValue < neighborValue)
                            {
                                _goalMap[openPoint] = newValue;
                                openSet.UpdatePriority(_nodes[openPoint], newValue);
                                edgeSet.Add(openPoint);
                            }
                        }
                        edgeSet.Remove(point);
                        closedSet.Add(point);
                        openSet.Remove(_nodes[point]);
                    }
                }
            }
        }

        #region IDisposable Support

        private bool _disposed;

        /// <summary>
        /// Destructor for IDisposable implementation.
        /// </summary>
#pragma warning disable CA1063 // We aren't really freeing unmanaged resources so the typical Dispose(false) call is irrelevant
        ~FleeMap()
#pragma warning restore CA1063
        {
            Dispose();
        }

        /// <summary>
        /// Function called to dispose of the class, automatically un-linking it from its goal map.
        /// </summary>
#pragma warning disable CA1063 // We aren't really freeing unmanaged resources so the typical Dispose(false) call is irrelevant
        public void Dispose()
#pragma warning restore CA1063
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
