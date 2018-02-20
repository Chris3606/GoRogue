using System;
using GoRogue.MapGeneration;

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
        /// <param name="x">
        /// Minimum x coordinate that is inside the rectangle.
        /// </param>
        /// <param name="y">
        /// Minimum y coordinate that is inside the rectangle.
        /// </param>
        /// <param name="width">
        /// Width of the rectangle.
        /// </param>
        /// <param name="height">
        /// Height of the rectangle.
        /// </param>
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
        /// <param name="minCorner">
        /// Minimum x and y values that are considered inside the rectangle.
        /// </param>
        /// <param name="maxCorner">
        /// Maximum x and y values that are considered inside the rectangle.
        /// </param>
        public Rectangle(Coord minCorner, Coord maxCorner)
        {
            X = Y = Width = Height = 0; // To allow assignment
            MinCorner = minCorner;
            MaxCorner = maxCorner;
        }

        /// <summary>
        /// Constructor. Takes a center point, and horizontal/vertical radius defining the bounds.
        /// </summary>
        /// <param name="center">
        /// The center point of the rectangle.
        /// </param>
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
            set
            {
                X = value.X - (Width / 2);
                Y = value.Y - (Width / 2);
            }
        }

        /// <summary>
        /// The height of the rectangle, in grid squares.
        /// </summary>
        public int Height { get; set; }

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
            set { MaxX = value.X; MaxY = value.Y; }
        }

        /// <summary>
        /// The maximum X-coordinate that is included in the rectangle.
        /// </summary>
        public int MaxX
        {
            get => X + Width - 1;
            set => Width = value - X + 1;
        }

        /// <summary>
        /// The maximum Y-coordinate that is included in the rectangle.
        /// </summary>
        public int MaxY
        {
            get => Y + Height - 1;
            set => Height = value - Y + 1;
        }

        /// <summary>
        /// Coord representing the minimum X and Y values that are included in the rectangle.
        /// </summary>
        public Coord MinCorner
        {
            get => Coord.Get(X, Y);
            set { X = value.X; Y = value.Y; }
        }

        /// <summary>
        /// The width of the rectangle, in grid squares.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Minimum X-coordinate of the rectangle.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Minimum Y-coordinate that is included in the rectangle.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Returns the rectangle that represents the intersection of the two rectangles specified,
        /// or the empty rectangle if the specified rectangles do not intersect.
        /// </summary>
        /// <param name="r1">
        /// First rectangle.
        /// </param>
        /// <param name="r2">
        /// Second rectangle.
        /// </param>
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
        /// Gets the smallest possible rectangle that includes the entire area of both r1 and r2.
        /// </summary>
        /// <param name="r1">
        /// First rectangle.
        /// </param>
        /// <param name="r2">
        /// Second rectangle.
        /// </param>
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
        /// <param name="r1">
        /// First rectangle.
        /// </param>
        /// <param name="r2">
        /// Second rectangle.
        /// </param>
        /// <returns>
        /// true if the rectangles do NOT encompass the same area, false otherwise.
        /// </returns>
        public static bool operator !=(Rectangle r1, Rectangle r2)
        {
            return !(r1 == r2);
        }

        /// <summary>
        /// See Equals.
        /// </summary>
        /// <param name="r1">
        /// First rectangle.
        /// </param>
        /// <param name="r2">
        /// Second rectangle.
        /// </param>
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
        /// <param name="position">
        /// The position to check.
        /// </param>
        /// <returns>
        /// Whether or not the specified point is considered within the rectangle.
        /// </returns>
        public bool Contains(Coord position)
        {
            return (position.X >= X && position.X < (X + Width) && position.Y >= Y && position.Y < (Y + Height));
        }

        /// <summary>
        /// Returns whether or not the specified rectangle is considered completely contained within
        /// the current one.
        /// </summary>
        /// <param name="other">
        /// The rectangle to check.
        /// </param>
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
        /// <param name="other">
        /// Rectangle to compare the current one to.
        /// </param>
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
        /// <param name="obj">
        /// Object to compare to.
        /// </param>
        /// <returns>
        /// true if the object is a Rectangle instance and encompasses the same area, false otherwise.
        /// </returns>
        public override bool Equals(Object obj)
        {
            return obj is Rectangle && this == (Rectangle)obj;
        }

        /// <summary>
        /// Expands the rectangle to include the additional specified number of tiles on the
        /// left/right and top/bottom.
        /// </summary>
        /// <param name="horizontalChange">
        /// Number of additional tiles to include on the left/right of the rectangle.
        /// </param>
        /// <param name="verticalChange">
        /// Number of additional tiles to include on the top/bottom of the rectangle.
        /// </param>
        public void Expand(int horizontalChange, int verticalChange)
        {
            X -= horizontalChange;
            Width += 2 * horizontalChange;

            Y -= verticalChange;
            Height += 2 * verticalChange;
        }

        /// <summary>
        /// Simple hashing.
        /// </summary>
        /// <returns>
        /// Hash code for rectangle.
        /// </returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
        }

        /// <summary>
        /// Returns whether or not the given rectangle intersects the current one.
        /// </summary>
        /// <param name="other">
        /// The rectangle to check.
        /// </param>
        /// <returns>
        /// True if the given rectangle intersects with the current one, false otherwise.
        /// </returns>
        public bool Intersects(Rectangle other)
        {
            return (other.X < X + Width && X < other.X + other.Width && other.Y < Y + Height && Y < other.Y + other.Height);
        }

        /// <summary>
        /// Formats as (X, Y) -&gt; (MaxX, MaxY)
        /// </summary>
        /// <returns>
        /// String formatted as above.
        /// </returns>
        public override string ToString()
        {
            return MinCorner + " -> " + MaxCorner;
        }
    }
}