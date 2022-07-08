using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.FOV
{
    /// <summary>
    /// Implements a grid view which, given an <see cref="IReadOnlyFOV"/> instance, can calculate the value for <see cref="IReadOnlyFOV.DoubleResultView"/> based on
    /// <see cref="IReadOnlyFOV.BooleanResultView"/>.  The double returned will be 0 if the square is outside the FOV, and will be a value in the range (0, 1] which
    /// is a function of the distance from the appropriate source otherwise.
    /// </summary>
    /// <remarks>
    /// This is useful to use as the DoubleResultView property of an FOV, if the FOV algorithm really only deals in boolean values and the doubles returned are purely
    /// a function of the calculation's radius/distance.  FOV implementations using this method of determining the DoubleResultView values from the BooleanResultView values
    /// can typically use less memory than algorithms dealing directly in doubles; but accessing values from DoubleResultView will be slower, especially when multiple
    /// CalculateAppend calls are used before values are retrieved.
    /// </remarks>
    [PublicAPI]
    public class FOVBooleanToDoubleTranslationView : GridViewBase<double>
    {
        /// <inheritdoc/>
        public override int Width => _parent.TransparencyView.Width;

        /// <inheritdoc/>
        public override int Height => _parent.TransparencyView.Height;

        /// <inheritdoc/>
        public override double this[Point pos]
        {
            get
            {
                if (!_parent.BooleanResultView[pos]) return 0.0;

                int calculationsPerformedCount = _parent.CalculationsPerformed.Count;
                if (calculationsPerformedCount == 1)
                {
                    var calc = _parent.CalculationsPerformed[0];
                    var decay = 1.0 / (calc.Radius + 1);
                    return 1.0 - decay * calc.DistanceCalc.Calculate(calc.Origin, pos);

                }

                var maxValue = 0.0;
                for (int i = 0; i < calculationsPerformedCount; i++)
                {
                    var calc = _parent.CalculationsPerformed[i];
                    var decay = 1.0 / (calc.Radius + 1);
                    var bright = 1.0 - decay * calc.DistanceCalc.Calculate(calc.Origin, pos);
                    if (bright > maxValue) maxValue = bright;
                }

                return maxValue;
            }
        }

        private readonly IReadOnlyFOV _parent;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parent">The <see cref="IReadOnlyFOV"/> instance for which this grid view is producing double values.</param>
        public FOVBooleanToDoubleTranslationView(IReadOnlyFOV parent)
        {
            _parent = parent;
        }
    }

    /// <summary>
    /// Base class which is convenient for defining FOV algorithms which produce an array of booleans as output, and thus they derive
    /// their <see cref="IReadOnlyFOV.DoubleResultView"/> values from the boolean values, by making the double value returned a function of distance from
    /// the origin relative to radius.
    /// </summary>
    /// <remarks>
    /// This class inherits from <see cref="FOVBase"/>, and all the caveats mentioned in its remarks section should apply.  If you instead
    /// want to define an FOV algorithm as returning double values, and deriving boolean from that, you can use <see cref="DoubleBasedFOVBase"/>
    /// instead.  If neither of those use cases fits your situation, feel free to use <see cref="FOVBase"/> or <see cref="IFOV"/> directly.
    ///
    /// Although it can vary by implementation, if all other things are equal, classes that use this implementation as opposed to <see cref="DoubleBasedFOVBase"/>
    /// generally tend to take up less memory, however they may take more time to retrieve values from <see cref="IReadOnlyFOV.DoubleResultView"/>.
    /// Retrieving values from <see cref="IReadOnlyFOV.BooleanResultView"/> is generally faster than from DoubleResultView, since the double values are derived
    /// from the boolean values.
    ///
    /// Although using <see cref="DoubleBasedFOVBase"/> is usually a better default, the tradeoffs of this method can be good for extremely large maps, especially
    /// when CalculateAppend isn't used much and/or only BooleanResultView is primarily used.
    ///
    /// Library implementations typically provide versions of any given algorithm defined via both this class and <see cref="DoubleBasedFOVBase"/> where possible.
    /// </remarks>
    [PublicAPI]
    public abstract class BooleanBasedFOVBase : FOVBase
    {
        /// <inheritdoc />
        public override IGridView<double> DoubleResultView { get; }

        /// <summary>
        /// View in which the results of LOS calculations are stored.  The values in this view are used to derive
        /// the public result views.
        /// </summary>
        /// <remarks>
        /// The requirements for these values are identical to <see cref="BooleanResultView"/>.  BooleanResultView simply
        /// exposes these values directly; DoubleResultView returns 0 for any location where BooleanResultView returns false,
        /// and otherwise returns a non-0 double that is a function of distance/falloff from the source (a value in range (0, 1]).
        /// </remarks>
        protected ISettableGridView<bool> ResultView;
        /// <inheritdoc />
        public override IGridView<bool> BooleanResultView => ResultView;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="transparencyView">
        /// The values used to calculate field of view. Values of true are considered
        /// non-blocking (transparent) to line of sight, while false values are considered
        /// to be blocking.
        /// </param>
        /// <param name="resultView">The view in which FOV calculations are stored.</param>
        protected BooleanBasedFOVBase(IGridView<bool> transparencyView, ISettableGridView<bool> resultView)
            : base(transparencyView)
        {
            ResultView = resultView;
            DoubleResultView = new FOVBooleanToDoubleTranslationView(this);
        }
    }
}
