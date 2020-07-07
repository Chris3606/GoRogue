using System;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Class designed to make implementing simple ISettableMapViews more convenient, by providing the
    /// get/set functionality via a function that is passed in at construction. For a version that
    /// implements <see cref="IMapView{T}" /> as opposed to <see cref="ISettableMapView{T}" />, see
    /// <see cref="LambdaMapView{T}" />.
    /// </summary>
    /// <remarks>
    /// See <see cref="LambdaMapView{T}" />.  Identical in nature, but takes both get and set functionality
    /// via functions.
    /// </remarks>
    /// <typeparam name="T">The type of value being returned by the indexer functions.</typeparam>
    [PublicAPI]
    public sealed class LambdaSettableMapView<T> : SettableMapViewBase<T>
    {
        private readonly Func<int> _heightGetter;
        private readonly Func<Point, T> _valueGetter;
        private readonly Action<Point, T> _valueSetter;
        private readonly Func<int> _widthGetter;

        /// <summary>
        /// Constructor. Takes the width and height of the map, and the functions to use to
        /// retrieve/set the value for a location.
        /// </summary>
        /// <remarks>
        /// This constructor is useful if the width and height of the underlying representation do
        /// not change, so they can safely be passed in as constants.
        /// </remarks>
        /// <param name="width">The (constant) width of the map.</param>
        /// <param name="height">The (constant) height of the map.</param>
        /// <param name="valueGetter">
        /// A function/lambda that returns the value of type T associated with the location it is given.
        /// </param>
        /// <param name="valueSetter">
        /// A function/lambda that updates the map being represented accordingly, given a type T and
        /// position to which it was set.
        /// </param>
        public LambdaSettableMapView(int width, int height, Func<Point, T> valueGetter, Action<Point, T> valueSetter)
            : this(() => width, () => height, valueGetter, valueSetter)
        { }

        /// <summary>
        /// Constructor. Takes functions that retrieve the width and height of the map, and the
        /// functions used to retrieve/set the value for a location.
        /// </summary>
        /// <remarks>
        /// This constructor is useful if the width and height of the map being represented may
        /// change -- one can provide functions that retrieve the width and height of the map being
        /// represented, and these functions will be called any time the <see cref="Width" /> and <see cref="Height" />
        /// properties are retrieved.
        /// </remarks>
        /// <param name="widthGetter">
        /// A function/lambda that retrieves the width of the map being represented.
        /// </param>
        /// <param name="heightGetter">
        /// A function/lambda that retrieves the height of the map being represented.
        /// </param>
        /// <param name="valueGetter">
        /// A function/lambda that returns the value of type T associated with the location it is given.
        /// </param>
        /// <param name="valueSetter">
        /// A function/lambda that updates the map being represented accordingly, given a type T and
        /// position to which it was set.
        /// </param>
        public LambdaSettableMapView(Func<int> widthGetter, Func<int> heightGetter, Func<Point, T> valueGetter,
                                     Action<Point, T> valueSetter)
        {
            _widthGetter = widthGetter;
            _heightGetter = heightGetter;
            _valueGetter = valueGetter;
            _valueSetter = valueSetter;
        }

        /// <inheritdoc />
        public override int Height => _heightGetter();

        /// <inheritdoc />
        public override int Width => _widthGetter();

        /// <summary>
        /// Given a position, returns/sets the "value" associated with that location, by calling the
        /// valueGetter/valueSetter functions provided at construction.
        /// </summary>
        /// <param name="pos">Location to retrieve/set the value for.</param>
        /// <returns>
        /// The "value" associated with the provided location, according to the valueGetter function
        /// provided at construction.
        /// </returns>
        public override T this[Point pos]
        {
            get => _valueGetter(pos);
            set => _valueSetter(pos, value);
        }

        /// <summary>
        /// Returns a string representation of the LambdaSettableMapView.
        /// </summary>
        /// <returns>A string representation of the LambdaSettableMapView.</returns>
        public override string ToString() => this.ExtendToString();

        /// <summary>
        /// Returns a string representation of the map view, using <paramref name="elementStringifier" />
        /// to determine what string represents each value.
        /// </summary>
        /// <remarks>
        /// This could be used, for example, on an LambdaSettableMapView of boolean values, to output '#' for
        /// false values, and '.' for true values.
        /// </remarks>
        /// <param name="elementStringifier">
        /// Function determining the string representation of each element.
        /// </param>
        /// <returns>A string representation of the LambdaSettableMapView.</returns>
        public string ToString(Func<T, string> elementStringifier)
            => this.ExtendToString(elementStringifier: elementStringifier);

        /// <summary>
        /// Prints the values in the LambdaSettableMapView, using the function specified to turn elements into
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
        /// <returns>A string representation of the LambdaSettableMapView.</returns>
        public string ToString(int fieldSize, Func<T, string>? elementStringifier = null)
            => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);
    }
}
