using System;

namespace GoRogue.MapViews
{
	/// <summary>
	/// Implementation of the <see cref="ISettableMapView{T}"/> interface that uses a
	/// 2D array to store data.
	/// </summary>
	/// <remarks>
	/// An <see cref="ArrayMap2D{T}"/> can be implicitly converted to its underlying 2D array,
	/// which allows exposing that array to code that works with 2D arrays.  Modifications in the array
	/// appear in the map view as well.
	/// 
	/// If you need a 1D array instead of 2D, then you should use <see cref="ArrayMap{T}"/> instead.
	/// </remarks>
	/// <typeparam name="T">The type of value being stored.</typeparam>
	[Serializable]
	public sealed class ArrayMap2D<T> : ISettableMapView<T>, ICloneable, IEquatable<ArrayMap2D<T>>
	{
		private readonly T[,] _array;

		/// <summary>
		/// Constructor. Takes width and height of array.
		/// </summary>
		/// <param name="width">Width of array.</param>
		/// <param name="height">Height of array.</param>
		public ArrayMap2D(int width, int height)
			: this(new T[width, height])
		{ }

		/// <summary>
		/// Constructor.  Takes an existing 2D array to use as the underlying data structure.
		/// </summary>
		/// <param name="existingArray">An existing 2D array to use as the data structure.</param>
		public ArrayMap2D(T[,] existingArray)
		{
			_array = existingArray;
		}

		/// <summary>
		/// The height of the map.
		/// </summary>
		public int Height => _array.GetLength(1);

		/// <summary>
		/// The width of the map.
		/// </summary>
		public int Width => _array.GetLength(0);

		/// <summary>
		/// Given an 1-dimensional index, returns/sets the value associated with the corresponding position
		/// in the underlying 2D array.
		/// </summary>
		/// <param name="index1D">1D index of location to retrieve the "value" for.</param>
		/// <returns>The "value" associated with the given location.</returns>
		public T this[int index1D]
		{
			get => _array[Coord.ToXValue(index1D, Width), Coord.ToYValue(index1D, Width)];
			set => _array[Coord.ToXValue(index1D, Width), Coord.ToYValue(index1D, Width)] = value;
		}

		/// <summary>
		/// Given an X and Y value, returns/sets the "value" associated with that location.
		/// </summary>
		/// <param name="x">X-value of location.</param>
		/// <param name="y">Y-value of location.</param>
		/// <returns>The "value" associated with that location.</returns>
		public T this[int x, int y]
		{
			get => _array[x, y];
			set => _array[x, y] = value;
		}

		/// <summary>
		/// Given a position, returns/sets the "value" associated with that location.
		/// </summary>
		/// <param name="pos">Location to get/set the value for.</param>
		/// <returns>The "value" associated with the provided location.</returns>
		public T this[Coord pos]
		{
			get => _array[pos.X, pos.Y];
			set => _array[pos.X, pos.Y] = value;
		}

		/// <summary>
		/// Performs deep copy of array map.
		/// </summary>
		/// <returns>The cloned ArrayMap2D.</returns>
		public object Clone()
		{
			var newObj = new ArrayMap2D<T>(Width, Height);

			for (int x = 0; x < Width; x++)
				for (int y = 0; y < Height; y++)
					newObj[x, y] = _array[x, y];

			return newObj;
		}

		/// <summary>
		/// Allows implicit conversion to 2D array.
		/// </summary>
		/// <param name="arrayMap">The ArrayMap2D to convert.</param>
		public static implicit operator T[,] (ArrayMap2D<T> arrayMap) => arrayMap._array;


		/// <summary>
		/// Sets each element in the ArrayMap to the default for type T.
		/// </summary>
		public void SetToDefault() => Array.Clear(_array, 0, _array.Length);

		/// <summary>
		/// Compares the current ArrayMap2D to the object given.
		/// </summary>
		/// <param name="obj"/>
		/// <returns>True if the given object is an ArrayMap2D&lt;T&gt; with a reference to the same underlying array, false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj is ArrayMap2D<T> e)
				return Equals(e);

			return false;
		}

		/// <summary>
		/// Compares the current ArrayMap2D to the one given.
		/// </summary>
		/// <param name="other"/>
		/// <returns>True if the given ArrayMap2D&lt;T&gt; with a reference to the same underlying array, false otherwise.</returns>
		public bool Equals(ArrayMap2D<T> other) => !ReferenceEquals(other, null) && _array == other._array;

		/// <summary>
		/// Returns a hash-value for this object.
		/// </summary>
		/// <returns/>
		public override int GetHashCode() => _array.GetHashCode();

		/// <summary>
		/// Compares the two ArrayMap2D instances.
		/// </summary>
		/// <param name="lhs"/>
		/// <param name="rhs"/>
		/// <returns>True if the two given ArrayMap2D&lt;T&gt; instances have a reference to the same underlying array, false otherwise.</returns>
		public static bool operator ==(ArrayMap2D<T> lhs, ArrayMap2D<T> rhs) => lhs?.Equals(rhs) ?? rhs is null;

		/// <summary>
		/// Compares the two ArrayMap2D instances.
		/// </summary>
		/// <param name="lhs"/>
		/// <param name="rhs"/>
		/// <returns>True if the two given ArrayMap2D&lt;T&gt; instances do NOT have a reference to the same underlying array, false otherwise.</returns>
		public static bool operator !=(ArrayMap2D<T> lhs, ArrayMap2D<T> rhs) => !(lhs == rhs);

		/// <summary>
		/// Returns a string representation of the 2D array.
		/// </summary>
		/// <returns>A string representation of the 2D array.</returns>
		public override string ToString() => this.ExtendToString();

		/// <summary>
		/// Returns a string representation of the 2D array, using the <paramref name="elementStringifier"/>
		/// function given to determine what string represents which value.
		/// </summary>
		/// <remarks>
		/// This could be used, for example, on an ArrayMap2D of boolean values, to output '#' for
		/// false values, and '.' for true values.
		/// </remarks>
		/// <param name="elementStringifier">
		/// Function determining the string representation of each element.
		/// </param>
		/// <returns>A string representation of the 2D array.</returns>
		public string ToString(Func<T, string> elementStringifier) => this.ExtendToString(elementStringifier: elementStringifier);

		/// <summary>
		/// Prints the values in the ArrayMap2D, using the function specified to turn elements into
		/// strings, and using the "field length" specified.
		/// </summary>
		/// <remarks>
		/// Each element of type T will have spaces added to cause it to take up exactly
		/// <paramref name="fieldSize"/> characters, provided <paramref name="fieldSize"/> 
		/// is less than the length of the element's string represention.
		/// </remarks>
		/// <param name="fieldSize">
		/// The size of the field to give each value.  A positive-number
		/// right-aligns the text within the field, while a negative number left-aligns the text.
		/// </param>
		/// <param name="elementStringifier">
		/// Function to use to convert each element to a string. null defaults to the ToString
		/// function of type T.
		/// </param>
		/// <returns>A string representation of the ArrayMap2D.</returns>
		public string ToString(int fieldSize, Func<T, string> elementStringifier = null) => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);
	}
}
