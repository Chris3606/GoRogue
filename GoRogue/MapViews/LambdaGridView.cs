using System;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Class implementing <see cref="IGridView{T}"/>, by providing the "get" functionality via a function that is
    /// passed in at construction.  For a version that implements <see cref="ISettableGridView{T}" />, see
    /// <see cref="LambdaSettableGridView{T}" />.
    /// </summary>
    /// <typeparam name="T">The type of value being returned by the indexer functions.</typeparam>
    [PublicAPI]
    public sealed class LambdaGridView<T> : GridViewBase<T>
    {
        private readonly Func<int> _heightGetter;
        private readonly Func<Point, T> _valueGetter;
        private readonly Func<int> _widthGetter;

        /// <summary>
        /// Constructor. Takes the width and height of the grid, and the function to use to retrieve
        /// the value for a location.
        /// </summary>
        /// <remarks>
        /// This constructor is useful if the width and height of the underlying representation do
        /// not change, so they can safely be passed in as constants.
        /// </remarks>
        /// <param name="width">The (constant) width of the map.</param>
        /// <param name="height">The (constant) height of the map.</param>
        /// <param name="valueGetter">
        /// A lambda/function that returns the value of type T associated with the location it is given.
        /// This function is called each time the grid view's indexers are called upon to retrieve a value
        /// from a location.
        /// </param>
        public LambdaGridView(int width, int height, Func<Point, T> valueGetter)
            : this(() => width, () => height, valueGetter)
        { }

        /// <summary>
        /// Constructor. Takes functions that retrieve the width and height of the grid, and the
        /// function used to retrieve the value for a location.
        /// </summary>
        /// <remarks>
        /// This constructor is useful if the width and height of the grid being represented may
        /// change -- one can provide lambdas/functions that retrieve the width and height of the grid being
        /// represented, and these functions will be called any time the <see cref="Width" /> and <see cref="Height" />
        /// properties are retrieved.
        /// </remarks>
        /// <param name="widthGetter">
        /// A function/lambda that retrieves the width of the grid being represented.
        /// </param>
        /// <param name="heightGetter">
        /// A function/lambda that retrieves the height of the grid being represented.
        /// </param>
        /// <param name="valueGetter">
        /// A function/lambda that returns the value of type T associated with the location it is given.
        /// </param>
        public LambdaGridView(Func<int> widthGetter, Func<int> heightGetter, Func<Point, T> valueGetter)
        {
            _widthGetter = widthGetter;
            _heightGetter = heightGetter;
            _valueGetter = valueGetter;
        }

        /// <inheritdoc />
        public override int Height => _heightGetter();

        /// <inheritdoc />
        public override int Width => _widthGetter();

        /// <summary>
        /// Given a position, returns the "value" associated with that position, by calling the
        /// valueGetter function provided at construction.
        /// </summary>
        /// <param name="pos">Location to retrieve the value for.</param>
        /// <returns>
        /// The "value" associated with the provided location, according to the valueGetter function
        /// provided at construction.
        /// </returns>
        public override T this[Point pos] => _valueGetter(pos);

        /// <summary>
        /// Returns a string representation of the grid view's values.
        /// </summary>
        /// <returns>A string representation of the map view.</returns>
        public override string ToString() => this.ExtendToString();

        /// <summary>
        /// Returns a string representation of the grid view's values, using <paramref name="elementStringifier" />
        /// to determine what string represents each value.
        /// </summary>
        /// <param name="elementStringifier">
        /// Function determining the string representation of each element.
        /// </param>
        /// <returns>A string representation of the grid view's values.</returns>
        public string ToString(Func<T, string> elementStringifier)
            => this.ExtendToString(elementStringifier: elementStringifier);

        /// <summary>
        /// Returns a string representing the grid view's values, using the function specified to turn elements into
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
        /// <returns>A string representation of the grid view's values.</returns>
        public string ToString(int fieldSize, Func<T, string>? elementStringifier = null)
            => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);
    }
}
