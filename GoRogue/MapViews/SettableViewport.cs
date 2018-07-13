namespace GoRogue.MapViews
{
    /// <summary>
    /// Similar to Viewport, but implements ISettableMapView and thus implements set functionality
    /// via relative coordinates.
    /// </summary>
    /// <typeparam name="T">Type being exposed by MapView.</typeparam>
    public class SettableViewport<T> : Viewport<T>, ISettableMapView<T>
    {
        /// <summary>
        /// Constructor. Takes the MapView to represent, and the initial ViewArea for that map.
        /// </summary>
        /// <param name="mapView">The map view being represented.</param>
        /// <param name="viewArea">The initial ViewArea for that map.</param>
        public SettableViewport(ISettableMapView<T> mapView, Rectangle viewArea)
            : base(mapView, viewArea) { }

        /// <summary>
        /// Constructor. Takes the MapView to represent. Initial ViewArea will be the entire MapView.
        /// </summary>
        /// <param name="mapView">The MapView to represent.</param>
        public SettableViewport(ISettableMapView<T> mapView) : base(mapView) { }

        /// <summary>
        /// The MapView that this Viewport is exposing values from.
        /// </summary>
        public new ISettableMapView<T> MapView
        {
            get => (ISettableMapView<T>)base.MapView;
        }

        /// <summary>
        /// Given a position in relative coordinates, sets/returns the "value" associated with that
        /// location in absolute coordinates.
        /// </summary>
        /// <param name="relativePosition">
        /// Viewport-relative position of the location to retrieve/set the value for.
        /// </param>
        /// <returns>
        /// The "value" associated with the absolute location represented on the underlying MapView.
        /// </returns>
        public new T this[Coord relativePosition]
        {
            get => base[relativePosition];
            set => MapView[ViewArea.Position + relativePosition] = value;
        }

        /// <summary>
        /// Given an X and Y value in relative coordinates, sets/returns the "value" associated with
        /// that location in absolute coordinates.
        /// </summary>
        /// <param name="relativeX">Viewport-relative X-value of location.</param>
        /// <param name="relativeY">Viewport-relative Y-value of location.</param>
        /// <returns>
        /// The "value" associated with the absolute location represented on the underlying MapView.
        /// </returns>
        public new T this[int relativeX, int relativeY]
        {
            get => base[relativeX, relativeY];
            set => MapView[ViewArea.X + relativeX, ViewArea.Y + relativeY] = value;
        }
    }
}