using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.SenseMapping
{
    /// <summary>
    /// Different types of algorithms that model how source values spread from their source's location.
    /// </summary>
    [PublicAPI]
    public enum SourceType
    {
        /// <summary>
        /// Performs calculation by pushing values out from the source location. Source values spread
        /// around corners a bit.
        /// </summary>
        Ripple,

        /// <summary>
        /// Similar to <see cref="Ripple" /> but with different spread mechanics. Values spread around edges like
        /// smoke or water, but maintains a tendency to curl towards the start position as it goes around edges.
        /// </summary>
        RippleLoose,

        /// <summary>
        /// Similar to <see cref="Ripple" />, but values spread around corners only very slightly.
        /// </summary>
        RippleTight,

        /// <summary>
        /// Similar to <see cref="Ripple" />, but values spread around corners a lot.
        /// </summary>
        RippleVeryLoose,

        /// <summary>
        /// Uses a Shadow-casting algorithm. All partially resistant grid locations are treated as
        /// being fully transparent (it's on-off blocking, where a value greater than or equal to the
        /// source's <see cref="SenseSource.Intensity" /> in the resistance map blocks, and all lower
        /// values don't).
        /// </summary>
        Shadow
    }

    /// <summary>
    /// Represents a source location to be used in a <see cref="SenseMap" />.
    /// </summary>
    /// <remarks>
    /// Typically, you create these, and then call <see cref="SenseMap.AddSenseSource(SenseSource)" />
    /// to add them to a sensory map, and perhaps retain a reference for the sake of moving it
    /// around or toggling it on-off.  Note that changing values such as <see cref="Position" /> and
    /// <see cref="Radius" /> after the source is created is possible, however changes will not be
    /// reflected in any <see cref="SenseMap" /> instances using this source until their next call
    /// to <see cref="SenseMap.Calculate" />.
    /// </remarks>
    [PublicAPI]
    public class SenseSource
    {
        private static readonly Direction[] s_counterClockWiseDirectionCache =
            AdjacencyRule.EightWay.DirectionsOfNeighborsCounterClockwise(Direction.Right).ToArray();

        private static readonly string[] s_typeWriteValues = Enum.GetNames(typeof(SourceType));

        private double _angle;
        private double _decay; // Set when radius is set
        private int _halfSize;

        private double _intensity;

        // Local calculation arrays, internal so SenseMap can easily copy them.
        internal double[,] _light;

        private bool[,] _nearLight;

        // Pre-allocated list
        //private List<Point> _neighbors;

        // Analyzer gets this wrong because it's returned by ref
#pragma warning disable IDE0044
        private Point _position;
#pragma warning restore IDE0044
        private double _radius;
        internal IGridView<double>? _resMap;

        private int _size;

        private double _span;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The spread mechanics to use for source values.</param>
        /// <param name="position">The position on a map that the source is located at.</param>
        /// <param name="radius">
        /// The maximum radius of the source -- this is the maximum distance the source values will
        /// emanate, provided the area is completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, such as <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
#pragma warning disable CS8618 // Uninitialized non-nullable variable for light and nearLight is incorrect, as the Radius setter initializes them.
        public SenseSource(SourceType type, Point position, double radius, Distance distanceCalc,
                           double intensity = 1.0)
#pragma warning restore CS8618
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException(nameof(radius), "SenseMap radius cannot be 0");

            if (intensity < 0)
                throw new ArgumentOutOfRangeException(nameof(intensity),
                    "SenseSource intensity cannot be less than 0.0.");

            Type = type;
            Position = position;
            Radius = radius; // Arrays are initialized by this setter
            DistanceCalc = distanceCalc;

            _resMap = null;
            Enabled = true;

            IsAngleRestricted = false;
            Intensity = intensity;

            // Stores max of 8 neighbors
            //_neighbors = new List<Point>(8);
        }

        /// <summary>
        /// Constructor.  Creates a source whose spreading is restricted to a certain angle and span.
        /// </summary>
        /// <param name="type">The spread mechanics to use for source values.</param>
        /// <param name="position">The position on a map that the source is located at.</param>
        /// <param name="radius">
        /// The maximum radius of the source -- this is the maximum distance the source values will
        /// emanate, provided the area is completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, such as <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        /// <param name="angle">
        /// The angle in degrees that specifies the outermost center point of the cone formed
        /// by the source's values. 0 degrees points right.
        /// </param>
        /// <param name="span">
        /// The angle, in degrees, that specifies the full arc contained in the cone formed by the source's values --
        /// <paramref name="angle" /> / 2 degrees are included on either side of the cone's center line.
        /// </param>
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
        public SenseSource(SourceType type, Point position, double radius, Distance distanceCalc, double angle,
                           double span, double intensity = 1.0)
            : this(type, position, radius, distanceCalc, intensity)
        {
            if (span < 0.0 || span > 360.0)
                throw new ArgumentOutOfRangeException(nameof(span),
                    "Span used to initialize SenseSource must be in range [0, 360]");

            IsAngleRestricted = true;
            Angle = angle;
            Span = span;
        }

        /// <summary>
        /// Constructor.  Creates a source whose spread is restricted to a certain angle and span.
        /// </summary>
        /// <param name="type">The spread mechanics to use for source values.</param>
        /// <param name="positionX">The x-value for the position on a map that the source is located at.</param>
        /// <param name="positionY">The y-value for the position on a map that the source is located at.</param>
        /// <param name="radius">
        /// The maximum radius of the source -- this is the maximum distance the source values will
        /// emanate, provided the area is completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, such as <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        /// <param name="angle">
        /// The angle in degrees that specifies the outermost center point of the cone formed
        /// by the source's values. 0 degrees points right.
        /// </param>
        /// <param name="span">
        /// The angle, in degrees, that specifies the full arc contained in the cone formed by the source's values --
        /// <paramref name="angle" /> / 2 degrees are included on either side of the cone's center line.
        /// </param>
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
        public SenseSource(SourceType type, int positionX, int positionY, double radius, Distance distanceCalc,
                           double angle, double span, double intensity = 1.0)
            : this(type, new Point(positionX, positionY), radius, distanceCalc, angle, span, intensity)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The spread mechanics to use for source values.</param>
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
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, such as <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
        public SenseSource(SourceType type, int positionX, int positionY, double radius, Distance distanceCalc,
                           double intensity = 1.0)
            : this(type, new Point(positionX, positionY), radius, distanceCalc, intensity)
        { }

        /// <summary>
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, such as <see cref="SadRogue.Primitives.Radius" />).
        /// </summary>
        public Distance DistanceCalc { get; set; }

        /// <summary>
        /// Whether or not this source is enabled. If a source is disabled when <see cref="SenseMap.Calculate" />
        /// is called, the source does not calculate values and is effectively assumed to be "off".
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The position on a map that the source is located at.
        /// </summary>
        public ref Point Position => ref _position;

        /// <summary>
        /// Whether or not the spreading of values from this source is restricted to an angle and span.
        /// </summary>
        public bool IsAngleRestricted { get; set; }

        /// <summary>
        /// The starting value of the source to spread.  Defaults to 1.0.
        /// </summary>
        public double Intensity
        {
            get => _intensity;

            set
            {
                if (value < 0.0)
                    throw new ArgumentOutOfRangeException(nameof(Intensity),
                        "Intensity for SenseSource cannot be set to less than 0.0.");


                _intensity = value;
                _decay = _intensity / (_radius + 1);
            }
        }

        /// <summary>
        /// If <see cref="IsAngleRestricted" /> is true, the angle in degrees that represents a line from the source's start to
        /// the outermost center point of the cone formed by the source's calculated values.  0 degrees points up, and
        /// increases in angle move clockwise (like a compass).
        /// Otherwise, this will be 0.0 degrees.
        /// </summary>
        public double Angle
        {
            get => IsAngleRestricted ? MathHelpers.WrapAround(_angle + 90, 360.0) : 0.0;
            set
            {
                // Offset internal angle to 90 degrees being up instead of right
                _angle = value - 90;

                // Wrap angle to proper degrees
                if (_angle > 360.0 || _angle < 0)
                    _angle = MathHelpers.WrapAround(_angle, 360.0);
            }
        }

        /// <summary>
        /// If <see cref="IsAngleRestricted" /> is true, the angle in degrees that represents the full arc of the cone formed by
        /// the source's calculated values.  Otherwise, it will be 360 degrees.
        /// </summary>
        public double Span
        {
            get => IsAngleRestricted ? _span : 360.0;
            set
            {
                if (value < 0.0 || value > 360.0)
                    throw new ArgumentOutOfRangeException(nameof(Span), "SenseSource Span must be in range [0, 360]");

                _span = value;
            }
        }

        /// <summary>
        /// The maximum radius of the source -- this is the maximum distance the source values will
        /// emanate, provided the area is completely unobstructed. Changing this will trigger
        /// resizing (re-allocation) of the underlying arrays.
        /// </summary>
        public double Radius
        {
            get => _radius;
            set
            {
                if (value <= 0.0)
                    throw new ArgumentOutOfRangeException(nameof(Radius),
                        "Radius for a SenseSource must be greater than 0.");

                _radius = Math.Max(1, value);
                // Can round down here because the EUCLIDEAN distance shape is always contained within
                // the CHEBYSHEV distance shape
                _size = (int)_radius * 2 + 1;
                // Any times 2 is even, plus one is odd. rad 3, 3*2 = 6, +1 = 7. 7/2=3, so math works
                _halfSize = _size / 2;
                _light = new double[_size, _size];
                // Allocate whether we use shadow or not, just to support.  Could be lazy but its just booleans
                _nearLight = new bool[_size, _size];

                _decay = _intensity / (_radius + 1);
            }
        }

        /// <summary>
        /// The spread mechanics to use for source values.
        /// </summary>
        public SourceType Type { get; set; }

        /// <summary>
        /// Returns a string representation of the configuration of this SenseSource.
        /// </summary>
        /// <returns>A string representation of the configuration of this SenseSource.</returns>
        public override string ToString()
            => $"Enabled: {Enabled}, Type: {s_typeWriteValues[(int)Type]}, Radius Mode: {(Radius)DistanceCalc}, Position: {Position}, Radius: {Radius}";

        // Set from lighting, just so we have a reference.

        // 2 * Radius + 1 -- the width/height dimension of the local arrays.
        internal void CalculateLight()
        {
            if (!Enabled) return;

            if (_resMap == null)
                throw new InvalidOperationException(
                    "Attempted to calculate the light of a sense map without a resistance map.  This is almost certainly a GoRogue bug.");

            InitArrays();
            switch (Type)
            {
                case SourceType.Ripple:
                case SourceType.RippleLoose:
                case SourceType.RippleTight:
                case SourceType.RippleVeryLoose:
                    if (IsAngleRestricted)
                    {
                        var angle = _angle * SadRogue.Primitives.MathHelpers.DegreePctOfCircle;
                        var span = _span * SadRogue.Primitives.MathHelpers.DegreePctOfCircle;
                        DoRippleFOV(RippleValue(Type), _resMap, angle, span);
                    }
                    else
                        DoRippleFOV(RippleValue(Type), _resMap);

                    break;

                case SourceType.Shadow:
                    if (IsAngleRestricted)
                    {
                        var angle = _angle * SadRogue.Primitives.MathHelpers.DegreePctOfCircle;
                        var span = _span * SadRogue.Primitives.MathHelpers.DegreePctOfCircle;

                        ShadowCastLimited(1, 1.0, 0.0, 0, 1, 1, 0, _resMap, angle, span);
                        ShadowCastLimited(1, 1.0, 0.0, 1, 0, 0, 1, _resMap, angle, span);

                        ShadowCastLimited(1, 1.0, 0.0, 0, -1, 1, 0, _resMap, angle, span);
                        ShadowCastLimited(1, 1.0, 0.0, -1, 0, 0, 1, _resMap, angle, span);

                        ShadowCastLimited(1, 1.0, 0.0, 0, -1, -1, 0, _resMap, angle, span);
                        ShadowCastLimited(1, 1.0, 0.0, -1, 0, 0, -1, _resMap, angle, span);

                        ShadowCastLimited(1, 1.0, 0.0, 0, 1, -1, 0, _resMap, angle, span);
                        ShadowCastLimited(1, 1.0, 0.0, 1, 0, 0, -1, _resMap, angle, span);
                    }
                    else
                        for (int i = 0; i < AdjacencyRule.Diagonals.DirectionsOfNeighborsCache.Length; i++)
                        {
                            var d = AdjacencyRule.Diagonals.DirectionsOfNeighborsCache[i];

                            ShadowCast(1, 1.0, 0.0, 0, d.DeltaX, d.DeltaY, 0, _resMap);
                            ShadowCast(1, 1.0, 0.0, d.DeltaX, 0, 0, d.DeltaY, _resMap);
                        }

                    break;
                default:
                    throw new NotSupportedException("SourceType used that is not supported.");
            }
        }

        private static int RippleValue(SourceType type)
        {
            switch (type)
            {
                case SourceType.Ripple:
                    return 2;

                case SourceType.RippleLoose:
                    return 3;

                case SourceType.RippleTight:
                    return 1;

                case SourceType.RippleVeryLoose:
                    return 6;

                default:
                    Console.Error.WriteLine("Unrecognized ripple type, defaulting to RIPPLE...");
                    return RippleValue(SourceType.Ripple);
            }
        }

        private void DoRippleFOV(int ripple, IGridView<double> map)
        {
            LinkedList<Point> dq = new LinkedList<Point>();
            dq.AddLast(new Point(_halfSize, _halfSize)); // Add starting point
            while (dq.Count != 0)
            {
                var p = dq.First!.Value;
                dq.RemoveFirst();

                if (_light[p.X, p.Y] <= 0 || _nearLight[p.X, p.Y])
                    continue; // Nothing left to spread!

                for (int i = 0; i < AdjacencyRule.EightWay.DirectionsOfNeighborsCache.Length; i++)
                {
                    var dir = AdjacencyRule.EightWay.DirectionsOfNeighborsCache[i];

                    var x2 = p.X + dir.DeltaX;
                    var y2 = p.Y + dir.DeltaY;
                    var globalX2 = Position.X - (int)Radius + x2;
                    var globalY2 = Position.Y - (int)Radius + y2;

                    if (globalX2 < 0 || globalX2 >= map.Width || globalY2 < 0 ||
                        globalY2 >= map.Height || // Bounds check
                        DistanceCalc.Calculate(_halfSize, _halfSize, x2, y2) > _radius
                    ) // +1 covers starting tile at least
                        continue;

                    var surroundingLight = NearRippleLight(x2, y2, globalX2, globalY2, ripple, map);
                    if (_light[x2, y2] < surroundingLight)
                    {
                        _light[x2, y2] = surroundingLight;
                        if (map[globalX2, globalY2] < _intensity) // Not a wall (fully blocking)
                            dq.AddLast(new Point(x2,
                                y2)); // Need to redo neighbors, since we just changed this entry's light.
                    }
                }
            }
        }

        private void DoRippleFOV(int ripple, IGridView<double> map, double angle, double span)
        {
            LinkedList<Point> dq = new LinkedList<Point>();
            dq.AddLast(new Point(_halfSize, _halfSize)); // Add starting point
            while (dq.Count != 0)
            {
                var p = dq.First!.Value;
                dq.RemoveFirst();

                if (_light[p.X, p.Y] <= 0 || _nearLight[p.X, p.Y])
                    continue; // Nothing left to spread!

                for (int i = 0; i < s_counterClockWiseDirectionCache.Length; i++)
                {
                    var dir = s_counterClockWiseDirectionCache[i];

                    var x2 = p.X + dir.DeltaX;
                    var y2 = p.Y + dir.DeltaY;
                    var globalX2 = Position.X - (int)Radius + x2;
                    var globalY2 = Position.Y - (int)Radius + y2;

                    if (globalX2 < 0 || globalX2 >= map.Width || globalY2 < 0 ||
                        globalY2 >= map.Height || // Bounds check
                        DistanceCalc.Calculate(_halfSize, _halfSize, x2, y2) > _radius
                    ) // +1 covers starting tile at least
                        continue;

                    var at2 = Math.Abs(angle - MathHelpers.ScaledAtan2Approx(y2 - _halfSize, x2 - _halfSize));
                    if (at2 > span * 0.5 && at2 < 1.0 - span * 0.5)
                        continue;

                    var surroundingLight = NearRippleLight(x2, y2, globalX2, globalY2, ripple, map);
                    if (_light[x2, y2] < surroundingLight)
                    {
                        _light[x2, y2] = surroundingLight;
                        if (map[globalX2, globalY2] < _intensity) // Not a wall (fully blocking)
                            dq.AddLast(new Point(x2, y2)); // Need to redo neighbors, since we just changed this entry's light.
                    }
                }
            }
        }

        // Initializes arrays.
        private void InitArrays() // Prep for lighting calculations
        {
            Array.Clear(_light, 0, _light.Length);
            _light[_halfSize, _halfSize] = _intensity; // source light is center, starts out at our intensity
            if (Type != SourceType.Shadow) // Only clear if we are using it, since this is called at each calculate
                Array.Clear(_nearLight, 0, _nearLight.Length);
        }

        private double NearRippleLight(int x, int y, int globalX, int globalY, int rippleNeighbors,
                                       IGridView<double> map)
        {
            if (x == _halfSize && y == _halfSize)
                return _intensity;

            List<Point> neighbors = new List<Point>();
            for (int dirIdx = 0; dirIdx < AdjacencyRule.EightWay.DirectionsOfNeighborsCache.Length; dirIdx++)
            {
                var di = AdjacencyRule.EightWay.DirectionsOfNeighborsCache[dirIdx];

                var x2 = x + di.DeltaX;
                var y2 = y + di.DeltaY;

                // Out of bounds
                if (x2 < 0 || y2 < 0 || x2 >= _light.GetLength(0) || y2 >= _light.GetLength(1))
                    continue;

                var globalX2 = Position.X - (int)Radius + x2;
                var globalY2 = Position.Y - (int)Radius + y2;

                if (globalX2 >= 0 && globalX2 < map.Width && globalY2 >= 0 && globalY2 < map.Height)
                {
                    var tmpDistance = DistanceCalc.Calculate(_halfSize, _halfSize, x2, y2);
                    var idx = 0;

                    for (var i = 0; i < neighbors.Count && i <= rippleNeighbors; i++)
                    {
                        var c = neighbors[i];
                        var testDistance = DistanceCalc.Calculate(_halfSize, _halfSize, c.X, c.Y);
                        if (tmpDistance < testDistance)
                            break;

                        idx++;
                    }

                    neighbors.Insert(idx, new Point(x2, y2));
                }
            }

            if (neighbors.Count == 0)
                return 0;

            int maxNeighborIdx = Math.Min(neighbors.Count, rippleNeighbors);

            double curLight = 0;
            int lit = 0, indirects = 0;
            for (int neighborIdx = 0; neighborIdx < maxNeighborIdx; neighborIdx++)
            {
                var (pointX, pointY) = neighbors[neighborIdx];

                var gpx = Position.X - (int)Radius + pointX;
                var gpy = Position.Y - (int)Radius + pointY;

                if (_light[pointX, pointY] > 0)
                {
                    lit++;
                    if (_nearLight[pointX, pointY])
                        indirects++;

                    var dist = DistanceCalc.Calculate(x, y, pointX, pointY);
                    var resistance = map[gpx, gpy];
                    if (gpx == Position.X && gpy == Position.Y)
                        resistance = 0.0;

                    curLight = Math.Max(curLight, _light[pointX, pointY] - dist * _decay - resistance);
                }
            }

            if (map[globalX, globalY] >= _intensity || indirects >= lit)
                _nearLight[x, y] = true;

            return curLight;
        }

        private void ShadowCast(int row, double start, double end, int xx, int xy, int yx, int yy, IGridView<double> map)
        {
            double newStart = 0;
            if (start < end)
                return;

            var blocked = false;
            for (var distance = row; distance <= _radius && distance < _size + _size && !blocked; distance++)
            {
                var deltaY = -distance;
                for (var deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    var currentX = _halfSize + deltaX * xx + deltaY * xy;
                    var currentY = _halfSize + deltaX * yx + deltaY * yy;
                    var gCurrentX = Position.X - (int)_radius + currentX;
                    var gCurrentY = Position.Y - (int)_radius + currentY;
                    double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (!(gCurrentX >= 0 && gCurrentY >= 0 && gCurrentX < map.Width && gCurrentY < map.Height) ||
                        start < rightSlope)
                        continue;

                    if (end > leftSlope)
                        break;

                    var deltaRadius = DistanceCalc.Calculate(deltaX, deltaY);
                    if (deltaRadius <= _radius)
                    {
                        var bright = _intensity - _decay * deltaRadius;
                        _light[currentX, currentY] = bright;
                    }

                    if (blocked) // Previous cell was blocked
                    {
                        if (map[gCurrentX, gCurrentY] >= _intensity) // Hit a wall...
                            newStart = rightSlope;
                        else
                        {
                            blocked = false;
                            start = newStart;
                        }
                    }
                    else
                    {
                        if (map[gCurrentX, gCurrentY] >= _intensity && distance < _radius) // Wall within FOV
                        {
                            blocked = true;
                            ShadowCast(distance + 1, start, leftSlope, xx, xy, yx, yy, map);
                            newStart = rightSlope;
                        }
                    }
                }
            }
        }

        private void ShadowCastLimited(int row, double start, double end, int xx, int xy, int yx, int yy,
                                       IGridView<double> map, double angle, double span)
        {
            double newStart = 0;
            if (start < end)
                return;

            var blocked = false;
            for (var distance = row; distance <= _radius && distance < _size + _size && !blocked; distance++)
            {
                var deltaY = -distance;
                for (var deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    var currentX = _halfSize + deltaX * xx + deltaY * xy;
                    var currentY = _halfSize + deltaX * yx + deltaY * yy;
                    var gCurrentX = Position.X - (int)_radius + currentX;
                    var gCurrentY = Position.Y - (int)_radius + currentY;
                    double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (!(gCurrentX >= 0 && gCurrentY >= 0 && gCurrentX < map.Width && gCurrentY < map.Height) ||
                        start < rightSlope)
                        continue;
                    if (end > leftSlope)
                        break;

                    var deltaRadius = DistanceCalc.Calculate(deltaX, deltaY);
                    var at2 = Math.Abs(
                        angle - MathHelpers.ScaledAtan2Approx(currentY - _halfSize, currentX - _halfSize));

                    if (deltaRadius <= _radius && (at2 <= span * 0.5 || at2 >= 1.0 - span * 0.5))
                    {
                        var bright = _intensity - _decay * deltaRadius;
                        _light[currentX, currentY] = bright;
                    }

                    if (blocked) // Previous cell was blocked
                    {
                        if (map[gCurrentX, gCurrentY] >= _intensity) // Hit a wall...
                            newStart = rightSlope;
                        else
                        {
                            blocked = false;
                            start = newStart;
                        }
                    }
                    else if (map[gCurrentX, gCurrentY] >= _intensity && distance < _radius) // Wall within FOV
                    {
                        blocked = true;
                        ShadowCastLimited(distance + 1, start, leftSlope, xx, xy, yx, yy, map, angle, span);
                        newStart = rightSlope;
                    }
                }
            }
        }
    }
}
