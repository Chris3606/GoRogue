using System;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Class implementing <see cref="ISettableGridView{T}"/> by providing a functions that translate values from one
    /// grid view with complex data types, to a grid view with simple data types, and vice versa.  For a version that
    /// provides only "get" functionality, see <see cref="TranslationGridView{T1,T2}" />.
    /// </summary>
    /// <remarks>
    /// See <see cref="TranslationGridView{T1,T2}" />.  The use case is the same, except that this class
    /// implements <see cref="ISettableGridView{T}" /> instead, and thus also allows you to specify
    /// set-translations via TranslateSet.
    /// </remarks>
    /// <typeparam name="T1">The type of your underlying data.</typeparam>
    /// <typeparam name="T2">The type of the data being exposed by the grid view.</typeparam>
    [PublicAPI]
    public abstract class SettableTranslationGridView<T1, T2> : SettableGridViewBase<T2>
    {
        /// <summary>
        /// Constructor. Takes an existing grid view to create a view from.
        /// </summary>
        /// <param name="baseGrid">A grid view exposing your underlying map data.</param>
        protected SettableTranslationGridView(ISettableGridView<T1> baseGrid) => BaseGrid = baseGrid;

        /// <summary>
        /// Constructor. Takes an existing grid view to create a view from and applies view data to it.
        /// </summary>
        /// <remarks>
        /// Since this constructor must call TranslateSet to perform its function, do NOT
        /// call this constructor if the TranslateSet implementation depends on the derived
        /// class's constructor being completed to function properly.
        /// </remarks>
        /// <param name="baseGrid">Your underlying map data.</param>
        /// <param name="overlay">
        /// The view data to apply to the grid. Must have identical dimensions to <paramref name="baseGrid" />.
        /// </param>
        protected SettableTranslationGridView(ISettableGridView<T1> baseGrid, ISettableGridView<T2> overlay)
            : this(baseGrid) => this.ApplyOverlay(overlay);

        /// <summary>
        /// The grid view exposing your underlying data.
        /// </summary>
        public ISettableGridView<T1> BaseGrid { get; private set; }

        /// <inheritdoc />
        public override int Height => BaseGrid.Height;

        /// <inheritdoc />
        public override int Width => BaseGrid.Width;

        /// <summary>
        /// Given a position, translates and returns/sets the "value" associated with that position.
        /// </summary>
        /// <param name="pos">Location to get/set the value for.</param>
        /// <returns>The translated "value" associated with the provided location.</returns>
        public override T2 this[Point pos]
        {
            get => TranslateGet(pos, BaseGrid[pos]);
            set => BaseGrid[pos] = TranslateSet(pos, value);
        }

        /// <summary>
        /// Returns a string representation of the grid values.
        /// </summary>
        /// <returns>A string representation of the grid values.</returns>
        public override string ToString() => this.ExtendToString();

        /// <summary>
        /// Returns a string representation of the grid values, using <paramref name="elementStringifier" />
        /// to determine what string represents each value.
        /// </summary>
        /// <param name="elementStringifier">
        /// Function determining the string representation of each element.
        /// </param>
        /// <returns>A string representation of the grid view.</returns>
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
        /// Translates your underlying data into the view type. Takes only a value from the underlying data.
        /// If a position is also needed to perform the translation, use <see cref="TranslateGet(Point, T1)" />
        /// instead.
        /// </summary>
        /// <param name="value">The data value from your underlying data.</param>
        /// <returns>A value of the mapped data type.</returns>
        protected virtual T2 TranslateGet(T1 value) =>
            throw new NotSupportedException(
                $"{nameof(TranslateGet)}(T1) was not implemented, and {nameof(TranslateGet)}(Point, T1) was not re-implemented.  One of these two functions must be implemented.");

        /// <summary>
        /// Translates your underlying data into the view type. Takes a value from the underlying data and
        /// the corresponding position for that value. If a position is not needed to perform the
        /// translation, use <see cref="TranslateGet(T1)" /> instead.
        /// </summary>
        /// <param name="position">The position of the given data value from your underlying data structure.</param>
        /// <param name="value">The data value from your underlying structure.</param>
        /// <returns>A value of the mapped data type.</returns>
        protected virtual T2 TranslateGet(Point position, T1 value) => TranslateGet(value);

        /// <summary>
        /// Translates the view type into the appropriate form for your underlying data. Takes only a value
        /// from the grid view itself. If a position is also needed to perform the translation, use
        /// <see cref="TranslateSet(Point, T2)" /> instead.
        /// </summary>
        /// <param name="value">A value of the mapped data type.</param>
        /// <returns>The data value for your underlying representation.</returns>
        protected virtual T1 TranslateSet(T2 value) =>
            throw new NotSupportedException(
                $"{nameof(TranslateSet)}(T2) was not implemented, and {nameof(TranslateSet)}(Point, T2) was not re-implemented.  One of these two functions must be implemented.");

        /// <summary>
        /// Translates the view type into the appropriate form for your underlying data. Takes a value from
        /// the underlying data, and it corresponding position. If a position is not needed to perform
        /// the translation, use <see cref="TranslateSet(T2)" /> instead.
        /// </summary>
        /// <param name="position">The position of the given mapped data type.</param>
        /// <param name="value">A value of the mapped data type.</param>
        /// <returns>The data value for your underlying representation.</returns>
        protected virtual T1 TranslateSet(Point position, T2 value) => TranslateSet(value);
    }
}
