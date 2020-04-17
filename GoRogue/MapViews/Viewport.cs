using System;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Viewport is a class that effectively creates and maintains a "viewport", or subsection, of the map.
    /// Its indexers perform relative to absolute Pointinate translations, and return the proper value of
    /// type T from the underlying map.
    /// </summary>
    /// <remarks>
    /// Since some algorithms that use <see cref="IMapView{T}"/> implementations can be expensive to
    /// run on large maps (GoalMaps, etc), you can use viewports to present only a relevant subsection of the
    /// map to that algorithm.  It is generally useful for any case where you want an <see cref="IMapView{T}"/>
    /// that represents a subsection of some other <see cref="IMapView{T}"/>.
    /// 
    /// This implementation restricts the subsection of the map that is presented in such a way that no part
    /// of the viewport can be outside the boundary of its parent map view.  The viewport cannot be bigger than
    /// the map, and the viewport's position is "locked" to the edge so that it cannot be set in such a way that a portion
    /// of the viewport lies outside the bounds of the parent map.  If you would rather allow this and return
    /// a default value for locations outside the parent map, see <see cref="UnboundedViewport{T}"/>.
    /// </remarks>
    /// <typeparam name="T">The type being exposed by the Viewport.</typeparam>
    public class Viewport<T> : IMapView<T>
    {
        private readonly BoundedRectangle _boundedRect;

        /// <summary>
        /// Constructor. Takes the parent map view, and the initial subsection of that map view to represent.
        /// </summary>
        /// <param name="mapView">The map view being represented.</param>
        /// <param name="viewArea">The initial subsection of that map to represent.</param>
        public Viewport(IMapView<T> mapView, Rectangle viewArea)
        {
            MapView = mapView;
            _boundedRect = new BoundedRectangle(viewArea, MapView.Bounds());
        }

        /// <summary>
        /// Constructor. Takes the map view to represent. The viewport will represent the entire given map view.
        /// </summary>
        /// <param name="mapView">The map view to represent.</param>
        public Viewport(IMapView<T> mapView)
            : this(mapView, mapView.Bounds()) { }

        /// <summary>
        /// The height of the area being represented.
        /// </summary>
        public int Height => ViewArea.Height;

        /// <summary>
        /// The map view that this Viewport is exposing values from.
        /// </summary>
        public IMapView<T> MapView { get; private set; }

        /// <summary>
        /// The area of <see cref="MapView"/> that this Viewport is exposing.  Use <see cref="SetViewArea(Rectangle)"/>
        /// to set the viewing area.
        /// </summary>
        public ref readonly Rectangle ViewArea => ref _boundedRect.Area;

        /// <summary>
        /// The width of the area being represented.
        /// </summary>
        public int Width => ViewArea.Width;

        /// <summary>
        /// Given a position in relative 1d-array-index style, returns the "value" associated with that
        /// location in absolute Pointinates.
        /// </summary>
        /// <param name="relativeIndex1D">
        /// Viewport-relative position of the location to retrieve the value for, as a 1D array index.
        /// </param>
        /// <returns>
        /// The "value" associated with the absolute location represented on the underlying map view.
        /// </returns>
        public T this[int relativeIndex1D] => MapView[ViewArea.Position + Point.FromIndex(relativeIndex1D, Width)];

        /// <summary>
        /// Given a position in relative Pointinates, returns the "value" associated with that
        /// location in absolute Pointinates.
        /// </summary>
        /// <param name="relativePosition">
        /// Viewport-relative position of the location to retrieve the value for.
        /// </param>
        /// <returns>
        /// The "value" associated with the absolute location represented on the underlying map view.
        /// </returns>
        public virtual T this[Point relativePosition] => MapView[ViewArea.Position + relativePosition];

        /// <summary>
        /// Given an X and Y value in relative Pointinates, returns the "value" associated with that
        /// location in absolute Pointinates.
        /// </summary>
        /// <param name="relativeX">Viewport-relative X-value of location.</param>
        /// <param name="relativeY">Viewport-relative Y-value of location.</param>
        /// <returns>
        /// The "value" associated with the absolute location represented on the underlying map view.
        /// </returns>
        public virtual T this[int relativeX, int relativeY] => MapView[ViewArea.X + relativeX, ViewArea.Y + relativeY];

        /// <summary>
        /// Sets the viewing area for the viewport to the value given.  The viewport will automatically be bounded as needed to ensure that
        /// it remains within the bounds of the underlying IMapView.
        /// </summary>
        /// <param name="viewArea">The new view area.</param>
        public void SetViewArea(Rectangle viewArea) => _boundedRect.SetArea(viewArea);

        /// <summary>
        /// Returns a string representation of the Viewport.
        /// </summary>
        /// <returns>A string representation of the Viewport.</returns>
        public override string ToString() => this.ExtendToString();

        /// <summary>
        /// Returns a string representation of the map view, using <paramref name="elementStringifier"/>
        /// to determine what string represents each value.
        /// </summary>
        /// <remarks>
        /// This could be used, for example, on a Viewport of boolean values, to output '#' for
        /// false values, and '.' for true values.
        /// </remarks>
        /// <param name="elementStringifier">
        /// Function determining the string representation of each element.
        /// </param>
        /// <returns>A string representation of the Viewport.</returns>
        public string ToString(Func<T, string> elementStringifier) => this.ExtendToString(elementStringifier: elementStringifier);

        /// <summary>
        /// Prints the values in the Viewport, using the function specified to turn elements into
        /// strings, and using the "field length" specified.
        /// </summary>
        /// <remarks>
        /// Each element of type T will have spaces added to cause it to take up exactly
        /// <paramref name="fieldSize"/> characters, provided <paramref name="fieldSize"/> 
        /// is less than the length of the element's string represention.
        /// </remarks>
        /// <param name="fieldSize">
        /// The size of the field to give each value.  A positive-number
        /// right-aligns the text within the field, while a negative number left-aligns the text.
        /// </param>
        /// <param name="elementStringifier">
        /// Function to use to convert each element to a string. null defaults to the ToString
        /// function of type T.
        /// </param>
        /// <returns>A string representation of the Viewport.</returns>
        public string ToString(int fieldSize, Func<T, string>? elementStringifier = null) => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);
    }
}
