using System;

namespace GoRogue.MapViews
{
	/// <summary>
	/// Since some algorithms that use MapViews can be expensive to run entirely on large maps (such
	/// as GoalMaps), Viewport is a class that effectively creates and maintains a "viewport" of the
	/// map. Its indexers perform relative to absolute coordinate translations, and return the proper
	/// value of type T from the underlying map..
	/// </summary>
	/// <typeparam name="T">The type being exposed by the MapView.</typeparam>
	public class Viewport<T> : IMapView<T>
	{
		private BoundedRectangle _boundedRect;

		/// <summary>
		/// Constructor. Takes the MapView to represent, and the initial ViewArea for that map.
		/// </summary>
		/// <param name="mapView">The map view being represented.</param>
		/// <param name="viewArea">The initial ViewArea for that map.</param>
		public Viewport(IMapView<T> mapView, Rectangle viewArea)
		{
			MapView = mapView;
			_boundedRect = new BoundedRectangle(viewArea, MapView.Bounds());
		}

		/// <summary>
		/// Constructor. Takes the MapView to represent. Initial ViewArea will be the entire MapView.
		/// </summary>
		/// <param name="mapView">The MapView to represent.</param>
		public Viewport(IMapView<T> mapView)
			: this(mapView, mapView.Bounds()) { }

		/// <summary>
		/// The height of the ViewArea.
		/// </summary>
		public int Height
		{
			get => ViewArea.Height;
		}

		/// <summary>
		/// The MapView that this Viewport is exposing values from.
		/// </summary>
		public IMapView<T> MapView { get; private set; }

		/// <summary>
		/// The area of the base MapView that this Viewport is exposing. Although this property does
		/// not explicitly expose a set accessor, it is returning a reference and as such may be
		/// assigned to. When accessed, the rectangle is automatically restricted by the edges of the
		/// map as necessary.
		/// </summary>
		public ref Rectangle ViewArea
		{
			get => ref _boundedRect.Area;
		}

		/// <summary>
		/// The width of the ViewArea.
		/// </summary>
		public int Width
		{
			get => ViewArea.Width;
		}

		public T this[int relativeIndex1D] => MapView[ViewArea.Position + Coord.ToCoord(relativeIndex1D, Width)];

		/// <summary>
		/// Given a position in relative coordinates, returns the "value" associated with that
		/// location in absolute coordinates.
		/// </summary>
		/// <param name="relativePosition">
		/// Viewport-relative position of the location to retrieve the value for.
		/// </param>
		/// <returns>
		/// The "value" associated with the absolute location represented on the underlying MapView.
		/// </returns>
		public virtual T this[Coord relativePosition] => MapView[ViewArea.Position + relativePosition];

		/// <summary>
		/// Given an X and Y value in relative coordinates, returns the "value" associated with that
		/// location in absolute coordinates.
		/// </summary>
		/// <param name="relativeX">Viewport-relative X-value of location.</param>
		/// <param name="relativeY">Viewport-relative Y-value of location.</param>
		/// <returns>
		/// The "value" associated with the absolute location represented on the underlying MapView.
		/// </returns>
		public virtual T this[int relativeX, int relativeY] => MapView[ViewArea.X + relativeX, ViewArea.Y + relativeY];

		/// <summary>
		/// Returns a string representation of the Viewport.
		/// </summary>
		/// <returns>A string representation of the Viewport.</returns>
		public override string ToString() => this.ExtendToString();

		/// <summary>
		/// Returns a string representation of the Viewport, using the elementStringifier function
		/// given to determine what string represents which value.
		/// </summary>
		/// <remarks>
		/// This could be used, for example, on an Viewport of boolean values, to output '#' for
		/// false values, and '.' for true values.
		/// </remarks>
		/// <param name="elementStringifier">
		/// Function determining the string representation of each element.
		/// </param>
		/// <returns>A string representation of the Viewport.</returns>
		public string ToString(Func<T, string> elementStringifier) => this.ExtendToString(elementStringifier: elementStringifier);

		/// <summary>
		/// Prints the values in the Viewport, using the function specified to turn elements into
		/// strings, and using the "field length" specified. Each element of type T will have spaces
		/// added to cause it to take up exactly fieldSize characters, provided fieldSize is less
		/// than the length of the element's string represention. A positive-number right-aligns the
		/// text within the field, while a negative number left-aligns the text.
		/// </summary>
		/// <param name="fieldSize">The size of the field to give each value.</param>
		/// <param name="elementStringifier">
		/// Function to use to convert each element to a string. Null defaults to the ToString
		/// function of type T.
		/// </param>
		/// <returns>A string representation of the Viewport.</returns>
		public string ToString(int fieldSize, Func<T, string> elementStringifier = null) => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);
	}
}