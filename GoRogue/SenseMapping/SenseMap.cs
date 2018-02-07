using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoRogue.SenseMapping
{
    /// <summary>
    /// Class responsible for calculating a map for senses (sound, light, etc). Anything that has a
    /// resistance map, where 1.0 is completely impenetrable, and 0.0 is no resistance at all, can
    /// use this system. Typically used for FOV, however can also be used for sound maps, etc.
    /// Supports a few different types of spreading mechanics, including every one in the SourceType
    /// enum. Included in these is FOV/FOV-style shadowcasting. This will be much faster than ripple
    /// variations. Once one calls Calculate, one can use Coords, or x and y values, to access this
    /// class like an array. The double one gets back will be the "sensory value". This is a number
    /// between 1.0 and 0.0, where 1.0 is maximum intensity (max brightness in the case of the
    /// sources being light, for example), and 0.0 is no intensity at all.
    /// </summary>
    public class SenseMap : IEnumerable<double>, IMapView<double>
    {
        private List<SenseSource> _senseSources;

        private HashSet<Coord> currentSenseMap;

        private int lastHeight;

        private int lastWidth;

        private HashSet<Coord> previousSenseMap;

        private IMapView<double> resMap;

        // Making these 1D didn't really affect performance that much, though may be worth it on
        // large maps
        private double[,] senseMap;

        /// <summary>
        /// Constructor. Takes the resistance map to use for calculations.
        /// </summary>
        /// <param name="resMap">
        /// The resistance map to use for calculations.
        /// </param>
        public SenseMap(IMapView<double> resMap)
        {
            this.resMap = resMap;
            senseMap = new double[resMap.Width, resMap.Height];
            lastWidth = resMap.Width;
            lastHeight = resMap.Height;

            _senseSources = new List<SenseSource>();

            previousSenseMap = new HashSet<Coord>();
            currentSenseMap = new HashSet<Coord>();
        }

        /// <summary>
        /// IEnumerable of only positions currently "in" the SenseMap, eg. all positions that have a
        /// value other than 0.0.
        /// </summary>
        public IEnumerable<Coord> CurrentSenseMap { get => currentSenseMap; }

        /// <summary>
        /// Height of sense map.
        /// </summary>
        public int Height { get => resMap.Height; }

        /// <summary>
        /// IEnumerable of positions that DO have a non-zero value in the sense map as of the most
        /// current Calculate call, but DID NOT have a non-zero value after the previous time
        /// Calculate was called.
        /// </summary>
        public IEnumerable<Coord> NewlyInSenseMap { get => currentSenseMap.Where(pos => !previousSenseMap.Contains(pos)); }

        /// <summary>
        /// IEnumerable of positions that DO NOT have a non-zero value in the sense map as of the
        /// most current Calculate call, but DID have a non-zero value after the previous time
        /// Calculate was called.
        /// </summary>
        public IEnumerable<Coord> NewlyOutOfSenseMap { get => previousSenseMap.Where(pos => !currentSenseMap.Contains(pos)); }

        /// <summary>
        /// Read-only list of all sources currently taken into account. Some may have their enabled
        /// flag set to false, so all of these may or may not be counted when Calculate is called.
        /// </summary>
        public IList<SenseSource> SenseSources { get => _senseSources.AsReadOnly(); }

        /// <summary>
        /// Width of sense map.
        /// </summary>
        public int Width { get => resMap.Width; }

        /// <summary>
        /// Array-style indexer that takes a Coord as the index. Because of this function, given a
        /// SenseMap called mySenseMap, you may call mySenseMap[Coord.Get(1, 3)] to access the
        /// "sensory value" at (1, 3).
        /// </summary>
        /// <param name="pos">
        /// Position to get the sensory value for.
        /// </param>
        /// <returns>
        /// The "sensory value" at the given location.
        /// </returns>
        public double this[Coord pos]
        {
            get { return senseMap[pos.X, pos.Y]; }
        }

        /// <summary>
        /// Array-style indexer that takes an (x, y) value as the index. Similarly to the Coord
        /// indexer, you may call mySenseMap[1, 3] to get the "sensory value" at 1, 3.
        /// </summary>
        /// <param name="x">
        /// X-coordinate to retrieve the sensory value for.
        /// </param>
        /// <param name="y">
        /// Y-Coordinate to retrieve the sensory value for.
        /// </param>
        /// <returns>
        /// The sensory value at (x, y)
        /// </returns>
        public double this[int x, int y]
        {
            get { return senseMap[x, y]; }
        }

        /// <summary>
        /// Adds the given SenseSource to the list of sources. If the source has its
        /// SenseSource.Enabled flag set when Calculate is next called, then this will be used as a source.
        /// </summary>
        /// <param name="senseSource">
        /// The "source" to add.
        /// </param>
        public void AddSenseSource(SenseSource senseSource)
        {
            _senseSources.Add(senseSource);
            senseSource.resMap = resMap;
        }

        /// <summary>
        /// Function to make it do things. For each enabled source in the source list, it calculates
        /// the sense map, and puts them all together when this function is called. Sensory values
        /// are capped at 1.0 automatically.
        /// </summary>
        public void Calculate()
        {
            if (lastWidth != resMap.Width || lastHeight != resMap.Height)
            {
                senseMap = new double[resMap.Width, resMap.Height];
                lastWidth = resMap.Width;
                lastHeight = resMap.Height;
            }
            else
                Array.Clear(senseMap, 0, senseMap.Length);

            previousSenseMap = currentSenseMap;
            currentSenseMap = new HashSet<Coord>();

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
                blitSenseSource(senseSource, senseMap, currentSenseMap, resMap);
        }

        /// <summary>
        /// Enumerator, in case you want to use this as a list of doubles.
        /// </summary>
        /// <returns>
        /// Enumerable of doubles (the sensory values).
        /// </returns>
        public IEnumerator<double> GetEnumerator()
        {
            for (int y = 0; y < resMap.Height; y++)
                for (int x = 0; x < resMap.Width; x++)
                    yield return senseMap[x, y];
        }

        /// <summary>
        /// Generic enumerator.
        /// </summary>
        /// <returns>
        /// Enumerator for looping.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Removes the given source from the list of sources. This would typically be used when the
        /// source is destroyed, expired, or otherwise removed from the map. For temporary disabling,
        /// it is better to use the SenseSource.Enabled flag.
        /// </summary>
        /// <param name="senseSource">
        /// The SenseSource to remove.
        /// </param>
        public void RemoveSenseSource(SenseSource senseSource)
        {
            _senseSources.Remove(senseSource);
            senseSource.resMap = null;
        }

        // Blits given source's lightMap onto the global lightmap given
        private static void blitSenseSource(SenseSource source, double[,] destination, HashSet<Coord> sourceMap, IMapView<double> resMap)
        {
            // Calculate actual radius bounds, given constraint based on location
            int minX = Math.Min(source.Radius, source.Position.X);
            int minY = Math.Min(source.Radius, source.Position.Y);
            int maxX = Math.Min(source.Radius, resMap.Width - 1 - source.Position.X);
            int maxY = Math.Min(source.Radius, resMap.Height - 1 - source.Position.Y);

            // Use radius bounds to extrapalate global coordinate scheme mins and maxes
            Coord gMin = source.Position - Coord.Get(minX, minY);
            //Coord gMax = source.Position + Coord.Get(maxX, maxY);

            // Use radius bound to extrapalate light-local coordinate scheme min and max bounds that
            // are actually blitted
            Coord lMin = Coord.Get(source.Radius - minX, source.Radius - minY);
            Coord lMax = Coord.Get(source.Radius + maxX, source.Radius + maxY);

            for (int xOffset = 0; xOffset <= lMax.X - lMin.X; xOffset++)
            //Parallel.For(0, lMax.X - lMin.X + 1, xOffset => // By light radius 30 or so, there is enough work to get benefit here.  Manual thread splitting may also be an option.
            {
                for (int yOffset = 0; yOffset <= lMax.Y - lMin.Y; yOffset++)
                {
                    // Offset local/current by proper amount, and update lightmap
                    Coord c = Coord.Get(xOffset, yOffset);
                    Coord gCur = gMin + c;
                    Coord lCur = lMin + c;

                    destination[gCur.X, gCur.Y] = Math.Min(destination[gCur.X, gCur.Y] + source.light[lCur.X, lCur.Y], 1); // Add light, cap at 1 for now.  may just uncap this later.
                    if (destination[gCur.X, gCur.Y] > 0.0)
                        sourceMap.Add(gCur);
                }
            } //);
        }
    }
}