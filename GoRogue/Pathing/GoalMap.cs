using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;

namespace GoRogue.Pathing
{
	/// <summary>
	/// Implementation of a goal map system, also known as Dijkstra maps
	/// ( http://www.roguebasin.com/index.php?title=The_Incredible_Power_of_Dijkstra_Maps )
	/// </summary>
	/// <remarks>
	/// This class encapsulates the work of building a goal map from your map level.  You provide
	/// the constructor with a map level exposed as GoalStates (obstacle, a goal, or an open space),
	/// and GoalMap will compute the goal map for the level.
	/// 
	/// When the underlying circumstances of the level change, the GoalMap instance will need to
	/// be updated.  Call Update() if obstacles have changed, such as digging through walls or opening
	/// or closing a door, or UpdatePathsOnly() if the goals have changed but not the obstacles.
	/// 
	/// Each cell is a Nullable&lt;double&gt;, where a null is an obstacle, and a value indicates
	/// a distance from a goal, with 0 being a goal tile.
	/// </remarks>
	public class GoalMap : IMapView<double?>
	{
		/// <summary>
		/// The underlying map.
		/// </summary>
		public IMapView<GoalState> BaseMap { get; private set; }

		private HashSet<Coord> _walkable = new HashSet<Coord>();
		private HashSet<Coord> _edgeSet = new HashSet<Coord>();
		private HashSet<Coord> _closedSet = new HashSet<Coord>();

		private ArrayMap<double?> _goalMap;
		private Distance _distanceMeasurement;

		/// <summary>
		/// Width of the goal map.
		/// </summary>
		public int Width { get => BaseMap.Width; }

		/// <summary>
		/// Height of the goal map.
		/// </summary>
		public int Height { get => BaseMap.Height; }

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
		/// Constructor. Takes a base map and a distance measurement to use for calculation.
		/// </summary>
		/// <param name="baseMap">The underlying map as GoalStates.</param>
		/// <param name="distanceMeasurement">The distance measurement (and implicitly the AdjacencyRule)
		/// to use for calculation.</param>
		public GoalMap(IMapView<GoalState> baseMap, Distance distanceMeasurement)
		{
			BaseMap = baseMap ?? throw new ArgumentNullException(nameof(baseMap));
			_distanceMeasurement = distanceMeasurement ?? throw new ArgumentNullException(nameof(baseMap));

			_goalMap = new ArrayMap<double?>(baseMap.Width, baseMap.Height);
			Update();
		}

		/// <summary>
		/// Re-evaluates the entire goal map.  Should be called when obstacles change.
		/// If the obstacles have not changed but the goals have, call UpdatePathsOnly()
		/// for better efficiency.
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
					} else {
						_walkable.Add(Coord.Get(x, y));
					}
				}
			}
			return UpdatePathsOnly();
		}

		/// <summary>
		/// Re-evaluates the walkable portion of the goal map.  Should be called anytime
		/// the goals change.
		/// </summary>
		/// <returns>False if no goals were produced by the evaluator, true otherwise</returns>
		public bool UpdatePathsOnly()
		{
			var highVal = (double)(BaseMap.Width * BaseMap.Height);
			_edgeSet.Clear();
			_closedSet.Clear();
			foreach (var coord in _walkable)
			{
				var state = BaseMap[coord];
				if (state == GoalState.Clear)
				{
					_goalMap[coord] = highVal;
				} else {
					_goalMap[coord] = 0.0;
					_edgeSet.Add(coord);
				}
			}
			while (_edgeSet.Count > 0)
			{
				foreach (var coord in _edgeSet.ToArray())
				{
					var current = _goalMap[coord].Value;
					foreach (var openPoint in ((AdjacencyRule)_distanceMeasurement).Neighbors(coord))
					{
						if (_closedSet.Contains(openPoint) || !_walkable.Contains(openPoint))
							continue;
						var neighborValue = _goalMap[openPoint].Value;
						var newValue = current + _distanceMeasurement.Calculate(coord, openPoint);
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
			return _closedSet.Count > 0;
		}

		public override string ToString() =>
			_goalMap.ToString(val => val.HasValue ? val.Value.ToString() : "null");

        public string ToString(string formatString) =>
            _goalMap.ToString(val => val.HasValue ? val.Value.ToString(formatString) : "null");


        public string ToString(int fieldSize)
            => _goalMap.ToString(fieldSize);

        public string ToString(int fieldSize, string formatString)
            => _goalMap.ToString(fieldSize, val => val.HasValue ? val.Value.ToString(formatString) : "null");
	}
}
