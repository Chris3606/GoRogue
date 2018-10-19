using GoRogue.MapGeneration;
using GoRogue.Random;
using System;
using System.Collections.Generic;
using Troschuetz.Random;

namespace GoRogue
{
	/// <summary>
	/// Represents a rectangle in terms of grid squares. Provides numerous functions pertaining to area.
	/// </summary>
	public struct Rectangle : IEquatable<Rectangle>
	{
		/// <summary>
		/// The empty rectangle. Has origin of (0, 0) with 0 width and height.
		/// </summary>
		public static readonly Rectangle EMPTY = new Rectangle(0, 0, 0, 0);

		/// <summary>
		/// Constructor. Takes the minimum x and y values of the rectangle, along with the width and height.
		/// </summary>
		/// <param name="x">Minimum x coordinate that is inside the rectangle.</param>
		/// <param name="y">Minimum y coordinate that is inside the rectangle.</param>
		/// <param name="width">Width of the rectangle.</param>
		/// <param name="height">Height of the rectangle.</param>
		public Rectangle(int x, int y, int width, int height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Constructor. Takes the minimum and maximum points that are considered within the rectangle.
		/// </summary>
		/// <param name="minExtent">Minimum x and y values that are considered inside the rectangle.</param>
		/// <param name="maxExtent">Maximum x and y values that are considered inside the rectangle.</param>
		public Rectangle(Coord minExtent, Coord maxExtent)
		{
			X = minExtent.X;
			Y = minExtent.Y;
			Width = maxExtent.X - X + 1;
			Height = maxExtent.Y - Y + 1;
		}

		/// <summary>
		/// Constructor. Takes a center point, and horizontal/vertical radius defining the bounds.
		/// </summary>
		/// <param name="center">The center point of the rectangle.</param>
		/// <param name="horizontalRadius">
		/// Number of units to the left and right of the center point that are included within the rectangle.
		/// </param>
		/// <param name="verticalRadius">
		/// Number of units to the top and bottom of the center point that are included within the rectangle.
		/// </param>
		public Rectangle(Coord center, int horizontalRadius, int verticalRadius)
		{
			X = center.X - horizontalRadius;
			Y = center.Y - verticalRadius;
			Width = 2 * horizontalRadius + 1;
			Height = 2 * verticalRadius + 1;
		}

		/// <summary>
		/// Calculates the area of the rectangle.
		/// </summary>
		public int Area { get => Width * Height; }

		/// <summary>
		/// The center coordinate of the rectangle, rounded up if the exact center is between two
		/// positions. The center of a rectangle with width/height 1 is its Position.
		/// </summary>
		public Coord Center
		{
			get => Coord.Get(X + (Width / 2), Y + (Height / 2));
		}

		/// <summary>
		/// The height of the rectangle.
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		/// Whether or not this rectangle is empty (has width and height of 0).
		/// </summary>
		public bool IsEmpty { get => (Width == 0 && Height == 0); }

		/// <summary>
		/// The maximum X and Y coordinates that are included in the rectangle.
		/// </summary>
		public Coord MaxExtent
		{
			get => Coord.Get(MaxExtentX, MaxExtentY);
		}

		/// <summary>
		/// The maximum X-coordinate that is included in the rectangle.
		/// </summary>
		public int MaxExtentX
		{
			get => X + Width - 1;
		}

		/// <summary>
		/// The maximum Y-coordinate that is included in the rectangle.
		/// </summary>
		public int MaxExtentY
		{
			get => Y + Height - 1;
		}

		/// <summary>
		/// Minimum extent of the rectangle (minimum x and y values that are included within it).
		/// Identical to the Position because we define the rectangle's position by its minimum extent.
		/// </summary>
		public Coord MinExtent { get => Coord.Get(X, Y); }

		/// <summary>
		/// X-value of the minimum extent of the rectangle (minimum x value that is included within
		/// it). Identical to the X value because we define the rectangle's position by its minimum extent.
		/// </summary>
		public int MinExtentX { get => X; }

		/// <summary>
		/// Y-value of the minimum extent of the rectangle (minimum y value that is included within
		/// it). Identical to the Y value because we define the rectangle's position by its minimum extent.
		/// </summary>
		public int MinExtentY { get => Y; }

		/// <summary>
		/// Calculates the perimeter length of the rectangle.
		/// </summary>
		public int Perimeter { get => (2 * Width) + (2 * Height); }

		/// <summary>
		/// Coord representing the position (min x- and y-values) of the rectangle.
		/// </summary>
		public Coord Position
		{
			get => Coord.Get(X, Y);
		}

		/// <summary>
		/// Returns a coordinate (Width, Height), which represents the size of the rectangle.
		/// </summary>
		public Coord Size { get => Coord.Get(Width, Height); }

		/// <summary>
		/// The width of the rectangle.
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// X-coordinate of position of the rectangle.
		/// </summary>
		public int X { get; private set; }

		/// <summary>
		/// Y-coordinate of position of the rectangle.
		/// </summary>
		public int Y { get; private set; }

		/// <summary>
		/// Creates a rectangle with the given position and size.  Effectively a constructor, but with extra overloads not possible to provide in constructors alone.
		/// </summary>
		/// <param name="x">Minimum x coordinate that is inside the rectangle.</param>
		/// <param name="y">Minimum y coordinate that is inside the rectangle.</param>
		/// <param name="width">Width of the rectangle.</param>
		/// <param name="height">Height of the rectangle.</param>>
		/// <returns>A new rectangle at the given position with the given width and height.</returns>
		public static Rectangle CreateWithSize(int x, int y, int width, int height) => new Rectangle(x, y, width, height);

		/// <summary>
		/// Creates a rectangle with the given position and size.  Effectively a constructor, but with extra overloads not possible to provide in constructors alone.
		/// </summary>
		/// <param name="position">Minimum (x, y) values that are inside the resulting rectangle.</param>
		/// <param name="size">The size of the rectangle, in form (width, height).</param>
		/// <returns>A new rectangle at the given position with the given size.</returns>
		public static Rectangle CreateWithSize(Coord position, Coord size) => new Rectangle(position.X, position.Y, size.X, size.Y);

		/// <summary>
		/// Creates a rectangle with the given minimum and maximum extents. Effectively a constructor, but with extra overloads not possible to provide in constructors alone. 
		/// </summary>
		/// <param name="minX">Minimum x coordinate that is inside the rectangle.</param>
		/// <param name="minY">Minimum y coordinate that is inside the rectangle.</param>
		/// <param name="maxX">Maximum x coordinate that is inside the rectangle.</param>
		/// <param name="maxY">Maximum y coordinate that is inside the rectangle.</param>
		/// <returns>A new Rectangle with the given minimum and maximum extents.</returns>
		public static Rectangle CreateWithExtents(int minX, int minY, int maxX, int maxY) => new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);

		/// <summary>
		/// Creates a rectangle with the given minimum and maximum extents. Effectively a constructor, but with extra overloads not possible to provide in constructors alone. 
		/// </summary>
		/// <param name="minExtent">Minimum (x, y) coordinates that are inside the rectangle.</param>
		/// <param name="maxExtent">Maximum (x, y) coordinates that are inside the rectangle.</param>
		/// <returns>A new Rectangle with the given minimum and maximum extents.</returns>
		public static Rectangle CreateWithExtents(Coord minExtent, Coord maxExtent) => new Rectangle(minExtent, maxExtent);

		/// <summary>
		/// Creates a rectangle centered on the given position, with the given horizontal and vertical radius values.  Effectively a constructor, but with extra overloads
		/// not possible to provide in constructors alone. 
		/// </summary>
		/// <param name="centerX">X-value of the center of the rectangle.</param>
		/// <param name="centerY">Y-value of the center of the rectangle.</param>
		/// <param name="horizontalRadius">Number of units to the left and right of the center point that are included within the rectangle.</param>
		/// <param name="verticalRadius">Number of units to the top and bottom of the center point that are included within the rectangle.</param>
		/// <returns>A new rectangle with the given center point and radius values.</returns>
		public static Rectangle CreateWithRadius(int centerX, int centerY, int horizontalRadius, int verticalRadius)
			=> new Rectangle(centerX - horizontalRadius, centerY - verticalRadius, 2 * horizontalRadius + 1, 2 * verticalRadius + 1);

		/// <summary>
		/// Creates a rectangle centered on the given position, with the given horizontal and vertical radius values.  Effectively a constructor, but with extra overloads
		/// not possible to provide in constructors alone.  
		/// </summary>
		/// <param name="center">Center of the rectangle.</param>
		/// <param name="horizontalRadius">Number of units to the left and right of the center point that are included within the rectangle.</param>
		/// <param name="verticalRadius">Number of units to the top and bottom of the center point that are included within the rectangle.</param>
		/// <returns>A new rectangle with the given center point and radius values.</returns>
		public static Rectangle CreateWithRadius(Coord center, int horizontalRadius, int verticalRadius) => new Rectangle(center, horizontalRadius, verticalRadius);

		/// <summary>
		/// Gets a MapArea representing every cell in rect1 that is NOT in rect2.
		/// </summary>
		/// <param name="rect1">First operand.</param>
		/// <param name="rect2">Second operand.</param>
		/// <returns>A MapArea representing every cell in rect1 that is NOT in rect2.</returns>
		public static MapArea GetDifference(Rectangle rect1, Rectangle rect2)
		{
			var retVal = new MapArea();

			foreach (var pos in rect1.Positions())
			{
				if (rect2.Contains(pos))
					continue;

				retVal.Add(pos);
			}

			return retVal;
		}

		/// <summary>
		/// Gets a MapArea representing the exact union of the specified rectangles.
		/// </summary>
		/// <param name="r1">First rectangle.</param>
		/// <param name="r2">Second rectangle.</param>
		/// <returns>A MapArea containing exactly those positions in one (or both) rectangles.</returns>
		public static MapArea GetExactUnion(Rectangle r1, Rectangle r2)
		{
			var retVal = new MapArea();

			for (int x = r1.X; x <= r1.MaxExtentX; x++)
				for (int y = r1.Y; y <= r1.MaxExtentY; y++)
					retVal.Add(x, y);

			for (int x = r2.X; x <= r2.MaxExtentX; x++)
				for (int y = r2.Y; y <= r2.MaxExtentY; y++)
					retVal.Add(x, y);

			return retVal;
		}

		/// <summary>
		/// Returns the rectangle that represents the intersection of the two rectangles specified,
		/// or the empty rectangle if the specified rectangles do not intersect.
		/// </summary>
		/// <param name="r1">First rectangle.</param>
		/// <param name="r2">Second rectangle.</param>
		/// <returns>
		/// Rectangle representing the intersection of r1 and r2, or the empty rectangle if the two
		/// rectangles do not intersect.
		/// </returns>
		public static Rectangle GetIntersection(Rectangle r1, Rectangle r2)
		{
			if (r1.Intersects(r2))
			{
				int minX = Math.Max(r1.X, r2.X);
				int minY = Math.Max(r1.Y, r2.Y);

				int exclusiveMaxX = Math.Min(r1.X + r1.Width, r2.X + r2.Width);
				int exclusiveMaxY = Math.Min(r1.Y + r1.Height, r2.Y + r2.Height);

				return new Rectangle(minX, minY, exclusiveMaxX - minX, exclusiveMaxY - minY);
			}

			return EMPTY;
		}

		/// <summary>
		/// Gets the smallest possible rectangle that includes the entire area of both r1 and r2.
		/// </summary>
		/// <param name="r1">First rectangle.</param>
		/// <param name="r2">Second rectangle.</param>
		/// <returns>
		/// The smallest possible rectangle that includes the entire area of both r1 and r2.
		/// </returns>
		public static Rectangle GetUnion(Rectangle r1, Rectangle r2)
		{
			int x = Math.Min(r1.X, r2.X);
			int y = Math.Min(r1.Y, r2.Y);

			return new Rectangle(x, y, Math.Max(r1.X + r1.Width, r2.X + r2.Width) - x, Math.Max(r1.Y + r1.Height, r2.Y + r2.Height) - y);
		}

		/// <summary>
		/// Opposite of !=.
		/// </summary>
		/// <param name="r1">First rectangle.</param>
		/// <param name="r2">Second rectangle.</param>
		/// <returns>true if the rectangles do NOT encompass the same area, false otherwise.</returns>
		public static bool operator !=(Rectangle r1, Rectangle r2)
		{
			return !(r1 == r2);
		}

		/// <summary>
		/// See Equals.
		/// </summary>
		/// <param name="r1">First rectangle.</param>
		/// <param name="r2">Second rectangle.</param>
		/// <returns>
		/// true if the area of the two rectangles encompass the exact same area, false otherwise.
		/// </returns>
		public static bool operator ==(Rectangle r1, Rectangle r2)
		{
			return r1.X == r2.X && r1.Y == r2.Y && r1.Width == r2.Width && r1.Height == r2.Height;
		}

		/// <summary>
		/// Creates and returns a new rectangle that is the same size as the current one, but with
		/// the center moved to the given position.
		/// </summary>
		/// <param name="center">The center-point for the new Rectangle.</param>
		/// <returns>
		/// A new Rectangle that is the same size as the current one, but with the center moved to
		/// the given location.
		/// </returns>
		public Rectangle CenterOn(Coord center)
			=> new Rectangle(center.X - (Width / 2), center.Y - (Height / 2), Width, Height);

		/// <summary>
		/// Creates and returns a new rectangle that is the same size as the current one, but with
		/// the center moved to the given position.
		/// </summary>
		/// <param name="x">X-value for the center-point of the new Rectangle.</param>
		/// <param name="y">Y-value for the center-point of the new Rectangle.</param>
		/// <returns>
		/// A new Rectangle that is the same size as the current one, but with the center moved to
		/// the given location.
		/// </returns>
		public Rectangle CenterOn(int x, int y) => CenterOn(Coord.Get(x, y));

		/// <summary>
		/// Creates and returns a new Rectangle whose position is the same as the current one, but
		/// has its height changed by the given delta-change value.
		/// </summary>
		/// <param name="deltaHeight">Delta-change for the height of the new Rectangle.</param>
		/// <returns>A new Rectangle whose height is modified by the given delta-change value.</returns>
		public Rectangle ChangeHeight(int deltaHeight)
			=> new Rectangle(X, Y, Width, Height + deltaHeight);

		/// <summary>
		/// Creates and returns a new Rectangle whose position is the same as the current one, but
		/// has its width and height changed by the given delta-change values.
		/// </summary>
		/// <param name="deltaWidth">Delta-change for the width of the new rectangle.</param>
		/// <param name="deltaHeight">Delta-change for the height of the new rectangle.</param>
		/// <returns>
		/// A new rectangle whose width/height are modified by the given delta-change values.
		/// </returns>
		public Rectangle ChangeSize(int deltaWidth, int deltaHeight)
			=> new Rectangle(X, Y, Width + deltaWidth, Height + deltaHeight);

		/// <summary>
		/// Creates and returns a new Rectangle whose position is the same as the current one, but
		/// has its width and height changed by the given delta-change values.
		/// </summary>
		/// <param name="deltaChange">
		/// Vector (deltaWidth, deltaHeight) specifying the delta-change values for the width/height
		/// of the new Rectangle.
		/// </param>
		/// <returns>
		/// A new rectangle whose width/height are modified by the given delta-change values.
		/// </returns>
		public Rectangle ChangeSize(Coord deltaChange)
			=> new Rectangle(X, Y, Width + deltaChange.X, Height + deltaChange.Y);

		/// <summary>
		/// Creates and returns a new Rectangle whose position is the same as the current one, but
		/// has its width changed by the given delta-change value.
		/// </summary>
		/// <param name="deltaWidth">Delta-change for the width of the new Rectangle.</param>
		/// <returns>A new Rectangle whose width is modified by the given delta-change value.</returns>
		public Rectangle ChangeWidth(int deltaWidth)
			=> new Rectangle(X, Y, Width + deltaWidth, Height);

		/// <summary>
		/// Returns whether or not the specified point is considered within the rectangle.
		/// </summary>
		/// <param name="position">The position to check.</param>
		/// <returns>Whether or not the specified point is considered within the rectangle.</returns>
		public bool Contains(Coord position)
		{
			return (position.X >= X && position.X < (X + Width) && position.Y >= Y && position.Y < (Y + Height));
		}

		/// <summary>
		/// Returns whether or not the specified point is considered within the rectangle.
		/// </summary>
		/// <param name="x">The x-value position to check.</param>
		/// <param name="y">The y-value position to check.</param>
		/// <returns>Whether or not the specified point is considered within the rectangle.</returns>
		public bool Contains(int x, int y) => Contains(Coord.Get(x, y));

		/// <summary>
		/// Returns whether or not the specified rectangle is considered completely contained within
		/// the current one.
		/// </summary>
		/// <param name="other">The rectangle to check.</param>
		/// <returns>
		/// True if the given rectangle is completely contained within the current one, false otherwise.
		/// </returns>
		public bool Contains(Rectangle other)
		{
			return (X <= other.X && other.X + other.Width <= X + Width && Y <= other.Y && other.Y + other.Height <= Y + Height);
		}

		/// <summary>
		/// Compares based upon whether or not the areas contained within the rectangle are identical
		/// in both position and size.
		/// </summary>
		/// <param name="other">Rectangle to compare the current one to.</param>
		/// <returns>
		/// true if the area of the two rectangles encompass the exact same area, false otherwise.
		/// </returns>
		public bool Equals(Rectangle other)
		{
			return (X == other.X && Y == other.Y && Width == other.Width && Height == other.Height);
		}

		/// <summary>
		/// Compares to arbitray object.
		/// </summary>
		/// <param name="obj">Object to compare to.</param>
		/// <returns>
		/// true if the object is a Rectangle instance and encompasses the same area, false otherwise.
		/// </returns>
		public override bool Equals(Object obj)
		{
			return obj is Rectangle && this == (Rectangle)obj;
		}

		/// <summary>
		/// Returns a new Rectangle, expanded to include the additional specified number of tiles on
		/// the left/right and top/bottom.
		/// </summary>
		/// <param name="horizontalChange">
		/// Number of additional tiles to include on the left/right of the rectangle.
		/// </param>
		/// <param name="verticalChange">
		/// Number of additional tiles to include on the top/bottom of the rectangle.
		/// </param>
		/// <returns>A new Rectangle, expanded appropriately.</returns>
		public Rectangle Expand(int horizontalChange, int verticalChange)
			=> new Rectangle(X - horizontalChange, Y - verticalChange, Width + (2 * horizontalChange), Height + (2 * verticalChange));

		/// <summary>
		/// Simple hashing.
		/// </summary>
		/// <returns>Hash code for rectangle.</returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
		}

		/// <summary>
		/// Returns whether or not the given rectangle intersects the current one.
		/// </summary>
		/// <param name="other">The rectangle to check.</param>
		/// <returns>True if the given rectangle intersects with the current one, false otherwise.</returns>
		public bool Intersects(Rectangle other)
		{
			return (other.X < X + Width && X < other.X + other.Width && other.Y < Y + Height && Y < other.Y + other.Height);
		}

		/// <summary>
		/// Creates and returns a new Rectangle that has its Position moved to the given position.
		/// </summary>
		/// <param name="position">The Position for the new rectangle.</param>
		/// <returns>A new rectangle that has its Position changed to the given value.</returns>
		public Rectangle Move(Coord position)
			=> new Rectangle(position.X, position.Y, Width, Height);

		/// <summary>
		/// Creates and returns a new Rectangle that has its Position moved to the given position.
		/// </summary>
		/// <param name="x">X-value for the position of the new Rectangle.</param>
		/// <param name="y">Y-value for the position of the new Rectangle.</param>
		/// <returns>A new rectangle with the Position changed to the given value.</returns>
		public Rectangle Move(int x, int y) => Move(Coord.Get(x, y));

		/// <summary>
		/// Creates and returns a new Rectangle that has its Position moved in the given direction.
		/// </summary>
		/// <param name="direction">The direction to move the new Rectangle in.</param>
		/// <returns>A new rectangle that has its position moved in the given direction.</returns>
		public Rectangle MoveIn(Direction direction)
		{
			var newPos = Position + direction;
			return new Rectangle(newPos.X, newPos.Y, Width, Height);
		}

		/// <summary>
		/// Creates and returns a new Rectangle that has its X value moved to the given x-coordinate.
		/// </summary>
		/// <param name="x">The X value for the new Rectangle.</param>
		/// <returns>A new rectangle with X changed to the given value.</returns>
		public Rectangle MoveX(int x) => new Rectangle(x, Y, Width, Height);

		/// <summary>
		/// Creates and returns a new Rectangle that has its Y value moved to the given y-coordinate.
		/// </summary>
		/// <param name="y">The Y value for the new Rectangle.</param>
		/// <returns>A new rectangle with Y changed to the given value.</returns>
		public Rectangle MoveY(int y) => new Rectangle(X, y, Width, Height);

		/// <summary>
		/// Returns all positions in the rectangle, in order of for (y = 0...) for (x = 0...) nested
		/// for loop.
		/// </summary>
		/// <returns>All positions in the rectangle.</returns>
		public IEnumerable<Coord> Positions()
		{
			for (int y = Y; y <= MaxExtentY; y++)
				for (int x = X; x <= MaxExtentX; x++)
					yield return Coord.Get(x, y);
		}

		/// <summary>
		/// Returns a random position from within this rectangle.
		/// </summary>
		/// <param name="rng">The rng to use. Defaults to SingletonRandom.DefaultRNG.</param>
		/// <returns>A random position from within the rectangle.</returns>
		public Coord RandomPosition(IGenerator rng = null)
		{
			if (rng == null)
				rng = SingletonRandom.DefaultRNG;

			return Coord.Get(rng.Next(X, MaxExtentX + 1), rng.Next(Y, MaxExtentY + 1));
		}

		/// <summary>
		/// Returns a random position from within this rectangle, for which the selector function
		/// specified returns true Random positions will continuously be generated until one that
		/// qualifies is found.
		/// </summary>
		/// <param name="selector">
		/// Selector function that takes a Coord, and returns true if it is an acceptable selection,
		/// and false otherwise.
		/// </param>
		/// <param name="rng">The rng to use. Defaults to SingletonRandom.DefaultRNG.</param>
		/// <returns>
		/// A random position from within the rectangle for which the given selector function
		/// returned true.
		/// </returns>
		public Coord RandomPosition(Func<Coord, bool> selector, IGenerator rng = null)
		{
			if (rng == null)
				rng = SingletonRandom.DefaultRNG;

			var c = Coord.Get(rng.Next(X, MaxExtentX + 1), rng.Next(Y, MaxExtentY + 1));

			while (!selector(c))
				c = Coord.Get(rng.Next(X, MaxExtentX + 1), rng.Next(Y, MaxExtentY + 1));

			return c;
		}

		/// <summary>
		/// Creates and returns a new rectangle that has the same position and width as the current
		/// one, but with the height changed to the given value.
		/// </summary>
		/// <param name="height">The height for the new Rectangle.</param>
		/// <returns>A new rectangle with the Height changed to the given value.</returns>
		public Rectangle SetHeight(int height)
			=> new Rectangle(X, Y, Width, height);

		/// <summary>
		/// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
		/// the maximum extent is the specified value.
		/// </summary>
		/// <param name="maxExtent">The maximum extent of the new rectangle.</param>
		/// <returns>A new Rectangle that has its maximum extent adjusted to the specified value.</returns>
		public Rectangle SetMaxExtent(Coord maxExtent)
			=> new Rectangle(MinExtent, maxExtent);

		/// <summary>
		/// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
		/// the maximum extent is the specified value.
		/// </summary>
		/// <param name="x">The x-value for the minimum extent of the new rectangle.</param>
		/// <param name="y">The y-value for the minimum extent of the new rectangle.</param>
		/// <returns>A new Rectangle that has its maximum extent adjusted to the specified value.</returns>
		public Rectangle SetMaxExtent(int x, int y)
			=> new Rectangle(MinExtent, Coord.Get(x, y));

		/// <summary>
		/// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
		/// the x-value of maximum extent is changed to the specified value.
		/// </summary>
		/// <param name="x">The x-coordinate for the maximum extent of the new rectangle.</param>
		/// <returns>A new rectangle, with the MaxExtentX adjusted to the specified value.</returns>
		public Rectangle SetMaxExtentX(int x)
			=> new Rectangle(MinExtent, Coord.Get(x, MaxExtentY));

		/// <summary>
		/// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
		/// the y-value of maximum extent is changed to the specified value.
		/// </summary>
		/// <param name="y">The y-coordinate for the maximum extent of the new rectangle.</param>
		/// <returns>A new rectangle, with the MaxExtentY adjusted to the specified value.</returns>
		public Rectangle SetMaxExtentY(int y)
			=> new Rectangle(MinExtent, Coord.Get(MaxExtentX, y));

		/// <summary>
		/// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
		/// the minimum extent is the specified value.
		/// </summary>
		/// <param name="minExtent">The minimum extent of the new rectangle.</param>
		/// <returns>A new Rectangle that has its minimum extent adjusted to the specified value.</returns>
		public Rectangle SetMinExtent(Coord minExtent)
			=> new Rectangle(minExtent, MaxExtent);

		/// <summary>
		/// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
		/// the minimum extent is the specified value.
		/// </summary>
		/// <param name="x">The x-value for the minimum extent of the new rectangle.</param>
		/// <param name="y">The y-value for the minimum extent of the new rectangle.</param>
		/// <returns>A new Rectangle that has its minimum extent adjusted to the specified value.</returns>
		public Rectangle SetMinExtent(int x, int y)
			=> new Rectangle(Coord.Get(x, y), MaxExtent);

		/// <summary>
		/// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
		/// the x-value of minimum extent is changed to the specified value.
		/// </summary>
		/// <param name="x">The x-coordinate for the minimum extent of the new rectangle.</param>
		/// <returns>A new rectangle, with the MinExtentX adjusted to the specified value.</returns>
		public Rectangle SetMinExtentX(int x)
			=> new Rectangle(Coord.Get(x, MinExtentY), MaxExtent);

		/// <summary>
		/// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
		/// the y-value of minimum extent is changed to the specified value.
		/// </summary>
		/// <param name="y">The y-coordinate for the minimum extent of the new rectangle.</param>
		/// <returns>A new rectangle, with the MinExtentY adjusted to the specified value.</returns>
		/// &gt;
		public Rectangle SetMinExtentY(int y)
			=> new Rectangle(Coord.Get(MinExtentX, y), MaxExtent);

		/// <summary>
		/// Creates and returns a new Rectangle whose position is the same as the current one, but
		/// has the specified width and height.
		/// </summary>
		/// <param name="width">The width for the new rectangle.</param>
		/// <param name="height">The height for the new rectangle.</param>
		/// <returns>A new Rectangle with the given width and height.</returns>
		public Rectangle SetSize(int width, int height)
			=> new Rectangle(X, Y, width, height);

		/// <summary>
		/// Creates and returns a new Rectangle whose position is the same as the current one, but
		/// has the specified width and height.
		/// </summary>
		/// <param name="size">Vector (width, height) specifying the width/height of the new rectangle.</param>
		/// <returns>A new Rectangle with the given width and height.</returns>
		public Rectangle SetSize(Coord size)
			=> new Rectangle(X, Y, size.X, size.Y);

		/// <summary>
		/// Creates and returns a new rectangle that is exactly the same as the current one, but with
		/// the width changed to the given value.
		/// </summary>
		/// <param name="width">The width for the new Rectangle.</param>
		/// <returns>A new rectangle with the Width changed to the given value.</returns>
		public Rectangle SetWidth(int width) => new Rectangle(X, Y, width, Height);

		/// <summary>
		/// Formats as (X, Y) -&gt; (MaxX, MaxY)
		/// </summary>
		/// <returns>String formatted as above.</returns>
		public override string ToString()
		{
			return Position + " -> " + MaxExtent;
		}

		/// <summary>
		/// Creates and returns a new Rectangle whose position has been moved by the given
		/// delta-change values.
		/// </summary>
		/// <param name="deltaChange">Delta-x and delta-y values by which to move the new Rectangle.</param>
		/// <returns>
		/// A new rectangle, whose position has been moved by the given delta-change values.
		/// </returns>
		public Rectangle Translate(Coord deltaChange)
			=> new Rectangle(X + deltaChange.X, Y + deltaChange.Y, Width, Height);

		/// <summary>
		/// Creates and returns a new Rectangle whose position has been moved by the given
		/// delta-change values.
		/// </summary>
		/// <param name="dx">Delta-x value by which to move the new Rectangle.</param>
		/// <param name="dy">Delta-y value by which to move the new Rectangle.</param>
		/// <returns>
		/// A new rectangle, whose position has been moved by the given delta-change values.
		/// </returns>
		public Rectangle Translate(int dx, int dy)
			=> new Rectangle(X + dx, Y + dy, Width, Height);

		/// <summary>
		/// Creates and returns a new Rectangle whose x-position has been moved by the given delta value.
		/// </summary>
		/// <param name="dx">Value by which to move the new Rectangle's x-position.</param>
		/// <returns>A new rectangle, whose x-position has been moved by the given delta-x value.</returns>
		public Rectangle TranslateX(int dx)
			=> new Rectangle(X + dx, Y, Width, Height);

		/// <summary>
		/// Creates and returns a new Rectangle whose y-position has been moved by the given delta value.
		/// </summary>
		/// <param name="dy">Value by which to move the new Rectangle's y-position.</param>
		/// <returns>A new rectangle, whose y-position has been moved by the given delta-y value.</returns>
		public Rectangle TranslateY(int dy)
			=> new Rectangle(X, Y + dy, Width, Height);
	}
}