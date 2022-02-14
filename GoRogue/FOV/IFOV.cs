using System;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.FOV
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
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, eg. <see cref="SadRogue.Primitives.Radius" />).
        /// </summary>
        public readonly Distance DistanceCalc;

        /// <summary>
        /// The angle in degrees that specifies the outermost center point of the field of view cone. 0 degrees
        /// points upward, and increases move clockwise (like a compass)
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
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, eg. <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        /// <param name="angle">
        /// The angle in degrees that specifies the outermost center point of the field of view cone. 0 degrees
        /// points up, and increases move the cone clockwise (like a compass).
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
    /// Interface representing the capability to calculate a grid-based field of view for a map.
    /// </summary>
    /// <remarks>
    /// If you want a concrete implementation of FOV, see the <see cref="RecursiveShadowcastingFOV"/> class.  If you're implementing your
    /// own FOV system, you may want to consider inheriting from <see cref="FOVBase"/> if possible, as it implements
    /// much of the boilerplate code involved in implementing this interface.
    ///
    /// This interface conceptualizes FOV as a calculation based on a "transparency" map, represented by the
    /// <see cref="IReadOnlyFOV.TransparencyView"/> property.  This view acts as input to the calculation; a value of
    /// "true" indicates that a given position is transparent (i.e. does NOT block LOS), whereas a value of "false"
    /// indicates that a position DOES block line of sight.
    ///
    /// The calculation will be performed by the Calculate functions.  It should perform calculations such that
    /// the following fields produce the correct output (which is noted in the property documentation):
    ///     - <see cref="IReadOnlyFOV.BooleanResultView"/>
    ///     - <see cref="IReadOnlyFOV.DoubleResultView"/>
    ///     - <see cref="IReadOnlyFOV.CurrentFOV"/>
    ///     - <see cref="IReadOnlyFOV.NewlySeen"/>
    ///     - <see cref="IReadOnlyFOV.NewlyUnseen"/>
    ///
    /// The Calculate functions should also fire the <see cref="Recalculated"/> event.
    ///
    /// Some of the output values can be derived based on others; for example, by definition, BooleanResultView should
    /// simply return true for any square where DoubleResultView returns a value > 0.0.  For this reason, as stated above,
    /// it is recommended that custom implementations inherit from <see cref="FOVBase"/> where possible, as it sets this
    /// all up correctly, and instead requires that you implement the minimal subset of properties/functions.
    /// </remarks>
    [PublicAPI]
    public interface IFOV : IReadOnlyFOV
    {
        /// <summary>
        /// Fired whenever the FOV is recalculated.
        /// </summary>
        event EventHandler<FOVRecalculatedEventArgs>? Recalculated;

        /// <summary>
        /// Fired when the existing FOV is reset prior to calculating a new one.
        /// </summary>
        event EventHandler? VisibilityReset;

        /// <summary>
        /// Calculates FOV given an origin point and a radius, overwriting the current FOV entirely. If no radius is
        /// specified, simply calculates with a radius of maximum integer value, which is effectively infinite. Radius
        /// is computed as a circle around the source (type <see cref="SadRogue.Primitives.Radius.Circle" />).
        /// </summary>
        /// <param name="originX">Coordinate x-value of the origin.</param>
        /// <param name="originY">Coordinate y-value of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// If no radius is specified, it is effectively infinite.
        /// </param>
        public void Calculate(int originX, int originY, double radius = double.MaxValue);

        /// <summary>
        /// Calculates FOV given an origin point and a radius, adding the result onto the currently visible cells. If no
        /// radius is specified, simply calculates with a radius of maximum integer value, which is effectively infinite.
        /// Radius is computed as a circle around the source (type <see cref="SadRogue.Primitives.Radius.Circle" />).
        /// </summary>
        /// <param name="originX">Coordinate x-value of the origin.</param>
        /// <param name="originY">Coordinate y-value of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// If no radius is specified, it is effectively infinite.
        /// </param>
        public void CalculateAppend(int originX, int originY, double radius = double.MaxValue);

        /// <summary>
        /// Calculates FOV given an origin point and a radius, overwriting the current FOV entirely. If no radius is
        /// specified, simply calculates with a radius of maximum integer value, which is effectively infinite.
        /// Radius is computed as a circle around the source (type <see cref="SadRogue.Primitives.Radius.Circle" />).
        /// </summary>
        /// <param name="origin">Position of origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// If no radius is specified, it is effectively infinite.
        /// </param>
        public void Calculate(Point origin, double radius = double.MaxValue);

        /// <summary>
        /// Calculates FOV given an origin point and a radius, adding the result onto the currently visible cells. If no
        /// radius is specified, simply calculates with a radius of maximum integer value, which is effectively infinite.
        /// Radius is computed as a circle around the source (type <see cref="SadRogue.Primitives.Radius.Circle" />).
        /// </summary>
        /// <param name="origin">Position of origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// If no radius is specified, it is effectively infinite.
        /// </param>
        public void CalculateAppend(Point origin, double radius = double.MaxValue);

        /// <summary>
        /// Calculates FOV given an origin point, a radius, and radius shape, overwriting the current FOV entirely.
        /// </summary>
        /// <param name="originX">Coordinate x-value of the origin.</param>
        /// <param name="originY">Coordinate y-value of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, eg. <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        public void Calculate(int originX, int originY, double radius, Distance distanceCalc);

        /// <summary>
        /// Calculates FOV given an origin point, a radius, and radius shape, adding the result onto the currently
        /// visible cells.
        /// </summary>
        /// <param name="originX">Coordinate x-value of the origin.</param>
        /// <param name="originY">Coordinate y-value of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, eg. <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        public void CalculateAppend(int originX, int originY, double radius, Distance distanceCalc);

        /// <summary>
        /// Calculates FOV given an origin point, a radius, and a radius shape, overwriting the current FOV entirely.
        /// </summary>
        /// <param name="origin">Coordinate of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, eg. <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        public void Calculate(Point origin, double radius, Distance distanceCalc);

        /// <summary>
        /// Calculates FOV given an origin point, a radius, and a radius shape, , adding the result onto the currently
        /// visible cells.
        /// </summary>
        /// <param name="origin">Coordinate of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, eg. <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        public void CalculateAppend(Point origin, double radius, Distance distanceCalc);

        /// <summary>
        /// Calculates FOV given an origin point, a radius, a radius shape, and the given field of view
        /// restrictions <paramref name="angle" /> and <paramref name="span" />.  The current field of view will be
        /// entirely overwritten with the new one.  The resulting field of view, if unobstructed, will be a cone defined
        /// by the angle and span given.
        /// </summary>
        /// <param name="originX">Coordinate x-value of the origin.</param>
        /// <param name="originY">Coordinate y-value of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, eg. <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        /// <param name="angle">
        /// The angle in degrees that specifies the outermost center point of the field of view cone. 0 degrees
        /// points up, and angle increases result in the cone moving clockwise (like a compass).
        /// </param>
        /// <param name="span">
        /// The angle, in degrees, that specifies the full arc contained in the field of view cone --
        /// <paramref name="angle" /> / 2 degrees are included on either side of the cone's center line.
        /// </param>
        public void Calculate(int originX, int originY, double radius, Distance distanceCalc, double angle,
            double span);

        /// <summary>
        /// Calculates FOV given an origin point, a radius, a radius shape, and the given field of view
        /// restrictions <paramref name="angle" /> and <paramref name="span" />.  The new field of view will be
        /// added onto the current one.  The resulting field of view, if unobstructed, will be a cone defined
        /// by the angle and span given.
        /// </summary>
        /// <param name="originX">Coordinate x-value of the origin.</param>
        /// <param name="originY">Coordinate y-value of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, eg. <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        /// <param name="angle">
        /// The angle in degrees that specifies the outermost center point of the field of view cone. 0 degrees
        /// points up, and angle increases result in the cone moving clockwise (like a compass).
        /// </param>
        /// <param name="span">
        /// The angle, in degrees, that specifies the full arc contained in the field of view cone --
        /// <paramref name="angle" /> / 2 degrees are included on either side of the cone's center line.
        /// </param>
        public void CalculateAppend(int originX, int originY, double radius, Distance distanceCalc, double angle,
                              double span);

        /// <summary>
        /// Calculates FOV given an origin point, a radius, a radius shape, and the given field of view
        /// restrictions <paramref name="angle" /> and <paramref name="span" />.  The current field of view will be
        /// entirely overwritten with the new one.  The resulting field of view,
        /// if unobstructed, will be a cone defined by the angle and span given.
        /// </summary>
        /// <param name="origin">Coordinate of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, eg. <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        /// <param name="angle">
        /// The angle in degrees that specifies the outermost center point of the field of view cone. 0 degrees
        /// points up, and angle increases result in the cone moving clockwise (like a compass).
        /// </param>
        /// <param name="span">
        /// The angle, in degrees, that specifies the full arc contained in the field of view cone --
        /// <paramref name="angle" /> / 2 degrees are included on either side of the span line.
        /// </param>
        public void Calculate(Point origin, double radius, Distance distanceCalc, double angle, double span);

        /// <summary>
        /// Calculates FOV given an origin point, a radius, a radius shape, and the given field of view
        /// restrictions <paramref name="angle" /> and <paramref name="span" />.  The new field of view will be
        /// added onto the current one.  The resulting field of view, if unobstructed, will be a cone defined by the
        /// angle and span given.
        /// </summary>
        /// <param name="origin">Coordinate of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, eg. <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        /// <param name="angle">
        /// The angle in degrees that specifies the outermost center point of the field of view cone. 0 degrees
        /// points up, and angle increases result in the cone moving clockwise (like a compass).
        /// </param>
        /// <param name="span">
        /// The angle, in degrees, that specifies the full arc contained in the field of view cone --
        /// <paramref name="angle" /> / 2 degrees are included on either side of the span line.
        /// </param>
        public void CalculateAppend(Point origin, double radius, Distance distanceCalc, double angle, double span);

        /// <summary>
        /// Resets the given field of view to no tiles visible.
        /// </summary>
        /// <remarks>
        /// After this function is called, any value in <see cref="IReadOnlyFOV.DoubleResultView"/> will be 0, and any
        /// value in <see cref="IReadOnlyFOV.BooleanResultView"/> will be false.  Additionally,
        /// <see cref="IReadOnlyFOV.CurrentFOV"/> will be blank.
        /// </remarks>
        public void Reset();


    }
}
