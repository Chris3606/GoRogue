using System;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Implementation of the <see cref="ISettableMapView{T}" /> interface that uses a
    /// 1D array to store data.
    /// </summary>
    /// <remarks>
    /// An <see cref="ArrayMap{T}" /> can be implicitly converted to its underlying 1D array,
    /// which allows exposing that array to code that works with 1D arrays.  Modifications in the array
    /// appear in the map view as well.
    /// If you need a 2D array instead of 1D, then you should use <see cref="ArrayMap2D{T}" /> instead.
    /// </remarks>
    /// <typeparam name="T">The type of value being stored.</typeparam>
    [Serializable]
    [PublicAPI]
    public sealed class ArrayMap<T> : SettableMapViewBase<T>, ICloneable, IEquatable<ArrayMap<T>>
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
        /// <param name="width">The width of the 2D area represented by <paramref name="existingArray" />.</param>
        public ArrayMap(T[] existingArray, int width)
        {
            if (existingArray.Length % width != 0)
                throw new ArgumentException($"Existing array must have length equal to {nameof(width)}*height.",
                    nameof(existingArray));

            _array = existingArray;
            Width = width;
            Height = existingArray.Length / width;
        }

        /// <summary>
        /// Performs deep copy of array map.
        /// </summary>
        /// <returns>The cloned ArrayMap.</returns>
        public object Clone()
        {
            var newObj = new ArrayMap<T>(Width, Height);

            for (var x = 0; x < Width; x++)
                for (var y = 0; y < Height; y++)
                    newObj[x, y] = _array[Point.ToIndex(x, y, Width)];

            return newObj;
        }

        /// <summary>
        /// Compares the current ArrayMap to the one given.
        /// </summary>
        /// <param name="other" />
        /// <returns>True if the given ArrayMap&lt;T&gt; with a reference to the same underlying array, false otherwise.</returns>
        public bool Equals(ArrayMap<T>? other) => !(other is null) && _array == other._array;

        /// <inheritdoc />
        public override int Height { get; }

        /// <inheritdoc />
        public override int Width { get; }

        /// <inheritdoc />
        public override T this[Point pos]
        {
            get => _array[pos.ToIndex(Width)];
            set => _array[pos.ToIndex(Width)] = value;
        }

#pragma warning disable CA2225 // The proper equivalent function is provided, however because the type is [] instead of Array the analyzer cannot determine this properly.
        /// <summary>
        /// Allows implicit conversion to 1D array.  Does not copy the underlying values.
        /// </summary>
        /// <param name="arrayMap">ArrayMap to convert.</param>
        public static implicit operator T[](ArrayMap<T> arrayMap) => arrayMap._array;
#pragma warning restore CA2225

        /// <summary>
        /// Converts to 1D array, without copying the values.  Typically using this method is unnecessary
        /// and you can use the implicit conversion defined for this type instead.
        /// </summary>
        /// <returns>The underlying ArrayMap data as a 1D array.</returns>
        public T[] ToArray() => this;

        /// <summary>
        /// Sets each element in the ArrayMap to the default for type T.
        /// </summary>
        public void SetToDefault() => Array.Clear(_array, 0, _array.Length);

        /// <summary>
        /// Compares the current ArrayMap to the object given.
        /// </summary>
        /// <param name="obj" />
        /// <returns>
        /// True if the given object is an ArrayMap&lt;T&gt; with a reference to the same underlying array, false
        /// otherwise.
        /// </returns>
        public override bool Equals(object? obj)
        {
            if (obj is ArrayMap<T> e)
                return Equals(e);

            return false;
        }

        /// <summary>
        /// Returns a hash-value for this object.
        /// </summary>
        /// <returns />
        public override int GetHashCode() => _array.GetHashCode();

        /// <summary>
        /// Compares the two ArrayMap instances.
        /// </summary>
        /// <param name="lhs" />
        /// <param name="rhs" />
        /// <returns>
        /// True if the two given ArrayMap&lt;T&gt; instances have a reference to the same underlying array, false
        /// otherwise.
        /// </returns>
        public static bool operator ==(ArrayMap<T>? lhs, ArrayMap<T>? rhs) => lhs?.Equals(rhs) ?? rhs is null;

        /// <summary>
        /// Compares the two ArrayMap instances.
        /// </summary>
        /// <param name="lhs" />
        /// <param name="rhs" />
        /// <returns>
        /// True if the two given ArrayMap&lt;T&gt; instances do NOT have a reference to the same underlying array, false
        /// otherwise.
        /// </returns>
        public static bool operator !=(ArrayMap<T>? lhs, ArrayMap<T>? rhs) => !(lhs == rhs);

        /// <summary>
        /// Returns a string representation of the 2D array.
        /// </summary>
        /// <returns>A string representation of the 2D array.</returns>
        public override string ToString() => this.ExtendToString();

        /// <summary>
        /// Returns a string representation of the map data, using <paramref name="elementStringifier" />
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
        public string ToString(Func<T, string> elementStringifier)
            => this.ExtendToString(elementStringifier: elementStringifier);

        /// <summary>
        /// Prints the values in the ArrayMap, using the function specified to turn elements into
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
        /// <returns>A string representation of the ArrayMap.</returns>
        public string ToString(int fieldSize, Func<T, string>? elementStringifier = null)
            => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);
    }
}
