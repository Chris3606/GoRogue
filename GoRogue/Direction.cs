using System;

namespace GoRogue
{
    /// <summary>
    /// Utility class to handle Directions on a grid map. The built-in variables hold the dx and dy
    /// for that direction, at unit scale.
    /// </summary>
    /// <remarks>
    /// It also contains functions to provide ways to iterate over the directions in common orders --
    /// for example, it contains a function that returns an Enumerable of all the directions in
    /// clockwise order, as well as a similar function for cardinal directions in a clockwise order,
    /// among many others. It is also possible to invert the y coordinates (where UP is 0, 1 and DOWN
    /// is 0, -1, etc). Direction instances can also be added to Coords to produce the coordinate
    /// directly adjacent in the direction added.
    /// </remarks>
    public class Direction
    {
        private static readonly Direction[] directions;

        private static readonly string[] writeVals = Enum.GetNames(typeof(Types));

        private static bool _yIncreasesUpward;

        // Used to optimize calcs for a function later on
        private static Direction[] directionSides = new Direction[2];

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

        private Direction(int dx, int dy, Types type)
        {
            DeltaX = dx;
            DeltaY = dy;
            this.Type = type;
        }

        /// <summary>
        /// Enum representing Direction types. Useful for easy mapping of Direction types to a
        /// primitive type (for cases like a switch statement).
        /// </summary>
        public enum Types
        {
            /// <summary>
            /// Type for Direction.UP.
            /// </summary>
            UP,

            /// <summary>
            /// Type for Direction.UP_RIGHT.
            /// </summary>
            UP_RIGHT,

            /// <summary>
            /// Type for Direction.RIGHT.
            /// </summary>
            RIGHT,

            /// <summary>
            /// Type for Direction.DOWN_RIGHT.
            /// </summary>
            DOWN_RIGHT,

            /// <summary>
            /// Type for Direction.DOWN.
            /// </summary>
            DOWN,

            /// <summary>
            /// Type for Direction.DOWN_LEFT.
            /// </summary>
            DOWN_LEFT,

            /// <summary>
            /// Type for Direction.LEFT.
            /// </summary>
            LEFT,

            /// <summary>
            /// Type for Direction.UP_LEFT.
            /// </summary>
            UP_LEFT,

            /// <summary>
            /// Type for Direction.NONE.
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
        /// Whether or not a positive y-value indicates an UP motion. If true, Directions with an UP
        /// component will have positive y-values, and ones with DOWN components will have negative
        /// values. Setting this to false (which is the default) inverts this.
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
        /// Delta X for direction. Right is positive, left is negative.
        /// </summary>
        public int DeltaX { get; private set; }

        /// <summary>
        /// Delta Y for direction. Down is positive, up is negative, since this is how graphics
        /// coordinates typically work.
        /// </summary>
        public int DeltaY { get; private set; }

        /// <summary>
        /// Enum type corresponding to Direction being represented.
        /// </summary>
        public Types Type { get; private set; }

        internal static int yMult { get; private set; }

        /// <summary>
        /// Returns the cardinal direction that most closely matches the degree heading of the given line.
        /// Rounds clockwise if the heading is exactly on a diagonal direction.  Similar to GetDirection,
        /// except gives only cardinal directions.
        /// </summary>
        /// <param name="start">Starting coordinate of the line.</param>
        /// <param name="end">Ending coordinate of the line.</param>
        /// <returns>The cardinal direction that most closely matches the heading formed by the given line.</returns>
        public static Direction GetCardinalDirection(Coord start, Coord end) => GetCardinalDirection(end.X - start.X, end.Y - start.Y);

        /// <summary>
        /// Returns the cardinal direction that most closely matches the degree heading of the given line.
        /// Rounds clockwise if the heading is exactly on a diagonal direction.  Similar to GetDirection,
        /// except gives only cardinal directions.
        /// </summary>
        /// <param name="startX">X-coordinate of the starting position of the line.</param>
        /// <param name="startY">Y-coordinate of the starting position of the line.</param>
        /// <param name="endX">X-coordinate of the ending position of the line.</param>
        /// <param name="endY">Y-coordinate of the ending position of the line.</param>
        /// <returns>The cardinal direction that most closely matches the heading formed by the given line.</returns>
        public static Direction GetCardinalDirection(int startX, int startY, int endX, int endY) => GetCardinalDirection(endX - startX, endY - startY);

        /// <summary>
        /// Returns the cardinal direction that most closely matches the degree heading of a line
        /// with the given dx and dy values. Rounds clockwise if exactly on a diagonal. Similar to GetDirection,
        /// except gives only cardinal directions.
        /// </summary>
        /// <param name="deltaChange">Vector representing the change in x and change in y across
        /// the line (deltaChange.X is the change in x, deltaChange.Y is the change in y).</param>
        /// <returns>
        /// The cardinal direction that most closely matches the degree heading of the given line.
        /// </returns>
        public static Direction GetCardinalDirection(Coord deltaChange) => GetCardinalDirection(deltaChange.X, deltaChange.Y);

        /// <summary>
        /// Returns the cardinal direction that most closely matches the degree heading of a line
        /// with the given dx and dy values. Rounds clockwise if exactly on a diagonal direction. Similar to GetDirection,
        /// except gives only cardinal directions.
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
        /// <returns>The direction that most closely matches the heading formed by the given line.</returns>
        public static Direction GetDirection(Coord start, Coord end) => GetDirection(end.X - start.X, end.Y - start.Y);

        /// <summary>
        /// Returns the direction that most closely matches the degree heading of the given line.
        /// Rounds clockwise if the heading is exactly between two directions.
        /// </summary>
        /// <param name="startX">X-coordinate of the starting position of the line.</param>
        /// <param name="startY">Y-coordinate of the starting position of the line.</param>
        /// <param name="endX">X-coordinate of the ending position of the line.</param>
        /// <param name="endY">Y-coordinate of the ending position of the line.</param>
        /// <returns>The direction that most closely matches the heading formed by the given line.</returns>
        public static Direction GetDirection(int startX, int startY, int endX, int endY) => GetDirection(endX - startX, endY - startY);

        /// <summary>
        /// Returns the direction that most closely matches the degree heading of a line with the
        /// given delta-x and delta-y values. Rounds clockwise if the heading is exactly between two directions.
        /// </summary>
        /// <param name="deltaChange">Vector representing the change in x and change in y across
        /// the line (deltaChange.X is the change in x, deltaChange.Y is the change in y).</param>
        /// <returns>The direction that most closely matches the heading formed by the given input.</returns>
        public static Direction GetDirection(Coord deltaChange) => GetDirection(deltaChange.X, deltaChange.Y);

        /// <summary>
        /// Returns the direction that most closely matches the degree heading of a line with the
        /// given delta-x and delta-y values. Rounds clockwise if the heading is exactly between two directions.
        /// </summary>
        /// <param name="dx">The change in x-values across the line.</param>
        /// <param name="dy">The change in y-values across the line.</param>
        /// <returns>The direction that most closely matches the heading formed by the given input.</returns>
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

        /// <summary>
        /// - operator. Returns the direction i directions counterclockwise of the given direction if
        ///   i is positive. If is negative, the direction is moved clockwise. If any amount is added
        /// to NONE, it returns NONE.
        /// </summary>
        /// <param name="d">Direction to "subtract" from.</param>
        /// <param name="i">Number of directions to "subtract".</param>
        /// <returns>
        /// The direction i directions counterclockwise of d, if i is positive, or clockwise of d if
        /// i is negative. NONE is returned if NONE was added to.
        /// </returns>
        public static Direction operator -(Direction d, int i) => (d == NONE) ? NONE : directions[MathHelpers.WrapAround((int)d.Type - i, 8)];

        /// <summary>
        /// -- operator (decrement). Returns the direction directly counterclockwise of the original
        ///    direction. If NONE is decremented, returns NONE.
        /// </summary>
        /// <param name="d">Direction to decrement.</param>
        /// <returns>The direction directly counterclockwise of d, or NONE if none was decremented.</returns>
        public static Direction operator --(Direction d) => (d == NONE) ? NONE : directions[MathHelpers.WrapAround((int)d.Type - 1, 8)];

        /// <summary>
        /// + operator. Returns the direction i directions clockwise of the given direction if i is
        ///   positive. If is negative, the direction is moved counterclockwise. If any amount is
        /// added to NONE, it returns NONE.
        /// </summary>
        /// <param name="d">Direction to "add" to.</param>
        /// <param name="i">Number of directions to "add".</param>
        /// <returns>
        /// The direction i directions clockwise of d, if i is positive, or counterclockwise of d if
        /// i is negative. NONE is returned if NONE was added to.
        /// </returns>
        public static Direction operator +(Direction d, int i) => (d == NONE) ? NONE : directions[MathHelpers.WrapAround((int)d.Type + i, 8)];

        /// <summary>
        /// ++ operator (increment). Returns the direction directly clockwise of the original
        ///    direction. If NONE is incremented, returns NONE.
        /// </summary>
        /// <param name="d">Direction to increment.</param>
        /// <returns>The direction directly clockwise of d, or NONE if none is incremented.</returns>
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
        /// Writes the string ("UP", "UP_RIGHT", etc.) for the direction.
        /// </summary>
        /// <returns>String representation of the direction.</returns>
        public override string ToString() => writeVals[(int)Type];
    }
}