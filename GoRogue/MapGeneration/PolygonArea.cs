using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.PointHashers;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// An area with an arbitrary number of sides and corners
    /// </summary>
    [PublicAPI]
    public class PolygonArea : IReadOnlyArea
    {
        #region Properties/Fields
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
        /// The left-most X-value of the Polygon
        /// </summary>
        public int Left => Bounds.MinExtentX;

        /// <summary>
        /// The right-most X-value of the Polygon
        /// </summary>
        public int Right => Bounds.MaxExtentX;

        /// <summary>
        /// The top-most Y-value of the Polygon
        /// </summary>
        public int Top => Direction.YIncreasesUpward ? Bounds.MaxExtentY : Bounds.MinExtentY;

        /// <summary>
        /// The bottom-most Y-value of the Polygon
        /// </summary>
        public int Bottom => Direction.YIncreasesUpward ? Bounds.MinExtentY : Bounds.MaxExtentY;

        /// <summary>
        /// How Wide this Polygon is
        /// </summary>
        public int Width => Bounds.Width;

        /// <summary>
        /// how tall this Polygon is
        /// </summary>
        public int Height => Bounds.Height;

        /// <summary>
        /// The Center point of this Polygon
        /// </summary>
        /// <remarks>There is no guarantee that the center point lies within the polygon</remarks>
        public Point Center => Bounds.Center;

        /// <summary>
        /// Returns true if the position provided is a corner of this polygon
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool IsCorner(Point position) => Corners.Contains(position);

        /// <summary>
        /// Returns a string detailing the region's corner locations.
        /// </summary>
        public override string ToString()
        {
            var answer = new StringBuilder("PolygonArea: ");
            foreach (var corner in _corners)
                answer.Append($"{corner} => ");
            return answer.ToString();
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new Polygon, with corners at the provided points
        /// </summary>
        /// <param name="corners">Each corner of the polygon, which is copied into a new list</param>
        /// <param name="algorithm">Which Line Algorithm to use</param>
        /// <exception cref="ArgumentException">Must have 3 or more corners; Algorithm must produce ordered lines.</exception>
        public PolygonArea(IEnumerable<Point> corners, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
            : this(corners.ToList(), algorithm) { }

        /// <summary>
        /// Creates a new Polygon, with corners at the provided points
        /// </summary>
        /// <param name="corners">The corners of this polygon</param>
        /// <param name="algorithm">Which Line Algorithm to use</param>
        /// <exception cref="ArgumentException">Must have 3 or more corners; Algorithm must produce ordered lines.</exception>
        public PolygonArea(ref List<Point> corners, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
            : this(corners, algorithm) { }

        /// <summary>
        /// Returns a new PolygonArea with corners at the provided points.
        /// </summary>
        /// <param name="algorithm">Which Line-drawing algorithm to use</param>
        /// <param name="corners">The points which are corners for this polygon</param>
        /// <exception cref="ArgumentException">Must have 3 or more corners; Algorithm must produce ordered lines.</exception>
        public PolygonArea(Lines.Algorithm algorithm, params Point[] corners)
            : this(corners, algorithm) { }

        /// <summary>
        /// Returns a new polygon with corners at the provided points, using the algorithm DDA to produce lines
        /// </summary>
        /// <param name="corners">The corners of the polygon</param>
        /// <exception cref="ArgumentException">Must have 3 or more corners; Algorithm must produce ordered lines.</exception>
        public PolygonArea(params Point[] corners) : this(corners, Lines.Algorithm.DDA) { }

        private PolygonArea(List<Point> corners, Lines.Algorithm algorithm)
        {
            _corners = corners;
            LineAlgorithm = algorithm;
            CheckCorners();
            CheckAlgorithm();
            _outerPoints = new MultiArea();

            // Draw corners
            DrawFromCorners();
            // Create proper inner points area
            var (minExtent, maxExtent) = _outerPoints.Bounds;
            _innerPoints = new Area(new KnownRangeHasher(minExtent, maxExtent));
            // Determine inner points based on outer points and bounds
            SetInnerPoints();

            _points = new MultiArea { _outerPoints, _innerPoints };
        }
        #endregion

        #region Private Initialization Functions

        private void CheckCorners() => CheckCorners(_corners.Count);
        private static void CheckCorners(int corners)
        {
            if (corners < 3)
                throw new ArgumentException("Polygons must have 3 or more sides to be representable in 2 dimensions");
        }

        private void CheckAlgorithm() => CheckAlgorithm(LineAlgorithm);
        private static void CheckAlgorithm(Lines.Algorithm algorithm)
        {
            if (algorithm == Lines.Algorithm.Bresenham || algorithm == Lines.Algorithm.Orthogonal)
                throw new ArgumentException("Line Algorithm must produce ordered lines.");
        }

        // Draws lines from each corner to the next
        private void DrawFromCorners()
        {
            // TODO: It may be useful to change the hashing algorithm used by these areas as well since the bounds of each
            // boundary are known before creation; however the computation does itself take some time; needs testing.
            // The bulk of the performance gain that is achieved during creation specifically is achieved via caching
            // outer points in SetInnerPoints anyway.
            for (int i = 0; i < _corners.Count - 1; i++)
                _outerPoints.Add(new Area(Lines.Get(_corners[i], _corners[i+1], LineAlgorithm)));

            _outerPoints.Add(new Area(Lines.Get(_corners[^1], _corners[0], LineAlgorithm)));
        }

        // Uses an odd-even rule to determine whether we are in the area or not and fills InnerPoints accordingly
        private void SetInnerPoints()
        {
            // Calculate bounds and cache outer points so that we can efficiently check whether an arbitrary point
            // is an outer point or not.
            var bounds = _outerPoints.Bounds;
            var outerPointsSet =
                new HashSet<Point>(_outerPoints, new KnownRangeHasher(bounds.MinExtent, bounds.MaxExtent));

            // The top and bottom rows can never contain an inner point, so skip them.
            for (int y = bounds.MinExtentY + 1; y < bounds.MaxExtentY; y++)
            {
                var lineIndicesEncountered = new HashSet<int>();

                // Must include MinExtentX so that it can accurately count lines encountered.
                // Doesn't need MaxExtentX since no inner point can be equal to or greater than that.
                for (int x = bounds.MinExtentX; x < bounds.MaxExtentX; x++)
                {
                    var curPoint = new Point(x, y);

                    // If we find an outer point, we must count it as seen on this y-line
                    if (outerPointsSet.Contains(curPoint))
                    {
                        // Add all boundary lines that contain the point we've found.  Each point could be part
                        // of 1 or 2 boundary lines (corners are in 2).  Note however that it is _not_ necessarily
                        // sufficient to just check if the current point is a corner to know whether it's part of
                        // only 1 or two, as non-corners could be a part of 2 lines if the angle between two boundaries
                        // is extremely small (due to imprecision of representing lines on an integral grid).
                        for (int i = 0; i < _outerPoints.SubAreas.Count; i++)
                        {
                            var boundary = _outerPoints.SubAreas[i];
                            if (!boundary.Contains(curPoint)) continue;

                            // We must count the line as encountered IFF it contains a point with a y-value
                            // less than the current point's y.  By definition of a line, such a point would _have_
                            // to occur at one of the two ends.
                            if (boundary[0].Y < y || boundary[^1].Y < y)
                                lineIndicesEncountered.Add(i);
                        }
                    }
                    // Otherwise, a point is an inner point IFF the scan-line has crossed an odd number of outer
                    // boundaries on its way to the current point
                    else
                    {
                        if (lineIndicesEncountered.Count % 2 == 1)
                            _innerPoints.Add(curPoint);
                    }
                }
            }
        }
        #endregion

        #region Static Creation Methods
        /// <summary>
        /// Creates a new Polygon from a GoRogue.Rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle</param>
        /// <param name="algorithm">Line-drawing algorithm to use for finding boundaries.</param>
        /// <exception cref="ArgumentException">Must have 3 or more corners; Algorithm must produce ordered lines.</exception>
        /// <returns>A new Polygon in the shape of a rectangle</returns>
        public static PolygonArea Rectangle(Rectangle rectangle, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
            => new PolygonArea(algorithm, rectangle.MinExtent, (rectangle.MaxExtentX, rectangle.MinExtentY), rectangle.MaxExtent,
                (rectangle.MinExtentX, rectangle.MaxExtentY));

        /// <summary>
         /// Creates a new Polygon in the shape of a parallelogram.
         /// </summary>
         /// <param name="origin">Origin of the parallelogram.</param>
         /// <param name="width">Width of the parallelogram.</param>
         /// <param name="height">Height of the parallelogram.</param>
        /// <param name="fromTop">Whether the parallelogram extends downward-right or upwards-right from the start</param>
         /// <param name="algorithm">Line-drawing algorithm to use for finding boundaries.</param>
        /// <exception cref="ArgumentException">Must have 3 or more corners; Algorithm must produce ordered lines.</exception>
         /// <returns>A new Polygon in the shape of a parallelogram</returns>
         public static PolygonArea Parallelogram(Point origin, int width, int height, bool fromTop = false,
            Lines.Algorithm algorithm = Lines.Algorithm.DDA)
         {
             CheckAlgorithm(algorithm);

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
        /// <exception cref="ArgumentException">Must have 3 or more corners; Algorithm must produce ordered lines.</exception>
        /// <param name="algorithm">Which line-drawing algorithm to use</param>
        /// <returns></returns>
        public static PolygonArea RegularPolygon(Point center, int numberOfSides, double radius, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
        {
            CheckCorners(numberOfSides);
            CheckAlgorithm(algorithm);

            var corners = new List<Point>(numberOfSides);
            var increment = 360.0 / numberOfSides;

            for (int i = 0; i < numberOfSides; i ++)
            {
                var theta = SadRogue.Primitives.MathHelpers.ToRadian(i * increment);
                var corner = new PolarCoordinate(radius, theta).ToCartesian();
                corner += center;
                corners.Add(corner);
            }

            return new PolygonArea(ref corners, algorithm);
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
            CheckCorners(points);
            CheckAlgorithm(algorithm);

            if (outerRadius < 0)
                throw new ArgumentException("outerRadius must be positive.");
            if (innerRadius < 0)
                throw new ArgumentException("innerRadius must be positive.");

            points *= 2;
            var corners = new List<Point>(points);
            var increment = 360.0 / points;

            for (int i = 0; i < points; i ++)
            {
                var radius = i % 2 == 0 ? outerRadius : innerRadius;
                var theta = SadRogue.Primitives.MathHelpers.ToRadian(i * increment);
                var corner = new PolarCoordinate(radius, theta).ToCartesian();
                corner += center;
                corners.Add(corner);
            }

            return new PolygonArea(ref corners, algorithm);
        }
        #endregion

        #region IReadOnlyArea Implementation
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
            var corners = new List<Point>(_corners.Count);
            for (int i = 0; i < Corners.Count; i++)
            {
                corners.Add(Corners[i] + delta);
            }

            return new PolygonArea(ref corners);
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

            var corners = new List<Point>(_corners.Count);
            for (int i = 0; i < Corners.Count; i++)
            {
                corners.Add(Corners[i].Rotate(degrees, origin));
            }

            return new PolygonArea(ref corners);
        }

        /// <summary>
        /// Flip horizontally around an X-axis
        /// </summary>
        /// <param name="x">The value around which to flip.</param>
        /// <returns>A new, flipped PolygonArea</returns>
        public PolygonArea FlipHorizontal(int x)
        {
            var corners = new List<Point>(_corners.Count);
            for (int i = 0; i < Corners.Count; i++)
            {
                corners.Add((Corners[i] - (x,0)) * (-1, 1) + (x,0));
            }

            return new PolygonArea(ref corners);
        }

        /// <summary>
        /// Flip vertically around a Y-axis
        /// </summary>
        /// <param name="y">The value around which to flip.</param>
        public PolygonArea FlipVertical(int y)
        {
            var corners = new List<Point>(_corners.Count);
            for (int i = 0; i < Corners.Count; i++)
            {
                corners.Add((Corners[i] - (0,y)) * (1,-1) + (0,y));
            }

            return new PolygonArea(ref corners);
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
            var corners = new List<Point>(_corners.Count);
            for (int i = 0; i < Corners.Count; i++)
            {
                var corner = Corners[i];
                corner -= xy;
                corner = (corner.Y, corner.X);
                corner += xy;
                corners.Add(corner);
            }

            return new PolygonArea(ref corners);
        }
        #endregion
    }
}
