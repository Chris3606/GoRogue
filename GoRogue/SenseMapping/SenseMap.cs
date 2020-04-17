using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoRogue.MapViews;
using SadRogue.Primitives;

namespace GoRogue.SenseMapping
{
    /// <summary>
    /// Class responsible for calculating a map for senses (sound, light, etc), or generally anything
    /// that can be modeled as sources propegating through a map that has degrees of resistance to spread.
    /// </summary>
    /// <remarks>
    /// Generally, this class can be used to model the result of applying ripple-like or shadowcasting-like
    /// "spreading" of values from one or more sources through a map.  This can include modeling the spreading
    /// of light, sound, heat for a heatmap, etc. through a map.  You create one or more <see cref="SenseSource"/>
    /// instances representing your various sources, add them to the SenseMap, and call <see cref="Calculate"/>
    /// when you wish to re-calculate the SenseMap.
    /// 
    /// Like most GoRogue algorithm implementations, SenseMap takes as a construction parameter an IMapView that represents
    /// the map.  Specifically, it takes an <see cref="IMapView{Double}"/>, where the double value at each location
    /// represents the "resistance" that location has to the passing of source values through it.  The values must be >= 0.0,
    /// where 0.0 means that a location has no resistance to spreading of source values, and greater values represent greater
    /// resistance.  The scale of this resistance is arbitrary, and is related to the <see cref="SenseSource.Intensity"/> of
    /// your sources.  As a source spreads through a given location, a value equal to the resistance value of that location
    /// is subtracted from the source's value (plus the normal fallof for distance).
    /// 
    /// The map can be calculated by calling the <see cref="Calculate"/> function.
    /// 
    /// This class exposes the resulting sensory values values to you via indexers -- SenseMap implements
    /// <see cref="IMapView{Double}"/>, where 0.0 indicates no sources were able to spread to the given location (eg, either it was
    /// stopped or fell off due to distance), and a value greater than 0.0 indicates the combined intensity of any sources
    /// that reached the given location.
    /// </remarks>
    public class SenseMap : IReadOnlySenseMap, IEnumerable<double>, IMapView<double>
    {
        private readonly List<SenseSource> _senseSources;

        private HashSet<Point> _currentSenseMap;

        private int _lastHeight;

        private int _lastWidth;

        private HashSet<Point> _previousSenseMap;

        private readonly IMapView<double> _resMap;

        // Making these 1D didn't really affect performance that much, though may be worth it on
        // large maps
        private double[,] _senseMap;

        /// <summary>
        /// Constructor. Takes the resistance map to use for calculations.
        /// </summary>
        /// <param name="resMap">The resistance map to use for calculations.</param>
        public SenseMap(IMapView<double> resMap)
        {
            _resMap = resMap;
            _senseMap = new double[resMap.Width, resMap.Height];
            _lastWidth = resMap.Width;
            _lastHeight = resMap.Height;

            _senseSources = new List<SenseSource>();

            _previousSenseMap = new HashSet<Point>();
            _currentSenseMap = new HashSet<Point>();
        }

        /// <summary>
        /// IEnumerable of only positions currently "in" the SenseMap, eg. all positions that have a
        /// value other than 0.0.
        /// </summary>
        public IEnumerable<Point> CurrentSenseMap => _currentSenseMap;

        /// <summary>
        /// Height of sense map.
        /// </summary>
        public int Height => _resMap.Height;

        /// <summary>
        /// IEnumerable of positions that DO have a non-zero value in the sense map as of the most
        /// current <see cref="Calculate"/> call, but DID NOT have a non-zero value after the previous time
        /// <see cref="Calculate"/> was called.
        /// </summary>
        public IEnumerable<Point> NewlyInSenseMap => _currentSenseMap.Where(pos => !_previousSenseMap.Contains(pos));

        /// <summary>
        /// IEnumerable of positions that DO NOT have a non-zero value in the sense map as of the
        /// most current <see cref="Calculate"/> call, but DID have a non-zero value after the previous time
        /// <see cref="Calculate"/> was called.
        /// </summary>
        public IEnumerable<Point> NewlyOutOfSenseMap => _previousSenseMap.Where(pos => !_currentSenseMap.Contains(pos));

        /// <summary>
        /// Read-only list of all sources currently considered part of the SenseMap. Some may have their
        /// <see cref="SenseSource.Enabled"/> flag set to false, so all of these may or may not be counted
        /// when <see cref="Calculate"/> is called.
        /// </summary>
        public IReadOnlyList<SenseSource> SenseSources => _senseSources.AsReadOnly();

        /// <summary>
        /// Width of the sense map.
        /// </summary>
        public int Width => _resMap.Width;

        /// <summary>
        /// Returns the "sensory value" for the given position.
        /// </summary>
        /// <param name="index1D">Position to return the sensory value for, as a 1d-index-style value.</param>
        /// <returns>The sense-map value for the given position.</returns>
        public double this[int index1D] => _senseMap[Point.ToXValue(index1D, Width), Point.ToYValue(index1D, Width)];

        /// <summary>
        /// Returns the "sensory value" for the given position.
        /// </summary>
        /// <param name="pos">The position to return the sensory value for.</param>
        /// <returns>The sensory value for the given position.</returns>
        public double this[Point pos] => _senseMap[pos.X, pos.Y];

        /// <summary>
        /// Returns the "sensory value" for the given position.
        /// </summary>
        /// <param name="x">X-Pointinate of the position to return the sensory value for.</param>
        /// <param name="y">Y-Pointinate of the position to return the sensory value for.</param>
        /// <returns>The sensory value for the given position.</returns>
        public double this[int x, int y] => _senseMap[x, y];

        /// <summary>
        /// Adds the given source to the list of sources. If the source has its
        /// <see cref="SenseSource.Enabled"/> flag set when <see cref="Calculate"/> is next called, then
        /// it will be counted as a source.
        /// </summary>
        /// <param name="senseSource">The source to add.</param>
        public void AddSenseSource(SenseSource senseSource)
        {
            _senseSources.Add(senseSource);
            senseSource._resMap = _resMap;
        }

        /// <summary>
        /// Returns a read-only representation of the SenseMap.
        /// </summary>
        /// <returns>This SenseMap object as <see cref="IReadOnlySenseMap"/>.</returns>
        public IReadOnlySenseMap AsReadOnly() => this;

        /// <summary>
        /// Calcuates the map.  For each enabled source in the source list, it calculates
        /// the source's spreading, and puts them all together in the sense map's output.
        /// </summary>
        public void Calculate()
        {
            if (_lastWidth != _resMap.Width || _lastHeight != _resMap.Height)
            {
                _senseMap = new double[_resMap.Width, _resMap.Height];
                _lastWidth = _resMap.Width;
                _lastHeight = _resMap.Height;
            }
            else
                Array.Clear(_senseMap, 0, _senseMap.Length);

            _previousSenseMap = _currentSenseMap;
            _currentSenseMap = new HashSet<Point>();

            if (_senseSources.Count > 1) // Probably not the proper condition, but useful for now.
            {
                Parallel.ForEach(_senseSources, senseSource =>
                {
                    senseSource.calculateLight();
                });
            }
            else
                foreach (var senseSource in _senseSources)
                    senseSource.calculateLight();

            // Flush sources to actual senseMap
            foreach (var senseSource in _senseSources)
                blitSenseSource(senseSource, _senseMap, _currentSenseMap, _resMap);
        }

        /// <summary>
        /// Enumerator, in case you want to use this as a list of doubles.
        /// </summary>
        /// <returns>Enumerable of doubles (the sensory values).</returns>
        public IEnumerator<double> GetEnumerator()
        {
            for (int y = 0; y < _resMap.Height; y++)
                for (int x = 0; x < _resMap.Width; x++)
                    yield return _senseMap[x, y];
        }

        // Warning about hidden overload intentionally disabled -- the two methods are equivalent but
        // the ToString method that takes 0, as opposed to all optional, parameters is necessary to
        // override the one from base class object. That one calls this one so the "hidden" overload
        // is of no harm.
#pragma warning disable RECS0137

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
#pragma warning restore RECS0137

        {
            string result = "";

            for (int y = 0; y < _resMap.Height; y++)
            {
                for (int x = 0; x < _resMap.Width; x++)
                {
                    if (_senseMap[x, y] > 0.0)
                        result += (isACenter(x, y)) ? center : sourceValue;
                    else
                        result += normal;

                    result += " ";
                }

                result += '\n';
            }

            return result;
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
        => _senseMap.ExtendToStringGrid(elementStringifier: (double obj) => obj.ToString("0." + "0".Multiply(decimalPlaces)));

        /// <summary>
        /// Generic enumerator.
        /// </summary>
        /// <returns>Enumerator for looping.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Removes the given source from the list of sources. Genearlly, use this if a source is permanently removed
        /// from a map. For temporary disabling, you should generally use the <see cref="SenseSource.Enabled"/> flag.
        /// </summary>
        /// <remarks>
        /// The source values that this sense source was responsible for are NOT removed from the sensory output values
        /// until <see cref="Calculate"/> is next called.
        /// </remarks>
        /// <param name="senseSource">The source to remove.</param>
        public void RemoveSenseSource(SenseSource senseSource)
        {
            _senseSources.Remove(senseSource);
            senseSource._resMap = null;
        }

        private bool isACenter(int x, int y)
        {
            foreach (var source in _senseSources)
                if (source.Position.X == x && source.Position.Y == y)
                    return true;

            return false;
        }

        // Blits given source's lightMap onto the global lightmap given
        private static void blitSenseSource(SenseSource source, double[,] destination, HashSet<Point> sourceMap, IMapView<double> resMap)
        {
            // Calculate actual radius bounds, given constraint based on location
            int minX = Math.Min((int)source.Radius, source.Position.X);
            int minY = Math.Min((int)source.Radius, source.Position.Y);
            int maxX = Math.Min((int)source.Radius, resMap.Width - 1 - source.Position.X);
            int maxY = Math.Min((int)source.Radius, resMap.Height - 1 - source.Position.Y);

            // Use radius bounds to extrapalate global Pointinate scheme mins and maxes
            Point gMin = source.Position - new Point(minX, minY);
            //Point gMax = source.Position + Point.Get(maxX, maxY);

            // Use radius bound to extrapalate light-local Pointinate scheme min and max bounds that
            // are actually blitted
            Point lMin = new Point((int)source.Radius - minX, (int)source.Radius - minY);
            Point lMax = new Point((int)source.Radius + maxX, (int)source.Radius + maxY);

            for (int xOffset = 0; xOffset <= lMax.X - lMin.X; xOffset++)
            //Parallel.For(0, lMax.X - lMin.X + 1, xOffset => // By light radius 30 or so, there is enough work to get benefit here.  Manual thread splitting may also be an option.
            {
                for (int yOffset = 0; yOffset <= lMax.Y - lMin.Y; yOffset++)
                {
                    // Offset local/current by proper amount, and update lightmap
                    Point c = new Point(xOffset, yOffset);
                    Point gCur = gMin + c;
                    Point lCur = lMin + c;

                    destination[gCur.X, gCur.Y] = destination[gCur.X, gCur.Y] + source._light[lCur.X, lCur.Y]; // Add source values,
                    if (destination[gCur.X, gCur.Y] > 0.0)
                        sourceMap.Add(gCur);
                }
            } //);
        }
    }
}
