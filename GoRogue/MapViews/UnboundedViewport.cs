using System;

namespace GoRogue.MapViews
{
	/// <summary>
	/// Class like Viewport&lt;T&gt;, however teh view area is in no way bounded to the edges of the underlying map.
	/// Instead, if you access a position that cannot map to any valid position in the underlying map view, a default is returned.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class UnboundedViewport<T> : IMapView<T>
	{
		/// <summary>
		/// Constructor. Takes the MapView to represent, and the initial ViewArea for that map.
		/// </summary>
		/// <param name="mapView">The map view being represented.</param>
		/// <param name="viewArea">The initial ViewArea for that map.</param>
		/// <param name="defaultValue">The value to return if a position is accessed that is outside the actual underlying map view.</param>
		public UnboundedViewport(IMapView<T> mapView, Rectangle viewArea, T defaultValue = default(T))
		{
			MapView = mapView;
			_viewArea = viewArea;
			DefaultValue = defaultValue;

		}

		/// <summary>
		/// Constructor. Takes the MapView to represent. Initial ViewArea will be the entire MapView.
		/// </summary>
		/// <param name="mapView">The MapView to represent.</param>
		/// <param name="defaultValue">The value to return if a position is accessed that is outside the actual underlying map view.</param>
		public UnboundedViewport(IMapView<T> mapView, T defaultValue = default(T))
			: this(mapView, mapView.Bounds(), defaultValue) { }

		/// <summary>
		/// The height of the ViewArea.
		/// </summary>
		public int Height
		{
			get => _viewArea.Height;
		}

		/// <summary>
		/// The MapView that this Viewport is exposing values from.
		/// </summary>
		public IMapView<T> MapView { get; private set; }

		private Rectangle _viewArea;
		/// <summary>
		/// The area of the base MapView that this Viewport is exposing. Although this property does
		/// not explicitly expose a set accessor, it is returning a reference and as such may be
		/// assigned to. This viewport is NOT bounded to base map edges -- for this functionality, see the Viewport&lt;T&gt; class.
		/// </summary>
		public ref Rectangle ViewArea
		{
			get => ref _viewArea;
		}

		/// <summary>
		/// The width of the ViewArea.
		/// </summary>
		public int Width
		{
			get => _viewArea.Width;
		}

		/// <summary>
		/// The value to return if a position is accessed that is outside the actual underlying map view.
		/// </summary>
		public readonly T DefaultValue;

		public T this[int relativeIndex1D] => this[Coord.ToCoord(relativeIndex1D, Width)];

		/// <summary>
		/// Given a position in relative coordinates, returns the "value" associated with that
		/// location in absolute coordinates.
		/// </summary>
		/// <param name="relativePosition">
		/// Viewport-relative position of the location to retrieve the value for.
		/// </param>
		/// <returns>
		/// The "value" associated with the absolute location represented on the underlying MapView, or DefaultValue if the absolute position does not exist
		/// in the underlying map view.
		/// </returns>
		public virtual T this[Coord relativePosition]
		{
			get
			{
				var pos = _viewArea.Position + relativePosition;

				if (MapView.Contains(pos))
					return MapView[pos];

				return DefaultValue;
			}
		}

		/// <summary>
		/// Given an X and Y value in relative coordinates, returns the "value" associated with that
		/// location in absolute coordinates.
		/// </summary>
		/// <param name="relativeX">Viewport-relative X-value of location.</param>
		/// <param name="relativeY">Viewport-relative Y-value of location.</param>
		/// <returns>
		/// The "value" associated with the absolute location represented on the underlying MapView, or DefaultValue if the absolute
		/// position does not exist in the underlying MapView.
		/// </returns>
		public virtual T this[int relativeX, int relativeY]
		{
			get
			{
				int absX = ViewArea.X + relativeX;
				int absY = _viewArea.Y + relativeY;

				if (MapView.Contains(absX, absY))
					return MapView[absX, absY];

				return DefaultValue;
			}
		}

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
