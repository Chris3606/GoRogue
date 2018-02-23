using System;

namespace GoRogue
{
    /// <summary>
    /// 2d coordinate class. You cannot create instances of this class using a constructor --
    /// instead, use the Get function to create instances. Also provides numerous static functions to
    /// deal with grid/Coord-related math, operations, etc.
    /// </summary>
    /// <remarks>
    /// If you want the coordinate (1, 2), use Coord.Get(1, 2), and it returns you a Coord instance.
    /// These instances are read-only, however operators such as addition, etc., are provided. This
    /// is due to optimizations under the hood. By default, the class keeps a static, internal array
    /// that contains an instance of every coordinate between (-3, -3), and (255, 255). If the
    /// coordinate x and y values given to Get are within this range, Get returns the appropriate
    /// instance from the array. If the coordinates given are outside of the range, it simply returns
    /// a new instance. Since most of the coordinates used in roguelike/grid based games will be
    /// between (-3, -3) and (255, 255), this can end up drastically reducing the number of memory
    /// allocations that need to be done since there will exist one and only one instance of the
    /// Coord class for each value in that range. Since allocations are expensive due to garbage
    /// collections, this can significantly improve on efficiency. Later, support may be added for
    /// modifying this range as necessary.
    /// </remarks>
    public class Coord
    {
        /// <summary>
        /// X-value of the coordinate.
        /// </summary>
        public readonly int X;

        /// <summary>
        /// Y-value of the coordinate.
        /// </summary>
        public readonly int Y;

        private static Coord[,] POOL = new Coord[259, 259];

        // Make sure pool is initialized to proper width/height.
        static Coord()
        {
            int width = POOL.GetLength(0), height = POOL.GetLength(1);
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    POOL[i, j] = new Coord(i - 3, j - 3);
        }

        private Coord()
        {
            X = 0;
            Y = 0;
        }

        private Coord(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Calculates degree bearing of the line (start =&gt; end), where 0 is in the direction Direction.UP.
        /// </summary>
        /// <param name="start">Coordinate of line starting point.</param>
        /// <param name="end">Coordinate of line ending point.</param>
        /// <returns>The degree bearing of the line specified by the two given points.</returns>
        public static double BearingOfLine(Coord start, Coord end) => BearingOfLine(end.X - start.X, end.Y - start.Y);
        /// <summary>
        /// Calculates degree bearing of the line ((startX, startY) =&gt; (endX, endY)), where 0 is in the direction Direction.UP.
        /// </summary>
        /// <param name="startX">X-value of the coordinate of line starting point.</param>
        /// <param name="startY">Y-value of the coordinate of line starting point.</param>
        /// <param name="endX">X-value of the coordinate of line ending point.</param>
        /// /// <param name="endY">Y-value of the coordinate of line ending point.</param>
        /// <returns>The degree bearing of the line specified by the two given points.</returns>
        public static double BearingOfLine(int startX, int startY, int endX, int endY) => BearingOfLine(endX - startX, endY - startY);

        /// <summary>
        /// Calculates the degree bearing of a line with the given delta-x and delta-y values, where 0 degreees is in
        /// the direction Direction.UP.
        /// </summary>
        /// <param name="deltaChange">Vector, where deltaChange.X is the change in x-values across the line, and deltaChange.Y
        /// is the change in y-values across the line.</param>
        /// <returns>The degree bearing of the line with the given dx and dy values.</returns>
        public static double BearingOfLine(Coord deltaChange) => BearingOfLine(deltaChange.X, deltaChange.Y);

        /// <summary>
        /// Calculates the degree bearing of a line with the given delta-x and delta-y values, where 0 degreees is in
        /// the direction Direction.UP.
        /// </summary>
        /// <param name="dx">The change in x-values across the line.</param>
        /// <param name="dy">the change in y-values across the line</param>
        /// <returns>The degree bearing of the line with the given dx and dy values.</returns>
        public static double BearingOfLine(int dx, int dy)
        {
            dy *= Direction.yMult;
            double angle = Math.Atan2(dy, dx);
            double degree = MathHelpers.ToDegree(angle);
            degree += 450; // Rotate to all positive such that 0 is up
            degree %= 360; // Normalize
            return degree;
        }



        /// <summary>
        /// Returns the result of the euclidean distance formula, without the square root -- eg.,
        /// (c2.X - c1.X) * (c2.X - c1.X) + (c2.Y - c1.Y) * (c2.Y - c1.Y). Use this if you only care
        /// about the magnitude of the distance -- eg., if you're trying to compare two distances.
        /// Omitting the square root provides a speed increase.
        /// </summary>
        /// <param name="c1">The first point.</param>
        /// <param name="c2">The second point.</param>
        /// <returns>
        /// The "magnitude" of the euclidean distance between the two points -- basically the
        /// distance formula without the square root.
        /// </returns>
        public static double EuclideanDistanceMagnitude(Coord c1, Coord c2) => EuclideanDistanceMagnitude(c2.X - c1.X, c2.Y - c1.Y);

        /// <summary>
        /// Returns the result of the euclidean distance formula, without the square root -- eg.,
        /// (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1). Use this if you only care
        /// about the magnitude of the distance -- eg., if you're trying to compare two distances.
        /// Omitting the square root provides a speed increase.
        /// </summary>
        /// <param name="x1">The x-value of the first location.</param>
        /// <param name="y1">The y-value of the first location.</param>
        /// <param name="x2">The x-value of the second location.</param>
        /// <param name="y2">The y-value of the second location.</param>
        /// <returns>
        /// The "magnitude" of the euclidean distance between the two points -- basically the
        /// distance formula without the square root.
        /// </returns>
        public static double EuclideanDistanceMagnitude(int x1, int y1, int x2, int y2) => EuclideanDistanceMagnitude(x2 - x1, y2 - y1);

        /// <summary>
        /// Returns the result of the euclidean distance formula, without the square root, given the
        /// dx and dy values between two points -- eg., deltaChange.X * deltaChange.X + deltaChange.Y * deltaChange.Y.
        /// Use this if you only care about the magnitude of the distance -- eg., if you're trying to compare two distances.
        /// Omitting the square root provides a speed increase.
        /// </summary>
        /// <param name = "deltaChange" > Vector, where deltaChange.X is the change in x-values between the two points, and
        /// deltaChange.Y is the change in y-values between the two points.</param>
        /// <returns>
        /// The "magnitude" of the euclidean distance of two locations with the given dx and dy values -- basically the
        /// distance formula without the square root.
        /// </returns>
        public static double EuclideanDistanceMagnitude(Coord deltaChange) => EuclideanDistanceMagnitude(deltaChange.X, deltaChange.Y);

        /// <summary>
        /// Returns the result of the euclidean distance formula, without the square root, given the
        /// dx and dy values between two points -- eg., deltaChange.X * deltaChange.X + deltaChange.Y * deltaChange.Y.
        /// Use this if you only care about the magnitude of the distance -- eg., if you're trying to compare two distances.
        /// Omitting the square root provides a speed increase.
        /// </summary>
        /// <param name = "dx" > The change in x-values between the two points.</param>
        /// <param name = "dy" > The change in y-values between the two points.</param>
        /// <returns>
        /// The "magnitude" of the euclidean distance of two locations with the given dx and dy values -- basically the
        /// distance formula without the square root.
        /// </returns>
        public static double EuclideanDistanceMagnitude(int dx, int dy) => dx * dx + dy * dy;

        /// <summary>
        /// Returns the proper Coord instance for the given x and y values. Will return the one in
        /// the array if the values are in the appropriate range, otherwise will create a new one and
        /// return that one.
        /// </summary>
        /// <param name="x">The x-value for the coordinate.</param>
        /// <param name="y">The y-value for the coordinate.</param>
        /// <returns>The Coord representing the given x-value and y-value.</returns>
        public static Coord Get(int x, int y)
        {
            if (x >= -3 && y >= -3 && x < POOL.GetLength(0) - 3 && y < POOL.GetLength(1) - 3)
                return POOL[x + 3, y + 3];
            else return new Coord(x, y);
        }

        /// <summary>
        /// Returns the midpoint between the two points.
        /// </summary>
        /// <param name="c1">The first point.</param>
        /// <param name="c2">The second point.</param>
        /// <returns>The midpoint between c1 and c2.</returns>
        public static Coord Midpoint(Coord c1, Coord c2) =>
            Get((int)Math.Round((c1.X + c2.X) / 2.0f, MidpointRounding.AwayFromZero), (int)Math.Round((c1.Y + c2.Y) / 2.0f, MidpointRounding.AwayFromZero));

        /// <summary>
        /// Returns the midpoint between the two points.
        /// </summary>
        /// <param name="x1">The x-value of the first location.</param>
        /// <param name="y1">The y-value of the first location.</param>
        /// <param name="x2">The x-value of the second location.</param>
        /// <param name="y2">The y-value of the second location.</param>
        /// <returns>The midpoint between the two points.</returns>
        public static Coord Midpoint(int x1, int y1, int x2, int y2) => Midpoint(Coord.Get(x1, y1), Coord.Get(x2, y2));

        /// <summary>
        /// - operator. Returns the coordinate (c1.X - c2.X, c1.Y - c2.Y)
        /// </summary>
        /// <param name="c1">The first coordinate.</param>
        /// <param name="c2">The coordinate to subtract from c1.</param>
        /// <returns>c1 - c2, eg. (c1.X - c2.X, c1.Y - c2.Y)</returns>
        public static Coord operator -(Coord c1, Coord c2) => Get(c1.X - c2.X, c1.Y - c2.Y);

        /// <summary>
        /// - operator. Subtracts scalar i from the x and y values of c1, eg. returns (c.X - i, c.Y - i).
        /// </summary>
        /// <param name="c">Coordinate to subtract the scalar from.</param>
        /// <param name="i">Scalar to subtract from the coordinate.</param>
        /// <returns></returns>
        public static Coord operator -(Coord c, int i) => Get(c.X - i, c.Y - i);

        /// <summary>
        /// True if either the x-values or y-values are not equal.
        /// </summary>
        /// <param name="c1">First coordinate to compare.</param>
        /// <param name="c2">Second coordinate to compare.</param>
        /// <returns>
        /// True if either the x-values or y-values are not equal, false if they are both equal.
        /// </returns>
        public static bool operator !=(Coord c1, Coord c2) => !(c1 == c2);

        /// <summary>
        /// * operator. Multiplies the x-value and y-value of c by i, eg. returns (c.X * i, c.Y * i)
        /// </summary>
        /// <param name="c">Coordinate to multiply by the scalar.</param>
        /// <param name="i">Scalar to multiply the coordinate by.</param>
        /// <returns>Coordinate (c.X * i, c.Y * i)</returns>
        public static Coord operator *(Coord c, int i) => Get(c.X * i, c.Y * i);

        /// <summary>
        /// * operator, similar to the int version. Rounds x-value and y-value to the nearest
        ///   integer. Effectively "scale c by i".
        /// </summary>
        /// <param name="c">The coordinate to multiply by the scalar.</param>
        /// <param name="i">The scalar to multiply the coordinate by.</param>
        /// <returns>
        /// Coordinate (c.X * i, c.Y * i), with the resulting values rounded to nearest integer.
        /// </returns>
        public static Coord operator *(Coord c, double i) =>
            Get((int)Math.Round(c.X * i, MidpointRounding.AwayFromZero), (int)Math.Round(c.Y * i, MidpointRounding.AwayFromZero));

        /// <summary>
        /// / operator. Divides the x-value and y-value of c by i, eg. returns (c.X / i, c.Y / i).
        /// Rounds resulting values to the nearest integer.
        /// </summary>
        /// <param name="c">The coordinate to divide by scalar.</param>
        /// <param name="i">The scalar to divide the coordinate by.</param>
        /// <returns>(c.X / i, c.Y / i), with the resulting values rounded to the nearest integer.</returns>
        public static Coord operator /(Coord c, int i) =>
            Get((int)Math.Round(c.X / (double)i, MidpointRounding.AwayFromZero), (int)Math.Round(c.Y / (double)i, MidpointRounding.AwayFromZero));

        /// <summary>
        /// / operator. Similar to int version.
        /// </summary>
        /// <param name="c">The coordinate to divide by scalar.</param>
        /// <param name="i">The scalar to divide the coordinate by.</param>
        /// <returns>(c.X / i, c.Y / i), with the resulting values rounded to the nearest integer.</returns>
        public static Coord operator /(Coord c, double i) =>
            Get((int)Math.Round(c.X / i, MidpointRounding.AwayFromZero), (int)Math.Round(c.Y / i, MidpointRounding.AwayFromZero));

        /// <summary>
        /// + operator. Returns the coordinate (c1.X + c2.X, c1.Y + c2.Y).
        /// </summary>
        /// <param name="c1">The first coordinate.</param>
        /// <param name="c2">The coordinate to add to c1.</param>
        /// <returns>c1 + c2, eg. (c1.X + c2.X, c1.Y + c2.Y)</returns>
        public static Coord operator +(Coord c1, Coord c2) => Get(c1.X + c2.X, c1.Y + c2.Y);

        /// <summary>
        /// + operator. Adds scalar i to the x and y values of c; eg., returns (c.X + i, c.Y + i).
        /// </summary>
        /// <param name="c">Coordinate to add scalar to.</param>
        /// <param name="i">Scalar to add to coordinate.</param>
        /// <returns>Coordinate resulting from adding scalar i to x-value and y-value of c1.</returns>
        public static Coord operator +(Coord c, int i) => Get(c.X + i, c.Y + i);

        /// <summary>
        /// + operator. Translates the given coordinate by the given direction, eg. returns (c.X +
        ///   d.DeltaX, c.Y + d.DeltaY).
        /// </summary>
        /// <param name="c">The coordinate to translate by the given direction.</param>
        /// <param name="d">The direction to translate the coordinate by.</param>
        /// <returns>
        /// The coordinate translated by the given direction, eg. (c.X + d.DeltaX, c.Y + d.DeltaY
        /// </returns>
        public static Coord operator +(Coord c, Direction d) => Get(c.X + d.DeltaX, c.Y + d.DeltaY);

        /// <summary>
        /// True if c1.X == c2.X and c1.Y == c2.Y.
        /// </summary>
        /// <param name="c1">First coodinate to compare.</param>
        /// <param name="c2">Second coordinate to compare.</param>
        /// <returns>True if the two coordinates are equal, false if not.</returns>
        public static bool operator ==(Coord c1, Coord c2)
        {
            if (ReferenceEquals(c1, c2))
                return true;

            if (ReferenceEquals(null, c1) || ReferenceEquals(null, c2))
                return false;

            return c1.X == c2.X && c1.Y == c2.Y;
        }

        /// <summary>
        /// Reverses the ToIndex function, returning the Coord represented by a given index.
        /// </summary>
        /// <param name="index">The index in 1D form.</param>
        /// <param name="rowCount">The number of rows.</param>
        /// <returns>The Coord represented by the 1D index given.</returns>
        public static Coord ToCoord(int index, int rowCount) => Get(index % rowCount, index / rowCount);

        /// <summary>
        /// Returns y * rowCount + x. Same as Coord.ToIndex(int rowCount), just takes x and y instead.
        /// </summary>
        /// <param name="x">X-value of the coordinate.</param>
        /// <param name="y">Y-value of the coordinate.</param>
        /// <param name="rowCount">The number of rows, used to do math to calculate index.</param>
        /// <returns>The 1D index of this Coord.</returns>
        public static int ToIndex(int x, int y, int rowCount) => y * rowCount + x;

        /// <summary>
        /// Reverses the ToIndex function, returning only the X-value for the given index.
        /// </summary>
        /// <param name="index">The index in 1D form.</param>
        /// <param name="rowCount">The number of rows.</param>
        /// <returns>The X-value for the location represented by the given index.</returns>
        public static int ToXValue(int index, int rowCount) => index % rowCount;

        /// <summary>
        /// Reverses the ToIndex function, returning only the Y-value for the given index.
        /// </summary>
        /// <param name="index">The index in 1D form.</param>
        /// <param name="rowCount">The number of rows.</param>
        /// <returns>The Y-value for the location represented by the given index.</returns>
        public static int ToYValue(int index, int rowCount) => index / rowCount;

        /// <summary>
        /// Same as operator == in this case; returns false if obj is not a Coord.
        /// </summary>
        /// <param name="obj">The object to compare the current Coord to.</param>
        /// <returns>
        /// True if o is a Coord instance, and the two coordinates are equal, false otherwise.
        /// </returns>
        public override bool Equals(object obj) => this == (obj as Coord); // If cast is null, operator== will take care of it.

        /// <summary>
        /// Returns a hash code for the Coord. The important parts: it should be fairly fast and it
        /// does not collide often. The details: it uses a seperate bit-mixing algorithm for X and Y,
        /// with X and Y each multiplied by a differet large integer, then xors the mixed values, and
        /// does a right shift, then multiplies by an overflowing prime number.
        /// </summary>
        /// <returns>The hash-code for the Coord.</returns>
        public override int GetHashCode()
        {
            // Intentional overflow on both of these, part of hash-code generation
            int x2 = (int)(0x9E3779B9 * X), y2 = 0x632BE5AB * Y;
            return (int)(((uint)(x2 ^ y2) >> ((x2 & 7) + (y2 & 7))) * 0x85157AF5);
        }

        /// <summary>
        /// Returns a value that can be used to index this location in a 2D array that is actually
        /// encoded in a 1D array. Actual value is Y * rowCount + X. The 2D array being represented
        /// should have [width, height] of [rowCount, dontCare] for this index to be valid. Note
        /// that, when this method is used, if one needs to use a nested for loop to iterate over
        /// this array, it is best to do for y... as the outer loop and for x..., as the inner loop,
        /// for cache performance reasons.
        /// </summary>
        /// <param name="rowCount">
        /// The number of rows, used to do math to calculate index. Usually the width of the 2D array.
        /// </param>
        /// <returns>The 1D index of this Coord.</returns>
        public int ToIndex(int rowCount) => Y * rowCount + X;

        /// <summary>
        /// Returns representation (X, Y).
        /// </summary>
        /// <returns>String (X, Y)</returns>
        public override string ToString() => $"({X},{Y})";

        /// <summary>
        /// Returns the coordinate resulting from adding dx to the X-value of the coordinate, and dy
        /// to the Y-value of the coordinate, eg. (X + dx, Y + dy). Provided for convenience.
        /// </summary>
        /// <param name="dx">Delta x to add to coordinate.</param>
        /// <param name="dy">Delta y to add to coordinate.</param>
        /// <returns>The coordinate (X + dx, Y + dy)</returns>
        public Coord Translate(int dx, int dy) => Get(X + dx, Y + dy);

        /// <summary>
        /// Returns the coordinate resulting from adding dx to the X-value of the coordinate, and dy
        /// to the Y-value of the coordinate, eg. (X + dx, Y + dy). Provided for convenience.
        /// </summary>
        /// <param name="deltaChange">Vector where deltaChange.X represents the delta-x value and
        /// deltaChange.Y represents the delta-y value.</param>
        /// <returns>The coordinate (X + deltaChange.X, Y + deltaChange.Y)</returns>
        public Coord Translate(Coord deltaChange) => Get(X + deltaChange.X, Y + deltaChange.Y);
    }
}