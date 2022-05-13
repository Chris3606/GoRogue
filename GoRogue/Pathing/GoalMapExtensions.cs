using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Pathing
{
    /// <summary>
    /// Contains extensions for <see cref="SadRogue.Primitives.GridViews.IGridView{T}" />, that pertain generally to goal maps.
    /// </summary>
    [PublicAPI]
    public static class GoalMapExtensions
    {
        /// <summary>
        /// Gets the direction of the neighbor with the minimum goal-map value from the given position.
        /// </summary>
        /// <param name="goalMap" />
        /// <param name="position">The position to get the minimum value for.</param>
        /// <param name="adjacencyRule">The adjacency rule to use to determine neighbors.</param>
        /// <returns>
        /// The direction that has the minimum value in the goal-map, or <see cref="SadRogue.Primitives.Direction.None" /> if the
        /// neighbors are all obstacles.
        /// </returns>
        public static Direction GetDirectionOfMinValue(this IGridView<double?> goalMap, Point position,
                                                       AdjacencyRule adjacencyRule)
        {
            var min = goalMap[position].HasValue ? goalMap[position]!.Value : double.MaxValue;
            var minDir = Direction.None;

            for (int i = 0; i < adjacencyRule.DirectionsOfNeighborsCache.Length; i++)
            {
                var newPosition = position + adjacencyRule.DirectionsOfNeighborsCache[i];

                if (!goalMap.Contains(newPosition) || !goalMap[newPosition].HasValue)
                    continue;

                // <= to prefer movement over non movement; known to be not null thanks to above continue
                if (goalMap[newPosition]!.Value <= min)
                {
                    min = goalMap[newPosition]!.Value; // Again known to be not null thanks to above continue
                    minDir = adjacencyRule.DirectionsOfNeighborsCache[i];
                }
            }

            return minDir; // Direction.None if all obstacles
        }

        /// <summary>
        /// Gets the direction of the neighbor with the minimum goal-map value from the given position.
        /// </summary>
        /// <param name="goalMap" />
        /// <param name="positionX">The x-value of the position to get the minimum value for.</param>
        /// <param name="positionY">The y-value of the position to get the minimum value for.</param>
        /// <param name="adjacencyRule">The adjacency rule to use to determine neighbors.</param>
        /// <returns>
        /// The direction that has the minimum value in the goal-map, or <see cref="SadRogue.Primitives.Direction.None" /> if the
        /// neighbors are all obstacles.
        /// </returns>
        public static Direction GetDirectionOfMinValue(this IGridView<double?> goalMap, int positionX, int positionY,
                                                       AdjacencyRule adjacencyRule)
            => goalMap.GetDirectionOfMinValue(new Point(positionX, positionY), adjacencyRule);
    }
}
