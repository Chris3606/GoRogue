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

        /// <inheritdoc/>
        public Rectangle Bounds => _points.Bounds;

        /// <inheritdoc/>
        public int Count => _points.Count;

        /// <inheritdoc/>
        public Point this[int index] => _points[index];
    }
}