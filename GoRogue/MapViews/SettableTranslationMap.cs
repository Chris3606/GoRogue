using System;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Settable map view class capable of taking complex data and providing a simpler view of it.
    /// For a version that provides only "get" functionality, see <see cref="TranslationMap{T1, T2}" />.
    /// </summary>
    /// <remarks>
    /// See <see cref="TranslationMap{T1, T2}" />.  The use case is the same, except that this class
    /// implements <see cref="ISettableMapView{T1}" /> instead, and thus also allows you to specify
    /// set-translations via TranslateSet.
    /// </remarks>
    /// <typeparam name="T1">The type of your underlying data.</typeparam>
    /// <typeparam name="T2">The type of the data being exposed to the algorithm.</typeparam>
    [PublicAPI]
    public abstract class SettableTranslationMap<T1, T2> : SettableMapViewBase<T2>
    {
        /// <summary>
        /// Constructor. Takes an existing map view to create a view from.
        /// </summary>
        /// <param name="baseMap">Your underlying map data.</param>
        protected SettableTranslationMap(ISettableMapView<T1> baseMap) => BaseMap = baseMap;

        /// <summary>
        /// Constructor. Takes an existing map view to create a view from and applies view data to it.
        /// </summary>
        /// <remarks>
        /// Since this constructor must call TranslateSet to perform its function, do NOT
        /// call this constructor if the TranslateSet implementation depends on the derived
        /// class's constructor being completed to function properly.
        /// </remarks>
        /// <param name="baseMap">Your underlying map data.</param>
        /// <param name="overlay">
        /// The view data to apply to the map. Must have identical dimensions to <paramref name="baseMap" />.
        /// </param>
        protected SettableTranslationMap(ISettableMapView<T1> baseMap, ISettableMapView<T2> overlay)
            : this(baseMap) => this.ApplyOverlay(overlay);

        /// <summary>
        /// The underlying map.
        /// </summary>
        public ISettableMapView<T1> BaseMap { get; private set; }

        /// <inheritdoc />
        public override int Height => BaseMap.Height;

        /// <inheritdoc />
        public override int Width => BaseMap.Width;

        /// <summary>
        /// Given a position, translates and returns/sets the "value" associated with that position.
        /// The  other indexers call this indexer for its functionality, so overriding this
        /// functionality also changes those overloads.
        /// </summary>
        /// <param name="pos">Location to get/set the value for.</param>
        /// <returns>The translated "value" associated with the provided location.</returns>
        public override T2 this[Point pos]
        {
            get => TranslateGet(pos, BaseMap[pos]);
            set => BaseMap[pos] = TranslateSet(pos, value);
        }

        /// <summary>
        /// Returns a string representation of the SettableTranslationMap.
        /// </summary>
        /// <returns>A string representation of the SettableTranslationMap.</returns>
        public override string ToString() => this.ExtendToString();

        /// <summary>
        /// Returns a string representation of the map view, using <paramref name="elementStringifier" />
        /// to determine what string represents each value.
        /// </summary>
        /// <remarks>
        /// This could be used, for example, on an SettableTranslationMap of boolean values, to output '#' for
        /// false values, and '.' for true values.
        /// </remarks>
        /// <param name="elementStringifier">
        /// Function determining the string representation of each element.
        /// </param>
        /// <returns>A string representation of the SettableTranslationMap.</returns>
        public string ToString(Func<T2, string> elementStringifier)
            => this.ExtendToString(elementStringifier: elementStringifier);

        /// <summary>
        /// Prints the values in the map view, using the function specified to turn elements into
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
        /// <returns>A string representation of the SettableTranslationMap.</returns>
        public string ToString(int fieldSize, Func<T2, string>? elementStringifier = null)
            => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);

        /// <summary>
        /// Translates your map data into the view type. Takes only a value from the underlying map.
        /// If a position is also needed to perform the translation, use <see cref="TranslateGet(Point, T1)" />
        /// instead.
        /// </summary>
        /// <param name="value">The data value from your map.</param>
        /// <returns>A value of the mapped data type</returns>
        protected virtual T2 TranslateGet(T1 value) =>
            throw new NotSupportedException(
                $"{nameof(TranslateGet)}(T1) was not implemented, and {nameof(TranslateGet)}(Point, T1) was not re-implemented.  One of these two functions must be implemented.");

        /// <summary>
        /// Translates your map data into the view type. Takes a value from the underlying map and
        /// the corresponding position for that value. If a position is not needed to perform the
        /// translation, use <see cref="TranslateGet(T1)" /> instead.
        /// </summary>
        /// <param name="position">The position of the given data value from your map.</param>
        /// <param name="value">The data value from your map.</param>
        /// <returns>A value of the mapped data type</returns>
        protected virtual T2 TranslateGet(Point position, T1 value) => TranslateGet(value);

        /// <summary>
        /// Translates the view type into the appropriate form for your map data. Takes only a value
        /// from the underlying map. If a position is also needed to perform the translation, use
        /// <see cref="TranslateSet(Point, T2)" /> instead.
        /// </summary>
        /// <param name="value">A value of the mapped data type</param>
        /// <returns>The data value for your map.</returns>
        protected virtual T1 TranslateSet(T2 value) =>
            throw new NotSupportedException(
                $"{nameof(TranslateSet)}(T2) was not implemented, and {nameof(TranslateSet)}(Point, T2) was not re-implemented.  One of these two functions must be implemented.");

        /// <summary>
        /// Translates the view type into the appropriate form for your map data. Takes a value from
        /// the underlying map, and it corresponding position. If a position is not needed to perform
        /// the translation, use <see cref="TranslateSet(T2)" /> instead.
        /// </summary>
        /// <param name="position">The position of the given mapped data type.</param>
        /// <param name="value">A value of the mapped data type</param>
        /// <returns>The data value for your map.</returns>
        protected virtual T1 TranslateSet(Point position, T2 value) => TranslateSet(value);
    }
}
