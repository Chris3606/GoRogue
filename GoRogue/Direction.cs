using System;
using System.Collections.Generic;

namespace GoRogue
{
    /// <summary>
    /// Utility class to handle Directions on a grid map.  The built-in variables hold the dx and dy for that direction, at unit scale.
    /// </summary>
    /// <remarks>
    /// It also contains functions to provide ways to iterate over the directions in common orders -- for example, it contains a function that returns an
    /// Enumerable of all the directions in clockwise order, as well as a similar function for cardinal directions in a clockwise order, among many others.  It is also possible
    /// to invert the y coordinates (where UP is 0, 1 and DOWN is 0, -1, etc).  Direction instances can also be added to Coords to produce the coordinate
    /// directly adjacent in the direction added.
    /// </remarks>
    public class Direction
    {
        private static int yMult;

        private static bool initYInc;
        private static bool _yIncreasesUpward;

        /// <summary>
        /// Whether or not a positive y-value indicates an UP motion.  If true, Directions with an UP component will have positive y-values,
        /// and ones with DOWN components will have negative values.  Setting this to false (which is the default) inverts this.
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

                    UP = new Direction(0, -1 * yMult, DirectionType.UP);
                    DOWN = new Direction(0, 1 * yMult, DirectionType.DOWN);
                    UP_LEFT = new Direction(-1, -1 * yMult, DirectionType.UP_LEFT);
                    UP_RIGHT = new Direction(1, -1 * yMult, DirectionType.UP_RIGHT);
                    DOWN_LEFT = new Direction(-1, 1 * yMult, DirectionType.DOWN_LEFT);
                    DOWN_RIGHT = new Direction(1, 1 * yMult, DirectionType.DOWN_RIGHT);
                }
            }
        }

        /// <summary>
        /// Delta X for direction.  Right is positive, left is negative.
        /// </summary>
        public int DeltaX { get; private set; }

        /// <summary>
        /// Delta Y for direction.  Down is positive, up is negative, since this is how graphics coordinates typically work.
        /// </summary>
        public int DeltaY { get; private set; }

        /// <summary>
        /// Function that returns an IEnumerable of Directions.  Commonly used internally by algorithms that iterate over neighbors
        /// and support multiple different distance calculations.
        /// </summary>
        /// <returns>IEnumerable of Directions.</returns>
        public delegate IEnumerable<Direction> NeighborsGetter();

        private enum DirectionType
        { UP, UP_RIGHT, RIGHT, DOWN_RIGHT, DOWN, DOWN_LEFT, LEFT, UP_LEFT, NONE };

        private DirectionType type;

        private static readonly string[] writeVals = { "UP", "UP_RIGHT", "RIGHT", "DOWN_RIGHT", "DOWN", "DOWN_LEFT", "LEFT", "UP_LEFT", "NONE" };

        private Direction(int dx, int dy, DirectionType type)
        {
            DeltaX = dx;
            DeltaY = dy;
            this.type = type;
        }

        /// <summary>
        /// Writes the string ("UP", "UP_RIGHT", etc.) for the direction.
        /// </summary>
        /// <returns>String representation of the direction.</returns>
        public override string ToString()
        {
            return writeVals[(int)type];
        }

        static Direction()
        {
            directions = new Direction[9];
            LEFT = new Direction(-1, 0, DirectionType.LEFT);
            RIGHT = new Direction(1, 0, DirectionType.RIGHT);
            NONE = new Direction(0, 0, DirectionType.NONE);
            initYInc = false;
            YIncreasesUpward = false; // Initializes rest of distance values
        }

        /// <summary>
        /// Up direction.
        /// </summary>
        public static Direction UP
        {
            get => directions[(int)DirectionType.UP];
            private set => directions[(int)DirectionType.UP] = value;
        }

        /// <summary>
        /// Down direction.
        /// </summary>
        public static Direction DOWN
        {
            get => directions[(int)DirectionType.DOWN];
            private set => directions[(int)DirectionType.DOWN] = value;
        }

        /// <summary>
        /// Left direction.
        /// </summary>
        public static Direction LEFT
        {
            get => directions[(int)DirectionType.LEFT];
            private set => directions[(int)DirectionType.LEFT] = value;
        }

        /// <summary>
        /// Right direction.
        /// </summary>
        public static Direction RIGHT
        {
            get => directions[(int)DirectionType.RIGHT];
            private set => directions[(int)DirectionType.RIGHT] = value;
        }

        /// <summary>
        /// Up-left direction.
        /// </summary>
        public static Direction UP_LEFT
        {
            get => directions[(int)DirectionType.UP_LEFT];
            private set => directions[(int)DirectionType.UP_LEFT] = value;
        }

        /// <summary>
        /// Up-right direction.
        /// </summary>
        public static Direction UP_RIGHT
        {
            get => directions[(int)DirectionType.UP_RIGHT];
            private set => directions[(int)DirectionType.UP_RIGHT] = value;
        }

        /// <summary>
        /// Down-left direction.
        /// </summary>
        public static Direction DOWN_LEFT
        {
            get => directions[(int)DirectionType.DOWN_LEFT];
            private set => directions[(int)DirectionType.DOWN_LEFT] = value;
        }

        /// <summary>
        /// Down-right direction.
        /// </summary>
        public static Direction DOWN_RIGHT
        {
            get => directions[(int)DirectionType.DOWN_RIGHT];
            private set => directions[(int)DirectionType.DOWN_RIGHT] = value;
        }

        /// <summary>
        /// No direction.
        /// </summary>
        public static Direction NONE
        {
            get => directions[(int)DirectionType.NONE];
            private set => directions[(int)DirectionType.NONE] = value;
        }

        private static readonly Direction[] directions;

        /// <summary>
        /// Returns all directions except for NONE, in clockwise order, starting with UP.
        /// </summary>
        /// <returns>All directions except for NONE, in clockwise order, starting with UP.</returns>
        public static IEnumerable<Direction> DirectionsClockwise()
        {
            yield return UP;
            yield return UP_RIGHT;
            yield return RIGHT;
            yield return DOWN_RIGHT;
            yield return DOWN;
            yield return DOWN_LEFT;
            yield return LEFT;
            yield return UP_LEFT;
        }

        /// <summary>
        /// Returns all directions except for NONE, in clockwise order, starting with the direction given.  If NONE
        /// is given at the starting point, starts with UP.
        /// </summary>
        /// <param name="startingPoint">The direction to "start" at -- returns this direction first.</param>
        /// <returns>All directions except for NONE, in clockwise order, starting with the starting point given, or UP
        /// if NONE is given as the starting point.</returns>
        public static IEnumerable<Direction> DirectionsClockwise(Direction startingPoint)
        {
            if (startingPoint == NONE)
                startingPoint = UP;

            for (int i = 1; i <= 8; i++)
            {
                yield return startingPoint;
                startingPoint++;
            }
        }

        /// <summary>
        /// Returns all directions except for NONE, in counterclockwise order, starting with UP.
        /// </summary>
        /// <returns>All directions except for NONE, in counterclockwise order, starting with UP.</returns>
        public static IEnumerable<Direction> DirectionsCounterClockwise()
        {
            yield return UP;
            yield return UP_LEFT;
            yield return LEFT;
            yield return DOWN_LEFT;
            yield return DOWN;
            yield return DOWN_RIGHT;
            yield return RIGHT;
            yield return UP_RIGHT;
        }

        /// <summary>
        /// Returns all directions except for NONE, in counterclockwise order, starting with the direction given.  If NONE
        /// is given at the starting point, starts with UP.
        /// </summary>
        /// <param name="startingPoint">The direction to "start" at -- returns this direction first.</param>
        /// <returns>All directions except for NONE, in counterclockwise order, starting with the starting point given, or UP
        /// if NONE is given as the starting point.</returns>
        public static IEnumerable<Direction> DirectionsCounterClockwise(Direction startingPoint)
        {
            if (startingPoint == NONE)
                startingPoint = UP;

            for (int i = 1; i <= 8; i++)
            {
                yield return startingPoint;
                startingPoint--;
            }
        }

        /// <summary>
        /// Returns only cardinal directions, in clockwise order, starting with UP.
        /// </summary>
        /// <returns>All cardinal directions, in clockwise order, starting with UP.</returns>
        public static IEnumerable<Direction> CardinalsClockwise()
        {
            yield return UP;
            yield return RIGHT;
            yield return DOWN;
            yield return LEFT;
        }

        /// <summary>
        /// Returns only cardinal directions, in clockwise order, starting with the startingPoint given.  If NONE
        /// is given, starts at UP.  If any other non-cardinal direction is given, it starts at the closest clockwise cardinal
        /// direction.
        /// </summary>
        /// <param name="startingPoint">The direction to "start" at -- returns this direction first.</param>
        /// <returns>All cardinal directions, in clockwise order, starting with the starting point given, as outlined in the function description.</returns>
        public static IEnumerable<Direction> CardinalsClockwise(Direction startingPoint)
        {
            if (startingPoint == NONE)
                startingPoint = UP;

            if ((int)startingPoint.type % 2 == 1)
                startingPoint++; // Make it a cardinal

            yield return startingPoint;
            yield return startingPoint + 2;
            yield return startingPoint + 4;
            yield return startingPoint + 6;
        }

        /// <summary>
        /// Returns only cardinal directions, in counterclockwise order, starting with UP.
        /// </summary>
        /// <returns>All cardinal directions, in counterclockwise order, starting with UP.</returns>
        public static IEnumerable<Direction> CardinalsCounterClockwise()
        {
            yield return UP;
            yield return LEFT;
            yield return DOWN;
            yield return RIGHT;
        }

        /// <summary>
        /// Returns only cardinal directions, in counterclockwise order, starting with the startingPoint given.  If NONE
        /// is given, starts at UP.  If any other non-cardinal direction is given, it starts at the closest counterclockwise cardinal
        /// direction.
        /// </summary>
        /// <param name="startingPoint">The direction to "start" at -- returns this direction first.</param>
        /// <returns>All cardinal directions, in counterclockwise order, starting with the starting point given, as outlined in the function description.</returns>
        public static IEnumerable<Direction> CardinalsCounterClockwise(Direction startingPoint)
        {
            if (startingPoint == NONE)
                startingPoint = UP;

            if ((int)startingPoint.type % 2 == 1)
                startingPoint--; // Make it a cardinal

            yield return startingPoint;
            yield return startingPoint - 2;
            yield return startingPoint - 4;
            yield return startingPoint - 6;
        }

        /// <summary>
        /// Returns only diagonal directions, top to bottom, left to right: UP_LEFT, UP_RIGHT, DOWN_LEFT, DOWN_RIGHT.
        /// </summary>
        /// <returns>Only diagonal directions.</returns>
        public static IEnumerable<Direction> DiagonalsTopBottom()
        {
            yield return UP_LEFT;
            yield return UP_RIGHT;
            yield return DOWN_LEFT;
            yield return DOWN_RIGHT;
        }

        /// <summary>
        /// Returns only diagonal directions in clockwise order, starting with UP_RIGHT.
        /// </summary>
        /// <returns>Only diagonal directions.</returns>
        public static IEnumerable<Direction> DiagonalsClockwise()
        {
            yield return UP_RIGHT;
            yield return DOWN_RIGHT;
            yield return DOWN_LEFT;
            yield return UP_LEFT;
        }

        /// <summary>
        /// Returns only diagonal directions, in clockwise order, starting with the startingPoint given.  If NONE
        /// is given, starts at UP_RIGHT.  If any cardinal direction is given, it starts at the closest clockwise diagonal direction.
        /// </summary>
        /// <param name="startingPoint">The direction to "start" at; returns this direction first.</param>
        /// <returns>All diagonal directions, in clockwise order, starting with the starting point given, as outlined in the function description.</returns>
        public static IEnumerable<Direction> DiagonalsClockwise(Direction startingPoint)
        {
            if (startingPoint == NONE)
                startingPoint = UP_RIGHT;

            if ((int)startingPoint.type % 2 == 0)
                startingPoint++; // Make it a diagonal

            yield return startingPoint;
            yield return startingPoint + 2;
            yield return startingPoint + 4;
            yield return startingPoint + 6;
        }

        /// <summary>
        /// Returns only diagonal directions in counterclockwise order, starting with UP_LEFT.
        /// </summary>
        /// <returns>Only diagonal directions.</returns>
        public static IEnumerable<Direction> DiagonalsCounterClockwise()
        {
            yield return UP_LEFT;
            yield return DOWN_LEFT;
            yield return DOWN_RIGHT;
            yield return UP_RIGHT;
        }

        /// <summary>
        /// Returns only diagonal directions, in clockwise order, starting with the startingPoint given.  If NONE
        /// is given, starts at UP_LEFT.  If any cardinal direction is given, it starts at the closest counterclockwise diagonal direction.
        /// </summary>
        /// <param name="startingPoint">The direction to "start" at; returns this direction first.</param>
        /// <returns>All diagonal directions, in counterclockwise order, starting with the starting point given, as outlined in the function description.</returns>
        public static IEnumerable<Direction> DiagonalsCounterClockwise(Direction startingPoint)
        {
            if (startingPoint == NONE)
                startingPoint = UP_LEFT;

            if ((int)startingPoint.type % 2 == 0)
                startingPoint--; // Make it a diagonal

            yield return startingPoint;
            yield return startingPoint - 2;
            yield return startingPoint - 4;
            yield return startingPoint - 6;
        }

        /// <summary>
        /// Returns outward facing directions (everything except NONE).  Similar to clockwise just not in clockwise order;
        /// checks cardinals first then diagonals.
        /// </summary>
        /// <returns>Outward facing directions (everything except NONE).</returns>
        public static IEnumerable<Direction> Outwards()
        {
            yield return UP;
            yield return DOWN;
            yield return LEFT;
            yield return RIGHT;
            yield return UP_LEFT;
            yield return UP_RIGHT;
            yield return DOWN_LEFT;
            yield return DOWN_RIGHT;
        }

        /// <summary>
        /// Calculates degree bearing of the line (from => to), where 0 is the direction UP.
        /// </summary>
        /// <param name="from">Coordinate of line starting point.</param>
        /// <param name="to">Coordinate of line ending point.</param>
        /// <returns>The degree bearing of the line specified by the two given points.</returns>
        public static double BearingOfLine(Coord from, Coord to)
        {
            int x = to.X - from.X;
            int y = to.Y - from.Y;
            y *= yMult;

            double angle = Math.Atan2(y, x);
            double degree = MathHelpers.ToDegree(angle);
            degree += 450; // Rotate to all positive such that 0 is up
            degree %= 360; // Normalize
            return degree;
        }

        /// <summary>
        /// Returns the direction that most closely matches the angle given by a line from (0, 0) to the input.  Straight up is degree 0, straight down is 180 degrees, etc.
        /// If the angle happens to be right on the border between 2 angles, it rounds "up", eg., we take the closest angle in the clockwise direction.
        /// </summary>
        /// <param name="x">X-coordinate of line-ending point.</param>
        /// <param name="y">Y-coordinate of line-ending point.</param>
        /// <returns>The direction that most closely matches the angle formed by the given input.</returns>
        public static Direction GetDirection(int x, int y)
        {
            if (x == 0 && y == 0)
                return NONE;

            y *= yMult;

            double angle = Math.Atan2(y, x);
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
        /// Returns the cardinal direction that most closely matches the angle given by a line from (0, 0) to the input.  Rounds clockwise if exactly on a diagonal.
        /// Similar to GetDirection, except gives only cardinal directions.
        /// </summary>
        /// <param name="x">X-coordinate of line-ending point.</param>
        /// <param name="y">Y-coordinate of line-ending point.</param>
        /// <returns>The cardinal direction that most closely matches the angle formed by the given input.</returns>
        public static Direction GetCardinalDirection(int x, int y)
        {
            if (x == 0 && y == 0)
                return NONE;

            y *= yMult;

            double angle = Math.Atan2(y, x);
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
        /// ++ operator (increment).  Returns the direction directly clockwise of the original direction.  If NONE is incremented,
        /// returns NONE.
        /// </summary>
        /// <param name="d">Direction to increment.</param>
        /// <returns>The direction directly clockwise of d, or NONE if none is incremented.</returns>
        public static Direction operator ++(Direction d) => (d == NONE) ? NONE : directions[MathHelpers.WrapAround((int)d.type + 1, 8)];

        /// <summary>
        /// -- operator (decrement).  Returns the direction directly counterclockwise of the original direction.  If NONE is decremented,
        /// returns NONE.
        /// </summary>
        /// <param name="d">Direction to decrement.</param>
        /// <returns>The direction directly counterclockwise of d, or NONE if none was decremented.</returns>
        public static Direction operator --(Direction d) => (d == NONE) ? NONE : directions[MathHelpers.WrapAround((int)d.type - 1, 8)];

        /// <summary>
        /// + operator. Returns the direction i directions clockwise of the given direction if i is positive.  If is negative,
        /// the direction is moved counterclockwise.  If any amount is added to NONE, it returns NONE.
        /// </summary>
        /// <param name="d">Direction to "add" to.</param>
        /// <param name="i">Number of directions to "add".</param>
        /// <returns>The direction i directions clockwise of d, if i is positive, or counterclockwise of d if i is negative.
        /// NONE is returned if NONE was added to.</returns>
        public static Direction operator +(Direction d, int i) => (d == NONE) ? NONE : directions[MathHelpers.WrapAround((int)d.type + i, 8)];

        /// <summary>
        /// - operator. Returns the direction i directions counterclockwise of the given direction if i is positive.  If is negative,
        /// the direction is moved clockwise.  If any amount is added to NONE, it returns NONE.
        /// </summary>
        /// <param name="d">Direction to "subtract" from.</param>
        /// <param name="i">Number of directions to "subtract".</param>
        /// <returns>The direction i directions counterclockwise of d, if i is positive, or clockwise of d if i is negative.
        /// NONE is returned if NONE was added to.</returns>
        public static Direction operator -(Direction d, int i) => (d == NONE) ? NONE : directions[MathHelpers.WrapAround((int)d.type - i, 8)];

        /// <summary>
        /// Returns a function that gets the Directions enumerable that represents the potential neighbors of a given cell, according to the given distance
        /// calculation.
        /// </summary>
        /// <param name="distanceType">The distance type defining neighbors.</param>
        /// <returns>A function that returns the IEnumerable&lt;Direction&gt; representing potential neighbor directions, given the specified distance calculation.</returns>
        public static NeighborsGetter GetNeighbors(Distance distanceType)
        {
            if (distanceType == Distance.MANHATTAN)
                return CardinalsClockwise;
            else
                return Outwards;
        }
    }
}