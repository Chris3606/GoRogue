using JetBrains.Annotations;
using SadRogue.Primitives.GridViews;

namespace GoRogue.FOV
{
    /// <summary>
    /// Base class which is convenient for defining FOV algorithms which produce an array of doubles as output, and thus they derive
    /// their <see cref="IReadOnlyFOV.BooleanResultView"/> values from the double values (anything greater than 0 is true).
    /// </summary>
    /// <remarks>
    /// This class inherits from <see cref="FOVBase"/>, and all the caveats mentioned in its remarks section should apply.  If you instead
    /// want to define an FOV algorithm as returning boolean values, and deriving doubles from that, you can use <see cref="BooleanBasedFOVBase"/>
    /// instead.  If neither of those use cases fits your situation, feel free to use <see cref="FOVBase"/> or <see cref="IFOV"/> directly.
    ///
    /// Although it can vary by implementation, if all other things are equal, classes that use this implementation as opposed to <see cref="BooleanBasedFOVBase"/>
    /// generally tend to take up more memory and may take more time to perform a call to the Calculate and CalculateAppend functions, however they will generally
    /// be able to retrieve values from <see cref="IReadOnlyFOV.DoubleResultView"/> more quickly.  Retrieving values from <see cref="IReadOnlyFOV.BooleanResultView"/>
    /// can be slightly slower than from DoubleResultView, since the boolean values are derived from the double values.
    ///
    /// If you have the same algorithm implemented based both on booleans and on doubles, the double version is usually a reasonable default, until/unless you have
    /// a reason to the contrary.
    /// </remarks>
    [PublicAPI]
    public abstract class DoubleBasedFOVBase : FOVBase
    {
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
        public override IGridView<double> DoubleResultView => ResultView;

        /// <inheritdoc />
        public override IGridView<bool> BooleanResultView { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="transparencyView">
        /// The values used to calculate field of view. Values of true are considered
        /// non-blocking (transparent) to line of sight, while false values are considered
        /// to be blocking.
        /// </param>
        /// <param name="resultView">The view in which FOV calculations are stored.</param>
        protected DoubleBasedFOVBase(IGridView<bool> transparencyView, ISettableGridView<double> resultView)
            : base(transparencyView)
        {
            ResultView = resultView;
            BooleanResultView = new LambdaTranslationGridView<double, bool>(ResultView, val => val > 0.0);
        }
    }
}
