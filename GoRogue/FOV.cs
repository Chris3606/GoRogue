using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue
{
    /// <summary>
    /// Arguments for event fired when FOV is recalculated.
    /// </summary>
    [PublicAPI]
    public class FOVRecalculatedEventArgs : EventArgs
    {
        /// <summary>
        /// Position of the FOV origin point.
        /// </summary>
        public readonly Point Origin;

        /// <summary>
        /// The maximum radius -- eg. the maximum distance of the field of view if completely unobstructed.
        /// </summary>
        public readonly double Radius;

        /// <summary>
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="Distance" />, eg. <see cref="Radius" />).
        /// </summary>
        public readonly Distance DistanceCalc;

        /// <summary>
        /// The angle in degrees that specifies the outermost center point of the field of view cone. 0 degrees
        /// points right.
        /// </summary>
        public readonly double Angle;

        /// <summary>
        /// The angle, in degrees, that specifies the full arc contained in the field of view cone --
        /// <see cref="Span"/> / 2 degrees are included on either side of the span line.
        /// </summary>
        public readonly double Span;

        /// <summary>
        /// Create and configure the event argument object.
        /// </summary>
        /// <param name="origin">Position of the FOV origin point.</param>
        /// <param name="radius">The maximum radius -- eg. the maximum distance of the field of view if completely unobstructed.</param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="Distance" />, eg. <see cref="Radius" />).
        /// </param>
        /// <param name="angle">
        /// The angle in degrees that specifies the outermost center point of the field of view cone. 0 degrees
        /// points right.
        /// </param>
        /// <param name="span">
        /// The angle, in degrees, that specifies the full arc contained in the field of view cone --
        /// <paramref name="span"/>> / 2 degrees are included on either side of the span line.
        /// </param>
        public FOVRecalculatedEventArgs(Point origin, double radius, Distance distanceCalc,
                                        double angle = 0.0, double span = 360.0)
        {
            Origin = origin;
            Radius = radius;
            DistanceCalc = distanceCalc;
            Angle = angle;
            Span = span;
        }
    }
    /// <summary>
    /// Implements the capability to calculate a grid-based field of view for a map.
    /// </summary>
    /// <remarks>
    /// Generally, this class can be used to calculate and expose the results of a field of view
    /// calculation for a map.  In includes many options pertaining to the shape and size of the
    /// field of view (including options for an infinite-radius field of view).  As well, for
    /// non-infinite size fields of view, the result contains built-in linear distance falloff for
    /// the sake of creating lighting/color/fidelity differences based on distance from the center.
    /// Like most GoRogue algorithms, FOV takes as a construction parameter an IMapView representing the map.
    /// Specifically, it takes an <see cref="IMapView{T}" />, where true indicates that a tile should be
    /// considered transparent, eg. not blocking to line of sight, and false indicates that a tile should be
    /// considered opaque, eg. blocking to line of sight.
    /// The field of view can then be calculated by calling one of the various Calculate overloads.
    /// The result of the calculation is exposed in two different forms.  First, the values are exposed to you
    /// via indexers -- the FOV class itself implements <see cref="IMapView{Double}" />, where a value of 1.0
    /// represents the center of the field of view calculation, and 0.0 indicates a location that is not inside
    /// the resulting field of view at all.  Values in between are representative of linear falloff based on
    /// distance from the source.
    /// Alternatively, if the distance from the source is irrelevant, FOV also provides the result of the calculation
    /// via <see cref="BooleanFOV" />, which is an <see cref="IMapView{Boolean}" /> where a value of true indicates
    /// that a location is within field of view, and a value of false indicates it is outside of the field of view.
    /// </remarks>
    [PublicAPI]
    public class FOV : IReadOnlyFOV
    {
        private readonly IMapView<bool> _fovMap;
        private HashSet<Point> _currentFOV;
        private double[,] _light;
        private HashSet<Point> _previousFOV;

        /// <summary>
        /// Fired whenever the FOV is recalculated.
        /// </summary>
        public event EventHandler<FOVRecalculatedEventArgs>? Recalculated;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fovMap">
        /// The values used to calculate field of view. Values of true are considered
        /// non-blocking (transparent) to line of sight, while false values are considered
        /// to be blocking.
        /// </param>
        public FOV(IMapView<bool> fovMap)
        {
            _fovMap = fovMap;
            BooleanFOV = new LambdaTranslationMap<double, bool>(this, val => val > 0.0);

            _light = new double[fovMap.Width, fovMap.Height];
            _currentFOV = new HashSet<Point>();
            _previousFOV = new HashSet<Point>();
        }

        /// <inheritdoc />
        public IMapView<bool> BooleanFOV { get; private set; }

        /// <inheritdoc />
        public int Count => Width * Height;

        /// <inheritdoc />
        public IEnumerable<Point> CurrentFOV => _currentFOV;

        /// <inheritdoc />
        public int Height => _fovMap.Height;

        /// <inheritdoc />
        public IEnumerable<Point> NewlySeen => _currentFOV.Where(pos => !_previousFOV.Contains(pos));

        /// <inheritdoc />
        public IEnumerable<Point> NewlyUnseen => _previousFOV.Where(pos => !_currentFOV.Contains(pos));

        /// <inheritdoc />
        public int Width => _fovMap.Width;

        /// <summary>
        /// Returns the field of view value for the given position.
        /// </summary>
        /// <param name="index1D">Position to return the field of view value for, as a 1D-index-style value.</param>
        /// <returns>The field of view value for the given position.</returns>
        public double this[int index1D] => _light[Point.ToXValue(index1D, Width), Point.ToYValue(index1D, Width)];

        /// <summary>
        /// Returns the field of view value for the given position.
        /// </summary>
        /// <param name="position">The position to return the field of view value for.</param>
        /// <returns>The field of view value for the given position.</returns>
        public double this[Point position] => _light[position.X, position.Y];

        /// <summary>
        /// Returns the field of view value for the given position.
        /// </summary>
        /// <param name="x">X-coordinate of the position to return the FOV value for.</param>
        /// <param name="y">Y-coordinate of the position to return the FOV value for.</param>
        /// <returns>The field of view value for the given position.</returns>
        public double this[int x, int y] => _light[x, y];

        /// <inheritdoc />
        public IReadOnlyFOV AsReadOnly() => this;

        // Note: since the values aren't compile-time constants, we have to do it this way (with overloads,
        // vs. default values).

        /// <summary>
        /// Calculates FOV given an origin point and a radius. If no radius is specified, simply
        /// calculates with a radius of maximum integer value, which is effectively infinite. Radius
        /// is computed as a circle around the source (type <see cref="Radius.Circle" />).
        /// </summary>
        /// <param name="originX">Coordinate x-value of the origin.</param>
        /// <param name="originY">Coordinate y-value of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// If no radius is specified, it is effectively infinite.
        /// </param>
        public void Calculate(int originX, int originY, double radius = double.MaxValue)
            => Calculate(originX, originY, radius, Radius.Circle);

        /// <summary>
        /// Calculates FOV given an origin point and a radius. If no radius is specified,
        /// simply calculates with a radius of maximum integer value, which is effectively infinite.
        /// Radius is computed as a circle around the source (type <see cref="Radius.Circle" />).
        /// </summary>
        /// <param name="origin">Position of origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// If no radius is specified, it is effectively infinite.
        /// </param>
        public void Calculate(Point origin, double radius = double.MaxValue)
            => Calculate(origin.X, origin.Y, radius, Radius.Circle);

        /// <summary>
        /// Calculates FOV given an origin point, a radius, and radius shape.
        /// </summary>
        /// <param name="originX">Coordinate x-value of the origin.</param>
        /// <param name="originY">Coordinate y-value of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="Distance" />, eg. <see cref="Radius" />).
        /// </param>
        public void Calculate(int originX, int originY, double radius, Distance distanceCalc)
        {
            radius = Math.Max(1, radius);
            var decay = 1.0 / (radius + 1);

            _previousFOV = _currentFOV;
            _currentFOV = new HashSet<Point>();

            InitializeLightMap();
            _light[originX, originY] = 1; // Full power to starting space
            _currentFOV.Add(new Point(originX, originY));

            foreach (var d in AdjacencyRule.Diagonals.DirectionsOfNeighbors())
            {
                ShadowCast(1, 1.0, 0.0, 0, d.DeltaX, d.DeltaY, 0, radius, originX, originY, decay, _light, _currentFOV,
                    _fovMap, distanceCalc);
                ShadowCast(1, 1.0, 0.0, d.DeltaX, 0, 0, d.DeltaY, radius, originX, originY, decay, _light, _currentFOV,
                    _fovMap, distanceCalc);
            }

            Recalculated?.Invoke(this, new FOVRecalculatedEventArgs(new Point(originX, originY), radius, distanceCalc));
        }

        /// <summary>
        /// Calculates FOV given an origin point, a radius, and a radius shape.
        /// </summary>
        /// <param name="origin">Coordinate of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="Distance" />, eg. <see cref="Radius" />).
        /// </param>
        public void Calculate(Point origin, double radius, Distance distanceCalc)
            => Calculate(origin.X, origin.Y, radius, distanceCalc);

        /// <summary>
        /// Calculates FOV given an origin point, a radius, a radius shape, and the given field of view
        /// restrictions <paramref name="angle" /> and <paramref name="span" />.  The resulting field of view,
        /// if unobstructed, will be a cone defined by the angle and span given.
        /// </summary>
        /// <param name="originX">Coordinate x-value of the origin.</param>
        /// <param name="originY">Coordinate y-value of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="Distance" />, eg. <see cref="Radius" />).
        /// </param>
        /// <param name="angle">
        /// The angle in degrees that specifies the outermost center point of the field of view cone. 0 degrees
        /// points right.
        /// </param>
        /// <param name="span">
        /// The angle, in degrees, that specifies the full arc contained in the field of view cone --
        /// <paramref name="angle" /> / 2 degrees are included on either side of the cone's center line.
        /// </param>
        public void Calculate(int originX, int originY, double radius, Distance distanceCalc, double angle, double span)
        {
            radius = Math.Max(1, radius);
            var decay = 1.0 / (radius + 1);

            angle = (angle > 360.0 || angle < 0 ? Math.IEEERemainder(angle, 360.0) : angle) *
                    SadRogue.Primitives.MathHelpers.DegreePctOfCircle;
            span *= SadRogue.Primitives.MathHelpers.DegreePctOfCircle;

            _previousFOV = _currentFOV;
            _currentFOV = new HashSet<Point>();

            InitializeLightMap();
            _light[originX, originY] = 1; // Full power to starting space
            _currentFOV.Add(new Point(originX, originY));

            ShadowCastLimited(1, 1.0, 0.0, 0, 1, 1, 0, radius, originX, originY, decay, _light, _currentFOV, _fovMap,
                distanceCalc, angle, span);
            ShadowCastLimited(1, 1.0, 0.0, 1, 0, 0, 1, radius, originX, originY, decay, _light, _currentFOV, _fovMap,
                distanceCalc, angle, span);

            ShadowCastLimited(1, 1.0, 0.0, 0, -1, 1, 0, radius, originX, originY, decay, _light, _currentFOV, _fovMap,
                distanceCalc, angle, span);
            ShadowCastLimited(1, 1.0, 0.0, -1, 0, 0, 1, radius, originX, originY, decay, _light, _currentFOV, _fovMap,
                distanceCalc, angle, span);

            ShadowCastLimited(1, 1.0, 0.0, 0, -1, -1, 0, radius, originX, originY, decay, _light, _currentFOV, _fovMap,
                distanceCalc, angle, span);
            ShadowCastLimited(1, 1.0, 0.0, -1, 0, 0, -1, radius, originX, originY, decay, _light, _currentFOV, _fovMap,
                distanceCalc, angle, span);

            ShadowCastLimited(1, 1.0, 0.0, 0, 1, -1, 0, radius, originX, originY, decay, _light, _currentFOV, _fovMap,
                distanceCalc, angle, span);
            ShadowCastLimited(1, 1.0, 0.0, 1, 0, 0, -1, radius, originX, originY, decay, _light, _currentFOV, _fovMap,
                distanceCalc, angle, span);

            Recalculated?.Invoke(this, new FOVRecalculatedEventArgs(new Point(originX, originY), radius, distanceCalc, angle, span));
        }

        /// <summary>
        /// Calculates FOV given an origin point, a radius, a radius shape, and the given field of view
        /// restrictions <paramref name="angle" /> and <paramref name="span" />.  The resulting field of view,
        /// if unobstructed, will be a cone defined by the angle and span given.
        /// </summary>
        /// <param name="origin">Coordinate of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="Distance" />, eg. <see cref="Radius" />).
        /// </param>
        /// <param name="angle">
        /// The angle in degrees that specifies the outermost center point of the field of view cone. 0 degrees
        /// points right.
        /// </param>
        /// <param name="span">
        /// The angle, in degrees, that specifies the full arc contained in the field of view cone --
        /// <paramref name="angle" /> / 2 degrees are included on either side of the span line.
        /// </param>
        public void Calculate(Point origin, double radius, Distance distanceCalc, double angle, double span)
            => Calculate(origin.X, origin.Y, radius, distanceCalc, angle, span);

        // Warning intentionally disabled -- see SenseMap.ToString for details as to why this is not bad.
#pragma warning disable RECS0137

        // ReSharper disable once MethodOverloadWithOptionalParameter
        /// <summary>
        /// ToString overload that customizes the characters used to represent the map.
        /// </summary>
        /// <param name="normal">The character used for any location not in FOV.</param>
        /// <param name="sourceValue">The character used for any location that is in FOV.</param>
        /// <returns>The string representation of FOV, using the specified characters.</returns>
        public string ToString(char normal = '-', char sourceValue = '+')
#pragma warning restore RECS0137
        {
            string result = "";

            for (var y = 0; y < _fovMap.Height; y++)
            {
                for (var x = 0; x < _fovMap.Width; x++)
                {
                    result += _light[x, y] > 0.0 ? sourceValue : normal;
                    result += " ";
                }

                result += '\n';
            }

            return result;
        }

        /// <summary>
        /// Returns a string representation of the map, with the actual values in the FOV, rounded to
        /// the given number of decimal places.
        /// </summary>
        /// <param name="decimalPlaces">The number of decimal places to round to.</param>
        /// <returns>A string representation of FOV, rounded to the given number of decimal places.</returns>
        public string ToString(int decimalPlaces)
            => _light.ExtendToStringGrid(elementStringifier: obj => obj.ToString("0." + "0".Multiply(decimalPlaces)));

        /// <summary>
        /// Returns a string representation of the map, where any location not in FOV is represented
        /// by a '-' character, and any position in FOV is represented by a '+'.
        /// </summary>
        /// <returns>A (multi-line) string representation of the FOV.</returns>
        public override string ToString() => ToString();

        private static void ShadowCast(int row, double start, double end, int xx, int xy, int yx, int yy,
                                       double radius, int startX, int startY, double decay, double[,] lightMap,
                                       HashSet<Point> fovSet,
                                       IMapView<bool> map, Distance distanceStrategy)
        {
            double newStart = 0;
            if (start < end)
                return;

            var blocked = false;
            for (var distance = row; distance <= radius && distance < map.Width + map.Height && !blocked; distance++)
            {
                var deltaY = -distance;
                for (var deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    var currentX = startX + deltaX * xx + deltaY * xy;
                    var currentY = startY + deltaX * yx + deltaY * yy;
                    double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (!(currentX >= 0 && currentY >= 0 && currentX < map.Width && currentY < map.Height) ||
                        start < rightSlope)
                        continue;
                    if (end > leftSlope)
                        break;

                    var deltaRadius = distanceStrategy.Calculate(deltaX, deltaY);
                    // If within lightable area, light if needed
                    if (deltaRadius <= radius)
                    {
                        var bright = 1 - decay * deltaRadius;
                        lightMap[currentX, currentY] = bright;
                        if (bright > 0.0)
                            fovSet.Add(new Point(currentX, currentY));
                    }

                    if (blocked) // Previous cell was blocked
                    {
                        if (!map[currentX, currentY]) // Hit a wall...
                            newStart = rightSlope;
                        else
                        {
                            blocked = false;
                            start = newStart;
                        }
                    }
                    else
                    {
                        if (map[currentX, currentY] || !(distance < radius)) continue;

                        blocked = true;
                        ShadowCast(distance + 1, start, leftSlope, xx, xy, yx, yy, radius, startX, startY, decay,
                            lightMap, fovSet, map, distanceStrategy);
                        newStart = rightSlope;
                    }
                }
            }
        }

        private static void ShadowCastLimited(int row, double start, double end, int xx, int xy, int yx, int yy,
                                              double radius, int startX, int startY, double decay,
                                              double[,] lightMap, HashSet<Point> fovSet, IMapView<bool> map,
                                              Distance distanceStrategy, double angle, double span)
        {
            double newStart = 0;
            if (start < end)
                return;

            var blocked = false;
            for (var distance = row; distance <= radius && distance < map.Width + map.Height && !blocked; distance++)
            {
                var deltaY = -distance;
                for (var deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    var currentX = startX + deltaX * xx + deltaY * xy;
                    var currentY = startY + deltaX * yx + deltaY * yy;
                    double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (!(currentX >= 0 && currentY >= 0 && currentX < map.Width && currentY < map.Height) ||
                        start < rightSlope)
                        continue;
                    if (end > leftSlope)
                        break;

                    var deltaRadius = distanceStrategy.Calculate(deltaX, deltaY);
                    var at2 = Math.Abs(angle - MathHelpers.ScaledAtan2Approx(currentY - startY, currentX - startX));

                    // Check if within lightable area, light if needed
                    if (deltaRadius <= radius && (at2 <= span * 0.5 || at2 >= 1.0 - span * 0.5))
                    {
                        var bright = 1 - decay * deltaRadius;
                        lightMap[currentX, currentY] = bright;

                        if (bright > 0.0)
                            fovSet.Add(new Point(currentX, currentY));
                    }

                    if (blocked) // Previous cell was blocking
                    {
                        if (!map[currentX, currentY]) // We hit a wall...
                            newStart = rightSlope;
                        else
                        {
                            blocked = false;
                            start = newStart;
                        }
                    }
                    else if (!map[currentX, currentY] && distance < radius) // Wall within line of sight
                    {
                        blocked = true;
                        ShadowCastLimited(distance + 1, start, leftSlope, xx, xy, yx, yy, radius, startX, startY, decay,
                            lightMap, fovSet, map, distanceStrategy, angle, span);
                        newStart = rightSlope;
                    }
                }
            }
        }

        private void InitializeLightMap()
        {
            if (_light.GetLength(0) != _fovMap.Width || _light.GetLength(1) != _fovMap.Height)
                _light = new double[_fovMap.Width, _fovMap.Height];
            else
                Array.Clear(_light, 0, _light.Length);
        }
    }
}
