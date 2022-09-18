using System;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.SenseMapping.Sources
{
    /// <summary>
    /// Interfaces representing sources which can be used by sense maps.
    /// </summary>
    [PublicAPI]
    public interface ISenseSource
    {
        /// <summary>
        /// A grid view representing the result of a sense map calculation.
        /// </summary>
        IGridView<double> ResultView { get; }

        /// <summary>
        /// The position on a map that the source is located at.
        /// </summary>
        ref Point Position { get; }

        /// <summary>
        /// The maximum radius of the source -- this is the maximum distance the source values will
        /// emanate, provided the area is completely unobstructed. Changing this will trigger
        /// resizing (re-allocation) of the underlying arrays.
        /// </summary>
        double Radius { get; set; }

        /// <summary>
        /// The amount of decrease in sense source value per unit of distance.  Calculated automatically as a product of
        /// <see cref="Intensity"/> and <see cref="Radius"/>.
        /// </summary>
        public double Decay { get; }

        /// <summary>
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="Distance" />, such as <see cref="SadRogue.Primitives.Radius" />).
        /// </summary>
        Distance DistanceCalc { get; set; }

        /// <summary>
        /// Whether or not this source is enabled. If a source is disabled when <see cref="SenseMap.Calculate" />
        /// is called, the source does not calculate values and is effectively assumed to be "off".
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Whether or not the spreading of values from this source is restricted to an angle and span.
        /// </summary>
        bool IsAngleRestricted { get; set; }

        /// <summary>
        /// The starting value of the source to spread.  Defaults to 1.0.
        /// </summary>
        double Intensity { get; set; }

        /// <summary>
        /// If <see cref="IsAngleRestricted" /> is true, the angle in degrees that represents a line from the source's start to
        /// the outermost center point of the cone formed by the source's calculated values.  0 degrees points up, and
        /// increases in angle move clockwise (like a compass).
        /// Otherwise, this will be 0.0 degrees.
        /// </summary>
        double Angle { get; set; }

        /// <summary>
        /// If <see cref="IsAngleRestricted" /> is true, the angle in degrees that represents the full arc of the cone formed by
        /// the source's calculated values.  Otherwise, it will be 360 degrees.
        /// </summary>
        double Span { get; set; }

        /// <summary>
        /// The resistance map used to perform calculations.
        /// </summary>
        /// <remarks>
        /// Sense map implementations will set this to the sense map's resistance map prior to calculating.  This can be set via
        /// <see cref="SetResistanceMap"/>, but you shouldn't do this unless you're creating a custom sense map implementation.
        /// </remarks>
        IGridView<double>? ResistanceView { get; }

        /// <summary>
        /// Fired when the radius of the source changes.
        /// </summary>
        event EventHandler? RadiusChanged;

        /// <summary>
        /// Perform the lighting calculations if the source is enabled, by first clearing results of the existing calculation, then re-calculating it
        /// by calling <see cref="OnCalculate"/>.
        /// </summary>
        void CalculateLight();

        /// <summary>
        /// Performs the actual spreading calculation.
        /// </summary>
        void OnCalculate();

        /// <summary>
        /// Should ONLY be called from SenseMap or equivalent implementations.  Sets the resistance map used by the source for calculations.
        /// </summary>
        /// <param name="resMap"></param>
        void SetResistanceMap(IGridView<double>? resMap);
    }
}
