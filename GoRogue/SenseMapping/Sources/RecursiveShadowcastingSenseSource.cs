using System;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.SenseMapping.Sources
{
    /// <summary>
    /// A sense source which performs its spreading calculations by using a recursive shadowcasting algorithm.
    /// </summary>
    /// <remarks>
    /// Any location on the resistance map which is not _fully_ blocking (eg. has a value less than the source's
    /// <see cref="ISenseSource.Intensity"/>) is considered to be fully transparent, because this implementation of
    /// shadow-casting is an on-off algorithm.
    ///
    /// This calculation is faster but obviously doesn't offer support for partial resistance.  It may be useful when you only
    /// want a rough light approximation, or where your resistance map is on-off anyway.
    /// </remarks>
    [PublicAPI]
    public class RecursiveShadowcastingSenseSource : SenseSourceBase
    {
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
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
        public RecursiveShadowcastingSenseSource(Point position, double radius, Distance distanceCalc, double intensity = 1)
            : base(position, radius, distanceCalc, intensity)
        { }

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
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
        public RecursiveShadowcastingSenseSource(int positionX, int positionY, double radius, Distance distanceCalc, double intensity = 1)
            : base(positionX, positionY, radius, distanceCalc, intensity)
        { }

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
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
        public RecursiveShadowcastingSenseSource(Point position, double radius, Distance distanceCalc, double angle, double span, double intensity = 1)
            : base(position, radius, distanceCalc, angle, span, intensity)
        { }

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
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
        public RecursiveShadowcastingSenseSource(int positionX, int positionY, double radius, Distance distanceCalc, double angle, double span, double intensity = 1)
            : base(positionX, positionY, radius, distanceCalc, angle, span, intensity)
        { }

        /// <summary>
        /// Performs the spread calculations via recursive shadowcasting.
        /// </summary>
        public override void OnCalculate()
        {
            if (IsAngleRestricted)
            {
                var angle = AngleInternal * SadRogue.Primitives.MathHelpers.DegreePctOfCircle;
                var span = Span * SadRogue.Primitives.MathHelpers.DegreePctOfCircle;

                ShadowCast(1, 1.0, 0.0, 0, 1, 1, 0, angle, span);
                ShadowCast(1, 1.0, 0.0, 1, 0, 0, 1, angle, span);

                ShadowCast(1, 1.0, 0.0, 0, -1, 1, 0, angle, span);
                ShadowCast(1, 1.0, 0.0, -1, 0, 0, 1, angle, span);

                ShadowCast(1, 1.0, 0.0, 0, -1, -1, 0, angle, span);
                ShadowCast(1, 1.0, 0.0, -1, 0, 0, -1, angle, span);

                ShadowCast(1, 1.0, 0.0, 0, 1, -1, 0, angle, span);
                ShadowCast(1, 1.0, 0.0, 1, 0, 0, -1, angle, span);
            }
            else
                for (var i = 0; i < AdjacencyRule.Diagonals.DirectionsOfNeighborsCache.Length; i++)
                {
                    var d = AdjacencyRule.Diagonals.DirectionsOfNeighborsCache[i];

                    ShadowCast(1, 1.0, 0.0, 0, d.DeltaX, d.DeltaY, 0, 0, 0);
                    ShadowCast(1, 1.0, 0.0, d.DeltaX, 0, 0, d.DeltaY, 0, 0);
                }
        }

        private void ShadowCast(int row, double start, double end, int xx, int xy, int yx, int yy, double angle, double span)
        {
            double newStart = 0;
            if (start < end)
                return;

            var blocked = false;
            for (var distance = row; distance <= Radius && distance < 2 * Size && !blocked; distance++)
            {
                var deltaY = -distance;
                for (var deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    var currentX = Center + deltaX * xx + deltaY * xy;
                    var currentY = Center + deltaX * yx + deltaY * yy;
                    // TODO: Is this round correct for negative coords?
                    var gCurrentX = Position.X - (int)Radius + currentX;
                    var gCurrentY = Position.Y - (int)Radius + currentY;
                    double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (!(gCurrentX >= 0 && gCurrentY >= 0 && gCurrentX < ResistanceView!.Width && gCurrentY < ResistanceView.Height) ||
                        start < rightSlope)
                        continue;

                    if (end > leftSlope)
                        break;

                    var deltaRadius = DistanceCalc.Calculate(deltaX, deltaY);
                    var inSpan = true;
                    if (IsAngleRestricted)
                    {
                        var at2 = Math.Abs(
                            angle - MathHelpers.ScaledAtan2Approx(currentY - Center, currentX - Center));
                        inSpan = at2 <= span * 0.5 || at2 >= 1.0 - span * 0.5;
                    }
                    if (deltaRadius <= Radius && inSpan)
                    {
                        var bright = Intensity - Decay * deltaRadius;
                        ResultViewBacking[currentX, currentY] = bright;
                    }

                    if (blocked) // Previous cell was blocked
                        if (ResistanceView![gCurrentX, gCurrentY] >= Intensity) // Hit a wall...
                            newStart = rightSlope;
                        else
                        {
                            blocked = false;
                            start = newStart;
                        }
                    else
                        if (ResistanceView![gCurrentX, gCurrentY] >= Intensity && distance < Radius) // Wall within FOV
                        {
                            blocked = true;
                            ShadowCast(distance + 1, start, leftSlope, xx, xy, yx, yy, angle, span);
                            newStart = rightSlope;
                        }
                }
            }
        }
    }
}
