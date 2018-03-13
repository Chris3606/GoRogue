using System;

namespace GoRogue
{
    /// <summary>
    /// Settable map view class capable of taking complex data and providing a simpler view of it.  For a version that provides
    /// only "get" functionality, see TranslationMap.
    /// </summary>
    /// <remarks>
    /// Many GoRogue algorithms work on a IMapView of a simple data type, which is likely to be a poor match
    /// for your game's actual map data.  For example, map generation works with bools, and FOV calculation
    /// with doubles, while your map data may model each map cell as a class or struct containing
    /// many different member values.
    /// 
    /// This class allows you to build descendant classes that override the TranslateGet and TranslateSet
    /// methods for simple mapping, or the this[Coord] properties if you need full access to the underlying data
    /// for context, in order to present a simplified view of your data to an algorithm without having to
    /// create the large amount of duplicate code associated with multiple ISettableMapView instances that all
    /// extract data from a Cell or Tile class.
    /// </remarks>
    /// <typeparam name="T1">The type of your underlying data.</typeparam>
    /// <typeparam name="T2">The type of the data being exposed to the algorithm.</typeparam>
    abstract public class SettableTranslationMap<T1, T2> : ISettableMapView<T2>
    {
        /// <summary>
        /// The underlying map.
        /// </summary>
        public ISettableMapView<T1> BaseMap { get; private set; }

        /// <summary>
        /// The width of the underlying map.
        /// </summary>
        public int Width { get => BaseMap.Width; }

        /// <summary>
        /// The height of the underlying map.
        /// </summary>
        public int Height { get => BaseMap.Height; }

        /// <summary>
        /// Given a Coord, translates and returns/sets the "value" associated with that location.  this[int x, int y] calls
        /// this indexer for its functionality, so overriding this functionality also changes that overload.
        /// </summary>
        /// <param name="pos">Location to get/set the value for.</param>
        /// <returns>The translated "value" associated with the provided location.</returns>
        public virtual T2 this[Coord pos]
        {
            get => TranslateGet(BaseMap[pos]);
            set => BaseMap[pos] = TranslateSet(value);
        }

        /// <summary>
        /// Given an X and Y value, translates and returns/sets the "value" associated with that location.  This function
        /// calls this[Coord pos], so override that indexer to change functionality.
        /// </summary>
        /// <param name="x">X-value of location.</param>
        /// <param name="y">Y-value of location.</param>
        /// <returns>The translated "value" associated with that location.</returns>
        public T2 this[int x, int y]
        {
            get => this[Coord.Get(x, y)];
            set => this[Coord.Get(x, y)] = value;
        }

        /// <summary>
        /// Constructor. Takes an existing map view to create a view from.
        /// </summary>
        /// <param name="baseMap">Your underlying map data.</param>
        protected SettableTranslationMap(ISettableMapView<T1> baseMap)
        {
            BaseMap = baseMap;
        }

        /// <summary>
        /// Constructor. Takes an existing map view to create a view from and applies view data to it.  
        /// </summary>
        /// <remarks>
        /// Since this constructor must call TranslateSet to do so, do NOT call this constructor if the TranslateSet implementation
        /// depends on the derived class's constructor being completed to function properly.
        /// </remarks>
        /// <param name="baseMap">Your underlying map data.</param>
        /// <param name="overlay">The view data to apply to the map.  Must have identical dimensions.
        /// to baseMap.</param>
        protected SettableTranslationMap(ISettableMapView<T1> baseMap, ISettableMapView<T2> overlay) : this(baseMap)
        {
            ApplyOverlay(overlay);
        }

        /// <summary>
        /// Applies view data to your map.
        /// </summary>
        /// <param name="overlay">The view data to apply to the map.  Must have identical dimensions
        /// to BaseMap.</param>
        public void ApplyOverlay(ISettableMapView<T2> overlay)
        {
            if (BaseMap.Height != overlay.Height || BaseMap.Width != overlay.Width)
                throw new ArgumentException("Overlay size must match base map size.");

            for (int y = 0; y < BaseMap.Height; ++y)
                for (int x = 0; x < BaseMap.Width; ++x)
                    this[x, y] = overlay[x, y];
        }



        /// <summary>
        /// Translates your map data into the view type.
        /// </summary>
        /// <param name="value">The data value from your map.</param>
        /// <returns>A value of the mapped data type</returns>
        protected abstract T2 TranslateGet(T1 value);

        /// <summary>
        /// Translates the view type into the appropriate form for your map data.
        /// </summary>
        /// <param name="value">A value of the mapped data type</param>
        /// <returns>The data value for your map.</returns>
        protected abstract T1 TranslateSet(T2 value);

        /// <summary>
        /// Returns a string representation of the underlying map.
        /// </summary>
        public override string ToString() => BaseMap.ToString();
    }
}
