using System.Collections.Generic;

namespace GoRogue
{
    /// <summary>
    /// Class representing a method for determining which coordinates are adjacent to a given coordinate,
    /// and in which directions those neighbors are.  Cannot be instantiated -- premade static instances
    /// are provided.
    /// </summary>
    public class AdjacencyRule
    {
        /// <summary>
        /// Enum representing AdjacencyRule types. Useful for easy mapping of AdjacencyRule types to a primitive type (for
        /// cases like a switch statement).
        /// </summary>
        public enum Types
        {
            /// <summary>
            /// Type for AdjacencyRule.CARDINALS.
            /// </summary>
            CARDINALS,
            /// <summary>
            /// Type for AdjacencyRule.DIAGONALS.
            /// </summary>
            DIAGONALS,
            /// <summary>
            /// Type for AdjacencyRule.EIGHT_WAY.
            /// </summary>
            EIGHT_WAY
        }
        /// <summary>
        /// Represents method of determining adjacency where neighbors are considered adjacent if they are
        /// in a cardinal direction, eg. 4-way (manhattan-based) connectivity.
        /// </summary>
        public static readonly AdjacencyRule CARDINALS = new AdjacencyRule(Types.CARDINALS);
        /// <summary>
        /// Represents method of determining adjacency where neighbors are considered adjacent if they are
        /// in a diagonal direction.
        /// </summary>
        public static readonly AdjacencyRule DIAGONALS = new AdjacencyRule(Types.DIAGONALS);
        /// <summary>
        /// Represents method of determining adjacency where all 8 possible neighbors are considered adjacent
        /// (eg. 8-way connectivity).
        /// </summary>
        public static readonly AdjacencyRule EIGHT_WAY = new AdjacencyRule(Types.EIGHT_WAY);
        
        /// <summary>
        /// Enum value representing the method of determining adjacency -- useful for using AdjacencyRule types
        /// in switch statements.
        /// </summary>
        public Types Type { get; private set; }

        // Constructor, takes type.
        private AdjacencyRule(Types type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets the AdjacencyRule class instance representing the adjacency type specified.
        /// </summary>
        /// <param name="adjacencyRuleType">
        /// The enum value for the adjacency method.
        /// </param>
        /// <returns>
        /// The AdjacencyRule class representing the given adjacency method type.
        /// </returns>
        public static AdjacencyRule ToAdjacencyRule(Types adjacencyRuleType)
        {
            switch (adjacencyRuleType)
            {
                case Types.CARDINALS:
                    return CARDINALS;
                case Types.DIAGONALS:
                    return DIAGONALS;
                case Types.EIGHT_WAY:
                    return EIGHT_WAY;
                default:
                    return null; // Will not occur
            }
        }

        /// <summary>
        /// Gets directions leading to neighboring locations, according to the current adjacency method.
        /// Cardinals are returned before any diagonals.
        /// </summary>
        /// <returns>Directions that lead to neighboring locations.</returns>
        public IEnumerable<Direction> DirectionsOfNeighbors()
        {
            switch (Type)
            {
                case Types.CARDINALS:
                    yield return Direction.UP;
                    yield return Direction.DOWN;
                    yield return Direction.LEFT;
                    yield return Direction.RIGHT;
                    break;
                case Types.DIAGONALS:
                    yield return Direction.UP_LEFT;
                    yield return Direction.UP_RIGHT;
                    yield return Direction.DOWN_LEFT;
                    yield return Direction.DOWN_RIGHT;
                    break;
                case Types.EIGHT_WAY:
                    yield return Direction.UP;
                    yield return Direction.DOWN;
                    yield return Direction.LEFT;
                    yield return Direction.RIGHT;
                    yield return Direction.UP_LEFT;
                    yield return Direction.UP_RIGHT;
                    yield return Direction.DOWN_LEFT;
                    yield return Direction.DOWN_RIGHT;
                    break;
            }
        }

        /// <summary>
        /// Gets directions leading to neighboring locations, according to the current adjacency method.
        /// Appropriate directions are returned in clockwise order from the given direction. Null or Direction.NONE
        /// causes the default to be used (UP for CARDINALS or EIGHT_WAY, and UP_RIGHT for DIAGONALS).
        /// </summary>
        /// <param name="startingDirection">The direction to start with.</param>
        /// <returns>Directions that lead to neighboring locations.</returns>
        public IEnumerable<Direction> DirectionsOfNeighborsClockwise(Direction startingDirection = null)
        {
            switch (Type)
            {
                case Types.CARDINALS:
                    if (startingDirection == null || startingDirection == Direction.NONE)
                        startingDirection = Direction.UP;

                    if ((int)startingDirection.Type % 2 == 1)
                        startingDirection++; // Make it a cardinal

                    yield return startingDirection;
                    yield return startingDirection + 2;
                    yield return startingDirection + 4;
                    yield return startingDirection + 6;
                    break;
                case Types.DIAGONALS:
                    if (startingDirection == null || startingDirection == Direction.NONE)
                        startingDirection = Direction.UP_RIGHT;

                    if ((int)startingDirection.Type % 2 == 0)
                        startingDirection++; // Make it a diagonal

                    yield return startingDirection;
                    yield return startingDirection + 2;
                    yield return startingDirection + 4;
                    yield return startingDirection + 6;
                    break;
                case Types.EIGHT_WAY:
                    if (startingDirection == null || startingDirection == Direction.NONE)
                        startingDirection = Direction.UP;

                    for (int i = 1; i <= 8; i++)
                    {
                        yield return startingDirection;
                        startingDirection++;
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets directions leading to neighboring locations, according to the current adjacency method.
        /// Appropriate directions are returned in counter-clockwise order from the given direction. Null
        /// or Direction.NONE causes the default to be used (UP for CARDINALS or EIGHT_WAY, and UP_RIGHT
        /// for DIAGONALS).
        /// </summary>
        /// <param name="startingDirection">The direction to start with.</param>
        /// <returns>Directions that lead to neighboring locations.</returns>
        public IEnumerable<Direction> DirectionsOfNeighborsCounterClockwise(Direction startingDirection = null)
        {
            switch (Type)
            {
                case Types.CARDINALS:
                    if (startingDirection == null || startingDirection == Direction.NONE)
                        startingDirection = Direction.UP;

                    if ((int)startingDirection.Type % 2 == 1)
                        startingDirection--; // Make it a cardinal

                    yield return startingDirection;
                    yield return startingDirection - 2;
                    yield return startingDirection - 4;
                    yield return startingDirection - 6;
                    break;
                case Types.DIAGONALS:
                    if (startingDirection == null || startingDirection == Direction.NONE)
                        startingDirection = Direction.UP_LEFT;

                    if ((int)startingDirection.Type % 2 == 0)
                        startingDirection--; // Make it a diagonal

                    yield return startingDirection;
                    yield return startingDirection - 2;
                    yield return startingDirection - 4;
                    yield return startingDirection - 6;
                    break;
                case Types.EIGHT_WAY:
                    if (startingDirection == null || startingDirection == Direction.NONE)
                        startingDirection = Direction.UP;

                    for (int i = 1; i <= 8; i++)
                    {
                        yield return startingDirection;
                        startingDirection--;
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Cardinals are returned before any diagonals.
        /// </summary>
        /// <param name="startingLocation">Location to return neighbors for.</param>
        /// <returns>All neighbors of the given location.</returns>
        public IEnumerable<Coord> Neighbors(Coord startingLocation)
        {
            foreach (var dir in DirectionsOfNeighbors())
                yield return startingLocation + dir;
        }

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Cardinals are returned before any diagonals.
        /// </summary>
        /// <param name="startingX">X-coordinate of location to return neighbors for.</param>
        /// <param name="startingY">Y-coordinate of location to return neighbors for.</param>
        /// <returns>All neighbors of the given location.</returns>
        public IEnumerable<Coord> Neighbors(int startingX, int startingY) => Neighbors(Coord.Get(startingX, startingY));

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Neighbors are returned in clockwise order from the given direction. Null
        /// or Direction.NONE causes the default to be used (UP for CARDINALS or EIGHT_WAY, and UP_RIGHT
        /// for DIAGONALS).
        /// </summary>
        /// <param name="startingLocation">Location to return neighbors for.</param>
        /// <param name="startingDirection">The neighbor in this direction will be returned first, proceeding clockwise.</param>
        /// <returns>All neighbors of the given location.</returns>
        public IEnumerable<Coord> NeighborsClockwise(Coord startingLocation, Direction startingDirection = null)
        {
            foreach (var dir in DirectionsOfNeighborsClockwise(startingDirection))
                yield return startingLocation + dir;
        }

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Neighbors are returned in clockwise order from the given direction. Null
        /// or Direction.NONE causes the default to be used (UP for CARDINALS or EIGHT_WAY, and UP_RIGHT
        /// for DIAGONALS).
        /// </summary>
        /// <param name="startingX">X-coordinate of location to return neighbors for.</param>
        /// <param name="startingY">Y-coordinate of location to return neighbors for.</param>
        /// <param name="startingDirection">The neighbor in this direction will be returned first, proceeding clockwise.</param>
        /// <returns>All neighbors of the given location.</returns>
        public IEnumerable<Coord> NeighborsClockwise(int startingX, int startingY, Direction startingDirection = null)
            => NeighborsClockwise(Coord.Get(startingX, startingY), startingDirection);

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Neighbors are returned in counter-clockwise order from the given direction. Null
        /// or Direction.NONE causes the default to be used (UP for CARDINALS or EIGHT_WAY, and UP_LEFT
        /// for DIAGONALS).
        /// </summary>
        /// <param name="startingLocation">Location to return neighbors for.</param>
        /// <param name="startingDirection">The neighbor in this direction will be returned first, proceeding counter-clockwise.</param>
        /// <returns>All neighbors of the given location.</returns>
        public IEnumerable<Coord> NeighborsCounterClockwise(Coord startingLocation, Direction startingDirection = null)
        {
            foreach (var dir in DirectionsOfNeighborsCounterClockwise(startingDirection))
                yield return startingLocation + dir;
        }

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Neighbors are returned in counter-clockwise order from the given direction. Null
        /// or Direction.NONE causes the default to be used (UP for CARDINALS or EIGHT_WAY, and UP_LEFT
        /// for DIAGONALS).
        /// </summary>
        /// <param name="startingX">X-coordinate of location to return neighbors for.</param>
        /// <param name="startingY">Y-coordinate of location to return neighbors for.</param>
        /// <param name="startingDirection">The neighbor in this direction will be returned first, proceeding counter-clockwise.</param>
        /// <returns>All neighbors of the given location.</returns>
        public IEnumerable<Coord> NeighborsCounterClockwise(int startingX, int startingY, Direction startingDirection = null)
            => NeighborsCounterClockwise(Coord.Get(startingX, startingY), startingDirection);
    }
}
