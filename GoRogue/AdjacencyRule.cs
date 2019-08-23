using System;
using System.Collections.Generic;

namespace GoRogue
{
	/// <summary>
	/// Class representing a method for determining which coordinates are adjacent to a given
	/// coordinate, and which directions those neighbors are in. Cannot be instantiated -- premade
	/// static instances are provided.
	/// </summary>
	[Serializable]
	public class AdjacencyRule : IEquatable<AdjacencyRule>
	{
		/// <summary>
		/// Represents method of determining adjacency where neighbors are considered adjacent if
		/// they are in a cardinal direction, eg. 4-way (manhattan-based) connectivity.
		/// </summary>
		[NonSerialized]
		public static readonly AdjacencyRule CARDINALS = new AdjacencyRule(Types.CARDINALS);

		/// <summary>
		/// Represents method of determining adjacency where neighbors are considered adjacent only
		/// if they are in a diagonal direction.
		/// </summary>
		[NonSerialized]
		public static readonly AdjacencyRule DIAGONALS = new AdjacencyRule(Types.DIAGONALS);

		/// <summary>
		/// Represents method of determining adjacency where all 8 possible neighbors are considered
		/// adjacent (eg. 8-way connectivity).
		/// </summary>
		[NonSerialized]
		public static readonly AdjacencyRule EIGHT_WAY = new AdjacencyRule(Types.EIGHT_WAY);

		[NonSerialized]
		private static readonly string[] writeVals = Enum.GetNames(typeof(Types));

		// Constructor, takes type.
		private AdjacencyRule(Types type)
		{
			Type = type;
		}

		/// <summary>
		/// Enum representing AdjacencyRule types. Each AdjacencyRule instance has a <see cref="Type"/> field
		/// which contains the corresponding value from this enum.  Useful for easy mapping of AdjacencyRule
		/// types to a primitive type (for cases like a switch statement).
		/// </summary>
		public enum Types
		{
			/// <summary>
			/// Type for <see cref="AdjacencyRule.CARDINALS"/>.
			/// </summary>
			CARDINALS,

			/// <summary>
			/// Type for <see cref="AdjacencyRule.DIAGONALS"/>.
			/// </summary>
			DIAGONALS,

			/// <summary>
			/// Type for <see cref="AdjacencyRule.EIGHT_WAY"/>.
			/// </summary>
			EIGHT_WAY
		}

		/// <summary>
		/// Enum value representing the method of determining adjacency -- useful for using
		/// AdjacencyRule types in switch statements.
		/// </summary>
		public readonly Types Type;

		/// <summary>
		/// Gets the AdjacencyRule class instance representing the adjacency type specified.
		/// </summary>
		/// <param name="adjacencyRuleType">The enum value for the adjacency method.</param>
		/// <returns>The AdjacencyRule class representing the given adjacency method type.</returns>
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
		/// Gets directions leading to neighboring locations, according to the current adjacency
		/// method. Cardinals are returned before any diagonals.
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
		/// Gets directions leading to neighboring locations, according to the current adjacency
		/// method. Appropriate directions are returned in clockwise order from the given starting
		/// direction.
		/// </summary>
		/// <param name="startingDirection">The direction to start with.  null or <see cref="Direction.NONE"/>
		/// causes the default starting direction to be used, which is UP for CARDINALS/EIGHT_WAY, and UP_RIGHT
		/// for diagonals.</param>
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
		/// Gets directions leading to neighboring locations, according to the current adjacency
		/// method. Appropriate directions are returned in counter-clockwise order from the given
		/// starting direction.
		/// </summary>
		/// <param name="startingDirection">The direction to start with.  null or <see cref="Direction.NONE"/>
		/// causes the default starting direction to be used, which is UP for CARDINALS/EIGHT_WAY, and UP_LEFT
		/// for diagonals.</param>
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
		public IEnumerable<Coord> Neighbors(int startingX, int startingY) => Neighbors(new Coord(startingX, startingY));

		/// <summary>
		/// Gets all neighbors of the specified location, based on the current adjacency method.
		/// Neighbors are returned in clockwise order, starting with the neighbor in the given
		/// starting direction.
		/// </summary>
		/// <param name="startingLocation">Location to return neighbors for.</param>
		/// <param name="startingDirection">
		/// The neighbor in this direction will be returned first, proceeding clockwise.
		/// If null or <see cref="Direction.NONE"/> is specified, the default starting direction
		/// is used, which is UP for CARDINALS/EIGHT_WAY, and UP_RIGHT for DIAGONALS.
		/// </param>
		/// <returns>All neighbors of the given location.</returns>
		public IEnumerable<Coord> NeighborsClockwise(Coord startingLocation, Direction startingDirection = null)
		{
			foreach (var dir in DirectionsOfNeighborsClockwise(startingDirection))
				yield return startingLocation + dir;
		}

		/// <summary>
		/// Gets all neighbors of the specified location, based on the current adjacency method.
		/// Neighbors are returned in clockwise order, starting with the neighbor in the given
		/// starting direction.
		/// </summary>
		/// <param name="startingX">X-coordinate of location to return neighbors for.</param>
		/// <param name="startingY">Y-coordinate of location to return neighbors for.</param>
		/// <param name="startingDirection">
		/// The neighbor in this direction will be returned first, proceeding clockwise.
		/// If null or <see cref="Direction.NONE"/> is specified, the default starting direction
		/// is used, which is UP for CARDINALS/EIGHT_WAY, and UP_RIGHT for DIAGONALS.
		/// </param>
		/// <returns>All neighbors of the given location.</returns>
		public IEnumerable<Coord> NeighborsClockwise(int startingX, int startingY, Direction startingDirection = null)
			=> NeighborsClockwise(new Coord(startingX, startingY), startingDirection);

		/// <summary>
		/// Gets all neighbors of the specified location, based on the current adjacency method.
		/// Neighbors are returned in counter-clockwise order, starting with the neighbor in the given
		/// starting direction.
		/// </summary>
		/// <param name="startingLocation">Location to return neighbors for.</param>
		/// <param name="startingDirection">
		/// The neighbor in this direction will be returned first, proceeding counter-clockwise.
		/// If null or <see cref="Direction.NONE"/> is specified, the default starting direction
		/// is used, which is UP for CARDINALS/EIGHT_WAY, and UP_LEFT for DIAGONALS.
		/// </param>
		/// <returns>All neighbors of the given location.</returns>
		public IEnumerable<Coord> NeighborsCounterClockwise(Coord startingLocation, Direction startingDirection = null)
		{
			foreach (var dir in DirectionsOfNeighborsCounterClockwise(startingDirection))
				yield return startingLocation + dir;
		}

		/// <summary>
		/// Gets all neighbors of the specified location, based on the current adjacency method.
		/// Neighbors are returned in counter-clockwise order, starting with the neighbor in the given
		/// starting direction.
		/// </summary>
		/// <param name="startingX">X-coordinate of location to return neighbors for.</param>
		/// <param name="startingY">Y-coordinate of location to return neighbors for.</param>
		/// <param name="startingDirection">
		/// The neighbor in this direction will be returned first, proceeding counter-clockwise.
		/// If null or <see cref="Direction.NONE"/> is specified, the default starting direction
		/// is used, which is UP for CARDINALS/EIGHT_WAY, and UP_LEFT for DIAGONALS.
		/// </param>
		/// <returns>All neighbors of the given location.</returns>
		public IEnumerable<Coord> NeighborsCounterClockwise(int startingX, int startingY, Direction startingDirection = null)
			=> NeighborsCounterClockwise(new Coord(startingX, startingY), startingDirection);

		/// <summary>
		/// Returns a string representation of the AdjacencyRule.
		/// </summary>
		/// <returns>A string representation of the AdjacencyRule.</returns>
		public override string ToString() => writeVals[(int)Type];

		/// <summary>
		/// Compares the current AdjacencyRule to the object given.
		/// </summary>
		/// <param name="obj"/>
		/// <returns>True if the given object is an AdjacencyRule with the same Type, false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj is AdjacencyRule e)
				return Equals(e);

			return false;
		}

		/// <summary>
		/// Compares the current AdjacencyRule to the one given.
		/// </summary>
		/// <param name="other"/>
		/// <returns>True if the given AdjacencyRule has the same Type, false otherwise.</returns>
		public bool Equals(AdjacencyRule other) => !ReferenceEquals(other, null) && Type == other.Type;

		/// <summary>
		/// Returns a hash-value for this object.
		/// </summary>
		/// <returns/>
		public override int GetHashCode() => Type.GetHashCode();

		/// <summary>
		/// Compares the two AdjacencyRule instances.
		/// </summary>
		/// <param name="lhs"/>
		/// <param name="rhs"/>
		/// <returns>True if the two given AdjacencyRule instances have the same Type, false otherwise.</returns>
		public static bool operator ==(AdjacencyRule lhs, AdjacencyRule rhs) => lhs?.Equals(rhs) ?? rhs is null;

		/// <summary>
		/// Compares the two AdjacencyRule instances.
		/// </summary>
		/// <param name="lhs"/>
		/// <param name="rhs"/>
		/// <returns>True if the two given AdjacencyRule instances do NOT have the same Type, false otherwise.</returns>
		public static bool operator !=(AdjacencyRule lhs, AdjacencyRule rhs) => !(lhs == rhs);
	}
}
