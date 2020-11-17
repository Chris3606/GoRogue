using System;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Implements <see cref="IGridView{T}"/> to expose a "viewport", or sub-area, of another grid view.
    /// Its indexers perform relative to absolute coordinate translations based on the viewport size/location, and
    /// return the proper value of type T from the underlying view.
    /// </summary>
    /// <remarks>
    /// This implementation restricts the subsection of the view that is presented in such a way that no part
    /// of the viewport can be outside the boundary of its parent grid view.  The viewport cannot be bigger than
    /// the underlying grid view, and the viewport's position is "locked" to the edge so that it cannot be set in such a
    /// way that a portion of the viewport lies outside the bounds of the parent view.  If you would rather allow this
    /// and return a default value for locations outside the parent grid view, see <see cref="UnboundedViewport{T}" />.
    /// </remarks>
    /// <typeparam name="T">The type being exposed by the Viewport.</typeparam>
    [PublicAPI]
    public class Viewport<T> : GridViewBase<T>
    {
        private readonly BoundedRectangle _boundedRect;

        /// <summary>
        /// Constructor. Takes the parent grid view, and the initial subsection of that grid view to represent.
        /// </summary>
        /// <param name="gridView">The grid view being represented.</param>
        /// <param name="viewArea">The initial subsection of that grid view to represent.</param>
        public Viewport(IGridView<T> gridView, Rectangle viewArea)
        {
            GridView = gridView;
            _boundedRect = new BoundedRectangle(viewArea, GridView.Bounds());
        }

        /// <summary>
        /// Constructor. Takes the map view to represent. The viewport will represent the entire given map view.
        /// </summary>
        /// <param name="gridView">The map view to represent.</param>
        public Viewport(IGridView<T> gridView)
            : this(gridView, gridView.Bounds())
        { }

        /// <summary>
        /// The grid view that this Viewport is exposing values from.
        /// </summary>
        public IGridView<T> GridView { get; private set; }

        /// <summary>
        /// The area of <see cref="GridView" /> that this Viewport is exposing.  Use <see cref="SetViewArea(Rectangle)" />
        /// to set the viewing area.
        /// </summary>
        public ref readonly Rectangle ViewArea => ref _boundedRect.Area;

        /// <summary>
        /// The height of the area being represented.
        /// </summary>
        public override int Height => ViewArea.Height;

        /// <summary>
        /// The width of the area being represented.
        /// </summary>
        public override int Width => ViewArea.Width;

        /// <summary>
        /// Given a position in relative coordinates, returns the "value" associated with that
        /// location in absolute coordinates.
        /// </summary>
        /// <param name="relativePosition">
        /// Viewport-relative position of the location to retrieve the value for.
        /// </param>
        /// <returns>
        /// The "value" associated with the absolute location represented on the underlying grid view.
        /// </returns>
        public override T this[Point relativePosition] => GridView[ViewArea.Position + relativePosition];

        /// <summary>
        /// Sets the viewing area for the viewport to the value given.  The viewport will automatically be bounded as
        /// needed to ensure that it remains within the bounds of the underlying IGridView.
        /// </summary>
        /// <param name="viewArea">The new view area.</param>
        public void SetViewArea(Rectangle viewArea) => _boundedRect.SetArea(viewArea);

        /// <summary>
        /// Returns a string representation of the grid values inside the viewport.
        /// </summary>
        /// <returns>A string representation of the values inside the viewport.</returns>
        public override string ToString() => this.ExtendToString();

        /// <summary>
        /// Returns a string representation of the grid values inside the viewport, using
        /// <paramref name="elementStringifier" /> to determine what string represents each value.
        /// </summary>
        /// <param name="elementStringifier">
        /// Function determining the string representation of each element.
        /// </param>
        /// <returns>A string representation of the grid values inside the viewport.</returns>
        public string ToString(Func<T, string> elementStringifier)
            => this.ExtendToString(elementStringifier: elementStringifier);

        /// <summary>
        /// Returns a string representation of the grid values inside the viewport, using the function specified to
        /// turn elements into strings, and using the "field length" specified.
        /// </summary>
        /// <remarks>
        /// Each element of type T will have spaces added to cause it to take up exactly
        /// <paramref name="fieldSize" /> characters, provided <paramref name="fieldSize" />
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
        /// <returns>A string representation of the grid values inside the viewport.</returns>
        public string ToString(int fieldSize, Func<T, string>? elementStringifier = null)
            => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);
    }
}
