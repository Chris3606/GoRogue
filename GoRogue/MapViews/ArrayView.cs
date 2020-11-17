using System;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Implementation of <see cref="ISettableGridView{T}" /> that uses a 1D array to store data.
    /// </summary>
    /// <remarks>
    /// An <see cref="ArrayView{T}" /> can be implicitly converted to its underlying 1D array,
    /// which allows exposing that array to code that works with 1D arrays.  Modifications in the array
    /// appear in the map view as well.
    ///
    /// If you need a 2D array instead of 1D, then you should use <see cref="ArrayView2D{T}" /> instead.
    /// </remarks>
    /// <typeparam name="T">The type of value being stored.</typeparam>
    [PublicAPI]
    public sealed class ArrayView<T> : SettableGridViewBase<T>, ICloneable, IEquatable<ArrayView<T>>
    {
        private readonly T[] _array;

        /// <summary>
        /// Constructor. Takes width and height of array to create.
        /// </summary>
        /// <param name="width">Width of array.</param>
        /// <param name="height">Height of array.</param>
        public ArrayView(int width, int height)
            : this(new T[width * height], width)
        { }

        /// <summary>
        /// Constructor.  Takes an existing 1D array to use as the underlying array, and
        /// the width of the 2D grid represented by that array.
        /// </summary>
        /// <param name="existingArray">Existing 1D array to use as the underlying array.</param>
        /// <param name="width">The width of the 2D grid represented by <paramref name="existingArray" />.</param>
        public ArrayView(T[] existingArray, int width)
        {
            if (existingArray.Length % width != 0)
                throw new ArgumentException($"Existing array must have length equal to {nameof(width)}*height.",
                    nameof(existingArray));

            _array = existingArray;
            Width = width;
            Height = existingArray.Length / width;
        }

        /// <summary>
        /// Performs deep copy of array view.
        /// </summary>
        /// <returns>The cloned ArrayView.</returns>
        public object Clone()
        {
            var newObj = new ArrayView<T>(Width, Height);

            for (var x = 0; x < Width; x++)
                for (var y = 0; y < Height; y++)
                    newObj[x, y] = _array[Point.ToIndex(x, y, Width)];

            return newObj;
        }

        /// <summary>
        /// Compares the current ArrayView to the one given.
        /// </summary>
        /// <param name="other" />
        /// <returns>True if the given ArrayView&lt;T&gt; with a reference to the same underlying array, false otherwise.</returns>
        public bool Equals(ArrayView<T>? other) => !(other is null) && _array == other._array;

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
        /// <param name="arrayView">ArrayView to convert.</param>
        public static implicit operator T[](ArrayView<T> arrayView) => arrayView._array;
#pragma warning restore CA2225

        /// <summary>
        /// Converts to 1D array, without copying the values.  Typically using this method is unnecessary
        /// and you can use the implicit conversion defined for this type instead.
        /// </summary>
        /// <returns>The underlying ArrayView data as a 1D array.</returns>
        public T[] ToArray() => this;

        /// <summary>
        /// Sets each element in the ArrayView to the default for type T.
        /// </summary>
        public void Clear() => Array.Clear(_array, 0, _array.Length);

        /// <summary>
        /// Compares the current ArrayView to the object given.
        /// </summary>
        /// <param name="obj" />
        /// <returns>
        /// True if the given object is an ArrayView&lt;T&gt; with a reference to the same underlying array, false
        /// otherwise.
        /// </returns>
        public override bool Equals(object? obj)
        {
            if (obj is ArrayView<T> e)
                return Equals(e);

            return false;
        }

        /// <summary>
        /// Returns a hash-value for this object.
        /// </summary>
        /// <returns />
        public override int GetHashCode() => _array.GetHashCode();

        /// <summary>
        /// Compares the two ArrayView instances.
        /// </summary>
        /// <param name="lhs" />
        /// <param name="rhs" />
        /// <returns>
        /// True if the two given ArrayView&lt;T&gt; instances have a reference to the same underlying array, false
        /// otherwise.
        /// </returns>
        public static bool operator ==(ArrayView<T>? lhs, ArrayView<T>? rhs) => lhs?.Equals(rhs) ?? rhs is null;

        /// <summary>
        /// Compares the two ArrayView instances.
        /// </summary>
        /// <param name="lhs" />
        /// <param name="rhs" />
        /// <returns>
        /// True if the two given ArrayView&lt;T&gt; instances do NOT have a reference to the same underlying array, false
        /// otherwise.
        /// </returns>
        public static bool operator !=(ArrayView<T>? lhs, ArrayView<T>? rhs) => !(lhs == rhs);

        /// <summary>
        /// Returns a string representation of the grid values.
        /// </summary>
        /// <returns>A string representation of the grid values.</returns>
        public override string ToString() => this.ExtendToString();

        /// <summary>
        /// Returns a string representation of the grid values, using <paramref name="elementStringifier" />
        /// to determine what string represents each value.
        /// </summary>
        /// <param name="elementStringifier">
        /// Function determining the string representation of each value.
        /// </param>
        /// <returns>A string representation of the grid values.</returns>
        public string ToString(Func<T, string> elementStringifier)
            => this.ExtendToString(elementStringifier: elementStringifier);

        /// <summary>
        /// Returns a string representation of the grid values using the given parameters.
        /// </summary>
        /// <remarks>
        /// Each element will have spaces added to cause it to take up exactly
        /// <paramref name="fieldSize" /> characters, provided <paramref name="fieldSize" />
        /// is less than the length of the value's string representation.
        /// </remarks>
        /// <param name="fieldSize">
        /// The size of the field to give each value.  A positive-number
        /// right-aligns the text within the field, while a negative number left-aligns the text.
        /// </param>
        /// <param name="elementStringifier">
        /// Function to use to convert each value to a string. null defaults to the ToString
        /// function of type T.
        /// </param>
        /// <returns>A string representation of the grid values.</returns>
        public string ToString(int fieldSize, Func<T, string>? elementStringifier = null)
            => this.ExtendToString(fieldSize, elementStringifier: elementStringifier);
    }
}
