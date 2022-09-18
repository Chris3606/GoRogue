using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoRogue.SenseMapping.Sources;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.SenseMapping
{
    /// <summary>
    /// Class responsible for calculating a map for senses (sound, light, etc), or generally anything
    /// that can be modeled as sources propagating through a map that has degrees of resistance to spread.
    /// </summary>
    /// <remarks>
    /// Generally, this class can be used to model the result of applying ripple-like or shadow-casting like
    /// "spreading" of values from one or more sources through a map.  This can include modeling the spreading
    /// of light, sound, heat for a heat-map, etc. through a map.  You create one or more <see cref="ISenseSource" />
    /// instances representing your various sources, add them to the SenseMap, and call <see cref="Calculate" />
    /// when you wish to re-calculate the SenseMap.
    /// Like most GoRogue algorithm implementations, SenseMap takes as a construction parameter an IGridView that represents
    /// the map.  Specifically, it takes an <see cref="SadRogue.Primitives.GridViews.IGridView{T}" />, where the double value at each location
    /// represents the "resistance" that location has to the passing of source values through it.  The values must be >= 0.0,
    /// where 0.0 means that a location has no resistance to spreading of source values, and greater values represent greater
    /// resistance.  The scale of this resistance is arbitrary, and is related to the <see cref="ISenseSource.Intensity" /> of
    /// your sources.  As a source spreads through a given location, a value equal to the resistance value of that location
    /// is subtracted from the source's value (plus the normal fall-of for distance).
    /// The map can be calculated by calling the <see cref="Calculate" /> function.
    /// This class exposes the resulting sensory values values to you via indexers -- SenseMap implements
    /// <see cref="SadRogue.Primitives.GridViews.IGridView{T}" />, where 0.0 indicates no sources were able to spread to the given location (eg, either
    /// it was
    /// stopped or fell off due to distance), and a value greater than 0.0 indicates the combined intensity of any sources
    /// that reached the given location.
    /// </remarks>
    [PublicAPI]
    public class SenseMap : IReadOnlySenseMap
    {
        /// <inheritdoc/>
        public IGridView<double> ResistanceView { get; }

        private readonly List<ISenseSource> _senseSources;
        /// <inheritdoc />
        public IReadOnlyList<ISenseSource> SenseSources => _senseSources.AsReadOnly();

        private HashSet<Point> _currentSenseMap;
        /// <inheritdoc />
        public IEnumerable<Point> CurrentSenseMap => _currentSenseMap;

        private int _lastHeight;

        private int _lastWidth;

        private HashSet<Point> _previousSenseMap;

        private ArrayView<double> _resultView;
        /// <inheritdoc />
        public IGridView<double> ResultView => _resultView;

        /// <summary>
        /// Whether or not to calculate each sense source's spread algorithm in parallel.  Has no effect if there is only one source added.
        /// </summary>
        /// <remarks>
        /// When this is set to true, calling of <see cref="ISenseSource.CalculateLight"/> will happen in parallel via multiple threads.  A
        /// Parallel.ForEach will be used, which will enable the use of a thread pool.
        ///
        /// In either case, sense sources always have their own result views on which they perform their calculations, so there is no concern with
        /// overlapping sources.  This also does not affect the copying of sense source's values from its local result view to the sense map's one.
        /// </remarks>
        public bool ParallelCalculate { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resistanceMap">The resistance map to use for calculations.</param>
        /// <param name="parallelCalculate">Whether or not to calculate the sense sources in parallel using Parallel.ForEach.  Has no effect if there is only one source added.</param>
        /// <param name="hasher">The hashing algorithm to use for points in hash sets.  Defaults to the default hash algorithm for Points.</param>
        public SenseMap(IGridView<double> resistanceMap, bool parallelCalculate = true, IEqualityComparer<Point>? hasher = null)
        {
            ParallelCalculate = parallelCalculate;
            hasher ??= EqualityComparer<Point>.Default;

            ResistanceView = resistanceMap;
            _resultView = new ArrayView<double>(resistanceMap.Width, resistanceMap.Height);
            _lastWidth = resistanceMap.Width;
            _lastHeight = resistanceMap.Height;

            _senseSources = new List<ISenseSource>();

            _previousSenseMap = new HashSet<Point>(hasher);
            _currentSenseMap = new HashSet<Point>(hasher);
        }

        /// <inheritdoc />
        public IEnumerable<Point> NewlyInSenseMap => _currentSenseMap.Where(pos => !_previousSenseMap.Contains(pos));

        /// <inheritdoc />
        public IEnumerable<Point> NewlyOutOfSenseMap => _previousSenseMap.Where(pos => !_currentSenseMap.Contains(pos));

        /// <inheritdoc />
        public IReadOnlySenseMap AsReadOnly() => this;

        /// <summary>
        /// Enumerator, in case you want to use this as a list of doubles.
        /// </summary>
        /// <returns>Enumerable of doubles (the sensory values).</returns>
        public IEnumerator<double> GetEnumerator()
        {
            for (var y = 0; y < ResistanceView.Height; y++)
                for (var x = 0; x < ResistanceView.Width; x++)
                    yield return _resultView[x, y];
        }

        /// <summary>
        /// Generic enumerator.
        /// </summary>
        /// <returns>Enumerator for looping.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Adds the given source to the list of sources. If the source has its
        /// <see cref="ISenseSource.Enabled" /> flag set when <see cref="Calculate" /> is next called, then
        /// it will be counted as a source.
        /// </summary>
        /// <param name="senseSource">The source to add.</param>
        public void AddSenseSource(ISenseSource senseSource)
        {
            _senseSources.Add(senseSource);
            senseSource.SetResistanceMap(ResistanceView);
        }

        /// <summary>
        /// Calculates the map.  For each enabled source in the source list, it calculates
        /// the source's spreading, and puts them all together in the sense map's output.
        /// </summary>
        public void Calculate()
        {
            if (_lastWidth != ResistanceView.Width || _lastHeight != ResistanceView.Height)
            {
                _resultView = new ArrayView<double>(ResistanceView.Width, ResistanceView.Height);
                _lastWidth = ResistanceView.Width;
                _lastHeight = ResistanceView.Height;
            }
            else
                _resultView.Clear();

            // Cycle current and previous hash sets to avoid re-allocation of internal buffers
            (_previousSenseMap, _currentSenseMap) = (_currentSenseMap, _previousSenseMap);
            _currentSenseMap.Clear();

            // Anything past 1 sense source seems to benefit notably from parallel execution
            if (_senseSources.Count > 1 && ParallelCalculate)
                Parallel.ForEach(_senseSources, senseSource => { senseSource.CalculateLight(); });
            else
                foreach (var senseSource in _senseSources)
                    senseSource.CalculateLight();

            // Flush sources to actual senseMap
            foreach (var senseSource in _senseSources)
                BlitSenseSource(senseSource, _resultView, _currentSenseMap, ResistanceView);
        }

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
                    if (_resultView[x, y] > 0.0)
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
            => _resultView.ExtendToString(elementStringifier: obj
                => obj.ToString("0." + "0".Multiply(decimalPlaces)));

        /// <summary>
        /// Removes the given source from the list of sources. Generally, use this if a source is permanently removed
        /// from a map. For temporary disabling, you should generally use the <see cref="ISenseSource.Enabled" /> flag.
        /// </summary>
        /// <remarks>
        /// The source values that this sense source was responsible for are NOT removed from the sensory output values
        /// until <see cref="Calculate" /> is next called.
        /// </remarks>
        /// <param name="senseSource">The source to remove.</param>
        public void RemoveSenseSource(ISenseSource senseSource)
        {
            _senseSources.Remove(senseSource);
            senseSource.SetResistanceMap(null);
        }

        private bool IsACenter(int x, int y)
        {
            foreach (var source in _senseSources)
                if (source.Position.X == x && source.Position.Y == y)
                    return true;

            return false;
        }

        // Blits given source's lightMap onto the global light-map given
        private static void BlitSenseSource(ISenseSource source, ISettableGridView<double> destination, HashSet<Point> sourceMap,
                                            IGridView<double> resMap)
        {
            // Calculate actual radius bounds, given constraint based on location
            var minX = Math.Min((int)source.Radius, source.Position.X);
            var minY = Math.Min((int)source.Radius, source.Position.Y);
            var maxX = Math.Min((int)source.Radius, resMap.Width - 1 - source.Position.X);
            var maxY = Math.Min((int)source.Radius, resMap.Height - 1 - source.Position.Y);

            // Use radius bounds to extrapolate global coordinate scheme mins and maxes
            var gMin = source.Position - new Point(minX, minY);
            //Point gMax = source.Position + Point.Get(maxX, maxY);

            // Use radius bound to extrapolate light-local coordinate scheme min and max bounds that
            // are actually blitted
            var lMin = new Point((int)source.Radius - minX, (int)source.Radius - minY);
            var lMax = new Point((int)source.Radius + maxX, (int)source.Radius + maxY);

            for (var xOffset = 0; xOffset <= lMax.X - lMin.X; xOffset++)
                for (var yOffset = 0; yOffset <= lMax.Y - lMin.Y; yOffset++)
                {
                    // Offset local/current by proper amount, and update light-map
                    var c = new Point(xOffset, yOffset);
                    var gCur = gMin + c;
                    var lCur = lMin + c;

                    // Null-forgiving because ResistanceView is set when sources are added, so this can't occur unless somebody has been
                    // messing with values they're not supposed to, and adding a check would cost performance.
                    destination[gCur.X, gCur.Y] += source.ResultView[lCur.X, lCur.Y]; // Add source values
                    if (destination[gCur.X, gCur.Y] > 0.0)
                        sourceMap.Add(gCur);
                }
        }
    }
}
