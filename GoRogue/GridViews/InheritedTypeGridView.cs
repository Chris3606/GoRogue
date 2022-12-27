using JetBrains.Annotations;
using SadRogue.Primitives.GridViews;

namespace GoRogue.GridViews
{
    /// <summary>
    /// Grid view that translates from T1 to T2, assuming T2 inherits from T1, by simply casting.
    /// </summary>
    /// <remarks>
    /// This is useful to implement full co-variance/contra-variance with grid views, if you know that values that will be
    /// put into the map will be of a particular type.  An exception will occur if casting a value to T2 fails.
    /// </remarks>
    /// <typeparam name="T1">Base type of grid view to implement.</typeparam>
    /// <typeparam name="T2">Value inheriting from/implementing T1.</typeparam>
    [PublicAPI]
    public class InheritedTypeGridView<T1, T2> : SettableTranslationGridView<T1, T2>
        where T1 : T2
    {
        /// <inheritdoc/>
        public InheritedTypeGridView(ISettableGridView<T1> baseGrid)
            : base(baseGrid)
        { }

        /// <inheritdoc/>
        public InheritedTypeGridView(ISettableGridView<T1> baseGrid, ISettableGridView<T2> overlay)
            : base(baseGrid, overlay)
        { }

        /// <inheritdoc/>
        protected override T2 TranslateGet(T1 value) => value;

        /// <inheritdoc/>
        protected override T1 TranslateSet(T2 value) => (T1)value!;
    }
}
