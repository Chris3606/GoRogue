using System;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// A simple <see cref="TranslationGridView{T1,T2}" /> implementation that allows you to provide a function/lambda
    /// at construction to use as the <see cref="TranslationGridView{T1,T2}.TranslateGet(Point, T1)" /> implementation.
    /// For a version offering "set" functionality, see <see cref="LambdaSettableTranslationGridView{T1,T2}" />.
    /// </summary>
    /// <typeparam name="T1">The type of your underlying data.</typeparam>
    /// <typeparam name="T2">The type of the data being exposed by the grid view.</typeparam>
    [PublicAPI]
    public sealed class LambdaTranslationGridView<T1, T2> : TranslationGridView<T1, T2>
    {
        private readonly Func<Point, T1, T2> _getter;

        /// <summary>
        /// Constructor. Takes an existing grid view to create a view from and a getter function
        /// taking only a value of type T1.
        /// </summary>
        /// <remarks>
        /// If a position is also needed to perform the translation, an overload is provided taking a
        /// corresponding function.
        /// </remarks>
        /// <param name="baseGrid">Your underlying grid data.</param>
        /// <param name="getter">The TranslateGet implementation.</param>
        public LambdaTranslationGridView(IGridView<T1> baseGrid, Func<T1, T2> getter)
            : base(baseGrid)
        {
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));

            _getter = (c, t1) => getter(t1);
        }

        /// <summary>
        /// Constructor. Takes an existing grid view to create a view from and a getter function
        /// taking a value of type T1 and its corresponding position.
        /// </summary>
        /// <param name="baseGrid">Your underlying grid data.</param>
        /// <param name="getter">The TranslateGet implementation.</param>
        public LambdaTranslationGridView(IGridView<T1> baseGrid, Func<Point, T1, T2> getter)
            : base(baseGrid) => _getter = getter ?? throw new ArgumentNullException(nameof(getter));

        /// <summary>
        /// Translates your underlying data into the view type by calling the getter function specified in the
        /// class constructor.
        /// </summary>
        /// <param name="position">Position corresponding to given value from your underlying data.</param>
        /// <param name="value">The value from your underlying data.</param>
        /// <returns>A value of the mapped data type (via the getter specified in the class constructor).</returns>
        protected override T2 TranslateGet(Point position, T1 value) => _getter(position, value);
    }
}
