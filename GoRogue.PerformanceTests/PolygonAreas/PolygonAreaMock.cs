using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests.PolygonAreas
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

        // Functions that generate PolygonAreaMocks that are equivalent to regions, for comparison
        #region Equivalent Region Creation
        public static PolygonAreaMock Rectangle(Rectangle r, Action<PolygonAreaMock> drawFromCornersMethod, Action<PolygonAreaMock> innerPointsCreationMethod, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
            => new PolygonAreaMock(drawFromCornersMethod, innerPointsCreationMethod, algorithm, r.MinExtent, (r.MaxExtentX, r.MinExtentY),
                r.MaxExtent, (r.MinExtentX, r.MaxExtentY));


        public static PolygonAreaMock ParallelogramFromTopCorner(Point origin, int width, int height, Action<PolygonAreaMock> drawFromCornersMethod, Action<PolygonAreaMock> innerPointsCreationMethod, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
        {
            var negative = Direction.YIncreasesUpward ? -1 : 1;

            Point nw = origin;
            Point ne = origin + new Point(width, 0);
            Point se = origin + new Point(width * 2, height * negative);
            Point sw = origin + new Point(width, height * negative);

            return new PolygonAreaMock(drawFromCornersMethod, innerPointsCreationMethod, algorithm, nw, ne, se, sw);
        }

        public static PolygonAreaMock ParallelogramFromBottomCorner(Point origin, int width, int height, Action<PolygonAreaMock> drawFromCornersMethod, Action<PolygonAreaMock> innerPointsCreationMethod, Lines.Algorithm algorithm = Lines.Algorithm.DDA)
        {
            var negative = Direction.YIncreasesUpward ? 1 : -1;

            Point nw = origin + (height, height * negative);
            Point ne = origin + (height + width, height * negative);
            Point se = origin + (width, 0);
            Point sw = origin;

            return new PolygonAreaMock(drawFromCornersMethod, innerPointsCreationMethod, algorithm, nw, ne, se, sw);
        }
        #endregion
    }
}
