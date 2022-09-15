using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.SenseMapping.Sources
{
    /// <summary>
    /// Different types of Ripple algorithms for how source values spread from their source's location.
    /// </summary>
    [PublicAPI]
    public enum RippleType
    {
        /// <summary>
        /// Performs calculation by pushing values out from the source location. Source values spread
        /// around corners a bit.
        /// </summary>
        Regular,

        /// <summary>
        /// Similar to <see cref="Regular" /> but with different spread mechanics. Values spread around edges like
        /// smoke or water, but maintains a tendency to curl towards the start position as it goes around edges.
        /// </summary>
        Loose,

        /// <summary>
        /// Similar to <see cref="Regular" />, but values spread around corners only very slightly.
        /// </summary>
        Tight,

        /// <summary>
        /// Similar to <see cref="Regular" />, but values spread around corners a lot.
        /// </summary>
        VeryLoose
    }

    [PublicAPI]
    public class RippleSenseSource : SenseSourceBase
    {
        /// <summary>
        /// The variation of the ripple algorithm being used.  See the <see cref="Sources.RippleType"/> value documentation for descriptions of each.
        /// </summary>
        public RippleType RippleType { get; set; }

        private BitArray _nearLight;
        // Pre-allocated list so we don't re-allocate small arrays
        private readonly List<Point> _neighbors;

        /// <summary>
        /// Creates a source which spreads outwards in all directions.
        /// </summary>
        /// <param name="position">The position on a map that the source is located at.</param>
        /// <param name="radius">
        /// The maximum radius of the source -- this is the maximum distance the source values will
        /// emanate, provided the area is completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="Distance" />, such as <see cref="Radius" />).
        /// </param>
        /// <param name="rippleType">The variation of the ripple algorithm to use.  See the <see cref="Sources.RippleType"/> value documentation for descriptions of each.</param>
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
        public RippleSenseSource(Point position, double radius, Distance distanceCalc,
            RippleType rippleType = RippleType.Regular, double intensity = 1)
            : base(position, radius, distanceCalc, intensity)
        {
            _nearLight = new BitArray(Size * Size);
            RadiusChanged += OnRadiusChanged;

            // Stores max of 8 neighbors
            _neighbors = new List<Point>(8);
        }

        /// <summary>
        /// Creates a source which spreads outwards in all directions.
        /// </summary>
        /// <param name="positionX">
        /// The X-value of the position on a map that the source is located at.
        /// </param>
        /// <param name="positionY">
        /// The Y-value of the position on a map that the source is located at.
        /// </param>
        /// <param name="radius">
        /// The maximum radius of the source -- this is the maximum distance the source values will
        /// emanate, provided the area is completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="Distance" />, such as <see cref="Radius" />).
        /// </param>
        /// <param name="rippleType">The variation of the ripple algorithm to use.  See the <see cref="Sources.RippleType"/> value documentation for descriptions of each.</param>
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
        public RippleSenseSource(int positionX, int positionY, double radius, Distance distanceCalc,
            RippleType rippleType = RippleType.Regular, double intensity = 1)
            : base(positionX, positionY, radius, distanceCalc, intensity)
        {
            _nearLight = new BitArray(Size * Size);
            RadiusChanged += OnRadiusChanged;

            // Stores max of 8 neighbors
            _neighbors = new List<Point>(8);
        }

        /// <summary>
        /// Constructor.  Creates a source which spreads only in a cone defined by the given angle and span.
        /// </summary>
        /// <param name="position">The position on a map that the source is located at.</param>
        /// <param name="radius">
        /// The maximum radius of the source -- this is the maximum distance the source values will
        /// emanate, provided the area is completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="Distance" />, such as <see cref="Radius" />).
        /// </param>
        /// <param name="angle">
        /// The angle in degrees that specifies the outermost center point of the cone formed
        /// by the source's values. 0 degrees points right.
        /// </param>
        /// <param name="span">
        /// The angle, in degrees, that specifies the full arc contained in the cone formed by the source's values --
        /// <paramref name="angle" /> / 2 degrees are included on either side of the cone's center line.
        /// </param>
        /// <param name="rippleType">The variation of the ripple algorithm to use.  See the <see cref="Sources.RippleType"/> value documentation for descriptions of each.</param>
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
        public RippleSenseSource(Point position, double radius, Distance distanceCalc, double angle, double span,
            RippleType rippleType = RippleType.Regular, double intensity = 1)
            : base(position, radius, distanceCalc, angle, span, intensity)
        {
            _nearLight = new BitArray(Size * Size);
            RadiusChanged += OnRadiusChanged;

            // Stores max of 8 neighbors
            _neighbors = new List<Point>(8);
        }

        /// <summary>
        /// Constructor.  Creates a source which spreads only in a cone defined by the given angle and span.
        /// </summary>
        /// <param name="positionX">The x-value for the position on a map that the source is located at.</param>
        /// <param name="positionY">The y-value for the position on a map that the source is located at.</param>
        /// <param name="radius">
        /// The maximum radius of the source -- this is the maximum distance the source values will
        /// emanate, provided the area is completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="Distance" />, such as <see cref="Radius" />).
        /// </param>
        /// <param name="angle">
        /// The angle in degrees that specifies the outermost center point of the cone formed
        /// by the source's values. 0 degrees points right.
        /// </param>
        /// <param name="span">
        /// The angle, in degrees, that specifies the full arc contained in the cone formed by the source's values --
        /// <paramref name="angle" /> / 2 degrees are included on either side of the cone's center line.
        /// </param>
        /// <param name="rippleType">The variation of the ripple algorithm to use.  See the <see cref="Sources.RippleType"/> value documentation for descriptions of each.</param>
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
        public RippleSenseSource(int positionX, int positionY, double radius, Distance distanceCalc, double angle,
            double span, RippleType rippleType = RippleType.Regular, double intensity = 1)
            : base(positionX, positionY, radius, distanceCalc, angle, span, intensity)
        {
            _nearLight = new BitArray(Size * Size);
            RadiusChanged += OnRadiusChanged;

            // Stores max of 8 neighbors
            _neighbors = new List<Point>(8);
        }

        /// <inheritdoc/>
        public override void OnCalculate()
        {
            if (IsAngleRestricted)
            {
                var angle = Angle * SadRogue.Primitives.MathHelpers.DegreePctOfCircle;
                var span = Span * SadRogue.Primitives.MathHelpers.DegreePctOfCircle;
                DoRippleFOV(RippleValue(RippleType), angle, span);
            }
            else
                DoRippleFOV(RippleValue(RippleType), 0, 0);
        }

        /// <inheritdoc/>
        protected override void Reset()
        {
            base.Reset();
            _nearLight.SetAll(false);
        }

        private void OnRadiusChanged(object? sender, EventArgs e)
        {
            _nearLight = new BitArray(Size * Size);
        }

        private void DoRippleFOV(int ripple, double angle, double span)
        {
            // TODO: Use dequeue for more speed; a circular array should have much better perf
            LinkedList<Point> dq = new LinkedList<Point>();
            dq.AddLast(new Point(Center, Center)); // Add starting point
            while (dq.Count != 0)
            {
                var p = dq.First!.Value;
                dq.RemoveFirst();

                if (ResultViewBacking[p.X, p.Y] <= 0 || _nearLight[p.ToIndex(Size)])
                    continue; // Nothing left to spread!

                for (int i = 0; i < AdjacencyRule.EightWay.DirectionsOfNeighborsCache.Length; i++)
                {
                    var dir = AdjacencyRule.EightWay.DirectionsOfNeighborsCache[i];

                    var x2 = p.X + dir.DeltaX;
                    var y2 = p.Y + dir.DeltaY;
                    var globalX2 = Position.X - (int)Radius + x2;
                    var globalY2 = Position.Y - (int)Radius + y2;

                    // Null-forgiving is fine; OnCalculate cannot be called with a null ResistanceMap
                    if (globalX2 < 0 || globalX2 >= ResistanceMap!.Width || globalY2 < 0 ||
                        globalY2 >= ResistanceMap.Height || // Bounds check
                        DistanceCalc.Calculate(Center, Center, x2, y2) > Radius
                       ) // +1 covers starting tile at least
                        continue;

                    if (IsAngleRestricted)
                    {
                        var at2 = Math.Abs(angle - MathHelpers.ScaledAtan2Approx(y2 - Center, x2 - Center));
                        if (at2 > span * 0.5 && at2 < 1.0 - span * 0.5)
                            continue;
                    }
                    

                    var surroundingLight = NearRippleLight(x2, y2, globalX2, globalY2, ripple);
                    if (ResultViewBacking[x2, y2] < surroundingLight)
                    {
                        ResultViewBacking[x2, y2] = surroundingLight;
                        if (ResistanceMap[globalX2, globalY2] < Intensity) // Not a wall (fully blocking)
                            dq.AddLast(new Point(x2,
                                y2)); // Need to redo neighbors, since we just changed this entry's light.
                    }
                }
            }
        }

        private double NearRippleLight(int x, int y, int globalX, int globalY, int rippleNeighbors)
        {
            if (x == Center && y == Center)
                return Intensity;

            for (int dirIdx = 0; dirIdx < AdjacencyRule.EightWay.DirectionsOfNeighborsCache.Length; dirIdx++)
            {
                var di = AdjacencyRule.EightWay.DirectionsOfNeighborsCache[dirIdx];

                var x2 = x + di.DeltaX;
                var y2 = y + di.DeltaY;

                // Out of bounds
                if (x2 < 0 || y2 < 0 || x2 >= ResultViewBacking.Width || y2 >= ResultViewBacking.Height)
                    continue;

                var globalX2 = Position.X - (int)Radius + x2;
                var globalY2 = Position.Y - (int)Radius + y2;

                // Null forgiving because this can only be called from OnCalculate, and ResistanceMap cannot be null when that function
                // is called; adding a check would cost performance unnecessarily
                if (globalX2 >= 0 && globalX2 < ResistanceMap!.Width && globalY2 >= 0 && globalY2 < ResistanceMap.Height)
                {
                    var tmpDistance = DistanceCalc.Calculate(Center, Center, x2, y2);
                    int idx = 0;

                    // Find where to insert the new element
                    int count = _neighbors.Count;
                    for (; idx < count && idx < rippleNeighbors; idx++)
                    {
                        var c = _neighbors[idx];
                        var testDistance = DistanceCalc.Calculate(Center, Center, c.X, c.Y);
                        if (tmpDistance < testDistance)
                            break;
                    }
                    // No point in inserting it after this point, it'd never be counted anyway.  Otherwise, if we're kicking
                    // an existing element off the end, we'll just remove it to prevent shifting it down pointlessly
                    if (idx < rippleNeighbors)
                    {
                        if (count >= rippleNeighbors)
                            _neighbors.RemoveAt(rippleNeighbors - 1);
                        _neighbors.Insert(idx, new Point(x2, y2));
                    }
                }
            }

            if (_neighbors.Count == 0)
                return 0;

            int maxNeighborIdx = Math.Min(_neighbors.Count, rippleNeighbors);

            double curLight = 0;
            int lit = 0, indirects = 0;
            for (int neighborIdx = 0; neighborIdx < maxNeighborIdx; neighborIdx++)
            {
                var (pointX, pointY) = _neighbors[neighborIdx];

                var gpx = Position.X - (int)Radius + pointX;
                var gpy = Position.Y - (int)Radius + pointY;

                if (ResultViewBacking[pointX, pointY] > 0)
                {
                    lit++;
                    if (_nearLight[Point.ToIndex(pointX, pointY, Size)])
                        indirects++;

                    var dist = DistanceCalc.Calculate(x, y, pointX, pointY);
                    var resistance = ResistanceMap![gpx, gpy];
                    if (gpx == Position.X && gpy == Position.Y)
                        resistance = 0.0;

                    curLight = Math.Max(curLight, ResultViewBacking[pointX, pointY] - dist * Decay - resistance);
                }
            }

            if (ResistanceMap![globalX, globalY] >= Intensity || indirects >= lit)
                _nearLight[Point.ToIndex(x, y, Size)] = true;

            _neighbors.Clear();
            return curLight;
        }

        private static int RippleValue(RippleType type)
        {
            return type switch
            {
                RippleType.Regular => 2,
                RippleType.Loose => 3,
                RippleType.Tight => 1,
                RippleType.VeryLoose => 6,
                _ => RippleValue(RippleType.Regular)
            };
        }
    }
}
