using SadRogue.Primitives;

namespace GoRogue.MapViews
{
	/// <summary>
	/// Similar to <see cref="Viewport{T}"/>, but implements <see cref="ISettableMapView{T}"/>and thus implements
	/// "set" functionality via relative Pointinates.
	/// </summary>
	/// <typeparam name="T">Type being exposed by map view.</typeparam>
	public class SettableViewport<T> : Viewport<T>, ISettableMapView<T>
	{
		/// <summary>
		/// Constructor. Takes the parent map view, and the initial subsection of that map view to represent.
		/// </summary>
		/// <param name="mapView">The map view being represented.</param>
		/// <param name="viewArea">The initial subsection of that map to represent.</param>
		public SettableViewport(ISettableMapView<T> mapView, Rectangle viewArea)
			: base(mapView, viewArea) { }

		/// <summary>
		/// Constructor. Takes the map view to represent. The viewport will represent the entire given map view.
		/// </summary>
		/// <param name="mapView">The map view to represent.</param>
		public SettableViewport(ISettableMapView<T> mapView) : base(mapView) { }

		/// <summary>
		/// The map view that this viewport is exposing values from.
		/// </summary>
		public new ISettableMapView<T> MapView
		{
			get => (ISettableMapView<T>)base.MapView;
		}

		/// <summary>
		/// Given a position in relative 1d-array-index style, returns/sets the "value" associated with that
		/// location in absolute Pointinates.
		/// </summary>
		/// <param name="relativeIndex1D">
		/// Viewport-relative position of the location to retrieve/set the value for, as a 1D array index.
		/// </param>
		/// <returns>
		/// The "value" associated with the absolute location represented on the underlying map view.
		/// </returns>
		public new T this[int relativeIndex1D]
		{
			get => base[relativeIndex1D];
			set => MapView[ViewArea.Position + Point.FromIndex(relativeIndex1D, Width)] = value;
		}

		/// <summary>
		/// Given a position in relative Pointinates, sets/returns the "value" associated with that
		/// location in absolute Pointinates.
		/// </summary>
		/// <param name="relativePosition">
		/// Viewport-relative position of the location to retrieve/set the value for.
		/// </param>
		/// <returns>
		/// The "value" associated with the absolute location represented on the underlying map view.
		/// </returns>
		public new T this[Point relativePosition]
		{
			get => base[relativePosition];
			set => MapView[ViewArea.Position + relativePosition] = value;
			
		}

		/// <summary>
		/// Given an X and Y value in relative Pointinates, sets/returns the "value" associated with
		/// that location in absolute Pointinates.
		/// </summary>
		/// <param name="relativeX">Viewport-relative X-value of location.</param>
		/// <param name="relativeY">Viewport-relative Y-value of location.</param>
		/// <returns>
		/// The "value" associated with the absolute location represented on the underlying map view.
		/// </returns>
		public new T this[int relativeX, int relativeY]
		{
			get => base[relativeX, relativeY];
			set => MapView[ViewArea.X + relativeX, ViewArea.Y + relativeY] = value;
		}
	}
}
