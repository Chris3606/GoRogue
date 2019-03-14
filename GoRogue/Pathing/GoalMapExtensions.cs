using GoRogue.MapViews;

namespace GoRogue.Pathing
{
	/// <summary>
	/// Contains extensions for <see cref="IMapView{Double}"/>, that pertain generally to goal maps.
	/// </summary>
	public static class GoalMapExtensions
	{
		/// <summary>
		/// Gets the direction of the neighbor with the minimum goal-map value from the given position.
		/// </summary>
		/// <param name="goalMap"/>
		/// <param name="position">The position to get the minimum value for.</param>
		/// <param name="adjacencyRule">The adjacency rule to use to determine neighbors.</param>
		/// <returns>
		/// The direction that has the minimum value in the goal-map, or <see cref="Direction.NONE"/> if the
		/// neighbors are all obstacles.
		/// </returns>
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

		/// <summary>
		/// Gets the direction of the neighbor with the minimum goal-map value from the given position.
		/// </summary>
		/// <param name="goalMap"/>
		/// <param name="positionX">The x-value of the position to get the minimum value for.</param>
		/// <param name="positionY">The y-value of the position to get the minimum value for.</param>
		/// <param name="adjacencyRule">The adjacency rule to use to determine neighbors.</param>
		/// <returns>
		/// The direction that has the minimum value in the goal-map, or <see cref="Direction.NONE"/> if the
		/// neighbors are all obstacles.
		/// </returns>
		public static Direction GetDirectionOfMinValue(this IMapView<double?> goalMap, int positionX, int positionY, AdjacencyRule adjacencyRule)
			=> goalMap.GetDirectionOfMinValue(new Coord(positionX, positionY), adjacencyRule);
	}
}