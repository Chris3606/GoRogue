using System;

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
	/// struct containing many different member values. This class allows you to build descendant
	/// classes that override the TranslateGet method(s) for simple mapping, or the "this" indexers
	/// if you need full access to the underlying data for context, in order to present a simplified
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

		public T2 this[int index1D]
		{
			get
			{
				var pos = Coord.ToCoord(index1D, Width);
				return TranslateGet(pos, BaseMap[pos]);
			}
		}

		/// <summary>
		/// Given an X and Y value, translates and returns the "value" associated with that location.
		/// This function calls this[Coord pos], so override that indexer to change functionality.
		/// </summary>
		/// <param name="x">X-value of location.</param>
		/// <param name="y">Y-value of location.</param>
		/// <returns>The translated "value" associated with that location.</returns>
		public T2 this[int x, int y]
		{
			get => this[new Coord(x, y)];
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
			get => TranslateGet(pos, BaseMap[pos]);
		}

		/// <summary>
		/// Returns a string representation of the TranslationMap.
		/// </summary>
		/// <returns>A string representation of the TranslationMap.</returns>
		public override string ToString() => this.ExtendToString();

		/// <summary>
		/// Returns a string representation of the TranslationMap, using the elementStringifier
		/// function given to determine what string represents which value.
		/// </summary>
		/// <remarks>
		/// This could be used, for example, on an TranslationMap of boolean values, to output '#'
		/// for false values, and '.' for true values.
		/// </remarks>
		/// <param name="elementStringifier">
		/// Function determining the string representation of each element.
		/// </param>
		/// <returns>A string representation of the TranslationMap.</returns>
		public string ToString(Func<T2, string> elementStringifier) => this.ExtendToString(elementStringifier: elementStringifier);

		/// <summary>
		/// Prints the values in the TranslationMap, using the function specified to turn elements
		/// into strings, and using the "field length" specified. Each element of type T will have
		/// spaces added to cause it to take up exactly fieldSize characters, provided fieldSize is
		/// less than the length of the element's string represention. A positive-number right-aligns
		/// the text within the field, while a negative number left-aligns the text.
		/// </summary>
		/// <param name="fieldSize">The size of the field to give each value.</param>
		/// <param name="elementStringifier">
		/// Function to use to convert each element to a string. Null defaults to the ToString
		/// function of type T.
		/// </param>
		/// <returns>A string representation of the TranslationMap.</returns>
		public string ToString(int fieldSize, Func<T2, string> elementStringifier = null) => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);

		/// <summary>
		/// Translates your map data into the view type using just the map data value. If you need
		/// the location to perform the translation, implement TranslateGet(Coord, T1) instead.
		/// </summary>
		/// <param name="value">The data value from your map.</param>
		/// <returns>A value of the mapped data type</returns>
		protected virtual T2 TranslateGet(T1 value) =>
			throw new NotImplementedException($"{nameof(TranslateGet)}(T1) was not implemented, and {nameof(TranslateGet)}(Coord, T1) was not re-implemented.  One of these two functions must be implemented.");

		/// <summary>
		/// Translates your map data into the view type using the position and the map data value. If
		/// you need only the data value to perform the translation, implement TranslateGet(T1) instead.
		/// </summary>
		/// <param name="position">The position of the given data value in your map.</param>
		/// <param name="value">The data value from your map.</param>
		/// <returns>A value of the mapped data type</returns>
		protected virtual T2 TranslateGet(Coord position, T1 value) => TranslateGet(value);
	}
}