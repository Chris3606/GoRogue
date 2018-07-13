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
        /// The empty rectangle. Has origin of 0, 0 with 0 width and height.
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
        /// Constructor. Takes the minimum and maximum Points taht are considered within the rectangle.
        /// </summary>
        /// <param name="minCorner">Minimum x and y values that are considered inside the rectangle.</param>
        /// <param name="maxCorner">Maximum x and y values that are considered inside the rectangle.</param>
        public Rectangle(Coord minCorner, Coord maxCorner)
        {
            X = minCorner.X;
            Y = minCorner.Y;
            Width = maxCorner.X - X + 1;
            Height = maxCorner.Y - Y + 1;
        }

        /// <summary>
        /// Constructor. Takes a center point, and horizontal/vertical radius defining the bounds.
        /// </summary>
        /// <param name="center">The center point of the rectangle.</param>
        /// <param name="horizontalRadius">
        /// Number of tiles to the left and right of the center point that are included within the rectangle.
        /// </param>
        /// <param name="verticalRadius">
        /// Number of tiles to the top and bottom of the center point that are included within the rectangle.
        /// </param>
        public Rectangle(Coord center, int horizontalRadius, int verticalRadius)
        {
            X = center.X - horizontalRadius;
            Y = center.Y - verticalRadius;
            Width = center.X + horizontalRadius;
            Height = center.Y + verticalRadius;
        }

        /// <summary>
        /// The center coordinate of the rectangle, rounded down if the exact center is floating point.
        /// </summary>
        public Coord Center
        {
            get => Coord.Get(X + (Width / 2), Y + (Height / 2));
        }

        /// <summary>
        /// The height of the rectangle, in grid squares.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Whether or not this rectangle is empty (has width and height of 0).
        /// </summary>
        public bool IsEmpty { get => (Width == 0 && Height == 0); }

        /// <summary>
        /// The maximum X and Y coordinates that are included in the rectangle.
        /// </summary>
        public Coord MaxCorner
        {
            get => Coord.Get(MaxX, MaxY);
        }

        /// <summary>
        /// The maximum X-coordinate that is included in the rectangle.
        /// </summary>
        public int MaxX
        {
            get => X + Width - 1;
        }

        /// <summary>
        /// The maximum Y-coordinate that is included in the rectangle.
        /// </summary>
        public int MaxY
        {
            get => Y + Height - 1;
        }

        /// <summary>
        /// Coord representing the position (min x- and y-values) of the rectangle.
        /// </summary>
        public Coord Position
        {
            get => Coord.Get(X, Y);
        }

        /// <summary>
        /// The width of the rectangle, in grid squares.
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

            for (int x = r1.X; x <= r1.MaxX; x++)
                for (int y = r1.Y; y <= r1.MaxY; y++)
                    retVal.Add(x, y);

            for (int x = r2.X; x <= r2.MaxX; x++)
                for (int y = r2.Y; y <= r2.MaxY; y++)
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
            return (X <= other.X && X + Width <= other.X + other.Width && Y <= other.Y && Y + Height <= other.Y + other.Height);
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
        /// Creates and returns a new rectangle that is exactly the same as the current one, but with
        /// the center moved to the given position.
        /// </summary>
        /// <param name="center">The center-point for the new Rectangle.</param>
        /// <returns>
        /// A new Rectangle that is exactly like the current one, but with the center moved to the
        /// given location.
        /// </returns>
        public Rectangle SetCenter(Coord center)
            => new Rectangle(center.X - (Width / 2), center.Y - (Height / 2), Width, Height);

        public Rectangle SetCenter(int x, int y) => SetCenter(Coord.Get(x, y));

        /// <summary>
        /// Creates and returns a new rectangle that is exactly the same as the current one, but with
        /// the height changed to the given value.
        /// </summary>
        /// <param name="height">The height for the new Rectangle.</param>
        /// <returns>A new rectangle with the Height changed to the given value.</returns>
        public Rectangle SetHeight(int height)
            => new Rectangle(X, Y, Width, height);

        /// <summary>
        /// Creates and returns a new Rectangle that has been stretched/shrunk so its MaxCorner is at the given position.
        /// </summary>
        /// <param name="maxCorner">The MaxCorner for the new rectangle.</param>
        /// <returns>A new rectangle that has been stretched/shrunk so the MaxCorner is the given value.</returns>
        public Rectangle SetMaxCorner(Coord maxCorner)
            => new Rectangle(X, Y, maxCorner.X - X + 1, maxCorner.Y - Y + 1);

        /// <summary>
        /// Creates and returns a new Rectangle that has its MaxX moved to the given x-coordinate.
        /// </summary>
        /// <param name="maxX">The MaxX value for the new Rectangle.</param>
        /// <returns>A new rectangle with MaxX changed to the given value.</returns>
        public Rectangle SetMaxX(int maxX) => new Rectangle(X, Y, maxX - X + 1, Height);

        /// <summary>
        /// Creates and returns a new Rectangle that has its MaxY moved to the given y-coordinate.
        /// </summary>
        /// <param name="maxY">The MaxY value for the new Rectangle.</param>
        /// <returns>A new rectangle with MaxY changed to the given value.</returns>
        /// &gt;
        public Rectangle SetMaxY(int maxY) => new Rectangle(X, Y, Width, maxY - Y + 1);

        /// <summary>
        /// Creates and returns a new Rectangle that has its MinCorner moved to the given position.
        /// </summary>
        /// <param name="position">The MinCorner for the new rectangle.</param>
        /// <returns>A new rectangle with the MinCorner changed to the given value.</returns>
        public Rectangle SetPosition(Coord position)
            => new Rectangle(position.X, position.Y, Width, Height);

        /// <summary>
        /// Creates and returns a new rectangle that is exactly the same as the current one, but with
        /// the width changed to the given value.
        /// </summary>
        /// <param name="width">The width for the new Rectangle.</param>
        /// <returns>A new rectangle with the Width changed to the given value.</returns>
        public Rectangle SetWidth(int width) => new Rectangle(X, Y, width, Height);

        /// <summary>
        /// Creates and returns a new Rectangle that has its X value moved to the given x-coordinate.
        /// </summary>
        /// <param name="x">The X value for the new Rectangle.</param>
        /// <returns>A new rectangle with X changed to the given value.</returns>
        public Rectangle SetX(int x) => new Rectangle(x, Y, Width, Height);

        /// <summary>
        /// Creates and returns a new Rectangle that has its Y value moved to the given y-coordinate.
        /// </summary>
        /// <param name="y">The Y value for the new Rectangle.</param>
        /// <returns>A new rectangle with Y changed to the given value.</returns>
        public Rectangle SetY(int y) => new Rectangle(X, y, Width, Height);

        /// <summary>
        /// Creates and returns a new Rectangle that has been resized to have the given width/height.
        /// </summary>
        /// <param name="width">Width for the new Rectangle.</param>
        /// <param name="height">Height for the new Rectangle.</param>
        /// <returns></returns>
        public Rectangle ResizeTo(int width, int height) => new Rectangle(X, Y, width, height);

        /// <summary>
        /// Creates and returns a new Rectangle that has its width and height modified by the given values.
        /// </summary>
        /// <param name="deltaWidth">Value to modify the new Rectangle's width by.</param>
        /// <param name="deltaHeight">Value to modify the new Rectangle's height by.</param>
        /// <returns></returns>
        public Rectangle Resize(int deltaWidth, int deltaHeight)
            => new Rectangle(X, Y, Width - deltaWidth, Height - deltaHeight);

        /// <summary>
        /// Returns all positions in the rectangle, in order of for (y = 0...) for (x = 0...) nested
        /// for loop.
        /// </summary>
        /// <returns>All positions in the rectangle.</returns>
        public IEnumerable<Coord> Positions()
        {
            for (int y = Y; y <= MaxY; y++)
                for (int x = X; x <= MaxX; x++)
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

            return Coord.Get(rng.Next(X, MaxX + 1), rng.Next(Y, MaxY + 1));
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

            var c = Coord.Get(rng.Next(X, MaxX + 1), rng.Next(Y, MaxY + 1));

            while (!selector(c))
                c = Coord.Get(rng.Next(X, MaxX + 1), rng.Next(Y, MaxY + 1));

            return c;
        }

        /// <summary>
        /// Formats as (X, Y) -&gt; (MaxX, MaxY)
        /// </summary>
        /// <returns>String formatted as above.</returns>
        public override string ToString()
        {
            return Position + " -> " + MaxCorner;
        }
    }
}