using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;

namespace GoRogue
{
    /// <summary>
    /// Static class containing extension helper methods for various built-in C# classes, as well as a
    /// static helper method for "swapping" references.
    /// </summary>
    [PublicAPI]
    public static class Utility
    {
        /// <summary>
        /// Adds an AsReadOnly method to <see cref="IDictionary{K, V}" />, similar to the AsReadOnly method of
        /// <see cref="IList{T}" />, that returns a read-only reference to the dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of keys of the dictionary.</typeparam>
        /// <typeparam name="TValue">Type of values of the dictionary.</typeparam>
        /// <param name="dictionary" />
        /// <returns>A ReadOnlyDictionary instance for the specified dictionary.</returns>
        public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary)
            where TKey : notnull
            => new ReadOnlyDictionary<TKey, TValue>(dictionary);

        /// <summary>
        /// "Multiplies", aka repeats, a string the given number of times.
        /// </summary>
        /// <param name="str" />
        /// <param name="numTimes">The number of times to repeat the string.</param>
        /// <returns>The current string repeated <paramref name="numTimes" /> times.</returns>
        public static string Multiply(this string str, int numTimes) => string.Concat(Enumerable.Repeat(str, numTimes));

        /// <summary>
        /// Swaps the values pointed to by <paramref name="lhs" /> and <paramref name="rhs" />.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="lhs" />
        /// <param name="rhs" />
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            (lhs, rhs) = (rhs, lhs);
        }

        /// <summary>
        /// Convenience function that yields the given item as a single-item IEnumerable.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="item" />
        /// <returns>An IEnumerable containing only the item the function is called on.</returns>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        /// <summary>
        /// Takes multiple parameters and converts them to an IEnumerable.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="values">Parameters (specified as multiple parameters to the function).</param>
        /// <returns>
        /// An IEnumerable of all of the given items, in the order they were given to the function.
        /// </returns>
        public static IEnumerable<T> Yield<T>(params T[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        /// <summary>
        /// Takes multiple enumerables of items, and flattens them into a single IEnumerable.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="lists">Lists to "flatten".</param>
        /// <returns>An IEnumerable containing the items of all the enumerables passed in.</returns>
        public static IEnumerable<T> Flatten<T>(params IEnumerable<T>[] lists)
        {
            foreach (var list in lists)
                foreach (var i in list)
                    yield return i;
        }
    }
}
