﻿using System;

namespace GoRogue.MapViews
{
	/// <summary>
	/// Implementation of the <see cref="ISettableMapView{T}"/> interface that uses a
	/// 1D array to store data.
	/// </summary>
	/// <remarks>
	/// An <see cref="ArrayMap{T}"/> can be implicitly converted to its underlying 1D array,
	/// which allows exposing that array to code that works with 1D arrays.  Modifications in the array
	/// appear in the map view as well.
	/// 
	/// If you need a 2D array instead of 1D, then you should use <see cref="ArrayMap2D{T}"/> instead.
	/// </remarks>
	/// <typeparam name="T">The type of value being stored.</typeparam>
	public sealed class ArrayMap<T> : ISettableMapView<T>, ICloneable
	{
		private readonly T[] _array;

		/// <summary>
		/// Constructor. Takes width and height of array.
		/// </summary>
		/// <param name="width">Width of array.</param>
		/// <param name="height">Height of array.</param>
		public ArrayMap(int width, int height)
			: this(new T[width * height], width)
		{ }

		/// <summary>
		/// Constructor.  Takes an existing 1D array to use as the underlying array, and
		/// the width of the 2D area represented by that array.
		/// </summary>
		/// <param name="existingArray">Existing 1D array to use as the underlying array.</param>
		/// <param name="width">The width of the 2D area represented by <paramref name="existingArray"/>.</param>
		public ArrayMap(T[] existingArray, int width)
		{
			if (existingArray.Length % width != 0)
				throw new ArgumentException($"Existing array must have length equal to {nameof(width)}*height.", nameof(existingArray));

			_array = existingArray;
			Width = width;
			Height = existingArray.Length / width;
		}

		/// <summary>
		/// The height of the map.
		/// </summary>
		public int Height { get; }

		/// <summary>
		/// The width of the map.
		/// </summary>
		public int Width { get; }

		/// <summary>
		/// Returns/sets the value associated with the given index in the underlying array.
		/// </summary>
		/// <param name="index1D">1D index of location to retrieve the "value" for.</param>
		/// <returns>The "value" associated with the given location.</returns>
		public T this[int index1D]
		{
			get => _array[index1D];
			set => _array[index1D] = value;
		}

		/// <summary>
		/// Given an X and Y value, returns/sets the "value" associated with that location.
		/// </summary>
		/// <param name="x">X-value of location.</param>
		/// <param name="y">Y-value of location.</param>
		/// <returns>The "value" associated with that location.</returns>
		public T this[int x, int y]
		{
			get => _array[Coord.ToIndex(x, y, Width)];
			set => _array[Coord.ToIndex(x, y, Width)] = value;
		}

		/// <summary>
		/// Given a position, returns/sets the "value" associated with that location.
		/// </summary>
		/// <param name="pos">Location to get/set the value for.</param>
		/// <returns>The "value" associated with the provided location.</returns>
		public T this[Coord pos]
		{
			get => _array[pos.ToIndex(Width)];
			set => _array[pos.ToIndex(Width)] = value;
		}

		/// <summary>
		/// Performs deep copy of array map.
		/// </summary>
		/// <returns>The cloned ArrayMap.</returns>
		public object Clone()
		{
			var newObj = new ArrayMap<T>(Width, Height);

			for (int x = 0; x < Width; x++)
				for (int y = 0; y < Height; y++)
					newObj[x, y] = _array[Coord.ToIndex(x, y, Width)];

			return newObj;
		}

		/// <summary>
		/// Allows implicit conversion to 1D array.
		/// </summary>
		/// <param name="arrayMap">ArrayMap to convert.</param>
		public static implicit operator T[](ArrayMap<T> arrayMap) => arrayMap._array;


		/// <summary>
		/// Sets each element in the ArrayMap to the default for type T.
		/// </summary>
		public void SetToDefault() => System.Array.Clear(_array, 0, _array.Length);

		/// <summary>
		/// Returns a string representation of the 2D array.
		/// </summary>
		/// <returns>A string representation of the 2D array.</returns>
		public override string ToString() => this.ExtendToString();

		/// <summary>
		/// Returns a string representation of the map data, using <paramref name="elementStringifier"/>
		/// to determine what string represents which value.
		/// </summary>
		/// <remarks>
		/// This could be used, for example, on an ArrayMap of boolean values, to output '#' for
		/// false values, and '.' for true values.
		/// </remarks>
		/// <param name="elementStringifier">
		/// Function determining the string representation of each element.
		/// </param>
		/// <returns>A string representation of the map data.</returns>
		public string ToString(Func<T, string> elementStringifier) => this.ExtendToString(elementStringifier: elementStringifier);

		/// <summary>
		/// Prints the values in the ArrayMap, using the function specified to turn elements into
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
		/// <returns>A string representation of the ArrayMap.</returns>
		public string ToString(int fieldSize, Func<T, string> elementStringifier = null) => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);
	}
}