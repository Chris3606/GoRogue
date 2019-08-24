using System;
using DrawingPoint = System.Drawing.Point;
#if ALLCONVERSIONS
using MonoPoint = Microsoft.Xna.Framework.Point;
#endif

namespace GoRogue
{
	/// <summary>
	/// Represents the concept of a "direction" on a grid, and "defines" the coordinate plane GoRogue
	/// uses via the <see cref="Direction.YIncreasesUpward"/> flag. Interacts with Coord and other
	/// supported library's equivalent types to allow easy translation of positions in a direction, and
	/// contains numerous helper functions for retrieving directions in various orders, getting direction
	/// closest to a line, etc.
	/// </summary>
	/// <remarks>
	/// The static <see cref="Direction.YIncreasesUpward"/> flag defines the way that many GoRogue algorithms
	/// interpret the coordinate plane.  By default, this flag is false, meaning that the y-value of positions
	/// is assumed to DECREASE as you proceed in the direction defined by <see cref="Direction.UP"/>, and
	/// increase as you go downward.  If the coordinate plane is displayed on the screen, the origin would be
	/// the top left corner.  This default setting matches the typical console/computer graphic definition of the
	/// coordinate plane.  Setting the flag to true inverts this, so that the y-value of positions INCREASES
	/// as you proceed in the direction defined by <see cref="Direction.UP"/>.  This places the origin in the bottom
	/// left corner, and matches a typical mathmatical definition of a euclidean coordinate plane, as well as the scene
	/// coordinate plane defined by Unity and other game engines.
	/// </remarks>
	[Serializable]
	public class Direction : IEquatable<Direction>
	{
		[NonSerialized]
		private static readonly Direction[] directions;

		[NonSerialized]
		private static readonly string[] writeVals = Enum.GetNames(typeof(Types));

		[NonSerialized]
		private static bool _yIncreasesUpward;

		// Used to optimize calcs for a function later on
		[NonSerialized]
		private static Direction[] directionSides = new Direction[2];

		[NonSerialized]
		private static bool initYInc;

		static Direction()
		{
			directions = new Direction[9];
			LEFT = new Direction(-1, 0, Types.LEFT);
			RIGHT = new Direction(1, 0, Types.RIGHT);
			NONE = new Direction(0, 0, Types.NONE);
			initYInc = false;
			YIncreasesUpward = false; // Initializes rest of distance values
		}

		/// <summary>
		/// Compares the current Direction to the object given.
		/// </summary>
		/// <param name="obj"/>
		/// <returns>True if the given object is a Direction with the same Type/dx/dy values, false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Direction e)
				return Equals(e);

			return false;
		}

		/// <summary>
		/// Compares the current Direction to the one given.
		/// </summary>
		/// <param name="other"/>
		/// <returns>True if the given Direction has the same type and delta-y/delta-x values, false otherwise.</returns>
		public bool Equals(Direction other) => !ReferenceEquals(other, null) && Type == other.Type && DeltaX == other.DeltaX && DeltaY == other.DeltaY;

		/// <summary>
		/// Returns a hash-value for this object.
		/// </summary>
		/// <returns/>
		public override int GetHashCode() => Type.GetHashCode();

		/// <summary>
		/// Compares the two Direction instances.
		/// </summary>
		/// <param name="lhs"/>
		/// <param name="rhs"/>
		/// <returns>True if the two given Direction instances have the same Type and delta values, false otherwise.</returns>
		public static bool operator ==(Direction lhs, Direction rhs) => lhs?.Equals(rhs) ?? rhs is null;

		/// <summary>
		/// Compares the two BoundedRectangle instances.
		/// </summary>
		/// <param name="lhs"/>
		/// <param name="rhs"/>
		/// <returns>True if the two given Direction instances do NOT have the same Type and delta values, false otherwise.</returns>
		public static bool operator !=(Direction lhs, Direction rhs) => !(lhs == rhs);

		private Direction(int dx, int dy, Types type)
		{
			DeltaX = dx;
			DeltaY = dy;
			this.Type = type;
		}

		/// <summary>
		/// Enum representing Direction types. Each Direction instance has a <see cref="Type"/> field
		/// which contains the corresponding value from this enum.  Useful for easy mapping of Direction
		/// types to a primitive type (for cases like a switch statement).
		/// </summary>
		public enum Types
		{
			/// <summary>
			/// Type for <see cref="Direction.UP"/>.
			/// </summary>
			UP,

			/// <summary>
			/// Type for <see cref="Direction.UP_RIGHT"/>.
			/// </summary>
			UP_RIGHT,

			/// <summary>
			/// Type for <see cref="Direction.RIGHT"/>.
			/// </summary>
			RIGHT,

			/// <summary>
			/// Type for <see cref="Direction.DOWN_RIGHT"/>.
			/// </summary>
			DOWN_RIGHT,

			/// <summary>
			/// Type for <see cref="Direction.DOWN"/>.
			/// </summary>
			DOWN,

			/// <summary>
			/// Type for <see cref="Direction.DOWN_LEFT"/>.
			/// </summary>
			DOWN_LEFT,

			/// <summary>
			/// Type for <see cref="Direction.LEFT"/>.
			/// </summary>
			LEFT,

			/// <summary>
			/// Type for <see cref="Direction.UP_LEFT"/>.
			/// </summary>
			UP_LEFT,

			/// <summary>
			/// Type for <see cref="Direction.NONE"/>.
			/// </summary>
			NONE
		};

		/// <summary>
		/// Down direction.
		/// </summary>
		public static Direction DOWN
		{
			get => directions[(int)Types.DOWN];
			private set => directions[(int)Types.DOWN] = value;
		}

		/// <summary>
		/// Down-left direction.
		/// </summary>
		public static Direction DOWN_LEFT
		{
			get => directions[(int)Types.DOWN_LEFT];
			private set => directions[(int)Types.DOWN_LEFT] = value;
		}

		/// <summary>
		/// Down-right direction.
		/// </summary>
		public static Direction DOWN_RIGHT
		{
			get => directions[(int)Types.DOWN_RIGHT];
			private set => directions[(int)Types.DOWN_RIGHT] = value;
		}

		/// <summary>
		/// Left direction.
		/// </summary>
		public static Direction LEFT
		{
			get => directions[(int)Types.LEFT];
			private set => directions[(int)Types.LEFT] = value;
		}

		/// <summary>
		/// No direction.
		/// </summary>
		public static Direction NONE
		{
			get => directions[(int)Types.NONE];
			private set => directions[(int)Types.NONE] = value;
		}

		/// <summary>
		/// Right direction.
		/// </summary>
		public static Direction RIGHT
		{
			get => directions[(int)Types.RIGHT];
			private set => directions[(int)Types.RIGHT] = value;
		}

		/// <summary>
		/// Up direction.
		/// </summary>
		public static Direction UP
		{
			get => directions[(int)Types.UP];
			private set => directions[(int)Types.UP] = value;
		}

		/// <summary>
		/// Up-left direction.
		/// </summary>
		public static Direction UP_LEFT
		{
			get => directions[(int)Types.UP_LEFT];
			private set => directions[(int)Types.UP_LEFT] = value;
		}

		/// <summary>
		/// Up-right direction.
		/// </summary>
		public static Direction UP_RIGHT
		{
			get => directions[(int)Types.UP_RIGHT];
			private set => directions[(int)Types.UP_RIGHT] = value;
		}

		/// <summary>
		/// Whether or not a positive y-value indicates an upward change. If true, Directions with an
		/// upwards component represent a positive change in y-value, and ones with downward components
		/// represent a negative change in y-value.  Setting this to false (which is the default) inverts
		/// this.
		/// </summary>
		public static bool YIncreasesUpward
		{
			get { return _yIncreasesUpward; }
			set
			{
				if (_yIncreasesUpward != value || !initYInc)
				{
					initYInc = true;
					_yIncreasesUpward = value;
					yMult = (_yIncreasesUpward) ? -1 : 1;

					UP = new Direction(0, -1 * yMult, Types.UP);
					DOWN = new Direction(0, 1 * yMult, Types.DOWN);
					UP_LEFT = new Direction(-1, -1 * yMult, Types.UP_LEFT);
					UP_RIGHT = new Direction(1, -1 * yMult, Types.UP_RIGHT);
					DOWN_LEFT = new Direction(-1, 1 * yMult, Types.DOWN_LEFT);
					DOWN_RIGHT = new Direction(1, 1 * yMult, Types.DOWN_RIGHT);
				}
			}
		}

		/// <summary>
		/// Change in x-value represented by this direction.
		/// </summary>
		public readonly int DeltaX;

		/// <summary>
		/// Change in y-value represented by this direction.
		/// </summary>
		public readonly int DeltaY;

		/// <summary>
		/// Enum type corresponding to direction being represented.
		/// </summary>
		public readonly Types Type;

		internal static int yMult { get; private set; }

		/// <summary>
		/// Returns the cardinal direction that most closely matches the degree heading of the given
		/// line. Rounds clockwise if the heading is exactly on a diagonal direction. Similar to
		/// <see cref="GetDirection(Coord, Coord)"/>, except this function returns only cardinal directions.
		/// </summary>
		/// <param name="start">Starting coordinate of the line.</param>
		/// <param name="end">Ending coordinate of the line.</param>
		/// <returns>
		/// The cardinal direction that most closely matches the heading indicated by the given line.
		/// </returns>
		public static Direction GetCardinalDirection(Coord start, Coord end) => GetCardinalDirection(end.X - start.X, end.Y - start.Y);

		/// <summary>
		/// Returns the cardinal direction that most closely matches the degree heading of the given
		/// line. Rounds clockwise if the heading is exactly on a diagonal direction. Similar to
		/// <see cref="GetDirection(int, int, int, int)"/>, except this function returns only cardinal directions.
		/// </summary>
		/// <param name="startX">X-coordinate of the starting position of the line.</param>
		/// <param name="startY">Y-coordinate of the starting position of the line.</param>
		/// <param name="endX">X-coordinate of the ending position of the line.</param>
		/// <param name="endY">Y-coordinate of the ending position of the line.</param>
		/// <returns>
		/// The cardinal direction that most closely matches the heading indicated by the given line.
		/// </returns>
		public static Direction GetCardinalDirection(int startX, int startY, int endX, int endY) => GetCardinalDirection(endX - startX, endY - startY);

		/// <summary>
		/// Returns the cardinal direction that most closely matches the degree heading of a line
		/// with the given delta-change values. Rounds clockwise if exactly on a diagonal. Similar to
		/// <see cref="GetDirection(Coord)"/>, except this function returns only cardinal directions.
		/// </summary>
		/// <param name="deltaChange">
		/// Vector representing the change in x and change in y across the line (deltaChange.X is the
		/// change in x, deltaChange.Y is the change in y).
		/// </param>
		/// <returns>
		/// The cardinal direction that most closely matches the degree heading of the given line.
		/// </returns>
		public static Direction GetCardinalDirection(Coord deltaChange) => GetCardinalDirection(deltaChange.X, deltaChange.Y);

		/// <summary>
		/// Returns the cardinal direction that most closely matches the degree heading of a line
		/// with the given dx and dy values. Rounds clockwise if exactly on a diagonal direction.
		/// Similar to <see cref="GetDirection(int, int)"/>, except this function returns only cardinal directions.
		/// </summary>
		/// <param name="dx">The change in x-values across the line.</param>
		/// <param name="dy">The change in x-values across the line.</param>
		/// <returns>
		/// The cardinal direction that most closely matches the degree heading of the given line.
		/// </returns>
		public static Direction GetCardinalDirection(int dx, int dy)
		{
			if (dx == 0 && dy == 0)
				return NONE;

			dy *= yMult;

			double angle = Math.Atan2(dy, dx);
			double degree = MathHelpers.ToDegree(angle);
			degree += 450; // Rotate angle such that it's all positives, and such that 0 is up.
			degree %= 360; // Normalize angle to 0-360

			if (degree < 45.0)
				return UP;
			if (degree < 135.0)
				return RIGHT;
			if (degree < 225.0)
				return DOWN;
			if (degree < 315.0)
				return LEFT;

			return UP;
		}

		/// <summary>
		/// Returns the direction that most closely matches the degree heading of the given line.
		/// Rounds clockwise if the heading is exactly between two directions.
		/// </summary>
		/// <param name="start">Starting coordinate of the line.</param>
		/// <param name="end">Ending coordinate of the line.</param>
		/// <returns>
		/// The direction that most closely matches the heading indicated by the given line.
		/// </returns>
		public static Direction GetDirection(Coord start, Coord end) => GetDirection(end.X - start.X, end.Y - start.Y);

		/// <summary>
		/// Returns the direction that most closely matches the degree heading of the given line.
		/// Rounds clockwise if the heading is exactly between two directions.
		/// </summary>
		/// <param name="startX">X-coordinate of the starting position of the line.</param>
		/// <param name="startY">Y-coordinate of the starting position of the line.</param>
		/// <param name="endX">X-coordinate of the ending position of the line.</param>
		/// <param name="endY">Y-coordinate of the ending position of the line.</param>
		/// <returns>
		/// The direction that most closely matches the heading indicated by the given line.
		/// </returns>
		public static Direction GetDirection(int startX, int startY, int endX, int endY) => GetDirection(endX - startX, endY - startY);

		/// <summary>
		/// Returns the direction that most closely matches the degree heading of a line with the
		/// given delta-change values. Rounds clockwise if the heading is exactly between two directions.
		/// </summary>
		/// <param name="deltaChange">
		/// Vector representing the change in x and change in y across the line (deltaChange.X is the
		/// change in x, deltaChange.Y is the change in y).
		/// </param>
		/// <returns>
		/// The direction that most closely matches the heading indicated by the given input.
		/// </returns>
		public static Direction GetDirection(Coord deltaChange) => GetDirection(deltaChange.X, deltaChange.Y);

		/// <summary>
		/// Returns the direction that most closely matches the degree heading of a line with the
		/// given dx and dy values. Rounds clockwise if the heading is exactly between two directions.
		/// </summary>
		/// <param name="dx">The change in x-values across the line.</param>
		/// <param name="dy">The change in y-values across the line.</param>
		/// <returns>
		/// The direction that most closely matches the heading indicated by the given input.
		/// </returns>
		public static Direction GetDirection(int dx, int dy)
		{
			if (dx == 0 && dy == 0)
				return NONE;

			dy *= yMult;

			double angle = Math.Atan2(dy, dx);
			double degree = MathHelpers.ToDegree(angle);
			degree += 450; // Rotate angle such that it's all positives, and such that 0 is up.
			degree %= 360; // Normalize angle to 0-360

			if (degree < 22.5)
				return UP;
			if (degree < 67.5)
				return UP_RIGHT;
			if (degree < 112.5)
				return RIGHT;
			if (degree < 157.5)
				return DOWN_RIGHT;
			if (degree < 202.5)
				return DOWN;
			if (degree < 247.5)
				return DOWN_LEFT;
			if (degree < 292.5)
				return LEFT;
			if (degree < 337.5)
				return UP_LEFT;

			return UP;
		}
		
		/* TODO: Resume Documentation proofing here */
		
		/// <summary>
		/// Moves the direction counter-clockwise <paramref name="i"/> times.
		/// </summary>
		/// <param name="d"/>
		/// <param name="i"/>
		/// <returns>
		/// The given direction moved counter-clockwise <paramref name="i"/> times.
		/// </returns>
		public static Direction operator -(Direction d, int i) => (d == NONE) ? NONE : directions[MathHelpers.WrapAround((int)d.Type - i, 8)];

		/// <summary>
		/// Moves the direction counter-clockwise by one.
		/// </summary>
		/// <param name="d"/>
		/// <returns>The direction one unit counterclockwise of <paramref name="d"/>.</returns>
		public static Direction operator --(Direction d) => (d == NONE) ? NONE : directions[MathHelpers.WrapAround((int)d.Type - 1, 8)];

		/// <summary>
		/// Moves the direction clockwise <paramref name="i"/> times.
		/// </summary>
		/// <param name="d"/>
		/// <param name="i"/>
		/// <returns>
		/// The given direction moved clockwise <paramref name="i"/> times.
		/// </returns>
		public static Direction operator +(Direction d, int i) => (d == NONE) ? NONE : directions[MathHelpers.WrapAround((int)d.Type + i, 8)];

		/// <summary>
		/// Moves the direction clockwise by one.
		/// </summary>
		/// <param name="d"/>
		/// <returns>The direction one unit clockwise of <paramref name="d"/>.</returns>
		public static Direction operator ++(Direction d) => (d == NONE) ? NONE : directions[MathHelpers.WrapAround((int)d.Type + 1, 8)];

		/// <summary>
		/// Gets the Direction class instance representing the direction type specified.
		/// </summary>
		/// <param name="directionType">The enum value for the direction.</param>
		/// <returns>The direction class representing the given direction.</returns>
		public static Direction ToDirection(Types directionType)
		{
			switch (directionType)
			{
				case Types.UP:
					return UP;

				case Types.UP_RIGHT:
					return UP_RIGHT;

				case Types.RIGHT:
					return RIGHT;

				case Types.DOWN_RIGHT:
					return DOWN_RIGHT;

				case Types.DOWN:
					return DOWN;

				case Types.DOWN_LEFT:
					return DOWN_LEFT;

				case Types.LEFT:
					return LEFT;

				case Types.UP_LEFT:
					return UP_LEFT;

				case Types.NONE:
					return NONE;

				default:
					return null; // Will not occur
			}
		}

		/// <summary>
		/// Writes the string (eg. "UP", "UP_RIGHT", etc.) for the direction.
		/// </summary>
		/// <returns>String representation of the direction.</returns>
		public override string ToString() => writeVals[(int)Type];

		#if ALLCONVERSIONS
		#region MonoGame Compatibility
		/// <summary>
		/// Translates the given position by one unit in the given direction.
		/// </summary>
		/// <param name="p"/>
		/// <param name="d"/>
		/// <returns>
		/// Position (p.X + d.DeltaX, p.Y + d.DeltaY).
		/// </returns>
		public static MonoPoint operator +(MonoPoint p, Direction d) => new MonoPoint(p.X + d.DeltaX, p.Y + d.DeltaY);
		#endregion
		#endif

		#region System.Drawing Compatibility
		/// <summary>
		/// Translates the given position by one unit in the given direction.
		/// </summary>
		/// <param name="p"/>
		/// <param name="d"/>
		/// <returns>
		/// Position (p.X + d.DeltaX, p.Y + d.DeltaY).
		/// </returns>
		public static DrawingPoint operator +(DrawingPoint p, Direction d) => new DrawingPoint(p.X + d.DeltaX, p.Y + d.DeltaY);
		#endregion
		
		#region Tuple Compatibility
		/// <summary>
		/// Translates the given position by one unit in the given direction.
		/// </summary>
		/// <param name="tuple"/>
		/// <param name="d"/>
		/// <returns>
		/// Tuple (tuple.y + d.DeltaX, tuple.y + d.DeltaY).
		/// </returns>
		public static (int x, int y) operator +((int x, int y) tuple, Direction d) => (tuple.x + d.DeltaX, tuple.y + d.DeltaY);
		#endregion
	}
}
