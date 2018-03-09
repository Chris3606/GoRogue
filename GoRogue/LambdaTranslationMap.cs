using System;

namespace GoRogue
{
	/// <summary>
	/// A simple TranslationMap implementation that allows you to provide lambdas for the translation functions.
	/// </summary>
	/// <remarks>
	/// If you have several views that simply expose a simple property of your underlying data, it could get
	/// tedious writing a TranslationMap for all of them. This class simplifies that task by letting you just
	/// provide the translation methods as delegates/lambdas without needing to build the rest of the
	/// infrastructure.
	/// </remarks>
	/// <typeparam name="T1">The type of your underlying data.</typeparam>
	/// <typeparam name="T2">The type of the data being exposed to the algorithm.</typeparam>
	public class LambdaTranslationMap<T1, T2> : TranslationMap<T1, T2>
	{
		private Func<T1, T2> _getter;
		private Func<T2, T1> _setter;

		/// <summary>
		/// Constructor. Takes an existing map view to create a view from.
		/// </summary>
		/// <param name="baseMap">Your underlying map data.</param>
		/// <param name="getter">The TranslateGet implementation.</param>
		/// <param name="setter">The TranslateSet implementation.</param>
		public LambdaTranslationMap(ISettableMapView<T1> baseMap, Func<T1, T2> getter, Func<T2, T1> setter)
			: base(baseMap)
		{
			_getter = getter ?? throw new ArgumentNullException(nameof(getter));
			_setter = setter ?? throw new ArgumentNullException(nameof(setter));
		}

		/// <summary>
		/// Constructor. Takes an existing map view to create a view from and applies view data to it.
		/// </summary>
		/// <param name="baseMap">Your underlying map data.</param>
		/// <param name="overlay">The view data to apply to the map.  Must have identical dimensions
		/// to baseMap.</param>
		/// <param name="getter">The TranslateGet implementation.</param>
		/// <param name="setter">The TranslateSet implementation.</param>
		public LambdaTranslationMap(ISettableMapView<T1> baseMap,
			ISettableMapView<T2> overlay,
			Func<T1, T2> getter,
			Func<T2, T1> setter)
			: base(baseMap)
		{
			_getter = getter ?? throw new ArgumentNullException(nameof(getter));
			_setter = setter ?? throw new ArgumentNullException(nameof(setter));

			ApplyOverlay(overlay);
		}

		/// <summary>
		/// Translates your map data into the view type by calling the lambda getter specified in the class constructor.
		/// </summary>
		/// <param name="value">The data value from your map.</param>
		/// <returns>A value of the mapped data type (via the getter specified in the class constructor).</returns>
		protected override T2 TranslateGet(T1 value) => _getter(value);

		/// <summary>
		/// Translates the view type into the appropriate form for your map data, by calling the lambda setter specified in the class constructor.
		/// </summary>
		/// <param name="value">A value of the mapped data type.</param>
		/// <returns>The data value for your map, (via the setter specified in the class constructor).</returns>
		protected override T1 TranslateSet(T2 value) => _setter(value);
	}
}
