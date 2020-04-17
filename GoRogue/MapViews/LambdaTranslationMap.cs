using System;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// A simple <see cref="TranslationMap{T1, T2}"/> implementation that allows you to provide a function/lambda
    /// at construction to use as the TranslateGet implementation. For a version offering "set" functionality,
    /// see <see cref="LambdaSettableTranslationMap{T1, T2}"/>.
    /// </summary>
    /// <remarks>
    /// If you have a view that simply exposes a simple property of your underlying data, it
    /// can be tedious to write a <see cref="TranslationMap{T1, T2}"/> implementation for
    /// all of them. This class simplifies that task by letting you just provide the
    /// translation method as a function/lambda, without needing to build the rest of the
    /// class infrastructure.
    /// </remarks>
    /// <typeparam name="T1">The type of your underlying data.</typeparam>
    /// <typeparam name="T2">The type of the data being exposed to the algorithm.</typeparam>
    public sealed class LambdaTranslationMap<T1, T2> : TranslationMap<T1, T2>
    {
        private readonly Func<Point, T1, T2> _getter;

        /// <summary>
        /// Constructor. Takes an existing map view to create a view from and a getter function
        /// taking only a value of type T1.
        /// </summary>
        /// <remarks>
        /// If a position is also needed to perform the translation, an overload is provided taking a
        /// corresponding function.
        /// </remarks>
        /// <param name="baseMap">Your underlying map data.</param>
        /// <param name="getter">The TranslateGet implementation.</param>
        public LambdaTranslationMap(IMapView<T1> baseMap, Func<T1, T2> getter)
            : base(baseMap)
        {
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));

            _getter = (c, t1) => getter(t1);
        }

        /// <summary>
        /// Constructor. Takes an existing map view to create a view from and a getter function
        /// taking a value of type T1 and its corresponding position.
        /// </summary>
        /// <param name="baseMap">Your underlying map data.</param>
        /// <param name="getter">The TranslateGet implementation.</param>
        public LambdaTranslationMap(IMapView<T1> baseMap, Func<Point, T1, T2> getter)
            : base(baseMap) => _getter = getter ?? throw new ArgumentNullException(nameof(getter));

        /// <summary>
        /// Translates your map data into the view type by calling the getter function specified in the
        /// class constructor.
        /// </summary>
        /// <param name="position">Position corresponding to given data value of your map.</param>
        /// <param name="value">The data value from your map.</param>
        /// <returns>A value of the mapped data type (via the getter specified in the class constructor).</returns>
        protected override T2 TranslateGet(Point position, T1 value) => _getter(position, value);
    }
}
