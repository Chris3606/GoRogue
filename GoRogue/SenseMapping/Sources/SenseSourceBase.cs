using System;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.SenseMapping.Sources
{
    /// <summary>
    /// A base class for creating <see cref="ISenseSource"/> implementations that boils an implementation down to primarily implementing the <see cref="OnCalculate"/>
    /// function.
    /// </summary>
    /// <remarks>
    /// This class uses an ArrayView as the <see cref="ResultView"/>, in order to enable common functions efficiently.  Use cases for a custom view here should be
    /// relatively limited, as the view must be settable and resizable; if a custom implementation is needed, you may implement <see cref="ISenseSource"/> directly.
    /// </remarks>
    [PublicAPI]
    public abstract class SenseSourceBase : ISenseSource
    {
        /// <summary>
        /// The size of the result view (eg. it's width and height); cached for efficiency and convenience.
        /// </summary>
        protected int Size;

        /// <summary>
        /// The coordinate which will be the center point of the result view, ie. the center is (Center, Center).
        /// </summary>
        /// <remarks>
        /// This is equivalent to Size / 2; however is cached for performance and convenience since this calculation is performed frequently.
        /// </remarks>
        protected int Center;

        // Null-forgiving because this is initialized in the Radius setter but we don't have access to the MemberNotNull attribute.
        /// <summary>
        /// The result view used to record results.
        /// </summary>
        protected ArrayView<double> ResultViewBacking = null!;
        /// <inheritdoc/>
        public IGridView<double> ResultView => ResultViewBacking;

        /// <inheritdoc/>
        public double Decay { get; private set; }

        private Point _position;
        /// <inheritdoc/>
        public ref Point Position => ref _position;

        private double _radius;
        /// <inheritdoc/>
        public double Radius
        {
            get => _radius;
            set
            {
                if (value <= 0.0)
                    throw new ArgumentOutOfRangeException(nameof(Radius),
                        "Radius for a SenseSource must be greater than 0.");

                var newRadius = Math.Max(1, value);
                if (newRadius.Equals(_radius)) return;

                _radius = newRadius;
                // Can round down here because the EUCLIDEAN distance shape is always contained within
                // the Chebyshev distance shape
                Size = (int)_radius * 2 + 1;
                // Any times 2 is even, plus one is odd. rad 3, 3*2 = 6, +1 = 7. 7/2=3, and this is used as an index in an array for the middle, so math works
                Center = Size / 2;
                ResultViewBacking = new ArrayView<double>(Size, Size);

                Decay = _intensity / (_radius + 1);

                RadiusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc/>
        public event EventHandler? RadiusChanged;

        /// <inheritdoc/>
        public Distance DistanceCalc { get; set; }

        /// <inheritdoc/>
        public bool Enabled { get; set; }

        /// <inheritdoc/>
        public bool IsAngleRestricted { get; set; }

        private double _intensity;
        /// <inheritdoc/>
        public double Intensity
        {
            get => _intensity;

            set
            {
                if (value < 0.0)
                    throw new ArgumentOutOfRangeException(nameof(Intensity),
                        "Intensity for sense source cannot be set to less than 0.0.");


                _intensity = value;
                Decay = _intensity / (_radius + 1);
            }
        }

        /// <summary>
        /// The <see cref="Angle"/> value, but offset 90 degrees clockwise; ie, 0 points right instead of up.  This value typically
        /// works better for actual light calculations (as the definition more closely matches the unit circle).
        /// </summary>
        protected double AngleInternal;

        // TODO: This definition is wrong.  +90 should be clockwise, -90 should be counter?
        /// <inheritdoc/>
        public double Angle
        {
            get => IsAngleRestricted ? MathHelpers.WrapAround(AngleInternal + 90, 360.0) : 0.0;
            set
            {
                // Offset internal angle to 90 degrees being up instead of right
                AngleInternal = value - 90;

                // Wrap angle to proper degrees
                if (AngleInternal > 360.0 || AngleInternal < 0)
                    AngleInternal = MathHelpers.WrapAround(AngleInternal, 360.0);
            }
        }

        private double _span;
        /// <inheritdoc/>
        public double Span
        {
            get => IsAngleRestricted ? _span : 360.0;
            set
            {
                if (value < 0.0 || value > 360.0)
                    throw new ArgumentOutOfRangeException(nameof(Span), "SenseSource Span must be in range [0, 360]");

                _span = value;

                IsAngleRestricted = !_span.Equals(360.0);
            }
        }

        /// <inheritdoc/>
        public IGridView<double>? ResistanceView { get; private set; }
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
        /// implicitly convertible to <see cref="Distance" />, such as <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
        protected SenseSourceBase(Point position, double radius, Distance distanceCalc,
            double intensity = 1.0)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException(nameof(radius), "Sense source radius cannot be 0");

            if (intensity < 0)
                throw new ArgumentOutOfRangeException(nameof(intensity),
                    "Sense source intensity cannot be less than 0.0.");

            Position = position;
            Radius = radius; // Arrays are initialized by this setter
            DistanceCalc = distanceCalc;

            ResistanceView = null;
            Enabled = true;

            IsAngleRestricted = false;
            Intensity = intensity;
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
        /// implicitly convertible to <see cref="Distance" />, such as <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
        protected SenseSourceBase(int positionX, int positionY, double radius, Distance distanceCalc,
            double intensity = 1.0)
            : this(new Point(positionX, positionY), radius, distanceCalc, intensity)
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
        /// implicitly convertible to <see cref="Distance" />, such as <see cref="SadRogue.Primitives.Radius" />).
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
        protected SenseSourceBase(Point position, double radius, Distance distanceCalc, double angle,
                           double span, double intensity = 1.0)
            : this(position, radius, distanceCalc, intensity)
        {
            if (span < 0.0 || span > 360.0)
                throw new ArgumentOutOfRangeException(nameof(span),
                    "Span used to initialize a sense source must be in range [0, 360]");

            Angle = angle;
            // This also sets IsAngleRestricted appropriately.
            Span = span;
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
        /// implicitly convertible to <see cref="Distance" />, such as <see cref="SadRogue.Primitives.Radius" />).
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
        protected SenseSourceBase(int positionX, int positionY, double radius, Distance distanceCalc,
            double angle, double span, double intensity = 1.0)
            : this(new Point(positionX, positionY), radius, distanceCalc, angle, span, intensity)
        { }

        /// <summary>
        /// Resets calculation state so a new set of calculations can begin.
        /// </summary>
        protected virtual void Reset()
        {
            ResultViewBacking.Clear();
            ResultViewBacking[Center, Center] = _intensity; // source light is center, starts out at our intensity
        }

        /// <inheritdoc/>
        public void CalculateLight()
        {
            if (!Enabled) return;

            if (ResistanceView == null)
                throw new InvalidOperationException(
                    "Attempted to calculate the light of a sense map without a resistance map.  This is almost certainly a bug in the implementation of the sense map.");

            Reset();
            OnCalculate();
        }

        /// <inheritdoc/>
        public abstract void OnCalculate();


        /// <inheritdoc/>
        public void SetResistanceMap(IGridView<double>? resMap) => ResistanceView = resMap;

        /// <summary>
        /// Returns a string representation of the configuration of this SenseSource.
        /// </summary>
        /// <returns>A string representation of the configuration of this SenseSource.</returns>
        public override string ToString()
            => $"Enabled: {Enabled}, Type: {GetType().Name}, Radius Mode: {(Radius)DistanceCalc}, Position: {Position}, Radius: {Radius}";
    }
}
