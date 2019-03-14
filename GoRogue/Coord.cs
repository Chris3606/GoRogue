using System;
using System.Drawing;
using DrawingPoint = System.Drawing.Point;
using MonoPoint = Microsoft.Xna.Framework.Point;

namespace GoRogue
{
	/// <summary>
	/// A structure that represents a standard 2D grid coordinate.  Provides numerous static functions,
	/// operators, and implicit converstions that enable common grid/position-related math and operations,
	/// as well as enable interoperability with position representations from other libraries.
	/// </summary>
	/// <remarks>
	/// Coord instances can be created using the standard Coord c = new Coord(x, y) syntax.  In addition,
	/// you may create a coord from a c# 7 tuple, like Coord c = (x, y);.  As well, Coord is implicitly convertible
	/// to position-based types from a few other supported libraries (like MonoGame), so you can even do things like
	/// Coord c = new Microsoft.Xna.Point(x, y);.  As well, Coord supports C# Deconstrution syntax.
	///
	/// In addition to implicit tuple/custom type converters, Coord provides operators and static helper functions that
	/// perform common grid math/operations, as well as interoperability with other grid-based classes like
	/// <see cref="Direction"/>  Generally speaking, operators also support interoperability with supported position
	/// representations from other libraries.  Functions taking a Coord also take any other type which Coord implicitly
	/// converts to.  Similarly, operator overloads are defined that support things like addition of Coord and supported
	/// types from other libraries.  For example, since Coord c = myCoord + myCoord2; is valid, so is
	/// Coord c = myCoord + myMicrosoftXnaPoint and Microsoft.Xna.Point point = myMicrosftXnaPoint + myCoord;.
	///
	/// Coord is designed to be extremely efficient and interoperable with equivalent representations in other libraries,
	/// so in general, in an environment where you have multiple point representations floating around, it is best to prefer
	/// Coord where possible, as something that accepts or works with Coord will generally work with other supported types
	/// as well.
	/// </remarks>
	public struct Coord : IEquatable<Coord>
	{
		/// <summary>
		/// Coord value that represents None or no position (since Coord is not a nullable type).
		/// Typically you would use this constant instead of null.
		/// </summary>
		/// <remarks>
		/// This constant has (x, y) values (int.MinValue, int.MinValue), so a coordinate with those
		/// x/y values is not considered a valid coordinate by many GoRogue functions.
		/// </remarks>
		public static readonly Coord NONE = new Coord(int.MinValue, int.MinValue);

		/// <summary>
		/// X-value of the coordinate.
		/// </summary>
		public readonly int X;

		/// <summary>
		/// Y-value of the coordinate.
		/// </summary>
		public readonly int Y;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="x">X-value for the coordinate.</param>
		/// <param name="y">Y-value for the coordinate.</param>
		public Coord(int x, int y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Calculates degree bearing of the line (start =&gt; end), where 0 points in the direction <see cref="Direction.UP"/>.
		/// </summary>
		/// <param name="start">Position of line starting point.</param>
		/// <param name="end">Position of line ending point.</param>
		/// <returns>The degree bearing of the line specified by the two given points.</returns>
		public static double BearingOfLine(Coord start, Coord end) => BearingOfLine(end.X - start.X, end.Y - start.Y);

		/// <summary>
		/// Calculates degree bearing of the line ((startX, startY) =&gt; (endX, endY)), where 0 points
		/// in the direction <see cref="Direction.UP"/>.
		/// </summary>
		/// <param name="startX">X-value of the position of line starting point.</param>
		/// <param name="startY">Y-value of the position of line starting point.</param>
		/// <param name="endX">X-value of the position of line ending point.</param>
		/// <param name="endY">Y-value of the position of line ending point.</param>
		/// <returns>The degree bearing of the line specified by the two given points.</returns>
		public static double BearingOfLine(int startX, int startY, int endX, int endY) => BearingOfLine(endX - startX, endY - startY);

		/// <summary>
		/// Calculates the degree bearing of a line with the given delta-x and delta-y values, where
		/// 0 degreees points in the direction <see cref="Direction.UP"/>.
		/// </summary>
		/// <param name="deltaChange">
		/// Vector, where deltaChange.X is the change in x-values across the line, and deltaChange.Y
		/// is the change in y-values across the line.
		/// </param>
		/// <returns>The degree bearing of the line with the given dx and dy values.</returns>
		public static double BearingOfLine(Coord deltaChange) => BearingOfLine(deltaChange.X, deltaChange.Y);

		/// <summary>
		/// Calculates the degree bearing of a line with the given delta-x and delta-y values, where
		/// 0 degreees points in the direction <see cref="Direction.UP"/>.
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
		/// Returns the result of the euclidean distance formula, without the square root -- eg., (x2
		/// - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1). Use this if you only care about the magnitude
		/// of the distance -- eg., if you're trying to compare two distances. Omitting the square
		/// root provides a speed increase.
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
		/// dx and dy values between two points -- eg., (deltaChange.X * deltaChange.X) + (deltaChange.Y
		/// * deltaChange.Y). Use this if you only care about the magnitude of the distance -- eg., if
		/// you're trying to compare two distances. Omitting the square root provides a speed increase.
		/// </summary>
		/// <param name="deltaChange">
		/// Vector, where deltaChange.X is the change in x-values between the two points, and
		/// deltaChange.Y is the change in y-values between the two points.
		/// </param>
		/// <returns>
		/// The "magnitude" of the euclidean distance of two locations with the given dx and dy
		/// values -- basically the distance formula without the square root.
		/// </returns>
		public static double EuclideanDistanceMagnitude(Coord deltaChange) => EuclideanDistanceMagnitude(deltaChange.X, deltaChange.Y);

		/// <summary>
		/// Returns the result of the euclidean distance formula, without the square root, given the
		/// dx and dy values between two points -- eg., (dx * dx) + (dy * dy). Use this if you only
		/// care about the magnitude of the distance -- eg., if you're trying to compare two distances.
		/// Omitting the square root provides a speed increase.
		/// </summary>
		/// <param name="dx">The change in x-values between the two points.</param>
		/// <param name="dy">The change in y-values between the two points.</param>
		/// <returns>
		/// The "magnitude" of the euclidean distance of two locations with the given dx and dy
		/// values -- basically the distance formula without the square root.
		/// </returns>
		public static double EuclideanDistanceMagnitude(int dx, int dy) => dx * dx + dy * dy;


		/// <summary>
		/// Returns the midpoint between the two points.
		/// </summary>
		/// <param name="c1">The first point.</param>
		/// <param name="c2">The second point.</param>
		/// <returns>The midpoint between <paramref name="c1"/> and <paramref name="c2"/>.</returns>
		public static Coord Midpoint(Coord c1, Coord c2) =>
			new Coord((int)Math.Round((c1.X + c2.X) / 2.0f, MidpointRounding.AwayFromZero), (int)Math.Round((c1.Y + c2.Y) / 2.0f, MidpointRounding.AwayFromZero));

		/// <summary>
		/// Returns the midpoint between the two points.
		/// </summary>
		/// <param name="x1">The x-value of the first location.</param>
		/// <param name="y1">The y-value of the first location.</param>
		/// <param name="x2">The x-value of the second location.</param>
		/// <param name="y2">The y-value of the second location.</param>
		/// <returns>The midpoint between the two points.</returns>
		public static Coord Midpoint(int x1, int y1, int x2, int y2) => Midpoint(new Coord(x1, y1), new Coord(x2, y2));

		/// <summary>
		/// Returns the coordinate (c1.X - c2.X, c1.Y - c2.Y)
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <returns>The coordinate(<paramref name="c1"/> - <paramref name="c2"/>).</returns>
		public static Coord operator -(Coord c1, Coord c2) => new Coord(c1.X - c2.X, c1.Y - c2.Y);

		/// <summary>
		/// Subtracts scalar <paramref name="i"/> from the x and y values of <paramref name="c"/>.
		/// </summary>
		/// <param name="c"></param>
		/// <param name="i"></param>
		/// <returns>The coordinate (c.X - <paramref name="i"/>, c.Y - <paramref name="i"/>)</returns>
		public static Coord operator -(Coord c, int i) => new Coord(c.X - i, c.Y - i);

		/// <summary>
		/// True if either the x-values or y-values are not equal.
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <returns>
		/// True if either the x-values or y-values are not equal, false if they are both equal.
		/// </returns>
		public static bool operator !=(Coord c1, Coord c2) => !(c1 == c2);

		/// <summary>
		/// Multiplies the x and y of <paramref name="c"/> by <paramref name="i"/>.
		/// </summary>
		/// <param name="c"></param>
		/// <param name="i"></param>
		/// <returns>Coordinate (c.X * <paramref name="i"/>, c.Y * <paramref name="i"/>)</returns>
		public static Coord operator *(Coord c, int i) => new Coord(c.X * i, c.Y * i);

		/// <summary>
		/// Multiplies the x and y value of <paramref name="c"/> by <paramref name="i"/>, rounding
		/// the result to the nearest integer.
		/// </summary>
		/// <param name="c"></param>
		/// <param name="i"></param>
		/// <returns>
		/// Coordinate (c.X * <paramref name="i"/>, c.Y * <paramref name="i"/>), with the resulting values
		/// rounded to nearest integer.
		/// </returns>
		public static Coord operator *(Coord c, double i) =>
			new Coord((int)Math.Round(c.X * i, MidpointRounding.AwayFromZero), (int)Math.Round(c.Y * i, MidpointRounding.AwayFromZero));

		/// <summary>
		/// Divides the x and y of <paramref name="c"/> by <paramref name="i"/>, rounding resulting values
		/// to the nearest integer.
		/// </summary>
		/// <param name="c"></param>
		/// <param name="i"></param>
		/// <returns>(c.X / <paramref name="i"/>, c.Y / <paramref name="i"/>), with the resulting values rounded to the nearest integer.</returns>
		public static Coord operator /(Coord c, int i) =>
			new Coord((int)Math.Round(c.X / (double)i, MidpointRounding.AwayFromZero), (int)Math.Round(c.Y / (double)i, MidpointRounding.AwayFromZero));

		/// <summary>
		/// Divides the x and y of <paramref name="c"/> by <paramref name="i"/>, rounding resulting values
		/// to the nearest integer.
		/// </summary>
		/// <param name="c"></param>
		/// <param name="i"></param>
		/// <returns>(c.X / <paramref name="i"/>, c.Y / <paramref name="i"/>), with the resulting values rounded to the nearest integer.</returns>
		public static Coord operator /(Coord c, double i) =>
			new Coord((int)Math.Round(c.X / i, MidpointRounding.AwayFromZero), (int)Math.Round(c.Y / i, MidpointRounding.AwayFromZero));

		/// <summary>
		/// Returns the coordinate (c1.X + c2.X, c1.Y + c2.Y).
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <returns>The coordinate (c1.X + c2.X, c1.Y + c2.Y)</returns>
		public static Coord operator +(Coord c1, Coord c2) => new Coord(c1.X + c2.X, c1.Y + c2.Y);

		/// <summary>
		/// Adds scalar i to the x and y values of <paramref name="c"/>.
		/// </summary>
		/// <param name="c"></param>
		/// <param name="i"></param>
		/// <returns>Coordinate (c.X + <paramref name="i"/>, c.Y + <paramref name="i"/>.</returns>
		public static Coord operator +(Coord c, int i) => new Coord(c.X + i, c.Y + i);

		/// <summary>
		/// Translates the given coordinate by one unit in the given direction.
		/// </summary>
		/// <param name="c"></param>
		/// <param name="d"></param>
		/// <returns>
		/// Coordinate (c.X + d.DeltaX, c.Y + d.DeltaY)
		/// </returns>
		public static Coord operator +(Coord c, Direction d) => new Coord(c.X + d.DeltaX, c.Y + d.DeltaY);

		/// <summary>
		/// True if c1.X == c2.X and c1.Y == c2.Y.
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
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
		/// Reverses the ToIndex functions, returning the position represented by a given index.
		/// </summary>
		/// <param name="index">The index in 1D form.</param>
		/// <param name="width">The width of the 2D array.</param>
		/// <returns>The position represented by the 1D index given.</returns>
		public static Coord ToCoord(int index, int width) => new Coord(index % width, index / width);

		/// <summary>
		/// Returns <paramref name="y"/> * <paramref name="width"/> + <paramref name="x"/>.
		/// </summary>
		/// <param name="x">X-value of the coordinate.</param>
		/// <param name="y">Y-value of the coordinate.</param>
		/// <param name="width">The width of the 2D array, used to do the math to calculate index.</param>
		/// <returns>The 1D index of this Coord.</returns>
		public static int ToIndex(int x, int y, int width) => y * width + x;

		/// <summary>
		/// Reverses the ToIndex functions, returning only the X-value for the given index.
		/// </summary>
		/// <param name="index">The index in 1D form.</param>
		/// <param name="width">The width of the 2D array.</param>
		/// <returns>The X-value for the location represented by the given index.</returns>
		public static int ToXValue(int index, int width) => index % width;

		/// <summary>
		/// Reverses the ToIndex functions, returning only the Y-value for the given index.
		/// </summary>
		/// <param name="index">The index in 1D form.</param>
		/// <param name="width">The width of the 2D array.</param>
		/// <returns>The Y-value for the location represented by the given index.</returns>
		public static int ToYValue(int index, int width) => index / width;

		/// <summary>
		/// Same as operator == in this case; returns false if <paramref name="obj"/> is not a Coord.
		/// </summary>
		/// <param name="obj">The object to compare the current Coord to.</param>
		/// <returns>
		/// True if <paramref name="obj"/> is a Coord instance, and the two coordinates are equal, false otherwise.
		/// </returns>
		public override bool Equals(object obj) => obj is Coord c && Equals(c); // If cast is null, operator== will take care of it.

		/// <summary>
		/// Returns a hash code for the Coord. The important parts: it should be fairly fast and it
		/// does not collide often.
		/// </summary>
		/// <remarks>
		/// This hashing algorithm uses a seperate bit-mixing algorithm for <see cref="X"/> and
		/// <see cref="Y"/>, with X and Y each multiplied by a differet large integer, then xors
		/// the mixed values, does a right shift, and finally multiplies by an overflowing prime
		/// number.  This hashing algorithm should produce an exceptionally low collision rate for
		/// coordinates between (0, 0) and (255, 255).
		/// </remarks>
		/// <returns>The hash-code for the Coord.</returns>
		public override int GetHashCode()
		{
			// Intentional overflow on both of these, part of hash-code generation
			int x2 = (int)(0x9E3779B9 * X), y2 = 0x632BE5AB * Y;
			return (int)(((uint)(x2 ^ y2) >> ((x2 & 7) + (y2 & 7))) * 0x85157AF5);
		}

		/// <summary>
		/// Returns a value that can be used to uniquely index this location 1D array.
		/// </summary>
		/// <param name="width">The width of the 2D map/array this location is referring to --
		/// used to do the math to calculate index.</param>
		/// <returns>The 1D index of this Coord.</returns>
		public int ToIndex(int width) => Y * width + X;

		/// <summary>
		/// Returns representation (X, Y).
		/// </summary>
		/// <returns>String (X, Y)</returns>
		public override string ToString() => $"({X},{Y})";

		/// <summary>
		/// Translates the coordinate by the given dx and dy values.
		/// </summary>
		/// <param name="dx">Delta x to add to coordinate.</param>
		/// <param name="dy">Delta y to add to coordinate.</param>
		/// <returns>The coordinate (<see cref="X"/> + <paramref name="dx"/>, <see cref="Y"/> + <paramref name="dy"/>)</returns>
		public Coord Translate(int dx, int dy) => new Coord(X + dx, Y + dy);

		/// <summary>
		/// Returns the coordinate resulting from adding dx to the X-value of the coordinate, and dy
		/// to the Y-value of the coordinate.
		/// </summary>
		/// <param name="deltaChange">
		/// Vector where deltaChange.X represents the delta-x value and deltaChange.Y represents the
		/// delta-y value.
		/// </param>
		/// <returns>The coordinate (<see cref="X"/> + deltaChange.X, <see cref="Y"/> + deltaChange.Y)</returns>
		public Coord Translate(Coord deltaChange) => new Coord(X + deltaChange.X, Y + deltaChange.Y);
		
		/// <summary>
		/// True if the given coordinate has equal x and y values to the current one.
		/// </summary>
		/// <param name="other">Coordinate to compare.</param>
		/// <returns>True if the two coordinates are equal, false if not.</returns>
		public bool Equals(Coord other) => X == other.X && Y == other.Y;

		#region MonoGame Compatibility
		/// <summary>
		/// Implicitly converts a Coord to an equivalent MonoGame point.
		/// </summary>
		/// <param name="c" />
		/// <returns />
		public static implicit operator MonoPoint(Coord c) => new MonoPoint(c.X, c.Y);
		/// <summary>
		/// Implicitly converts a MonoGame Point to an equivalent Coord.
		/// </summary>
		/// <param name="p" />
		/// <returns />
		public static implicit operator Coord(MonoPoint p) => new Coord(p.X, p.Y);
		/// <summary>
		/// Adds the x and y values of a Coord to a MonoGame Point.
		/// </summary>
		/// <param name="p" />
		/// <param name="c" />
		/// <returns>A MonoGame Point (p.X + c.X, p.Y + c.Y).</returns>
		public static MonoPoint operator +(MonoPoint p, Coord c) => new MonoPoint(p.X + c.X, p.Y + c.Y);
		/// <summary>
		/// Adds the x and y values of a MonoGame Point to a Coord.
		/// </summary>
		/// <param name="c" />
		/// <param name="p" />
		/// <returns>A Coord (c.X + p.X, c.Y + p.Y).</returns>
		public static Coord operator +(Coord c, MonoPoint p) => new Coord(c.X + p.X, c.Y + p.Y);
		
		/// <summary>
		/// Subtracts the x and y values of a Coord from a MonoGame Point.
		/// </summary>
		/// <param name="p" />
		/// <param name="c" />
		/// <returns>A MonoGame Point (p.X - c.X, p.Y - c.Y).</returns>
		public static MonoPoint operator -(MonoPoint p, Coord c) => new MonoPoint(p.X - c.X, p.Y - c.Y);
		/// <summary>
		/// Subtracts the x and y values of a MonoGame Point from a Coord.
		/// </summary>
		/// <param name="c" />
		/// <param name="p" />
		/// <returns>A Coord (c.X - p.X, c.Y - p.Y).</returns>
		public static Coord operator -(Coord c, MonoPoint p) => new Coord(c.X - p.X, c.Y - p.Y);
		#endregion

		#region System.Drawing Compatibility
		/// <summary>
		/// Implicitly converts a Coord to an equivalent System.Drawing.Point.
		/// </summary>
		/// <param name="c" />
		/// <returns />
		public static implicit operator DrawingPoint(Coord c) => new DrawingPoint(c.X, c.Y);
		/// <summary>
		/// Implicitly converts a System.Drawing.Point to an equivalent Coord.
		/// </summary>
		/// <param name="p" />
		/// <returns />
		public static implicit operator Coord(DrawingPoint p) => new Coord(p.X, p.Y);
		/// <summary>
		/// Implicitly converts a Coord to an equivalent System.Drawing.PointF.
		/// </summary>
		/// <param name="c" />
		/// <returns />
		public static implicit operator PointF(Coord c) => new PointF(c.X, c.Y);
		/// <summary>
		/// Implicitly converts a Coord to an equivalent System.Drawing.Size.
		/// </summary>
		/// <param name="c" />
		/// <returns />
		public static implicit operator Size(Coord c) => new Size(c.X, c.Y);
		/// <summary>
		/// Implicitly converts a System.Drawing.Size to an equivalent Coord.
		/// </summary>
		/// <param name="s" />
		/// <returns />
		public static implicit operator Coord(Size s) => new Coord(s.Width, s.Height);
		/// <summary>
		/// Implicitly converts a Coord to an equivalent System.Drawing.SizeF.
		/// </summary>
		/// <param name="c" />
		/// <returns />
		public static implicit operator SizeF(Coord c) => new SizeF(c.X, c.Y);
		
		/// <summary>
		/// Adds the x and y values of a Coord to a System.Drawing.Point.
		/// </summary>
		/// <param name="p" />
		/// <param name="c" />
		/// <returns>A System.Drawing.Point (p.X + c.X, p.Y + c.Y).</returns>
		public static DrawingPoint operator +(DrawingPoint p, Coord c) => new DrawingPoint(p.X + c.X, p.Y + c.Y);
		/// <summary>
		/// Adds the x and y values of a System.Drawing.Point to a Coord.
		/// </summary>
		/// <param name="c" />
		/// <param name="p" />
		/// <returns>A Coord (c.X + p.X, c.Y + p.Y).</returns>
		public static Coord operator +(Coord c, DrawingPoint p) => new Coord(c.X + p.X, c.Y + p.Y);
		/// <summary>
		/// Adds the x and y values of a Coord to a System.Drawing.PointF.
		/// </summary>
		/// <param name="p" />
		/// <param name="c" />
		/// <returns>A System.Drawing.PointF (p.X + c.X, p.Y + c.Y).</returns>
		public static PointF operator +(PointF p, Coord c) => new PointF(p.X + c.X, p.Y + c.Y);
		
		/// <summary>
		/// Subtracts the x and y values of a Coord from a System.Drawing.Point.
		/// </summary>
		/// <param name="p" />
		/// <param name="c" />
		/// <returns>A System.Drawing.Point (p.X - c.X, p.Y - c.Y).</returns>
		public static DrawingPoint operator -(DrawingPoint p, Coord c) => new DrawingPoint(p.X - c.X, p.Y - c.Y);
		/// <summary>
		/// Subtracts the x and y values of a System.Drawing.Point from a Coord.
		/// </summary>
		/// <param name="c" />
		/// <param name="p" />
		/// <returns>A Coord (c.X - p.X, c.Y - p.Y).</returns>
		public static Coord operator -(Coord c, DrawingPoint p) => new Coord(c.X - p.X, c.Y - p.Y);
		/// <summary>
		/// Subtracts the x and y values of a Coord from a System.Drawing.PointF.
		/// </summary>
		/// <param name="p" />
		/// <param name="c" />
		/// <returns>A System.Drawing.PointF (p.X - c.X, p.Y - c.Y).</returns>
		public static PointF operator -(PointF p, Coord c) => new PointF(p.X - c.X, p.Y - c.Y);
		
		/// <summary>
		/// Adds the x and y values of a Coord to a System.Drawing.Size.
		/// </summary>
		/// <param name="s" />
		/// <param name="c" />
		/// <returns>A System.Drawing.Size (s.Width + c.X, s.Height + c.Y).</returns>
		public static Size operator +(Size s, Coord c) => new Size(s.Width + c.X, s.Height + c.Y);
		/// <summary>
		/// Adds the x and y values of a System.Drawing.Size to a Coord.
		/// </summary>
		/// <param name="c" />
		/// <param name="s" />
		/// <returns>A Coord (c.X + s.Width, c.Y + s.Height).</returns>
		public static Coord operator +(Coord c, Size s) => new Coord(c.X + s.Width, c.Y + s.Height);
		/// <summary>
		/// Adds the x and y values of a Coord to a System.Drawing.SizeF.
		/// </summary>
		/// <param name="s" />
		/// <param name="c" />
		/// <returns>A System.Drawing.SizeF (s.Width + c.X, s.Height + c.Y).</returns>
		public static SizeF operator +(SizeF s, Coord c) => new SizeF(s.Width + c.X, s.Height + c.Y);
		
		/// <summary>
		/// Subtracts the x and y values of a Coord from a System.Drawing.Size.
		/// </summary>
		/// <param name="s" />
		/// <param name="c" />
		/// <returns>A System.Drawing.Size (s.Width - c.X, s.Height - c.Y).</returns>
		public static Size operator -(Size s, Coord c) => new Size(s.Width - c.X, s.Height - c.Y);
		/// <summary>
		/// Subtracts the x and y values of a System.Drawing.Size from a Coord.
		/// </summary>
		/// <param name="c" />
		/// <param name="s" />
		/// <returns>A Coord (c.X - s.Width, c.Y - s.Height).</returns>
		public static Coord operator -(Coord c, Size s) => new Coord(c.X - s.Width, c.Y - s.Height);
		/// <summary>
		/// Subtracts the x and y values of a Coord from a System.Drawing.SizeF.
		/// </summary>
		/// <param name="s" />
		/// <param name="c" />
		/// <returns>A System.Drawing.SizeF (s.Width - c.X, s.Height - c.Y).</returns>
		public static SizeF operator -(SizeF s, Coord c) => new SizeF(s.Width - c.X, s.Height - c.Y);
		#endregion

		#region TupleCompatibility
		/// <summary>
		/// Adds support for C# Deconstruction syntax.
		/// </summary>
		/// <param name="x" />
		/// <param name="y" />
		public void Deconstruct(out int x, out int y)
		{
			x = X;
			y = Y;
		}

		/// <summary>
		/// Implicitly converts a Coord to an equivalent tuple of two integers.
		/// </summary>
		/// <param name="c" />
		/// <returns />
		public static implicit operator (int x, int y) (Coord c) => (c.X, c.Y);
		/// <summary>
		/// Implicitly converts a tuple of two integers to an equivalent Coord.
		/// </summary>
		/// <param name="tuple" />
		/// <returns />
		public static implicit operator Coord((int x, int y) tuple) => new Coord(tuple.x, tuple.y);
		
		/// <summary>
		/// Adds the x and y values of a Coord to the corresponding values of a tuple of two integers.
		/// </summary>
		/// <param name="tuple" />
		/// <param name="c" />
		/// <returns>A tuple (tuple.x + c.X, tuple.y + c.Y).</returns>
		public static (int x, int y) operator +((int x, int y) tuple, Coord c) => (tuple.x + c.X, tuple.y + c.Y);
		/// <summary>
		/// Adds the x and y values of a tuple of two integers to a Coord.
		/// </summary>
		/// <param name="c" />
		/// <param name="tuple" />
		/// <returns>A Coord (c.X + tuple.x, c.Y + tuple.y).</returns>
		public static Coord operator +(Coord c, (int x, int y) tuple) => new Coord(c.X + tuple.x, c.Y + tuple.y);
		
		/// <summary>
		/// Subtracts the x and y values of a Coord from a tuple of two integers.
		/// </summary>
		/// <param name="tuple" />
		/// <param name="c" />
		/// <returns>A tuple (tuple.x - c.X, tuple.y - c.Y).</returns>
		public static (int x, int y) operator -((int x, int y) tuple, Coord c) => (tuple.x - c.X, tuple.y - c.Y);
		/// <summary>
		/// Subtracts the x and y values of a tuple of two integers from a Coord.
		/// </summary>
		/// <param name="c" />
		/// <param name="tuple" />
		/// <returns>A Coord (c.X - tuple.x, c.Y - tuple.y).</returns>
		public static Coord operator -(Coord c, (int x, int y) tuple) => new Coord(c.X - tuple.x, c.Y - tuple.y);
		#endregion
	}
}
