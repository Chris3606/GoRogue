using System;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Class like <see cref="Viewport{T}"/>, however the view area is in no way bounded to the edges of the underlying map.
    /// Instead, if you access a position that cannot map to any valid position in the underlying map view, a (specified)
    /// default value is returned.
    /// </summary>
    /// <typeparam name="T">The type being exposed by the UnboundedViewport.</typeparam>
    [PublicAPI]
    public class UnboundedViewport<T> : IMapView<T>
    {
        /// <summary>
        /// Constructor. Takes the parent map view, and the initial subsection of that map view to represent.
        /// </summary>
        /// <param name="mapView">The map view being represented.</param>
        /// <param name="viewArea">The initial subsection of that map to represent.</param>
        /// <param name="defaultValue">The value to return if a position is accessed that is outside the actual underlying map view.</param>
        public UnboundedViewport(IMapView<T> mapView, Rectangle viewArea, T defaultValue = default)
        {
            MapView = mapView;
            _viewArea = viewArea;
            DefaultValue = defaultValue;

        }

        /// <summary>
        /// Constructor. Takes the map view to represent. The viewport will represent the entire given map view.
        /// </summary>
        /// <param name="mapView">The map view to represent.</param>
        /// <param name="defaultValue">The value to return if a position is accessed that is outside the actual underlying map view.</param>
        public UnboundedViewport(IMapView<T> mapView, T defaultValue = default)
            : this(mapView, mapView.Bounds(), defaultValue) { }

        /// <summary>
        /// The height of the area being represented.
        /// </summary>
        public int Height => _viewArea.Height;

        /// <summary>
        /// The map view that this UnboundedViewport is exposing values from.
        /// </summary>
        public IMapView<T> MapView { get; private set; }

        // Analyzer misreads this because of ref return
#pragma warning disable IDE0044
        private Rectangle _viewArea;
#pragma warning restore IDE0044
        /// <summary>
        /// The area of the base MapView that this Viewport is exposing. Although this property does
        /// not explicitly expose a set accessor, it is returning a reference and as such may be
        /// assigned to. This viewport is NOT bounded to base map edges -- for this functionality, see the <see cref="Viewport{T}"/> class.
        /// </summary>
        public ref Rectangle ViewArea => ref _viewArea;

        /// <summary>
        /// The height of the area being represented.
        /// </summary>
        public int Width => _viewArea.Width;

        /// <summary>
        /// The value to return if a position is accessed that is outside the actual underlying map view.
        /// </summary>
        public readonly T DefaultValue;

        /// <summary>
        /// Given a position in relative 1d-array-index style, returns the "value" associated with that
        /// location in absolute coordinates.
        /// </summary>
        /// <param name="relativeIndex1D">
        /// Viewport-relative position of the location to retrieve the value for, as a 1D array index.
        /// </param>
        /// <returns>
        /// The "value" associated with the absolute location represented on the underlying map view,
        /// or <see cref="DefaultValue"/> if the absolute position does not exist in the underlying map view.
        /// </returns>
        public T this[int relativeIndex1D] => this[Point.FromIndex(relativeIndex1D, Width)];

        /// <summary>
        /// Given a position in relative coordinates, returns the "value" associated with that
        /// location in absolute coordinates.
        /// </summary>
        /// <param name="relativePosition">
        /// Viewport-relative position of the location to retrieve the value for.
        /// </param>
        /// <returns>
        /// The "value" associated with the absolute location represented on the underlying map view,
        /// or <see cref="DefaultValue"/> if the absolute position does not exist in the underlying map view.
        /// </returns>
        public virtual T this[Point relativePosition]
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
        /// The "value" associated with the absolute location represented on the underlying map view, 
        /// or <see cref="DefaultValue"/> if the absolute position does not exist in the underlying map view.
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
        /// Returns a string representation of the UnboundedViewport.
        /// </summary>
        /// <returns>A string representation of the UnboundedViewport.</returns>
        public override string ToString() => this.ExtendToString();

        /// <summary>
        /// Returns a string representation of the map view, using <paramref name="elementStringifier"/>
        /// to determine what string represents each value.
        /// </summary>
        /// <remarks>
        /// This could be used, for example, on an UnboundedViewport of boolean values, to output '#' for
        /// false values, and '.' for true values.
        /// </remarks>
        /// <param name="elementStringifier">
        /// Function determining the string representation of each element.
        /// </param>
        /// <returns>A string representation of the UnboundedViewport.</returns>
        public string ToString(Func<T, string> elementStringifier) => this.ExtendToString(elementStringifier: elementStringifier);

        /// <summary>
        /// Prints the values in the UnboundedViewport, using the function specified to turn elements into
        /// strings, and using the "field length" specified.
        /// </summary>
        /// <remarks>
        /// Each element of type T will have spaces added to cause it to take up exactly
        /// <paramref name="fieldSize"/> characters, provided <paramref name="fieldSize"/> 
        /// is less than the length of the element's string representation.
        /// </remarks>
        /// <param name="fieldSize">
        /// The size of the field to give each value.  A positive-number
        /// right-aligns the text within the field, while a negative number left-aligns the text.
        /// </param>
        /// <param name="elementStringifier">
        /// Function to use to convert each element to a string. null defaults to the ToString
        /// function of type T.
        /// </param>
        /// <returns>A string representation of the UnboundedViewport.</returns>
        public string ToString(int fieldSize, Func<T, string>? elementStringifier = null) => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);
    }
}
