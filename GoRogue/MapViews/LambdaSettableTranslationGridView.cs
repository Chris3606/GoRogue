using System;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// A simple <see cref="SettableTranslationGridView{T1,T2}" /> implementation that allows you to provide
    /// functions/lambdas for the translation functions. For a version offering only "get" functionality,
    /// see <see cref="LambdaTranslationGridView{T1,T2}" />.
    /// </summary>
    /// <typeparam name="T1">The type of your underlying data.</typeparam>
    /// <typeparam name="T2">The type of the data being exposed by the grid view.</typeparam>
    [PublicAPI]
    public sealed class LambdaSettableTranslationGridView<T1, T2> : SettableTranslationGridView<T1, T2>
    {
        private readonly Func<Point, T1, T2> _getter;
        private readonly Func<Point, T2, T1> _setter;

        /// <summary>
        /// Constructor. Takes an existing grid view to create a view from, and getter/setter
        /// functions taking only a value from the underlying representation.
        /// </summary>
        /// <remarks>
        /// If a position is also needed to perform the translation, an overload is provided taking
        /// corresponding functions.
        /// </remarks>
        /// <param name="baseGrid">Your underlying grid data.</param>
        /// <param name="getter">The TranslateGet implementation.</param>
        /// <param name="setter">The TranslateSet implementation.</param>
        public LambdaSettableTranslationGridView(ISettableGridView<T1> baseGrid, Func<T1, T2> getter, Func<T2, T1> setter)
            : base(baseGrid)
        {
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));
            if (setter == null)
                throw new ArgumentNullException(nameof(setter));

            _getter = (pos, t1) => getter(t1);
            _setter = (pos, t2) => setter(t2);
        }

        /// <summary>
        /// Constructor. Takes an existing grid view to create a view from, and getter/setter
        /// functions taking a map value and its corresponding position.
        /// </summary>
        /// <param name="baseGrid">Your underlying grid data.</param>
        /// <param name="getter">The TranslateGet implementation.</param>
        /// <param name="setter">The TranslateSet implementation.</param>
        public LambdaSettableTranslationGridView(ISettableGridView<T1> baseGrid, Func<Point, T1, T2> getter,
                                            Func<Point, T2, T1> setter)
            : base(baseGrid)
        {
            _getter = getter ?? throw new ArgumentNullException(nameof(getter));
            _setter = setter ?? throw new ArgumentNullException(nameof(setter));
        }

        /// <summary>
        /// Constructor. Takes an existing grid view to create a view from and applies view data to it.
        /// </summary>
        /// <param name="baseGrid">Your underlying grid data.</param>
        /// <param name="overlay">
        /// The view data to apply to the underlying representation. Must have identical dimensions to
        /// <paramref name="baseGrid" />.
        /// </param>
        /// <param name="getter">The TranslateGet implementation.</param>
        /// <param name="setter">The TranslateSet implementation.</param>
        public LambdaSettableTranslationGridView(ISettableGridView<T1> baseGrid, ISettableGridView<T2> overlay,
                                            Func<T1, T2> getter, Func<T2, T1> setter)
            : this(baseGrid, getter, setter) => this.ApplyOverlay(overlay);

        /// <summary>
        /// Constructor. Takes an existing grid view to create a view from and applies view data to it.
        /// </summary>
        /// <param name="baseGrid">Your underlying grid data.</param>
        /// <param name="overlay">
        /// The view data to apply to the underlying representation. Must have identical dimensions to
        /// <paramref name="baseGrid" />.
        /// </param>
        /// <param name="getter">The TranslateGet implementation.</param>
        /// <param name="setter">The TranslateSet implementation.</param>
        public LambdaSettableTranslationGridView(ISettableGridView<T1> baseGrid, ISettableGridView<T2> overlay,
                                            Func<Point, T1, T2> getter, Func<Point, T2, T1> setter)
            : this(baseGrid, getter, setter) => this.ApplyOverlay(overlay);

        /// <summary>
        /// Translates your grid data into the view type by calling the getter function specified in the
        /// class constructor.
        /// </summary>
        /// <param name="position">Position corresponding to given data value of your underlying representation.</param>
        /// <param name="value">The data value from your grid.</param>
        /// <returns>A value of the mapped data type (via the getter specified in the class constructor).</returns>
        protected override T2 TranslateGet(Point position, T1 value) => _getter(position, value);

        /// <summary>
        /// Translates the view type into the appropriate form for your grid data, by calling the
        /// setter function specified in the class constructor.
        /// </summary>
        /// <param name="position">Position corresponding to the given mapped data type.</param>
        /// <param name="value">A value of the mapped data type.</param>
        /// <returns>
        /// The value to apply to the underlying representation via the setter specified in the class constructor.
        /// </returns>
        protected override T1 TranslateSet(Point position, T2 value) => _setter(position, value);
    }
}
