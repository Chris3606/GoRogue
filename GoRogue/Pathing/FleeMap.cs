using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;
using Priority_Queue;

namespace GoRogue.Pathing
{
    class PositionNode : GenericPriorityQueueNode<double>
    {
        public Coord Position { get; }

        public PositionNode(Coord position) { Position = position; }
    }

    public class FleeMap : IMapView<double?>, IDisposable
    {
        private readonly GoalMap _baseMap;
        private ArrayMap<double?> _goalMap;
        private ArrayMap<PositionNode> _nodes;

        public double Magnitude { get; set; }
        public int Height => _goalMap.Height;
        public int Width => _goalMap.Width;
        public double? this[Coord pos] => _goalMap[pos];
        public double? this[int x, int y] => _goalMap[x, y];
        public FleeMap(GoalMap baseMap, double magnitude)
        {
            _baseMap = baseMap ?? throw new ArgumentNullException(nameof(baseMap));
            Magnitude = magnitude;
            _goalMap = new ArrayMap<double?>(baseMap.Width, baseMap.Height);
            _nodes = new ArrayMap<PositionNode>(baseMap.Width, baseMap.Height);
            foreach (var pos in _nodes.Positions())
                _nodes[pos] = new PositionNode(pos);

            _baseMap.Updated += Update;
        }
        public FleeMap(GoalMap baseMap) : this(baseMap, 1.2)
        {
        }

        /// <summary>
        /// Returns the flee-map values represented as a 2D grid-style string.
        /// </summary>
        /// <returns>A string representing the flee map values.</returns>
        public override string ToString() =>
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
                var newPoint = _baseMap[point] * -Magnitude;
                _goalMap[point] = newPoint;

                openSet.Enqueue(_nodes[point], newPoint.Value);
            }
            var edgeSet = new HashSet<Coord>();
            var closedSet = new HashSet<Coord>();

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
                    foreach (var coord in edgeSet.ToArray())
                    {
                        var current = _goalMap[coord].Value;
                        foreach (var openPoint in adjacencyRule.Neighbors(coord))
                        {
                            if (closedSet.Contains(openPoint) || _baseMap.BaseMap[openPoint] == GoalState.Obstacle)
                                continue;
                            var neighborValue = _goalMap[openPoint].Value;
                            var newValue = current + _baseMap.DistanceMeasurement.Calculate(coord, openPoint);
                            if (newValue < neighborValue)
                            {
                                _goalMap[openPoint] = newValue;
                                openSet.UpdatePriority(_nodes[openPoint], newValue);
                                edgeSet.Add(openPoint);
                            }
                        }
                        edgeSet.Remove(coord);
                        closedSet.Add(coord);
                        openSet.Remove(_nodes[coord]);
                    }
                }
            }
        }
        #region IDisposable Support
        private bool _disposed = false;
        ~FleeMap()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _baseMap.Updated -= Update;
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
        #endregion
    }
}
