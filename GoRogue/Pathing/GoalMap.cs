using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;
using SadRogue.Primitives;

namespace GoRogue.Pathing
{
    /// <summary>
    /// Implementation of a goal map system, also known as Dijkstra maps,
    /// based on <a href="http://www.roguebasin.com/index.php?title=The_Incredible_Power_of_Dijkstra_Maps">this article</a>
    /// </summary>
    /// <remarks>
    /// This class encapsulates the work of building a goal map from your map level. You provide the
    /// constructor with a map view representing the map as <see cref="GoalState"/> values, and
    /// GoalMap will compute the goal map for the level. When the underlying circumstances of the
    /// level change, the GoalMap instance will need to be updated. Call <see cref="Update"/> if obstacles
    /// have changed, or <see cref="UpdatePathsOnly"/> if the goals have changed but not the obstacles.
    /// 
    /// This class exposes the resulting goal map to you via indexers -- GoalMap implements
    /// <see cref="IMapView{Double}"/>, where <see langword="null"/> indicates a square is an obstacle,
    /// and any other value indicates distance from the nearest goal.  Thus, a value of 0 indicates a tile
    /// contains a goal.
    /// 
    /// For items following the GoalMap, they can simply call <see cref="GetDirectionOfMinValue(Point)"/>
    /// </remarks>
    public class GoalMap : IMapView<double?>
    {
        private readonly HashSet<Point> _closedSet = new HashSet<Point>();

        private readonly HashSet<Point> _edgeSet = new HashSet<Point>();

        private readonly ArrayMap<double?> _goalMap;

        private readonly HashSet<Point> _walkable = new HashSet<Point>();

        /// <summary>
        /// Constructor. Takes a base map and a distance measurement to use for calculation.
        /// </summary>
        /// <param name="baseMap">A map view that represents the map as
        /// <see cref="IMapView{GoalState}"/>GoalStates.</param>
        /// <param name="distanceMeasurement">
        /// The distance measurement (and implicitly the <see cref="AdjacencyRule"/>) to use for calculation.
        /// </param>
        public GoalMap(IMapView<GoalState> baseMap, Distance distanceMeasurement)
        {
            BaseMap = baseMap ?? throw new ArgumentNullException(nameof(baseMap));
            DistanceMeasurement = distanceMeasurement;

            _goalMap = new ArrayMap<double?>(baseMap.Width, baseMap.Height);
            Update();
        }

        /// <summary>
        /// Triggers when the GoalMap is updated.
        /// </summary>
        public event Action Updated = () => { };

        /// <summary>
        /// The map view of the underlying map used to determine where obstacles/goals are.
        /// </summary>
        public IMapView<GoalState> BaseMap { get; private set; }

        /// <summary>
        /// The distance measurement the GoalMap is using to calculate distance.
        /// </summary>
        public Distance DistanceMeasurement { get; }

        /// <summary>
        /// Height of the goal map.
        /// </summary>
        public int Height => BaseMap.Height;

        /// <summary>
        /// Width of the goal map.
        /// </summary>
        public int Width => BaseMap.Width;

        internal IEnumerable<Point> Walkable => _walkable;

        /// <summary>
        /// Returns the goal-map value for the given position.
        /// </summary>
        /// <param name="index1D">Position to return the goal-map value for, as a 1d-index-style value.</param>
        /// <returns>The goal-map value for the given position.</returns>
        public double? this[int index1D] => _goalMap[index1D];

        /// <summary>
        /// Returns the goal-map value for the given position.
        /// </summary>
        /// <param name="x">X-Pointinate of the position to return the goal-map value for.</param>
        /// <param name="y">Y-Pointinate of the position to return the goal-map value for.</param>
        /// <returns>The goal-map value for the given position.</returns>
        public double? this[int x, int y] => _goalMap[x, y];

        /// <summary>
        /// Returns the goal-map value for the given position.
        /// </summary>
        /// <param name="pos">The position to return the goal-map value for.</param>
        /// <returns>The goal-map value for the given position.</returns>
        public double? this[Point pos] => _goalMap[pos];

        /// <summary>
        /// Gets the direction of the neighbor with the minimum goal-map value from the given position.
        /// </summary>
        /// <param name="position">The position to get the minimum value for.</param>
        /// <returns>
        /// The direction that has the minimum value in the goal-map, or <see cref="Direction.None"/> if the
        /// neighbors are all obstacles.
        /// </returns>
        public Direction GetDirectionOfMinValue(Point position) => ((IMapView<double?>)this).GetDirectionOfMinValue(position, DistanceMeasurement);

        /// <summary>
        /// Gets the direction of the neighbor with the minimum goal-map value from the given position.
        /// </summary>
        /// <param name="positionX">The x-value of the position to get the minimum value for.</param>
        /// <param name="positionY">The y-value of the position to get the minimum value for.</param>
        /// <returns>
        /// The direction that has the minimum value in the goal-map, or <see cref="Direction.None"/> if the
        /// neighbors are all obstacles.
        /// </returns>
        public Direction GetDirectionOfMinValue(int positionX, int positionY)
            => ((IMapView<double?>)this).GetDirectionOfMinValue(positionX, positionY, DistanceMeasurement);

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

        /// <summary>
        /// Re-evaluates the entire goal map. Should be called when obstacles change. If the
        /// obstacles have not changed but the goals have, call <see cref="UpdatePathsOnly"/> for better efficiency.
        /// </summary>
        /// <returns>False if no goals were produced by the evaluator, true otherwise</returns>
        public bool Update()
        {
            _walkable.Clear();
            for (int y = 0; y < BaseMap.Height; ++y)
            {
                for (int x = 0; x < BaseMap.Width; ++x)
                {
                    var state = BaseMap[x, y];
                    if (state == GoalState.Obstacle)
                    {
                        _goalMap[x, y] = null;
                    }
                    else
                    {
                        _walkable.Add(new Point(x, y));
                    }
                }
            }
            return UpdatePathsOnly();
        }

        /// <summary>
        /// Re-evaluates the walkable portion of the goal map. Should be called anytime the goals change
        /// but the obstacles haven't.  If the obstacles have also changed, call <see cref="Update"/> instead.
        /// </summary>
        /// <returns>False if no goals were produced by the evaluator, true otherwise</returns>
        public bool UpdatePathsOnly()
        {
            var adjacencyRule = (AdjacencyRule)DistanceMeasurement;

            var highVal = (double)(BaseMap.Width * BaseMap.Height);
            _edgeSet.Clear();
            _closedSet.Clear();
            foreach (var Point in _walkable)
            {
                var state = BaseMap[Point];
                if (state == GoalState.Clear)
                {
                    _goalMap[Point] = highVal;
                }
                else
                {
                    _goalMap[Point] = 0.0;
                    _edgeSet.Add(Point);
                }
            }
            while (_edgeSet.Count > 0)
            {
                foreach (var point in _edgeSet.ToArray())
                {
                    var current = _goalMap[point]!.Value; // Known to be not null since the else condition above will have assigned to it.
                    foreach (var openPoint in adjacencyRule.Neighbors(point))
                    {
                        if (_closedSet.Contains(openPoint) || !_walkable.Contains(openPoint))
                            continue;
                        var neighborValue = _goalMap[openPoint]!.Value; // Known to be not null since it must be walkable.
                        var newValue = current + DistanceMeasurement.Calculate(point, openPoint);
                        if (newValue < neighborValue)
                        {
                            _goalMap[openPoint] = newValue;
                            _edgeSet.Add(openPoint);
                        }
                    }
                    _edgeSet.Remove(point);
                    _closedSet.Add(point);
                }
            }
            Updated();
            return _closedSet.Count > 0;
        }
    }
}
