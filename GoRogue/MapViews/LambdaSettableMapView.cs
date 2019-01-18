using System;

namespace GoRogue.MapViews
{
	/// <summary>
	/// Class designed to make implementing simple ISettableMapViews more convenient, by providing
	/// the "get" and "set" functionality via lambda functions. For a version that implements
	/// IMapView as opposed to ISettableMapView, see LambdaMapView.
	/// </summary>
	/// <remarks>
	/// Despite actual game map representations often consisting of complex types, exposing certain
	/// properties as primitive types (via ISettableMapView implementations) for GoRogue algorithms
	/// to use is often fairly simple (simply exposing/translating to a property in the actual map
	/// class, or similar). If your map consists of Cells of some sort, where there exists an
	/// instance of some class/struct per location that contains information about that location,
	/// TranslationMap/LambdaTranslationMap provide convenient ways to implement simple
	/// ISettableMapViews. In the case that no such single type exists, however, a more generic
	/// IMapView implementation is needed. This class takes the "get" and "set" functions as lambdas
	/// to shorten the process of creating such an implementation.
	/// </remarks>
	/// <typeparam name="T">The type of value being returned by the indexer functions.</typeparam>
	public sealed class LambdaSettableMapView<T> : ISettableMapView<T>
	{
		private Func<int> heightGetter;
		private Func<Coord, T> valueGetter;
		private Action<Coord, T> valueSetter;
		private Func<int> widthGetter;

		/// <summary>
		/// Constructor. Takes the width and height of the map, and the functions to use to
		/// retrieve/set the value for a location.
		/// </summary>
		/// <remarks>
		/// This constructor is useful if the width and height of the underlying representation do
		/// not change, so they can safely be passed in as constants.
		/// </remarks>
		/// <param name="width">The (constant) width of the map.</param>
		/// <param name="height">The (constant) height of the map.</param>
		/// <param name="valueGetter">
		/// A delegate/lambda that returns the value of type T associated with the location it is given.
		/// </param>
		/// <param name="valueSetter">
		/// A delegate/lambda that updates the map being represented accordingly, given a type T and
		/// position to which it was set.
		/// </param>
		public LambdaSettableMapView(int width, int height, Func<Coord, T> valueGetter, Action<Coord, T> valueSetter)
			: this(() => width, () => height, valueGetter, valueSetter) { }

		/// <summary>
		/// Constructor. Takes functions that retrieve the width and height of the map, and the
		/// functions used to retrieve/set the value for a location.
		/// </summary>
		/// <remarks>
		/// This constructor is useful if the width and height of the map being represented may
		/// change -- one can provide lambdas that retrieve the width and height of the map being
		/// represented, and these lambdas will be called any time the Width and Height properties of
		/// this class are retrieved.
		/// </remarks>
		/// <param name="widthGetter">
		/// A delegate/lambda that retrieves the width of the map being represented.
		/// </param>
		/// <param name="heightGetter">
		/// A delegate/lambda that retrieves the height of the map being represented.
		/// </param>
		/// <param name="valueGetter">
		/// A delegate/lambda that returns the value of type T associated with the location it is given.
		/// </param>
		/// <param name="valueSetter">
		/// A delegate/lambda that updates the map being represented accordingly, given a type T and
		/// position to which it was set.
		/// </param>
		public LambdaSettableMapView(Func<int> widthGetter, Func<int> heightGetter, Func<Coord, T> valueGetter, Action<Coord, T> valueSetter)
		{
			this.widthGetter = widthGetter;
			this.heightGetter = heightGetter;
			this.valueGetter = valueGetter;
			this.valueSetter = valueSetter;
		}

		/// <summary>
		/// The height of the map being represented.
		/// </summary>
		public int Height { get => heightGetter(); }

		/// <summary>
		/// The width of the map being represented.
		/// </summary>
		public int Width { get => widthGetter(); }

		/// <summary>
		/// Given an X and Y value, sets/returns the "value" associated with that location, by
		/// calling the valueGetter/valueSetter lambda provided at construction.
		/// </summary>
		/// <param name="x">X-value of location.</param>
		/// <param name="y">Y-value of location.</param>
		/// <returns>
		/// The "value" associated with that location, according to the valueGetter lambda provided
		/// at construction.
		/// </returns>
		public T this[int x, int y]
		{
			get => valueGetter(Coord.Get(x, y));
			set => valueSetter(Coord.Get(x, y), value);
		}

		/// <summary>
		/// Given a Coord, returns/sets the "value" associated with that location, by calling the
		/// valueGetter/valueSetter lambda provided at construction.
		/// </summary>
		/// <param name="pos">Location to retrieve the value for.</param>
		/// <returns>
		/// The "value" associated with the provided location, according to the valueGetter lambda
		/// provided at construction.
		/// </returns>
		public T this[Coord pos]
		{
			get => valueGetter(pos);
			set => valueSetter(pos, value);
		}

		/// <summary>
		/// Returns a string representation of the LambdaSettableMapView.
		/// </summary>
		/// <returns>A string representation of the LambdaSettableMapView.</returns>
		public override string ToString() => this.ExtendToString();

		/// <summary>
		/// Returns a string representation of the LambdaSettableMapView, using the
		/// elementStringifier function given to determine what string represents which value.
		/// </summary>
		/// <remarks>
		/// This could be used, for example, on an LambdaSettableMapView of boolean values, to output
		/// '#' for false values, and '.' for true values.
		/// </remarks>
		/// <param name="elementStringifier">
		/// Function determining the string representation of each element.
		/// </param>
		/// <returns>A string representation of the LambdaSettableMapView.</returns>
		public string ToString(Func<T, string> elementStringifier) => this.ExtendToString(elementStringifier: elementStringifier);

		/// <summary>
		/// Prints the values in the LambdaSettableMapView, using the function specified to turn
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
		/// <returns>A string representation of the LambdaSettableMapView.</returns>
		public string ToString(int fieldSize, Func<T, string> elementStringifier = null) => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);
	}
}