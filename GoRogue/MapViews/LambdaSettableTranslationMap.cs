using System;

namespace GoRogue.MapViews
{
	/// <summary>
	/// A simple TranslationMap implementation that allows you to provide lambdas for the translation
	/// functions. For a version offering only "get" functionality, see LambdaTranslationMap.
	/// </summary>
	/// <remarks>
	/// If you have several views that simply expose a simple property of your underlying data, it
	/// could get tedious writing a TranslationMap for all of them. This class simplifies that task
	/// by letting you just provide the translation methods as delegates/lambdas without needing to
	/// build the rest of the infrastructure.
	/// </remarks>
	/// <typeparam name="T1">The type of your underlying data.</typeparam>
	/// <typeparam name="T2">The type of the data being exposed to the algorithm.</typeparam>
	public sealed class LambdaSettableTranslationMap<T1, T2> : SettableTranslationMap<T1, T2>
	{
		private Func<Coord, T1, T2> _getter;
		private Func<Coord, T2, T1> _setter;

		/// <summary>
		/// Constructor. Takes an existing map view to create a view from, and getter/setter functions taking only a map value.
		/// </summary>
		/// <param name="baseMap">Your underlying map data.</param>
		/// <param name="getter">The TranslateGet implementation.</param>
		/// <param name="setter">The TranslateSet implementation.</param>
		public LambdaSettableTranslationMap(ISettableMapView<T1> baseMap, Func<T1, T2> getter, Func<T2, T1> setter)
			: base(baseMap)
		{
			if (getter == null)
				throw new ArgumentNullException(nameof(getter));
			if (setter == null)
				throw new ArgumentNullException(nameof(setter));

			_getter = (pos, t1) => getter(t1);
			_setter = (pos, t2) => setter(t2);
		}

		/// <summary>
		/// Constructor. Takes an existing map view to create a view from, and getter/setter functions taking a map value and its corresponding position.
		/// </summary>
		/// <param name="baseMap">Your underlying map data.</param>
		/// <param name="getter">The TranslateGet implementation.</param>
		/// <param name="setter">The TranslateSet implementation.</param>
		public LambdaSettableTranslationMap(ISettableMapView<T1> baseMap, Func<Coord, T1, T2> getter, Func<Coord, T2, T1> setter)
			: base(baseMap)
		{
			_getter = getter ?? throw new ArgumentException(nameof(getter));
			_setter = setter ?? throw new ArgumentException(nameof(setter));
		}

		/// <summary>
		/// Constructor. Takes an existing map view to create a view from and applies view data to it.
		/// </summary>
		/// <param name="baseMap">Your underlying map data.</param>
		/// <param name="overlay">
		/// The view data to apply to the map. Must have identical dimensions to baseMap.
		/// </param>
		/// <param name="getter">The TranslateGet implementation.</param>
		/// <param name="setter">The TranslateSet implementation.</param>
		public LambdaSettableTranslationMap(ISettableMapView<T1> baseMap, ISettableMapView<T2> overlay, Func<T1, T2> getter, Func<T2, T1> setter)
			: this(baseMap, getter, setter)
		{
			this.ApplyOverlay(overlay);
		}

		/// <summary>
		/// Constructor. Takes an existing map view to create a view from and applies view data to it.
		/// </summary>
		/// <param name="baseMap">Your underlying map data.</param>
		/// <param name="overlay">
		/// The view data to apply to the map. Must have identical dimensions to baseMap.
		/// </param>
		/// <param name="getter">The TranslateGet implementation.</param>
		/// <param name="setter">The TranslateSet implementation.</param>
		public LambdaSettableTranslationMap(ISettableMapView<T1> baseMap, ISettableMapView<T2> overlay, Func<Coord, T1, T2> getter, Func<Coord, T2, T1> setter)
			: this(baseMap, getter, setter)
		{
			this.ApplyOverlay(overlay);
		}

		/// <summary>
		/// Translates your map data into the view type by calling the lambda getter specified in the
		/// class constructor.
		/// </summary>
		/// <param name="position">Position corresponding to given data value of your map. </param>
		/// <param name="value">The data value from your map.</param>
		/// <returns>A value of the mapped data type (via the getter specified in the class constructor).</returns>
		protected override T2 TranslateGet(Coord position, T1 value) => _getter(position, value);

		/// <summary>
		/// Translates the view type into the appropriate form for your map data, by calling the
		/// lambda setter specified in the class constructor.
		/// </summary>
		/// <param name="position">Position corresponding to the given mapped data type.</param>
		/// <param name="value">A value of the mapped data type.</param>
		/// <returns>The data value for your map, (via the setter specified in the class constructor).</returns>
		protected override T1 TranslateSet(Coord position, T2 value) => _setter(position, value);
	}
}