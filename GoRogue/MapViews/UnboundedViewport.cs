using System;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Similar to <see cref="Viewport{T}"/>, except that the view area is in no way bounded to the edges of the
    /// underlying grid view.  Instead, if you access a position that cannot map to any valid position in the underlying
    /// grid view, a (specified) default value is returned.
    /// </summary>
    /// <typeparam name="T">The type being exposed by the UnboundedViewport.</typeparam>
    [PublicAPI]
    public class UnboundedViewport<T> : GridViewBase<T>
    {
        /// <summary>
        /// The value to return if a position is accessed that is outside the actual underlying grid view.
        /// </summary>
        public readonly T DefaultValue;

        // Analyzer misreads this because of ref return
#pragma warning disable IDE0044
        private Rectangle _viewArea;
#pragma warning restore IDE0044
        /// <summary>
        /// Constructor. Takes the parent grid view, and the initial subsection of that grid view to represent.
        /// </summary>
        /// <param name="gridView">The grid view being represented.</param>
        /// <param name="viewArea">The initial subsection of that grid to represent.</param>
        /// <param name="defaultValue">
        /// The value to return if a position is accessed that is outside the actual underlying grid view.
        /// </param>
        public UnboundedViewport(IGridView<T> gridView, Rectangle viewArea, T defaultValue = default)
        {
            GridView = gridView;
            _viewArea = viewArea;
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// Constructor. Takes the grid view to represent. The viewport will represent the entire given grid view.
        /// </summary>
        /// <param name="gridView">The grid view to represent.</param>
        /// <param name="defaultValue">
        /// The value to return if a position is accessed that is outside the actual underlying grid view.
        /// </param>
        public UnboundedViewport(IGridView<T> gridView, T defaultValue = default)
            : this(gridView, gridView.Bounds(), defaultValue)
        { }

        /// <summary>
        /// The grid view that this UnboundedViewport is exposing values from.
        /// </summary>
        public IGridView<T> GridView { get; private set; }

        /// <summary>
        /// The area of the base GridView that this Viewport is exposing. Although this property does
        /// not explicitly expose a set accessor, it is returning a reference and as such may be
        /// assigned to. This viewport is NOT bounded to base map edges -- for this functionality, see the
        /// <see cref="Viewport{T}" /> class.
        /// </summary>
        public ref Rectangle ViewArea => ref _viewArea;

        /// <summary>
        /// The height of the area being represented.
        /// </summary>
        public override int Height => _viewArea.Height;

        /// <summary>
        /// The width of the area being represented.
        /// </summary>
        public override int Width => _viewArea.Width;

        /// <summary>
        /// Given a position in relative coordinates, returns the "value" associated with that
        /// location in absolute coordinates.
        /// </summary>
        /// <param name="relativePosition">
        /// Viewport-relative position of the location to retrieve the value for.
        /// </param>
        /// <returns>
        /// The "value" associated with the absolute location represented on the underlying grid view,
        /// or <see cref="DefaultValue" /> if the absolute position does not exist in the underlying grid view.
        /// </returns>
        public override T this[Point relativePosition]
        {
            get
            {
                var pos = _viewArea.Position + relativePosition;
                return GridView.Contains(pos) ? GridView[pos] : DefaultValue;
            }
        }

        /// <summary>
        /// Returns a string representation of the grid values inside the viewport.
        /// </summary>
        /// <returns>A string representation of the grid values inside the viewport.</returns>
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
        /// Returns a string representation of the grid values inside the viewport, using the function specified to turn
        /// elements into strings, and using the "field length" specified.
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
