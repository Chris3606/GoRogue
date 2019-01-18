using System;

namespace GoRogue.MapViews
{
	/// <summary>
	/// Settable map view class capable of taking complex data and providing a simpler view of it.
	/// For a version that provides only "get" functionality, see TranslationMap.
	/// </summary>
	/// <remarks>
	/// Many GoRogue algorithms work on a IMapView of a simple data type, which is likely to be a
	/// poor match for your game's actual map data. For example, map generation works with bools, and
	/// FOV calculation with doubles, while your map data may model each map cell as a class or
	/// struct containing many different member values. This class allows you to build descendant
	/// classes that override the TranslateGet and TranslateSet method(s) for simple mapping, or the
	/// this[Coord] properties if you need full access to the underlying data for context, in order
	/// to present a simplified view of your data to an algorithm without having to create the large
	/// amount of duplicate code associated with multiple ISettableMapView instances that all extract
	/// data from a Cell or Tile class.
	/// </remarks>
	/// <typeparam name="T1">The type of your underlying data.</typeparam>
	/// <typeparam name="T2">The type of the data being exposed to the algorithm.</typeparam>
	abstract public class SettableTranslationMap<T1, T2> : ISettableMapView<T2>
	{
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
		/// Since this constructor must call TranslateSet to do so, do NOT call this constructor if
		/// the TranslateSet implementation depends on the derived class's constructor being
		/// completed to function properly.
		/// </remarks>
		/// <param name="baseMap">Your underlying map data.</param>
		/// <param name="overlay">
		/// The view data to apply to the map. Must have identical dimensions. to baseMap.
		/// </param>
		protected SettableTranslationMap(ISettableMapView<T1> baseMap, ISettableMapView<T2> overlay) : this(baseMap)
		{
			this.ApplyOverlay(overlay);
		}

		/// <summary>
		/// The underlying map.
		/// </summary>
		public ISettableMapView<T1> BaseMap { get; private set; }

		/// <summary>
		/// The height of the underlying map.
		/// </summary>
		public int Height { get => BaseMap.Height; }

		/// <summary>
		/// The width of the underlying map.
		/// </summary>
		public int Width { get => BaseMap.Width; }

		/// <summary>
		/// Given a Coord, translates and returns/sets the "value" associated with that location.
		/// this[int x, int y] calls this indexer for its functionality, so overriding this
		/// functionality also changes that overload.
		/// </summary>
		/// <param name="pos">Location to get/set the value for.</param>
		/// <returns>The translated "value" associated with the provided location.</returns>
		public virtual T2 this[Coord pos]
		{
			get => TranslateGet(pos, BaseMap[pos]);
			set => BaseMap[pos] = TranslateSet(pos, value);
		}

		/// <summary>
		/// Given an X and Y value, translates and returns/sets the "value" associated with that
		/// location. This function calls this[Coord pos], so override that indexer to change functionality.
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
		/// Returns a string representation of the SettableTranslationMap.
		/// </summary>
		/// <returns>A string representation of the SettableTranslationMap.</returns>
		public override string ToString() => this.ExtendToString();

		/// <summary>
		/// Returns a string representation of the SettableTranslationMap, using the
		/// elementStringifier function given to determine what string represents which value.
		/// </summary>
		/// <remarks>
		/// This could be used, for example, on an SettableTranslationMap of boolean values, to
		/// output '#' for false values, and '.' for true values.
		/// </remarks>
		/// <param name="elementStringifier">
		/// Function determining the string representation of each element.
		/// </param>
		/// <returns>A string representation of the SettableTranslationMap.</returns>
		public string ToString(Func<T2, string> elementStringifier) => this.ExtendToString(elementStringifier: elementStringifier);

		/// <summary>
		/// Prints the values in the SettableTranslationMap, using the function specified to turn
		/// elements into strings, and using the "field length" specified. Each element of type T
		/// will have spaces added to cause it to take up exactly fieldSize characters, provided
		/// fieldSize is less than the length of the element's string represention. A positive-number
		/// right-aligns the text within the field, while a negative number left-aligns the text.
		/// </summary>
		/// <param name="fieldSize">The size of the field to give each value.</param>
		/// <param name="elementStringifier">
		/// Function to use to convert each element to a string. Null defaults to the ToString
		/// function of type T.
		/// </param>
		/// <returns>A string representation of the SettableTranslationMap.</returns>
		public string ToString(int fieldSize, Func<T2, string> elementStringifier = null) => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);

		/// <summary>
		/// Translates your map data into the view type. Takes only a value from the underlying map.
		/// If a position is also needed to perform the translation, use TranslateGet(Coord, T1) instead.
		/// </summary>
		/// <param name="value">The data value from your map.</param>
		/// <returns>A value of the mapped data type</returns>
		protected virtual T2 TranslateGet(T1 value) =>
			throw new NotImplementedException($"{nameof(TranslateGet)}(T1) was not implemented, and {nameof(TranslateGet)}(Coord, T1) was not re-implemented.  One of these two functions must be implemented.");

		/// <summary>
		/// Translates your map data into the view type. Takes a value from the underlying map and
		/// the corresponding position for that value. If a position is not needed to perform the
		/// translation, use TranslateGet(T1) instead.
		/// </summary>
		/// <param name="position">The position of the given data value from your map.</param>
		/// <param name="value">The data value from your map.</param>
		/// <returns>A value of the mapped data type</returns>
		protected virtual T2 TranslateGet(Coord position, T1 value) => TranslateGet(value);

		/// <summary>
		/// Translates the view type into the appropriate form for your map data. Takes only a value
		/// from the underlying map. If a position is also needed to perform the translation, use
		/// TranslateSet(Coord, T2) instead.
		/// </summary>
		/// <param name="value">A value of the mapped data type</param>
		/// <returns>The data value for your map.</returns>
		protected virtual T1 TranslateSet(T2 value) =>
			throw new NotImplementedException($"{nameof(TranslateSet)}(T2) was not implemented, and {nameof(TranslateSet)}(Coord, T2) was not re-implemented.  One of these two functions must be implemented.");

		/// <summary>
		/// Translates the view type into the appropriate form for your map data. Takes a value from
		/// the underlying map, and it corresponding position. If a position is not needed to perform
		/// the translation, use TranslateSet(T2) instead.
		/// </summary>
		/// <param name="position">The position of the given mapped data type.</param>
		/// <param name="value">A value of the mapped data type</param>
		/// <returns>The data value for your map.</returns>
		protected virtual T1 TranslateSet(Coord position, T2 value) => TranslateSet(value);
	}
}