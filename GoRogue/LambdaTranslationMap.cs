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
	class LambdaTranslationMap<T1, T2> : TranslationMap<T1, T2>
	{
		private Func<T1, T2> _getter;
		private Func<T2, T1> _setter;

		[Obsolete("Use the constructor with Func<> arguments", true)]
		public LambdaTranslationMap(ISettableMapView<T1> baseMap) : base(baseMap)
		{
			throw new Exception("LambdaTranslationMap must be constructed with getter and setter delegate arguments");
		}

		[Obsolete("Use the constructor with Func<> arguments", true)]
		public LambdaTranslationMap(ISettableMapView<T1> baseMap, ISettableMapView<T2> overlay) : base(baseMap)
		{
			throw new Exception("LambdaTranslationMap must be constructed with getter and setter delegate arguments");
		}

		/// <summary>
		/// Constructor. Takes an existing map view to create a view from.
		/// </summary>
		/// <param name="baseMap">Your underlying map data.</param>
		/// <param name="getter">The TranslateGet implementation.</param>
		/// <param name="setter">The TranslateSet implementation.</param>
		public LambdaTranslationMap(ISettableMapView<T1> baseMap, Func<T1, T2> getter, Func<T2, T1> setter)
			: base(baseMap)
		{
			if (getter == null)
				throw new ArgumentNullException(nameof(getter));
			if (setter == null)
				throw new ArgumentNullException(nameof(setter));
			_getter = getter;
			_setter = setter;
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
			if (getter == null)
				throw new ArgumentNullException(nameof(getter));
			if (setter == null)
				throw new ArgumentNullException(nameof(setter));
			_getter = getter;
			_setter = setter;
			ApplyOverlay(overlay);
		}

		protected override T2 TranslateGet(T1 value)
		{
			return _getter(value);
		}

		protected override T1 TranslateSet(T2 value)
		{
			return _setter(value);
		}
	}
}
