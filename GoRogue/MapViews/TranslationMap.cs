namespace GoRogue.MapViews
{
	/// <summary>
	/// Map view class capable of taking complex data and providing a simpler view of it. For a
	/// version that provides "set" functionality, see SettableTranslationMap.
	/// </summary>
	/// <remarks>
	/// Many GoRogue algorithms work on a IMapView of a simple data type, which is likely to be a
	/// poor match for your game's actual map data. For example, map generation works with bools, and
	/// FOV calculation with doubles, while your map data may model each map cell as a class or
	/// struct containing many different member values. /// This class allows you to build descendant
	/// classes that override the TranslateGet method for simple mapping, or the this properties if
	/// you need full access to the underlying data for context, in order to present a simplified
	/// view of your data to an algorithm without having to create the large amount of duplicate code
	/// associated with multiple ISettableMapView instances that all extract data from a Cell or Tile class.
	/// </remarks>
	/// <typeparam name="T1">The type of your underlying data.</typeparam>
	/// <typeparam name="T2">The type of the data being exposed to the algorithm.</typeparam>
	public abstract class TranslationMap<T1, T2> : IMapView<T2>
	{
		/// <summary>
		/// Constructor. Takes an existing map view to create a view from.
		/// </summary>
		/// <param name="baseMap">Your underlying map data.</param>
		protected TranslationMap(IMapView<T1> baseMap)
		{
			BaseMap = baseMap;
		}

		/// <summary>
		/// The underlying map.
		/// </summary>
		public IMapView<T1> BaseMap { get; private set; }

		/// <summary>
		/// The height of the underlying map.
		/// </summary>
		public int Height { get => BaseMap.Height; }

		/// <summary>
		/// The width of the underlying map.
		/// </summary>
		public int Width { get => BaseMap.Width; }

		/// <summary>
		/// Given an X and Y value, translates and returns the "value" associated with that location.
		/// This function calls this[Coord pos], so override that indexer to change functionality.
		/// </summary>
		/// <param name="x">X-value of location.</param>
		/// <param name="y">Y-value of location.</param>
		/// <returns>The translated "value" associated with that location.</returns>
		public T2 this[int x, int y]
		{
			get => this[Coord.Get(x, y)];
		}

		/// <summary>
		/// Given a Coord, translates and returns the "value" associated with that location. this[int
		/// x, int y] calls this indexer for its functionality, so overriding this functionality also
		/// changes that overload.
		/// </summary>
		/// <param name="pos">Location to get the value for.</param>
		/// <returns>The translated "value" associated with the provided location.</returns>
		virtual public T2 this[Coord pos]
		{
			get => TranslateGet(BaseMap[pos]);
		}

		/// <summary>
		/// Returns a string representation of the underlying map.
		/// </summary>
		public override string ToString() => BaseMap.ToString();

		/// <summary>
		/// Translates your map data into the view type.
		/// </summary>
		/// <param name="value">The data value from your map.</param>
		/// <returns>A value of the mapped data type</returns>
		protected abstract T2 TranslateGet(T1 value);
	}
}