using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// An area with an arbitrary number of sides and corners
    /// </summary>
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
        public Lines.Algorithm LineAlgorithm { get; set; }

        /// <summary>
        /// Creates a new Polygon, with corners at the provided points
        /// </summary>
        /// <param name="corners">Each corner of the polygon, in the order in which they are connected</param>
        /// <param name="algoToUse">Which Line Algorithm to use</param>
        /// <remarks>It is recommended to pass the points in clockwise</remarks>
        public PolygonArea(IEnumerable<Point> corners, Lines.Algorithm algoToUse = Lines.Algorithm.BresenhamOrdered)
        {
            _corners = corners.ToList();
            LineAlgorithm = algoToUse;
            if (_corners.Count < 3)
                throw new ArgumentException("Polygons must have 3 or more sides to be representable in 2 dimensions");

            _outerPoints = new MultiArea();
            _innerPoints = new Area();
            _points = new MultiArea();
            DrawFromCorners();
            SetInnerPoints();
        }

        private void DrawFromCorners()
        {
            var count = _corners.Count;

            for (int i = 0; i < count - 1; i++)
            {
                _outerPoints.Add(new Area(Lines.Get(_corners[i], _corners[i+1], LineAlgorithm)));
            }

            _outerPoints.Add(new Area(Lines.Get(_corners[count - 1], _corners[0], LineAlgorithm)));
            _points.Add(_outerPoints);

        }

        private void SetInnerPoints()
        {
            var bounds = _points.Bounds;
            for(int y = bounds.MinExtentY; y < bounds.MaxExtentY; y++)
            {
                IReadOnlyArea? lastLine = null;
                var linesEncountered = new List<IReadOnlyArea>();
                for(int x = bounds.MinExtentX; x < bounds.MaxExtentX; x++)
                {
                    if (_outerPoints.Contains(x, y))
                    {
                        foreach (var boundary in GetBoundariesContaining(x, y))
                        {
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
