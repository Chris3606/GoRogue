﻿using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Pathing
{
    /// <summary>
    /// Implementation of a goal map system, also known as Dijkstra maps,
    /// based on <a href="http://www.roguebasin.com/index.php?title=The_Incredible_Power_of_Dijkstra_Maps">this article</a>
    /// </summary>
    /// <remarks>
    /// This class encapsulates the work of building a goal map from your map level. You provide the
    /// constructor with a map view representing the map as <see cref="GoalState" /> values, and
    /// GoalMap will compute the goal map for the level. When the underlying circumstances of the
    /// level change, the GoalMap instance will need to be updated. Call <see cref="Update" /> if obstacles
    /// have changed, or <see cref="UpdatePathsOnly" /> if the goals have changed but not the obstacles.
    /// This class exposes the resulting goal map to you via indexers -- GoalMap implements
    /// <see cref="SadRogue.Primitives.GridViews.IGridView{T}" />, where <see langword="null" /> indicates a square is an obstacle,
    /// and any other value indicates distance from the nearest goal.  Thus, a value of 0 indicates a tile
    /// contains a goal.
    /// For items following the GoalMap, they can simply call <see cref="GetDirectionOfMinValue(Point)" />
    /// </remarks>
    [PublicAPI]
    public class GoalMap : GridViewBase<double?>
    {
        private readonly BitArray _closedSet;

        private readonly Queue<Point> _openEdges;

        private readonly ArrayView<double?> _goalMap;

        private readonly List<Point> _walkable = new List<Point>();

        /// <summary>
        /// Constructor. Takes a base map and a distance measurement to use for calculation.
        /// </summary>
        /// <param name="baseMap">
        /// A map view that represents the map as an
        /// <see cref="SadRogue.Primitives.GridViews.IGridView{T}" /> of <see cref="GoalState"/>.  Must not change
        /// width/height after the goal map has been constructed.
        /// </param>
        /// <param name="distanceMeasurement">
        /// The distance measurement (and implicitly the <see cref="SadRogue.Primitives.AdjacencyRule" />) to use for calculation.
        /// </param>
        public GoalMap(IGridView<GoalState> baseMap, Distance distanceMeasurement)
        {
            BaseMap = baseMap ?? throw new ArgumentNullException(nameof(baseMap));
            DistanceMeasurement = distanceMeasurement;

            _closedSet = new BitArray(baseMap.Width * baseMap.Height);
            _openEdges = new Queue<Point>();

            _goalMap = new ArrayView<double?>(baseMap.Width, baseMap.Height);
            Update();
        }

        /// <summary>
        /// The map view of the underlying map used to determine where obstacles/goals are.  Must not change width or
        /// height after the <see cref="GoalMap"/> has been constructed.
        /// </summary>
        public IGridView<GoalState> BaseMap { get; private set; }

        /// <summary>
        /// The distance measurement the GoalMap is using to calculate distance.
        /// </summary>
        public readonly Distance DistanceMeasurement;

        internal IReadOnlyList<Point> Walkable => _walkable.AsReadOnly();

        /// <summary>
        /// Height of the goal map.
        /// </summary>
        public override int Height => BaseMap.Height;

        /// <summary>
        /// Width of the goal map.
        /// </summary>
        public override int Width => BaseMap.Width;

        /// <summary>
        /// Returns the goal-map value for the given position.
        /// </summary>
        /// <param name="pos">The position to return the goal-map value for.</param>
        /// <returns>The goal-map value for the given position.</returns>
        public override double? this[Point pos] => _goalMap[pos];

        /// <summary>
        /// Triggers when the GoalMap is updated.
        /// </summary>
        public event Action Updated = () => { };

        /// <summary>
        /// Gets the direction of the neighbor with the minimum goal-map value from the given position.
        /// </summary>
        /// <param name="position">The position to get the minimum value for.</param>
        /// <returns>
        /// The direction that has the minimum value in the goal-map, or <see cref="SadRogue.Primitives.Direction.None" /> if the
        /// neighbors are all obstacles.
        /// </returns>
        public Direction GetDirectionOfMinValue(Point position)
            => this.GetDirectionOfMinValue(position, DistanceMeasurement);

        /// <summary>
        /// Gets the direction of the neighbor with the minimum goal-map value from the given position.
        /// </summary>
        /// <param name="positionX">The x-value of the position to get the minimum value for.</param>
        /// <param name="positionY">The y-value of the position to get the minimum value for.</param>
        /// <returns>
        /// The direction that has the minimum value in the goal-map, or <see cref="SadRogue.Primitives.Direction.None" /> if the
        /// neighbors are all obstacles.
        /// </returns>
        public Direction GetDirectionOfMinValue(int positionX, int positionY)
            => this.GetDirectionOfMinValue(positionX, positionY, DistanceMeasurement);

        /// <summary>
        /// Returns the goal-map values represented as a 2D grid-style string.
        /// </summary>
        /// <returns>A string representing the goal map values.</returns>
        public override string ToString() =>
            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
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
        /// obstacles have not changed but the goals have, call <see cref="UpdatePathsOnly" /> for better efficiency.
        /// </summary>
        /// <returns>False if no goals were produced by the evaluator, true otherwise</returns>
        public bool Update()
        {
            if (BaseMap.Bounds() != this.Bounds())
                throw new InvalidOperationException(
                    $"Grid views used as the {nameof(BaseMap)} for {nameof(GoalMap)} instances must not change size.");

            _walkable.Clear();
            for (var y = 0; y < BaseMap.Height; ++y)
                for (var x = 0; x < BaseMap.Width; ++x)
                {
                    var state = BaseMap[x, y];
                    if (state == GoalState.Obstacle)
                        _goalMap[x, y] = null;
                    else
                        _walkable.Add(new Point(x, y));
                }

            return UpdatePathsOnlyUnchecked();
        }

        /// <summary>
        /// Re-evaluates the walkable portion of the goal map. Should be called anytime the goals change
        /// but the obstacles haven't.  If the obstacles have also changed, call <see cref="Update" /> instead.
        /// </summary>
        /// <returns>False if no goals were produced by the evaluator, true otherwise</returns>
        public bool UpdatePathsOnly()
        {
            if (BaseMap.Bounds() != this.Bounds())
                throw new InvalidOperationException(
                    $"Grid views used as the {nameof(BaseMap)} for {nameof(GoalMap)} instances must not change size.");

            return UpdatePathsOnlyUnchecked();
        }

        private bool UpdatePathsOnlyUnchecked()
        {
            bool visitedNodes = false;
            var adjacencyRule = (AdjacencyRule)DistanceMeasurement;
            var highVal = (double)(BaseMap.Width * BaseMap.Height);
            _openEdges.Clear();
            _closedSet.SetAll(false);

            var mapBounds = _goalMap.Bounds();

            for (int i = 0; i < _walkable.Count; i++)
            {
                var point = _walkable[i];
                var state = BaseMap[point];
                if (state == GoalState.Clear)
                    _goalMap[point] = highVal;
                else
                {
                    _goalMap[point] = 0.0;
                    _openEdges.Enqueue(point);
                }
            }

            while (_openEdges.Count > 0)
            {
                var point = _openEdges.Dequeue();

                // Known to be not null since the else condition above will have assigned to it.
                var current = _goalMap[point]!.Value;
                for (int j = 0; j < adjacencyRule.DirectionsOfNeighborsCache.Length; j++)
                {
                    // We only want to process walkable, non-visited cells that are within the map
                    var openPoint = point + adjacencyRule.DirectionsOfNeighborsCache[j];
                    if (!mapBounds.Contains(openPoint)) continue;
                    if (_closedSet[openPoint.ToIndex(Width)] || BaseMap[openPoint] == GoalState.Obstacle)
                        continue;

                    // Known to be not null since it must be walkable.
                    var neighborValue = _goalMap[openPoint]!.Value;
                    var newValue = current + DistanceMeasurement.Calculate(point, openPoint);
                    if (newValue < neighborValue)
                    {
                        _goalMap[openPoint] = newValue;
                        _openEdges.Enqueue(openPoint);
                    }
                }

                _closedSet[point.ToIndex(Width)] = true;
                visitedNodes = true;
            }

            Updated();
            return visitedNodes;
        }
    }
}
