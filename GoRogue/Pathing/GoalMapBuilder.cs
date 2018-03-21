using System;
using System.Collections.Generic;
using System.Linq;

namespace GoRogue.Pathing
{
	/// <summary>
	/// Implementation of a goal map system, also known as Dijkstra maps
	/// ( http://www.roguebasin.com/index.php?title=The_Incredible_Power_of_Dijkstra_Maps )
	/// </summary>
	/// <remarks>
	/// This class encapsulates the work of building a goal map from your map level.  You provide
	/// the constructor with a map level and an evaluation function containing the logic to determine
	/// whether a given tile is an obstacle, a goal, or an open space, and GoalMapBuilder will compute
	/// the goal map for the level.
	/// 
	/// When the underlying circumstances of the level change, the GoalMapBuilder instance will need to
	/// be updated.  Call Update() if obstacles have changed, such as digging through walls or opening
	/// or closing a door, or UpdatePathsOnly() if the goals have changed but not the obstacles.
	/// </remarks>
	/// <typeparam name="T">The type of value in the underlying map.</typeparam>
	public class GoalMapBuilder<T>
	{
		/// <summary>
		/// The underlying map
		/// </summary>
		public IMapView<T> BaseMap { get; }

		/// <summary>
		/// The goal map.  Each cell is a Nullable&lt;double&gt;, where a null is an obstacle, and a value
		/// indicates a distance from a goal, with 0 being a goal tile.
		/// </summary>
		public ArrayMap<double?> GoalMap { get; }

		private Func<T, Coord, GoalState> _evaluator;

		private HashSet<Coord> _walkable = new HashSet<Coord>();

		private HashSet<Coord> _edgeSet = new HashSet<Coord>();

		private HashSet<Coord> _closedSet = new HashSet<Coord>();

		/// <summary>
		/// Constructor. Takes a base map and a function to evaluate a tile's goal state.
		/// </summary>
		/// <param name="baseMap">The underlying map.</param>
		/// <param name="evaluator">Lambda that determines whether a tile is an obstacle, a goal, or open space</param>
		public GoalMapBuilder(IMapView<T> baseMap, Func<T, Coord, GoalState> evaluator)
		{
			if (baseMap == null)
				throw new ArgumentNullException(nameof(baseMap));
			if (evaluator == null)
				throw new ArgumentNullException(nameof(evaluator));

			BaseMap = baseMap;
			_evaluator = evaluator;
			GoalMap = new ArrayMap<double?>(baseMap.Width, baseMap.Height);
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
					var state = _evaluator(BaseMap[x, y], Coord.Get(x, y));
					if (state == GoalState.Obstacle)
					{
						GoalMap[x, y] = null;
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
				var state = _evaluator(BaseMap[coord], coord);
				if (state == GoalState.Clear)
				{
					GoalMap[coord] = highVal;
				} else {
					GoalMap[coord] = 0.0;
					_edgeSet.Add(coord);
				}
			}
			while (_edgeSet.Count > 0)
			{
				foreach (var coord in _edgeSet.ToArray())
				{
					var current = GoalMap[coord].Value;
					foreach (var openPoint in AdjacencyRule.EIGHT_WAY.Neighbors(coord))
					{
						if (_closedSet.Contains(openPoint) || !_walkable.Contains(openPoint))
							continue;
						var neighborValue = GoalMap[openPoint].Value;
						var newValue = current + Distance.EUCLIDEAN.DistanceBetween(coord, openPoint);
						if (newValue < neighborValue)
						{
							GoalMap[openPoint] = newValue;
							_edgeSet.Add(openPoint);
						}
					}
					_edgeSet.Remove(coord);
					_closedSet.Add(coord);
				}
			}
			return _closedSet.Count > 0;
		}
	}
}
