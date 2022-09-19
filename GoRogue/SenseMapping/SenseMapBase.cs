using System;
using System.Collections.Generic;
using System.Text;
using GoRogue.SenseMapping.Sources;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.SenseMapping
{
    [PublicAPI]
    public readonly struct CustomResultViewWithResize
    {
        public readonly ISettableGridView<double> ResultView;
        public readonly Func<int, int, ISettableGridView<double>, ISettableGridView<double>> Resizer;

        public CustomResultViewWithResize(ISettableGridView<double> resultView,
            Func<int, int, ISettableGridView<double>, ISettableGridView<double>> resizer)
        {
            ResultView = resultView;
            Resizer = resizer;
        }

        /// <summary>
        /// An array resize function appropriate for use as a <see cref="CustomResultViewWithResize.Resizer"/> when the grid view
        /// being used is an ArrayView.
        /// </summary>
        /// <param name="width"/>
        /// <param name="height"/>
        /// <param name="currentView"/>
        /// <returns>An array view re-allocated/cleared as appropriate.</returns>
        public static ISettableGridView<double> ArrayViewResizer(int width, int height, ISettableGridView<double> currentView)
        {
            var current = (ArrayView<double>)currentView;

            // No need to resize the entire thing; just steal the internal array and clear it to avoid re-allocation
            if (width * height == current.Count)
            {
                var newView = new ArrayView<double>(current, width);
                newView.Clear();
                return newView;
            }

            return new ArrayView<double>(width, height);
        }
    }

    [PublicAPI]
    public abstract class SenseMapBase : ISenseMap
    {
        /// <summary>
        /// The actual grid view which is used to record results.  Exposed publicly via
        /// <see cref="ResultView"/>.
        /// </summary>
        protected ISettableGridView<double> ResultViewBacking;

        /// <summary>
        /// The function to use to resize the ResultView if the resistance view changes sizes between calculate calls.
        /// The function should perform any necessary operations and return a grid view which is of the appropriate size.
        ///
        /// The function must return a view with all values set to 0.0, which has the width and height given.
        /// </summary>
        protected Func<int, int, ISettableGridView<double>, ISettableGridView<double>> ResultViewResizer;

        /// <inheritdoc/>
        public event EventHandler? Recalculated;

        /// <inheritdoc/>
        public event EventHandler? SenseMapReset;

        /// <inheritdoc/>
        public IGridView<double> ResistanceView { get; }

        /// <inheritdoc />
        public IGridView<double> ResultView => ResultViewBacking;

        private readonly List<ISenseSource> _senseSources;
        /// <inheritdoc />
        public IReadOnlyList<ISenseSource> SenseSources => _senseSources.AsReadOnly();

        /// <inheritdoc />
        public abstract IEnumerable<Point> CurrentSenseMap { get; }

        /// <inheritdoc />
        public abstract IEnumerable<Point> NewlyInSenseMap { get; }

        /// <inheritdoc />
        public abstract IEnumerable<Point> NewlyOutOfSenseMap { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resistanceView">The resistance map to use for calculations.</param>
        /// <param name="resultViewAndResizer">
        /// The view in which SenseMap calculation results are stored, along with a method to use to resize it as needed.
        ///
        /// If unspecified, an ArrayView will be used for the result view, and the resize function will allocate a new
        /// ArrayView of the appropriate size as needed.  This should be sufficient for most use cases.
        ///
        /// This function must return a view with all of its values set to 0.0, which has the given width and height.
        /// </param>
        protected SenseMapBase(IGridView<double> resistanceView, CustomResultViewWithResize? resultViewAndResizer)
        {
            resultViewAndResizer ??= new CustomResultViewWithResize(
                new ArrayView<double>(resistanceView.Width, resistanceView.Height),
                CustomResultViewWithResize.ArrayViewResizer);

            ResistanceView = resistanceView;
            _senseSources = new List<ISenseSource>();
            ResultViewBacking = resultViewAndResizer.Value.ResultView;
            ResultViewResizer = resultViewAndResizer.Value.Resizer;
        }

        /// <inheritdoc />
        public IReadOnlySenseMap AsReadOnly() => this;

        /// <inheritdoc/>
        public void AddSenseSource(ISenseSource senseSource)
        {
            _senseSources.Add(senseSource);
            senseSource.SetResistanceMap(ResistanceView);
        }

        /// <inheritdoc/>
        public void RemoveSenseSource(ISenseSource senseSource)
        {
            _senseSources.Remove(senseSource);
            senseSource.SetResistanceMap(null);
        }

        /// <inheritdoc/>
        public void Calculate()
        {
            Reset();

            OnCalculate();
            Recalculated?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void Reset()
        {
            if (ResistanceView.Width != ResultViewBacking.Width || ResistanceView.Height != ResultViewBacking.Height)
                ResultViewBacking = ResultViewResizer(ResistanceView.Width, ResistanceView.Height, ResultViewBacking);
            else
            {
                // TODO: Switch accounts for primitives library bug #76; remove when fixed
                switch (ResultViewBacking)
                {
                    case ArrayView<double> arrayView:
                        arrayView.Clear();
                        break;
                    case ArrayView2D<double> arrayView2d:
                        arrayView2d.Clear();
                        break;
                    default:
                        ResultViewBacking.Fill(0.0);
                        break;
                }
            }

            SenseMapReset?.Invoke(this, EventArgs.Empty);
        }

        protected abstract void OnCalculate();

        // ReSharper disable once MethodOverloadWithOptionalParameter
        /// <summary>
        /// ToString that customizes the characters used to represent the map.
        /// </summary>
        /// <param name="normal">The character used for any location not in the SenseMap.</param>
        /// <param name="center">
        /// The character used for any location that is the center-point of a source.
        /// </param>
        /// <param name="sourceValue">
        /// The character used for any location that is in range of a source, but not a center point.
        /// </param>
        /// <returns>The string representation of the SenseMap, using the specified characters.</returns>
        public string ToString(char normal = '-', char center = 'C', char sourceValue = 'S')
        {
            var result = new StringBuilder();

            for (var y = 0; y < ResistanceView.Height; y++)
            {
                for (var x = 0; x < ResistanceView.Width; x++)
                {
                    if (ResultView[x, y] > 0.0)
                        result.Append(IsACenter(x, y) ? center : sourceValue);
                    else
                        result.Append(normal);

                    result.Append(' ');
                }

                result.Append('\n');
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns a string representation of the map, where any location not in the SenseMap is
        /// represented by a '-' character, any position that is the center of some source is
        /// represented by a 'C' character, and any position that has a non-zero value but is not a
        /// center is represented by an 'S'.
        /// </summary>
        /// <returns>A (multi-line) string representation of the SenseMap.</returns>
        public override string ToString() => ToString();

        /// <summary>
        /// Returns a string representation of the map, with the actual values in the SenseMap,
        /// rounded to the given number of decimal places.
        /// </summary>
        /// <param name="decimalPlaces">The number of decimal places to round to.</param>
        /// <returns>
        /// A string representation of the map, rounded to the given number of decimal places.
        /// </returns>
        public string ToString(int decimalPlaces)
            => ResultView.ExtendToString(elementStringifier: obj
                => obj.ToString("0." + "0".Multiply(decimalPlaces)));

        private bool IsACenter(int x, int y)
        {
            foreach (var source in _senseSources)
                if (source.Position.X == x && source.Position.Y == y)
                    return true;

            return false;
        }

    }
}
