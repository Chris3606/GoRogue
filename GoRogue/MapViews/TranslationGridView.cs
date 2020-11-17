using System;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Class implementing <see cref="IGridView{T}"/> by providing a function that translates values from one grid view
    /// with complex data types, to a grid view with simple data types.  For a version that provides "set"
    /// functionality, see <see cref="SettableTranslationGridView{T1,T2}" />.
    /// </summary>
    /// <remarks>
    /// This class is useful if the underlying representation of the data you are creating a grid view for is complex,
    /// and you simply need to map a complex data type to a simpler one.  For example, you might implement the
    /// <see cref="TranslateGet(SadRogue.Primitives.Point,T1)"/> function to extract a property from a more complex
    /// structure.  If your mapping is very simple, or you do not wish to create a subclass, see
    /// <see cref="LambdaTranslationGridView{T1,T2}"/>.
    /// </remarks>
    /// <typeparam name="T1">The type of your underlying data.</typeparam>
    /// <typeparam name="T2">The type of the data being exposed by the grid view.</typeparam>
    [PublicAPI]
    public abstract class TranslationGridView<T1, T2> : GridViewBase<T2>
    {
        /// <summary>
        /// Constructor. Takes an existing grid view to create a view from.
        /// </summary>
        /// <param name="baseGrid">A grid view exposing your underlying data.</param>
        protected TranslationGridView(IGridView<T1> baseGrid) => BaseGrid = baseGrid;

        /// <summary>
        /// The underlying grid data, exposed as a grid view.
        /// </summary>
        public IGridView<T1> BaseGrid { get; private set; }

        /// <summary>
        /// The height of the grid.
        /// </summary>
        public override int Height => BaseGrid.Height;

        /// <summary>
        /// The width of the grid.
        /// </summary>
        public override int Width => BaseGrid.Width;

        /// <summary>
        /// Given a position, translates and returns the "value" associated with that position.
        /// </summary>
        /// <param name="pos">Location to get the value for.</param>
        /// <returns>The translated "value" associated with the provided location.</returns>
        public override T2 this[Point pos] => TranslateGet(pos, BaseGrid[pos]);

        /// <summary>
        /// Returns a string representation of the exposed grid values.
        /// </summary>
        /// <returns>A string representation of the exposed grid values.</returns>
        public override string ToString() => this.ExtendToString();

        /// <summary>
        /// Returns a string representation of the grid values, using <paramref name="elementStringifier" />
        /// to determine what string represents each value.
        /// </summary>
        /// <param name="elementStringifier">
        /// Function determining the string representation of each element.
        /// </param>
        /// <returns>A string representation of the grid values.</returns>
        public string ToString(Func<T2, string> elementStringifier)
            => this.ExtendToString(elementStringifier: elementStringifier);

        /// <summary>
        /// Returns a string representation of the grid values, using the function specified to turn elements into
        /// strings, and using the "field length" specified.
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
        /// <returns>A string representation of the grid values.</returns>
        public string ToString(int fieldSize, Func<T2, string>? elementStringifier = null)
            => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);

        /// <summary>
        /// Translates your actual data into the view type using just the data value itself. If you need
        /// the location as well to perform the translation, implement <see cref="TranslateGet(Point, T1)" />
        /// instead.
        /// </summary>
        /// <param name="value">The data value from the base grid view.</param>
        /// <returns>A value of the mapped data type.</returns>
        protected virtual T2 TranslateGet(T1 value) =>
            throw new NotSupportedException(
                $"{nameof(TranslateGet)}(T1) was not implemented, and {nameof(TranslateGet)}(Point, T1) was not re-implemented.  One of these two functions must be implemented.");

        /// <summary>
        /// Translates your actual data into the view type using the position and the data value itself. If
        /// you need only the data value to perform the translation, implement <see cref="TranslateGet(T1)" />
        /// instead.
        /// </summary>
        /// <param name="position">The position of the given data value.</param>
        /// <param name="value">The data value from your underlying grid view.</param>
        /// <returns>A value of the mapped data type.</returns>
        protected virtual T2 TranslateGet(Point position, T1 value) => TranslateGet(value);
    }
}
