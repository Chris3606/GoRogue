using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;
using Priority_Queue;

namespace GoRogue.Pathing
{
    public class FleeMap : IMapView<double?>, IDisposable
    {
        private readonly GoalMap _baseMap;
        private ArrayMap<double?> _goalMap;
        public double Magnitude { get; set; }
        public int Height => _goalMap.Height;
        public int Width => _goalMap.Width;
        public double? this[Coord pos] => _goalMap[pos];
        public double? this[int x, int y] => _goalMap[x, y];
        public FleeMap(GoalMap baseMap, double magnitude)
        {
            _baseMap = baseMap ?? throw new ArgumentNullException(nameof(baseMap));
            this.Magnitude = magnitude;
            _goalMap = new ArrayMap<double?>(baseMap.Width, baseMap.Height);
            _baseMap.Updated += this.Update;
        }
        public FleeMap(GoalMap baseMap) : this(baseMap, 1.2)
        {
        }

        /// <summary>
        /// Returns the goal-map values represented as a 2D grid-style string.
        /// </summary>
        /// <returns>A string representing the goal map values.</returns>
        public override string ToString() =>
            _goalMap.ToString(val => val.HasValue ? val.Value.ToString() : "null");

        /// <summary>
        /// Returns the goal-map values represented as a 2D-grid-style string, where any value that
        /// isn't null is formatted as per the specified format string.
        /// </summary>
        /// <param name="formatString">Format string to use for non-null values.</param>
        /// <returns>A string representing the goal-map values.</returns>
        public string ToString(string formatString) =>
            _goalMap.ToString(val => val.HasValue ? val.Value.ToString(formatString) : "null");

        /// <summary>
        /// Returns the goal-map values represented as a 2D-grid-style string, with the given field size.
        /// </summary>
        /// <param name="fieldSize">Number of characters allocated to each value in the string.</param>
        /// <returns>A string representing the goal-map values.</returns>
        public string ToString(int fieldSize)
            => _goalMap.ToString(fieldSize);

        /// <summary>
        /// Returns the goal-map values represented as a 2D-grid-style string, with the given field
        /// size, and any non-null values formatted using the given format string.
        /// </summary>
        /// <param name="fieldSize">Number of characters allocated to each value in the string.</param>
        /// <param name="formatString">Format string to use for non-null values.</param>
        /// <returns>A string representing the goal-map values.</returns>
        public string ToString(int fieldSize, string formatString)
            => _goalMap.ToString(fieldSize, val => val.HasValue ? val.Value.ToString(formatString) : "null");

        private void Update()
        {
            var adjacencyRule = (AdjacencyRule)_baseMap.DistanceMeasurement;

            // var openSet = new HashSet<Coord>();

            var openSet = new SimplePriorityQueue<Coord, double>();
            foreach (var point in _baseMap.Walkable)
            {
                var newPoint = _baseMap[point] * -Magnitude;
                _goalMap[point] = newPoint;
                //openSet.Add(point);
                if (!openSet.EnqueueWithoutDuplicates(point, newPoint.Value))
                    throw new Exception("Duplicate item enqueued.");
            }
            var edgeSet = new HashSet<Coord>();
            var closedSet = new HashSet<Coord>();
            // var walkable = new HashSet<Coord>(_baseMap.Walkable);
            while (openSet.Count > 0) //multiple runs are needed to deal with islands
            {
                /*
                var min = (double)Width * Height;
                var minPoint = Coord.Get(-1, -1);
                foreach (var point in openSet)
                {
                    var value = _goalMap[point];
                    if (value < min)
                    {
                        min = value.Value;
                        minPoint = point;
                    }
                }
                */
                var minPoint = openSet.Dequeue();
                closedSet.Add(minPoint);
                // openSet.Remove(minPoint);

                foreach (var openPoint in adjacencyRule.Neighbors(minPoint))
                {
                    if ((!closedSet.Contains(openPoint)) && _baseMap.BaseMap[openPoint] != GoalState.Obstacle) // walkable.Contains(openPoint))
                        edgeSet.Add(openPoint);
                }
                while (edgeSet.Count > 0)
                {
                    foreach (var coord in edgeSet.ToArray())
                    {
                        var current = _goalMap[coord].Value;
                        foreach (var openPoint in adjacencyRule.Neighbors(coord))
                        {
                            if (closedSet.Contains(openPoint) || _baseMap.BaseMap[openPoint] == GoalState.Obstacle) // !walkable.Contains(openPoint))
                                continue;
                            var neighborValue = _goalMap[openPoint].Value;
                            var newValue = current + _baseMap.DistanceMeasurement.Calculate(coord, openPoint);
                            if (newValue < neighborValue)
                            {
                                _goalMap[openPoint] = newValue;
                                if (openSet.Contains(openPoint))
                                    openSet.UpdatePriority(openPoint, newValue);
                                else
                                    throw new Exception("Values are changing that aren't walkable.");
                                edgeSet.Add(openPoint);
                            }
                        }
                        edgeSet.Remove(coord);
                        closedSet.Add(coord);
                        openSet.Remove(coord);
                    }
                }
                // openSet.RemoveWhere(c => closedSet.Contains(c));
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
                _baseMap.Updated -= this.Update;
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
        #endregion
    }
}
