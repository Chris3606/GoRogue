using GoRogue.MapViews;

namespace GoRogue.Pathing
{
	/// <summary>
	///  Contains extensions for IMapView of nullable double values, that pertain generally to goal maps.
	/// </summary>
	public static class GoalMapExtensions
	{
		/// <summary>
		/// Gets the direction of the neighbor with the minimum goal-map value from the given position.
		/// </summary>
		/// <param name="goalMap">The IMapView of nullable double values to operate on.  Never specified manually since this is an extension method.</param>
		/// <param name="position">The position to get the minimum value for.</param>
		/// <param name="adjacencyRule">The adjacency rule to use to determine neighbors.</param>
		/// <returns>The direction that has the minimum value in the goal-map, or Direction.NONE if the neighbors are all obstacles.</returns>
		public static Direction GetDirectionOfMinValue(this IMapView<double?> goalMap, Coord position, AdjacencyRule adjacencyRule)
		{
			double min = double.MaxValue;
			Direction minDir = Direction.NONE;

			foreach (var dir in adjacencyRule.DirectionsOfNeighbors())
			{
				Coord newPosition = position + dir;
				if (!goalMap[newPosition].HasValue)
					continue;

				if (goalMap[newPosition].Value < min)
				{
					min = goalMap[newPosition].Value;
					minDir = dir;
				}
			}

			return minDir; // Direction.NONE if all obstacles
		}
	}
}
