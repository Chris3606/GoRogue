using GoRogue.MapViews;
using System;
using System.Collections.Generic;
using System.Linq;

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
	/// For items following the GoalMap, they can simply call <see cref="GetDirectionOfMinValue(Coord)"/>
	/// </remarks>
	public class GoalMap : IMapView<double?>
	{
		private HashSet<Coord> _closedSet = new HashSet<Coord>();

		private HashSet<Coord> _edgeSet = new HashSet<Coord>();

		private ArrayMap<double?> _goalMap;

		private HashSet<Coord> _walkable = new HashSet<Coord>();

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
			DistanceMeasurement = distanceMeasurement ?? throw new ArgumentNullException(nameof(baseMap));

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
		public int Height { get => BaseMap.Height; }

		/// <summary>
		/// Width of the goal map.
		/// </summary>
		public int Width { get => BaseMap.Width; }

		internal IEnumerable<Coord> Walkable { get { return _walkable; } }

		/// <summary>
		/// Returns the goal-map value for the given position.
		/// </summary>
		/// <param name="index1D">Position to return the goal-map value for, as a 1d-index-style value.</param>
		/// <returns>The goal-map value for the given position.</returns>
		public double? this[int index1D] => _goalMap[index1D];

		/// <summary>
		/// Returns the goal-map value for the given position.
		/// </summary>
		/// <param name="x">X-coordinate of the position to return the goal-map value for.</param>
		/// <param name="y">Y-coordinate of the position to return the goal-map value for.</param>
		/// <returns>The goal-map value for the given position.</returns>
		public double? this[int x, int y] => _goalMap[x, y];

		/// <summary>
		/// Returns the goal-map value for the given position.
		/// </summary>
		/// <param name="pos">The position to return the goal-map value for.</param>
		/// <returns>The goal-map value for the given position.</returns>
		public double? this[Coord pos] => _goalMap[pos];

		/// <summary>
		/// Gets the direction of the neighbor with the minimum goal-map value from the given position.
		/// </summary>
		/// <param name="position">The position to get the minimum value for.</param>
		/// <returns>
		/// The direction that has the minimum value in the goal-map, or <see cref="Direction.NONE"/> if the
		/// neighbors are all obstacles.
		/// </returns>
		public Direction GetDirectionOfMinValue(Coord position) => ((IMapView<double?>)this).GetDirectionOfMinValue(position, DistanceMeasurement);

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
						_walkable.Add(new Coord(x, y));
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
			foreach (var coord in _walkable)
			{
				var state = BaseMap[coord];
				if (state == GoalState.Clear)
				{
					_goalMap[coord] = highVal;
				}
				else
				{
					_goalMap[coord] = 0.0;
					_edgeSet.Add(coord);
				}
			}
			while (_edgeSet.Count > 0)
			{
				foreach (var coord in _edgeSet.ToArray())
				{
					var current = _goalMap[coord].Value;
					foreach (var openPoint in adjacencyRule.Neighbors(coord))
					{
						if (_closedSet.Contains(openPoint) || !_walkable.Contains(openPoint))
							continue;
						var neighborValue = _goalMap[openPoint].Value;
						var newValue = current + DistanceMeasurement.Calculate(coord, openPoint);
						if (newValue < neighborValue)
						{
							_goalMap[openPoint] = newValue;
							_edgeSet.Add(openPoint);
						}
					}
					_edgeSet.Remove(coord);
					_closedSet.Add(coord);
				}
			}
			Updated();
			return _closedSet.Count > 0;
		}
	}
}