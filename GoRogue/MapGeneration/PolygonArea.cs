using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// An area with an arbitrary number of sides and corners
    /// </summary>
    [PublicAPI]
    public class PolygonArea : IReadOnlyArea
    {
        #region variables
        /// <summary>
        /// The corners of this polygon
        /// </summary>
        public IReadOnlyList<Point> Corners => _corners.AsReadOnly();
        private readonly List<Point> _corners;

        /// <summary>
        /// The exterior points of the polygon
        /// </summary>
        public IReadOnlyArea OuterPoints => _outerPoints;
        private readonly MultiArea _outerPoints;

        /// <summary>
        /// The interior points of the polygon
        /// </summary>
        public IReadOnlyArea InnerPoints => _innerPoints;
        private readonly Area _innerPoints;

        private readonly MultiArea _points;

        /// <summary>
        /// Which Line-Drawing algorithm to use
        /// </summary>
        public readonly Lines.Algorithm LineAlgorithm;

        /// <inheritdoc/>
        public Rectangle Bounds => _points.Bounds;

        /// <inheritdoc/>
        public int Count => _points.Count;

        /// <inheritdoc/>
        public Point this[int index] => _points[index];

        /// <summary>
        /// The left-most X-value of the region's four corners
        /// </summary>
        public int Left => _corners.OrderBy(p => p.X).First().X;

        /// <summary>
        /// The right-most X-value of the region's four corners
        /// </summary>
        public int Right => _corners.OrderBy(p => p.X).Last().X;

        /// <summary>
        /// The top-most Y-value of the region's four corners
        /// </summary>
        public int Top => Direction.YIncreasesUpward
            ? _corners.OrderBy(p => p.Y).Last().Y
            : _corners.OrderBy(p => p.Y).First().Y;

        /// <summary>
        /// The bottom-most Y-value of the region's four corners
        /// </summary>
        public int Bottom => Direction.YIncreasesUpward
            ? _corners.OrderBy(p => p.Y).First().Y
            : _corners.OrderBy(p => p.Y).Last().Y;

        /// <summary>
        /// How Wide this region is
        /// </summary>
        public int Width => Right - Left + 1;

        /// <summary>
        /// how tall this region is
        /// </summary>
        public int Height => (Direction.YIncreasesUpward ? Top - Bottom : Bottom - Top) + 1;

        /// <summary>
        /// The Center point of this region
        /// </summary>
        /// <remarks>There is no guarantee that the center point lies within the polygon</remarks>
        public Point Center => new Point((Left + Right) / 2, (Top + Bottom) / 2);
        #endregion

        #region constructors
        /// <summary>
        /// Creates a new Polygon, with corners at the provided points
        /// </summary>
        /// <param name="corners">Each corner of the polygon, which is copied into a new list</param>
        /// <param name="algorithm">Which Line Algorithm to use</param>
        /// <exception cref="ArgumentException">Must have 3 or more corners; Algorithm must produce ordered lines.</exception>
        public PolygonArea(IEnumerable<Point> corners, Lines.Algorithm algorithm = Lines.Algorithm.DDA) : this(corners.ToList(), algorithm) { }

        /// <summary>
        /// Creates a new Polygon, with corners at the provided points
        /// </summary>
        /// <param name="corners">The corners of this polygon</param>
        /// <param name="algorithm">Which Line Algorithm to use</param>
        public PolygonArea(ref List<Point> corners, Lines.Algorithm algorithm = Lines.Algorithm.DDA) : this(corners, algorithm) { }

        /// <summary>
        /// Returns a new PolygonArea with corners at the provided points.
        /// </summary>
        /// <param name="algorithm">Which Line-drawing algorithm to use</param>
        /// <param name="corners">The points which are corners for this polygon</param>
        public PolygonArea(Lines.Algorithm algorithm, params Point[] corners) : this(corners, algorithm) { }

        /// <summary>
        /// Returns a new polygon with corners at the provided points, using the algorithm DDA to produce lines
        /// </summary>
        /// <param name="corners">The corners of the polygon</param>
        public PolygonArea(params Point[] corners) : this(corners, Lines.Algorithm.DDA) { }

        private PolygonArea(List<Point> corners, Lines.Algorithm algorithm)
        {
            _corners = corners;
            if (_corners.Count < 3)
                throw new ArgumentException("Polygons must have 3 or more sides to be representable in 2 dimensions");

            if (algorithm == Lines.Algorithm.Bresenham || algorithm == Lines.Algorithm.Orthogonal)
                throw new ArgumentException("Line Algorithm must produce ordered lines.");
            LineAlgorithm = algorithm;
            _outerPoints = new MultiArea();
            _innerPoints = new Area();
            _points = new MultiArea { _outerPoints, _innerPoints };

            DrawFromCorners();
            SetInnerPoints();
        }
        #endregion

        #region private initialization
        //Draws lines from each corner to the next
        private void DrawFromCorners()
        {
            for (int i = 0; i < _corners.Count - 1; i++)
            {
                _outerPoints.Add(new Area(Lines.Get(_corners[i], _corners[i+1], LineAlgorithm)));
            }

            _outerPoints.Add(new Area(Lines.Get(_corners[^1], _corners[0], LineAlgorithm)));
        }

        //Uses an odd-even rule to determine whether we are in the area or not
        private void SetInnerPoints()
        {
            var bounds = _points.Bounds;

            //The top and bottom rows can never contain an inner point, so skip them.
            for(int y = bounds.MinExtentY + 1; y < bounds.MaxExtentY; y++)
            {
                var linesEncountered = new List<IReadOnlyArea>();

                //Must include MinExtentX so that it can accurately count lines encountered.
                //Doesn't need MaxExtentX since no inner point can be equal to or greater than that.
                for(int x = bounds.MinExtentX; x < bounds.MaxExtentX; x++)
                {
                    if (_outerPoints.Contains(x, y))
                    {
                        foreach (var boundary in GetBoundariesContaining(x, y))
                        {
                            //todo - optimize
                            if (boundary.Any(p => p.Y < y))
                            {
                                if (!linesEncountered.Contains(boundary))
                                {
                                    linesEncountered.Add(boundary);
                                }
                            }
                        }
                    }
                    else
                    {
                        if(linesEncountered.Count % 2 == 1)
                            _innerPoints.Add(x,y);
                    }
                }
            }
        }
        #endregion

        #region static creation methods
        /// <summary>
        /// Creates a new Region from a GoRogue.Rectangle.
        /// </summary>
        /// <param name="r">The rectangle</param>
        /// <param name="algorithm">Line-drawing algorithm to use for finding boundaries.</param>
        /// <returns>A new region in the shape of a rectangle</returns>
        public static PolygonArea Rectangle(Rectangle r, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
            => new PolygonArea(algorithm, r.MinExtent, (r.MaxExtentX, r.MinExtentY), r.MaxExtent,
                (r.MinExtentX, r.MaxExtentY));

        /// <summary>
         /// Creates a new Region in the shape of a parallelogram, with diagonals going down and right.
         /// </summary>
         /// <param name="origin">Origin of the parallelogram.</param>
         /// <param name="width">Width of the parallelogram.</param>
         /// <param name="height">Height of the parallelogram.</param>
        /// <param name="fromTop">Whether the parallelogram extends downward-right or upwards-right from the start</param>
         /// <param name="algorithm">Line-drawing algorithm to use for finding boundaries.</param>
         /// <returns>A new region in the shape of a parallelogram</returns>
         public static PolygonArea Parallelogram(Point origin, int width, int height, bool fromTop = false,
            Lines.Algorithm algorithm = Lines.Algorithm.DDA)
         {
             if (fromTop && Direction.YIncreasesUpward)
                 height *= -1;

             else if(!fromTop && !Direction.YIncreasesUpward)
                 height *= -1;


             Point p1 = origin;
             Point p2 = origin + new Point(width, 0);
             Point p3 = origin + new Point(width * 2, height);
             Point p4 = origin + new Point(width, height);

             return new PolygonArea(algorithm, p1, p2, p3, p4);
         }

        /// <summary>
        /// Creates a polygon whose sides are even-length
        /// </summary>
        /// <param name="center">The center point of this polygon</param>
        /// <param name="numberOfSides">Number of sides and corners on this polygon</param>
        /// <param name="radius">The desired distance between the center and each corner</param>
        /// <param name="algorithm">Which line-drawing algoirthm to use</param>
        /// <returns></returns>
        public static PolygonArea RegularPolygon(Point center, int numberOfSides, double radius, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
        {
            if(numberOfSides < 3)
                throw new ArgumentException("Polygons must have 3 or more sides to be representable in 2 dimensions");

            if (algorithm == Lines.Algorithm.Bresenham || algorithm == Lines.Algorithm.Orthogonal)
                throw new ArgumentException("Line Algorithm must produce ordered lines.");

            var corners = new List<Point>();
            var increment = 360 / numberOfSides;

            for (int i = 0; i < numberOfSides; i ++)
            {
                var theta = SadRogue.Primitives.MathHelpers.ToRadian(i * increment);
                var corner = new PolarCoordinate(radius, theta).ToCartesian();
                corner += center;
                corners.Add(corner);
            }

            //possible bug, compare the output of PolygonRoutine with 72 corners versus 73
            return new PolygonArea(corners, algorithm);
        }

        /// <summary>
        /// Creates a new star-shaped polygon
        /// </summary>
        /// <param name="center">The center point of the star</param>
        /// <param name="points">How many points this star has</param>
        /// <param name="outerRadius">The distance between the center and a tip of the star</param>
        /// <param name="innerRadius">The distance between the center and an armpit of the star</param>
        /// <param name="algorithm">Which line-drawing algorithm to use</param>
        /// <exception cref="ArgumentException">Stars must have 3 or more points; algorithm must be ordered; inner and outer radius must be positive</exception>
        public static PolygonArea RegularStar(Point center, int points, double outerRadius, double innerRadius,
            Lines.Algorithm algorithm = Lines.Algorithm.DDA)
        {
            if(points < 3)
                throw new ArgumentException("Polygons must have 3 or more sides to be representable in 2 dimensions");
            if (algorithm == Lines.Algorithm.Bresenham || algorithm == Lines.Algorithm.Orthogonal)
                throw new ArgumentException("Line Algorithm must produce ordered lines.");
            if (outerRadius < 0)
                throw new ArgumentException("outerRadius must be positive.");
            if (innerRadius < 0)
                throw new ArgumentException("innerRadius must be positive.");

            points *= 2;
            var corners = new List<Point>();
            var increment = 360 / points;

            for (int i = 0; i < points; i ++)
            {
                var radius = i % 2 == 0 ? outerRadius : innerRadius;
                var theta = SadRogue.Primitives.MathHelpers.ToRadian(i * increment);
                var corner = new PolarCoordinate(radius, theta).ToCartesian();
                corner += center;
                corners.Add(corner);
            }

            return new PolygonArea(corners, algorithm);
        }
        #endregion

        #region IReadOnlyArea
        //todo - optimize
        private IEnumerable<IReadOnlyArea> GetBoundariesContaining(int x, int y)
            => _outerPoints.SubAreas.Where(sa => sa.Contains(x, y));

        /// <inheritdoc/>
        public bool Matches(IReadOnlyArea? other) => _points.Matches(other);

        /// <inheritdoc/>
        public IEnumerator<Point> GetEnumerator() => _points.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => _points.GetEnumerator();

        /// <inheritdoc/>
        public bool Contains(IReadOnlyArea area) => _points.Contains(area);

        /// <inheritdoc/>
        public bool Contains(Point position) => _points.Contains(position);

        /// <inheritdoc/>
        public bool Contains(int positionX, int positionY) => _points.Contains(positionX, positionY);

        /// <inheritdoc/>
        public bool Intersects(IReadOnlyArea area) => _points.Intersects(area);
        #endregion

        #region Transformation

        /// <summary>
        /// Moves the Polygon in the indicated direction.
        /// </summary>
        /// <param name="dx">The X-value by which to shift</param>
        /// <param name="dy">The Y-value by which to shift</param>
        /// <returns></returns>
        public PolygonArea Translate(int dx, int dy)
            => Translate(new Point(dx, dy));

        /// <summary>
        /// Moves the Polygon in the indicated direction.
        /// </summary>
        /// <param name="delta">The amount (X and Y) to translate by.</param>
        /// <returns>A new, translated PolygonArea</returns>
        public PolygonArea Translate(Point delta)
        {
            var corners = new Point[Corners.Count];
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = Corners[i] + delta;
            }

            return new PolygonArea(corners);
        }

        /// <summary>
        /// Rotates the Polygon around it's center.
        /// </summary>
        /// <param name="degrees">The amount of degrees to rotate</param>
        /// <returns>A new, rotated PolygonArea</returns>
        public PolygonArea Rotate(double degrees) => Rotate(degrees, Center);

        /// <summary>
        /// Rotates the Polygon around a point of origin
        /// </summary>
        /// <param name="degrees">The amount of degrees to rotate</param>
        /// <param name="origin">The Point around which to rotate</param>
        /// <returns>A new, rotated PolygonArea</returns>
        public PolygonArea Rotate(double degrees, Point origin)
        {
            degrees = MathHelpers.WrapAround(degrees, 360);

            var corners = new Point[Corners.Count];
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = Corners[i].Rotate(degrees, origin);
            }

            return new PolygonArea(corners);
        }

        /// <summary>
        /// Flip horizontally around an X-axis
        /// </summary>
        /// <param name="x">The value around which to flip.</param>
        /// <returns>A new, flipped PolygonArea</returns>
        public PolygonArea FlipHorizontal(int x)
        {
            var corners = new Point[Corners.Count];
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = (Corners[i] - (x,0)) * (-1, 1) + (x,0);
            }

            return new PolygonArea(corners);
        }

        /// <summary>
        /// Flip vertically around a Y-axis
        /// </summary>
        /// <param name="y">The value around which to flip.</param>
        public PolygonArea FlipVertical(int y)
        {
            var corners = new Point[Corners.Count];
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = (Corners[i] - (0,y)) * (1,-1) + (0,y);
            }

            return new PolygonArea(corners);
        }

        /// <summary>
        /// Invert the X and Y values of a Polygon, respective to a diagonal line
        /// </summary>
        /// <param name="x">Any X-value of a point which intersects the line around which to transpose</param>
        /// <param name="y">Any Y-value of a Point which intersects the line around which to transpose</param>
        /// <returns>A new PolygonArea</returns>
        public PolygonArea Transpose(int x, int y)
            => Transpose((x, y));

        /// <summary>
        /// Invert the X and Y values of a Polygon, respective to a diagonal line
        /// </summary>
        /// <param name="xy">Any point which intersects the line around which to transpose</param>
        /// <returns>A new PolygonArea</returns>
        public PolygonArea Transpose(Point xy)
        {
            var corners = new Point[Corners.Count];
            for (int i = 0; i < corners.Length; i++)
            {
                var corner = Corners[i];
                corner -= xy;
                corner = (corner.Y, corner.X);
                corner += xy;
                corners[i] = corner;
            }

            return new PolygonArea(corners);
        }
        #endregion
    }
}
