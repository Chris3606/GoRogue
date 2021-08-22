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
        /// Creates a new Polygon, with corners at the provided points
        /// </summary>
        /// <param name="corners">Each corner of the polygon, in the order in which they are connected</param>
        /// <remarks>It is recommended to pass the points in clockwise</remarks>
        public PolygonArea(IEnumerable<Point> corners)
        {
            _corners = corners.ToList();
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
                _outerPoints.Add(new Area(Lines.Get(_corners[i], _corners[i+1])));
            }

            _outerPoints.Add(new Area(Lines.Get(_corners[count - 1], _corners[0])));
            _points.Add(_outerPoints);
        }

        private void SetInnerPoints()
        {
            int tally;
            bool inner;
            var bounds = _points.Bounds;
            for(int y = bounds.MinExtentY; y < bounds.MaxExtentY; y++)
            {
                IReadOnlyArea? lastLine = null;
                tally = 0;
                inner = false;
                for(int x = bounds.MinExtentX; x < bounds.MaxExtentX; x++)
                {
                    #region take one
                    /*
                        if(_corners.Contains((x,y)))
                        {
                            continue;
                        }
                        else if (lastLine != null && lastLine.Contains(x,y))
                        {
                            continue;
                        }
                        else if(_outerPoints.Contains((x,y)))
                        {
                            lastLine = GetBoundaryContaining(x,y)!;

                            if(lastLine.First().Y < lastLine.Last().Y)
                                tally++;
                            if(lastLine.First().Y > lastLine.Last().Y)
                                tally--;
                        }
                        else
                        {
                            inner = tally != 0;
                        }

                        if(inner && !_outerPoints.Contains(x,y))
                            _innerPoints.Add(x,y);
                    */
                    #endregion

                    #region take two
                    /*
                    if(_corners.Contains((x,y)))
                    {
                        continue;
                    }
                    else if (lastLine != null && lastLine.Contains(x,y))
                    {
                        continue;
                    }
                    else if(_outerPoints.Contains((x,y)))
                    {
                        var lines = GetBoundariesContaining(x, y);
                        var count = lines.Count();

                        if (count % 2 == 1)
                            inner = !inner;

                        lastLine = lines.Last();

                    }

                    if(inner && !_outerPoints.Contains(x,y))
                        _innerPoints.Add(x,y);
                    */
                    #endregion

                    #region take three
                    if(_outerPoints.Contains((x,y)))
                    {
                        var lines = GetBoundariesContaining(x, y).ToList();
                        var count = lines.Count;

                        if (count == 1 && lastLine != lines[0])
                        {
                            lastLine = lines[0];
                            inner = !inner;
                        }
                        else if (count == 1 && lastLine == lines[0])
                        {
                            continue;
                        }
                        else if (count == 2 && lastLine == lines[0])
                        {
                            lastLine = lines[1];
                            inner = !inner;
                        }
                    }

                    if(inner && !_outerPoints.Contains(x,y))
                        _innerPoints.Add(x,y);
                    #endregion
                }
            }
        }

        private IReadOnlyArea? GetBoundaryContaining(int x, int y)
            => _outerPoints.SubAreas.FirstOrDefault(sa => sa.Contains(x, y));

        private IEnumerable<IReadOnlyArea> GetBoundariesContaining(int x, int y)
            => _outerPoints.SubAreas.Where(sa => sa.Contains(x, y));

        /// <inheritdoc/>
        public bool Matches(IReadOnlyArea other) => _points.Matches(other);

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
