using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.FOV
{
    /// <summary>
    /// Base class that is convenient for creating custom implementations of the <see cref="IFOV"/> interface.
    /// </summary>
    [PublicAPI]
    public abstract class FOVBase : IFOV
    {
        /// <inheritdoc/>
        public IGridView<bool> TransparencyView { get; }

        /// <inheritdoc/>
        public event EventHandler<FOVRecalculatedEventArgs>? Recalculated;

        /// <inheritdoc/>
        public abstract IEnumerable<Point> CurrentFOV { get; }

        /// <inheritdoc/>
        public abstract IEnumerable<Point> NewlySeen { get; }

        /// <inheritdoc/>
        public abstract IEnumerable<Point> NewlyUnseen { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="transparencyView">
        /// The values used to calculate field of view. Values of true are considered
        /// non-blocking (transparent) to line of sight, while false values are considered
        /// to be blocking.
        /// </param>
        /// <param name="resultView">The view in which FOV calculations are stored.</param>
        protected FOVBase(IGridView<bool> transparencyView, ISettableGridView<double> resultView)
        {
            ResultView = resultView;
            TransparencyView = transparencyView;
            BooleanResultView = new LambdaTranslationGridView<double, bool>(ResultView, val => val > 0.0);
        }

        /// <inheritdoc />
        public IGridView<bool> BooleanResultView { get; }

        /// <summary>
        /// View in which the results of LOS calculations are stored.  The values in this view are used to derive
        /// the public result views.
        /// </summary>
        /// <remarks>
        /// The requirements for these values are identical to <see cref="DoubleResultView"/>.  DoubleResultView simply
        /// exposes these values directly; BooleanResultView considers any location where ResultView > 0 to be true.
        /// </remarks>
        protected ISettableGridView<double> ResultView;

        /// <inheritdoc />
        public IGridView<double> DoubleResultView => ResultView;

        /// <inheritdoc />
        public IReadOnlyFOV AsReadOnly() => this;

        /// <summary>
        /// Calculates FOV given an origin point, a radius, and radius shape.
        /// </summary>
        /// <remarks>
        /// Custom implementations would implement this function to perform their calculation; the Calculate functions
        /// call this then fire relevant events.
        /// </remarks>
        /// <param name="originX">Coordinate x-value of the origin.</param>
        /// <param name="originY">Coordinate y-value of the origin.</param>
        /// <param name="radius">
        /// The maximum radius -- basically the maximum distance of the field of view if completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="Distance" />, eg. <see cref="Radius" />).
        /// </param>
        protected abstract void OnCalculate(int originX, int originY, double radius, Distance distanceCalc);

        /// <summary>
        /// Calculates FOV given an origin point, a radius, a radius shape, and the given field of view
        /// restrictions <paramref name="angle" /> and <paramref name="span" />.  The resulting field of view,
        /// if unobstructed, will be a cone defined by the angle and span given.
        /// </summary>
        /// <remarks>
        /// Custom implementations would implement this function to perform their calculation; the Calculate functions
        /// call this then fire relevant events.
        /// </remarks>
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
        protected abstract void OnCalculate(int originX, int originY, double radius, Distance distanceCalc, double angle, double span);

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
            OnCalculate(originX, originY, radius, distanceCalc);

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
            OnCalculate(originX, originY, radius, distanceCalc, angle, span);

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

            for (var y = 0; y < BooleanResultView.Height; y++)
            {
                for (var x = 0; x < BooleanResultView.Width; x++)
                {
                    result += BooleanResultView[x, y] ? sourceValue : normal;
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
            => ResultView.ExtendToString(elementStringifier: obj => obj.ToString("0." + "0".Multiply(decimalPlaces)));

        /// <summary>
        /// Returns a string representation of the map, where any location not in FOV is represented
        /// by a '-' character, and any position in FOV is represented by a '+'.
        /// </summary>
        /// <returns>A (multi-line) string representation of the FOV.</returns>
        public override string ToString() => ToString();
    }
}
