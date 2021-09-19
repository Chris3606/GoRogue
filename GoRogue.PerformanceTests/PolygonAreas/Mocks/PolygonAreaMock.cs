using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests.PolygonAreas.Mocks
{
    // Object representing a polygon area's internal data and the general method of construction.  Necessary to allow the
    // benchmark functions to actually return a value which depends on the creation method, to avoid any of it being
    // optimized out.
    [PublicAPI]
    public class PolygonAreaMock
    {
        // Public properties that precisely mimic the internal structure of PolygonArea; designed to allow creation
        // functions to manipulate them just as they would in the real version
        #region Properties
        public readonly List<Point> Corners;

        public readonly Lines.Algorithm LineAlgorithm;

        public readonly MultiArea OuterPoints;

        // Cannot be readonly because this function allows the functions being tested control of its allocation
        public Area InnerPoints = null!;

        public readonly MultiArea Points;
        #endregion

        // Constructors that mimic PolygonArea; with minimal modifications to allow full customization of performance-relevant portions
        #region Constructors
        public PolygonAreaMock(IEnumerable<Point> corners, Action<PolygonAreaMock> drawFromCornersMethod, Action<PolygonAreaMock> innerPointsCreationMethod, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
            : this(corners.ToList(), drawFromCornersMethod, innerPointsCreationMethod, algorithm) { }


        public PolygonAreaMock(ref List<Point> corners, Action<PolygonAreaMock> drawFromCornersMethod, Action<PolygonAreaMock> innerPointsCreationMethod, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
            : this(corners, drawFromCornersMethod, innerPointsCreationMethod, algorithm) { }

        public PolygonAreaMock(Action<PolygonAreaMock> drawFromCornersMethod, Action<PolygonAreaMock> innerPointsCreationMethod, Lines.Algorithm algorithm, params Point[] corners)
            : this(corners, drawFromCornersMethod, innerPointsCreationMethod, algorithm) { }


        public PolygonAreaMock(Action<PolygonAreaMock> drawFromCornersMethod, Action<PolygonAreaMock> innerPointsCreationMethod, params Point[] corners)
            // ReSharper disable once RedundantArgumentDefaultValue
            : this(corners, drawFromCornersMethod, innerPointsCreationMethod, Lines.Algorithm.DDA) { }

        private PolygonAreaMock(List<Point> corners, Action<PolygonAreaMock> drawFromCornersMethod, Action<PolygonAreaMock> innerPointsCreationMethod, Lines.Algorithm algorithm)
        {
            Corners = corners;
            if (Corners.Count < 3)
                throw new ArgumentException("Polygons must have 3 or more sides to be representable in 2 dimensions");

            if (algorithm == Lines.Algorithm.Bresenham || algorithm == Lines.Algorithm.Orthogonal)
                throw new ArgumentException("Line Algorithm must produce ordered lines.");
            LineAlgorithm = algorithm;
            OuterPoints = new MultiArea();

            // Rearranged ordering relative to original, in order to enable full customization of the generated areas
            // (including the hashing algorithm used).  The functions themselves must perform InnerPoints allocation
            drawFromCornersMethod(this);
            innerPointsCreationMethod(this);

            // Must occur after above function calls because those functions must allocate InnerPoints
            Points = new MultiArea { OuterPoints, InnerPoints };
        }
        #endregion

        // Functions that generate PolygonAreaMocks that are equivalent to PolygonArea, for comparison
        #region Equivalent PolygonArea Creation
        public static PolygonAreaMock Rectangle(Rectangle r, Action<PolygonAreaMock> drawFromCornersMethod, Action<PolygonAreaMock> innerPointsCreationMethod, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
            => new PolygonAreaMock(drawFromCornersMethod, innerPointsCreationMethod, algorithm, r.MinExtent, (r.MaxExtentX, r.MinExtentY),
                r.MaxExtent, (r.MinExtentX, r.MaxExtentY));

        public static PolygonAreaMock Parallelogram(Point origin, int width, int height,
                                                    Action<PolygonAreaMock> drawFromCornersMethod,
                                                    Action<PolygonAreaMock> innerPointsCreationMethod,
                                                    bool fromTop = false, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
        {
            if (fromTop && Direction.YIncreasesUpward)
                height *= -1;

            else if(!fromTop && !Direction.YIncreasesUpward)
                height *= -1;


            Point p1 = origin;
            Point p2 = origin + new Point(width, 0);
            Point p3 = origin + new Point(width * 2, height);
            Point p4 = origin + new Point(width, height);

            return new PolygonAreaMock(drawFromCornersMethod, innerPointsCreationMethod, algorithm, p1, p2, p3, p4);
        }
        #endregion
    }
}
