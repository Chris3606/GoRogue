using System;

namespace GoRogue
{
	/// <summary>
	/// Map view class capable of taking complex data and providing a simpler view of it.
	/// </summary>
	/// <remarks>
	/// Many GoRogue algorithms work on a IMapView of a simple data type, which is likely to be a poor match
	/// for your game's actual map data.  For example, map generation works with bools, and FOV calculation
	/// with doubles, while your map data is likely to model each map cell as a class or struct containing
	/// many different member values.
	/// 
	/// This class allows you to build descendant classes that override the TranslateGet and TranslateSet
	/// methods for simple mapping, or the this properties if you need full access to the underlying data
	/// for context, in order to present a simplified view of your data to an algorithm without having to
	/// create multiple arrays and keep them in sync.
	/// </remarks>
	/// <typeparam name="T1">The type of your underlying data.</typeparam>
	/// <typeparam name="T2">The type of the data being exposed to the algorithm.</typeparam>
	abstract class TranslationMap<T1, T2> : ISettableMapView<T2>
	{
		private ISettableMapView<T1> _baseMap;

		/// <summary>
		/// Constructor. Takes an existing map view to create a view from.
		/// </summary>
		/// <param name="baseMap">Your underlying map data.</param>
		public TranslationMap(ISettableMapView<T1> baseMap)
		{
			_baseMap = baseMap;
		}

		/// <summary>
		/// Constructor. Takes an existing map view to create a view from and applies view data to it.
		/// </summary>
		/// <param name="baseMap">Your underlying map data.</param>
		/// <param name="overlay">The view data to apply to the map.  Must have identical dimensions
		/// to baseMap.</param>
		public TranslationMap(ISettableMapView<T1> baseMap, ISettableMapView<T2> overlay) : this(baseMap)
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
			if (_baseMap.Height != overlay.Height || _baseMap.Width != overlay.Width)
				throw new ArgumentException("Overlay size must match base map size.");

			for (int y = 0; y < _baseMap.Height; ++y)
				for (int x = 0; x < _baseMap.Width; ++x)
					this[x, y] = overlay[x, y];
		}

		/// <summary>
		/// Translates your map data into the view type
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
		/// Given an X and Y value, translates and returns/sets the "value" associated with that location.
		/// </summary>
		/// <param name="x">X-value of location.</param>
		/// <param name="y">Y-value of location.</param>
		/// <returns>The translated "value" associated with that location.</returns>
		virtual public T2 this[int x, int y]
		{
			get => TranslateGet(_baseMap[x, y]);
			set => _baseMap[x, y] = TranslateSet(value);
		}

		/// <summary>
		/// Given a Coord, translates and returns/sets the "value" associated with that location.
		/// </summary>
		/// <param name="pos">Location to get/set the value for.</param>
		/// <returns>The translated "value" associated with the provided location.</returns>
		virtual public T2 this[Coord pos]
		{
			get => TranslateGet(_baseMap[pos]);
			set => _baseMap[pos] = TranslateSet(value);
		}

		/// <summary>
		/// The height of the underlying map.
		/// </summary>
		public int Height { get => _baseMap.Height; }

		/// <summary>
		/// The width of the underlying map.
		/// </summary>
		public int Width { get => _baseMap.Width; }

		/// <summary>
		/// The underlying map.
		/// </summary>
		public ISettableMapView<T1> BaseMap { get => _baseMap; }

		/// <summary>
		/// Returns a string representation of the underlying map.
		/// </summary>
		public override string ToString() => _baseMap.ToString();
	}
}
