using GoRogue.Random;
using Troschuetz.Random;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.Linq;

namespace GoRogue
{
    /// <summary>
    /// Static class full of miscellaneous helper methods.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Extension method for List that implements a fisher-yates shuffle. Modifies the list it is
        /// called on to randomly rearrange the elements therein.
        /// </summary>
        /// <remarks>
        /// Since this is an extension method, if we have a List&lt;T&gt; myList, we can simply call
        /// myList.FisherYatesShuffle(rng). However, do note that for the method to be detected
        /// properly, the namespace GoRogue (the namespace the Utility class is in) must be in a
        /// using statement.
        /// </remarks>
        /// <typeparam name="T">Type of elements in the list.</typeparam>
        /// <param name="list">
        /// List being operated on -- never specified manually as this is an extension method.
        /// </param>
        /// <param name="rng">RNG to use.</param>
        static public void FisherYatesShuffle<T>(this List<T> list, IGenerator rng = null)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Extension method that selects and returns a random valid index from the list, using the
        /// rng specified.
        /// -1 is returned if the list is empty.
        /// </summary>
        /// <typeparam name="T">Type of elements in the list.</typeparam>
        /// <param name="list">
        /// List being operated on -- never specified manually as this is an extension method.
        /// </param>
        /// <param name="rng">RNG to use.</param>
        /// <returns>Index selected.</returns>
        static public int RandomIndex<T>(this IReadOnlyList<T> list, IGenerator rng = null)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;

            if (list.Count == 0)
                return -1;

            return rng.Next(list.Count);
        }

        
        /// <summary>
        /// Extension method that selects and returns a random item from the list, using the rng
        /// specified. Default for type T is returned if the list is empty.
        /// </summary>
        /// <typeparam name="T">Type of elements in the list.</typeparam>
        /// <param name="list">
        ///  List being operated on -- never specified manually as this is an extension method.
        /// </param>
        /// <param name="rng">RNG to use.</param>
        /// <returns>Item selected.</returns> 
        static public T RandomItem<T>(this IReadOnlyList<T> list, IGenerator rng = null)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;

            if (list.Count == 0)
                return default(T);

            return list[rng.Next(list.Count)];
        }

        /// <summary>
        /// Convenience function that swaps the values pointed to by x and y.
        /// </summary>
        /// <typeparam name="T">
        /// Type of values being swapped -- generally determinable implicitly by the compiler.
        /// </typeparam>
        /// <param name="lhs">Left-hand value.</param>
        /// <param name="rhs">Right-hand value.</param>
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        /// <summary>
        /// Adds an AsReadOnly method to IDictionary, similar to the AsReadOnly method of IList, that returns a read-only reference
        /// to the dictionary.
        /// </summary>
        /// <typeparam name="K">Type of keys of the dictionary.</typeparam>
        /// <typeparam name="V">Type of values of the dictionary.</typeparam>
        /// <param name="dictionary">Dictionary to create a read-only reference to -- never specified manually as this is an extension method.</param>
        /// <returns>A ReadOnlyDictionary instance for the specified dictionary.</returns>
        public static ReadOnlyDictionary<K, V> AsReadOnly<K, V>(this IDictionary<K, V> dictionary)
            => new ReadOnlyDictionary<K, V>(dictionary);

        /// <summary>
        /// Extension method for generic IEnumerable/List collection allowing printing the contents.  Takes the characters to surround the list in,
        /// the method to use to get the string representation of each element (defaulting to the ToString function of type T),
        /// and the characters to use to separate the list elements.
        /// </summary>
        /// <remarks> Defaults to a representation looking something like [elem1, elem2, elem3].</remarks>
        /// <typeparam name="T">Type of elements in the IEnumerable.</typeparam>
        /// <param name="enumerable">IEnumerable to stringify -- never specified manually as this is an extension method.</param>
        /// <param name="begin">Character(s) that should precede the list elements.</param>
        /// <param name="elementStringifier">Function to use to get the string representation of each element. Null uses the ToString function of type T.</param>
        /// <param name="separator">Characters to separate the list by.</param>
        /// <param name="end">Character(s) that should follow the list elements.</param>
        /// <returns>A string representation of the IEnumerable.</returns>
        public static string ExtendToString<T>(this IEnumerable<T> enumerable, string begin = "[", Func<T, string> elementStringifier = null, string separator = ", ", string end = "]")
        {
            if (elementStringifier == null)
                elementStringifier = (T obj) => obj.ToString();

            string result = begin;
            bool first = true;
            foreach (var item in enumerable)
            {
                if (first)
                    first = false;
                else
                    result += separator;

                result += elementStringifier(item);
            }
            result += end;

            return result;
        }

        /// <summary>
        /// Extension method for sets allowing printing the contents.  Takes the characters to surround the set elements in,
        /// the method to use to get the string representation of each element (defaulting to the ToString function of type T),
        /// and the characters to use to separate the set elements.
        /// </summary>
        /// <remarks> Defaults to a representation looking something like set(elem1, elem2, elem3).</remarks>
        /// <typeparam name="T">Type of elements in the IEnumerable.</typeparam>
        /// <param name="set">Set to stringify -- never specified manually as this is an extension method.</param>
        /// <param name="begin">Character(s) that should precede the set elements.</param>
        /// <param name="elementStringifier">Function to use to get the string representation of each element. Null uses the ToString function of type T.</param>
        /// <param name="separator">Characters to separate the list by.</param>
        /// <param name="end">Character(s) that should follow the list elements.</param>
        /// <returns>A string representation of the ISet.</returns>
        public static string ExtendToString<T>(this ISet<T> set, string begin = "set(", Func<T, string> elementStringifier = null, string separator = ", ", string end = ")")
            => ExtendToString((IEnumerable<T>)set, begin, elementStringifier, separator, end);

        /// <summary>
        /// Extension method for dictionaries allowing printing the contents.  Takes the characters to surround the dictionary elements in,
        /// the method to use to get the string representation of each key and value (defaulting to the ToString function of the types),
        /// and the characters to use to separate the value from keys, and the key-value pairs.
        /// </summary>
        /// <remarks> Defaults to a representation looking something like {key : value, key : value}.</remarks>
        /// <typeparam name="K">Type of keys of the dictionary.</typeparam>
        /// <typeparam name="V">Type of values of the dictionary.</typeparam>
        /// <param name="dictionary">Dictionary to stringify -- never specified manually as this is an extension method.</param>
        /// <param name="begin">Character(s) that should precede the dictionary elements.</param>
        /// <param name="keyStringifier">Function to use to get the string representation of each key. Null uses the ToString function of type K.</param>
        /// <param name="valueStringifier">Function to use to get the string representation of each value. Null uses the ToString function of type V.</param>
        /// <param name="kvSeparator">Characters to separate each value from its key.</param>
        /// <param name="pairSeparator">Characters to separate each key-value pair from the next.</param>
        /// <param name="end">Character(s) that should follow the dictionary elements.</param>
        /// <returns>A string representation of the IDictionary.</returns>
        public static string ExtendToString<K, V>(this IDictionary<K,V> dictionary, string begin = "{", Func<K, string> keyStringifier = null,
                                                   Func<V, string> valueStringifier = null, string kvSeparator = " : ", string pairSeparator = ", ", string end = "}")
        {
            if (keyStringifier == null)
                keyStringifier = (K obj) => obj.ToString();

            if (valueStringifier == null)
                valueStringifier = (V obj) => obj.ToString();

            string result = begin;
            bool first = true;
            foreach (var kvPair in dictionary)
            {
                if (first)
                    first = false;
                else
                    result += pairSeparator;

                result += keyStringifier(kvPair.Key) + kvSeparator + valueStringifier(kvPair.Value);
            }

            result += end;

            return result;
        }

        /// <summary>
        /// Extension method for 2D arrays allowing printing the contents.  Takes characters to surround the array, and each row, the method used
        /// to get the string representation of each element (defaulting to the ToString function of type T), and separation characters for each element and row.
        /// </summary>
        /// <typeparam name="T">Type of elements in the 2D array.</typeparam>
        /// <param name="array">The array to stringify -- never specified manually as this is an extension method.</param>
        /// <param name="begin">Character(s) that should precede the array.</param>
        /// <param name="beginRow">Character(s) that should precede each row.</param>
        /// <param name="elementStringifier">Function to use to get the string representation of each value.  Null uses the ToString function of type T.</param>
        /// <param name="rowSeparator">Character(s) to separate each row from the next.</param>
        /// <param name="elementSeparator">Character(s) to separate each element from the next.</param>
        /// <param name="endRow">Character(s) that should follow each row.</param>
        /// <param name="end">Character(s) that should follow the array.</param>
        /// <returns>A string representation of the 2D array.</returns>
        public static string ExtendToString<T>(this T[,] array, string begin = "[\n", string beginRow = "\t[", Func<T, string> elementStringifier = null,
                                                 string rowSeparator = ",\n", string elementSeparator = ", ", string endRow = "]", string end = "\n]")
        {
            if (elementStringifier == null)
                elementStringifier = (T obj) => obj.ToString();

            string result = begin;
            for (int x = 0; x < array.GetLength(0); x++)
            {
                result += beginRow;
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    result += elementStringifier(array[x, y]);
                    if (y != array.GetLength(1) - 1) result += elementSeparator;
                }

                result += endRow;
                if (x != array.GetLength(0) - 1) result += rowSeparator;
            }

            result += end;
            return result;
        }

        /// <summary>
        /// Extension method for 2D arrays allowing printing the contents, as if the array represents a grid-map.
        /// This differs from ExtendToString in that this method prints the grid where array[x+1, y] is printed to the
        /// RIGHT of array[x, y], rather than BELOW it.  Effectively it assumes the indexes being used are grid/coordinate
        /// plane coordinates.  Takes characters to surround the array, and each row, the method used
        /// to get the string representation of each element (defaulting to the ToString function of type T), and separation characters for each element and row.
        /// </summary>
        /// <typeparam name="T">Type of elements in the 2D array.</typeparam>
        /// <param name="array">The array to stringify -- never specified manually as this is an extension method.</param>
        /// <param name="begin">Character(s) that should precede the array.</param>
        /// <param name="beginRow">Character(s) that should precede each row.</param>
        /// <param name="elementStringifier">Function to use to get the string representation of each value.  Null uses the ToString function of type T.</param>
        /// <param name="rowSeparator">Character(s) to separate each row from the next.</param>
        /// <param name="elementSeparator">Character(s) to separate each element from the next.</param>
        /// <param name="endRow">Character(s) that should follow each row.</param>
        /// <param name="end">Character(s) that should follow the array.</param>
        /// <returns>A string representation of the 2D array, as if the array is a 2D grid-based map.</returns>
        public static string ExtendToStringGrid<T>(this T[,] array, string begin = "", string beginRow = "", Func<T, string> elementStringifier = null,
                                                      string rowSeparator = "\n", string elementSeparator = " ", string endRow = "", string end = "")
        {
            if (elementStringifier == null)
                elementStringifier = (T obj) => obj.ToString();

            string result = begin;
            for (int y = 0; y < array.GetLength(1); y++)
            {
                result += beginRow;
                for (int x = 0; x < array.GetLength(0); x++)
                {
                    result += elementStringifier(array[x, y]);
                    if (x != array.GetLength(0) - 1) result += elementSeparator;
                }

                result += endRow;
                if (y != array.GetLength(1) - 1) result += rowSeparator;
            }

            result += end;

            return result;
        }

        /// <summary>
        /// Extension method for 2D arrays allowing printing the contents, as if the array represents a grid-map.
        /// This differs from ExtendToString in that this method prints the grid where array[x+1, y] is printed to the
        /// RIGHT of array[x, y], rather than BELOW it.  Effectively it assumes the indexes being used are grid/coordinate
        /// plane coordinates.  Takes the size of the field to give each element, characters to surround the array,
        /// and each row, the method used to get the string representation of each element (defaulting to the ToString
        /// function of type T), and separation characters for each element and row.
        /// </summary>
        /// <typeparam name="T">Type of elements in the 2D array.</typeparam>
        /// <param name="array">The array to stringify -- never specified manually as this is an extension method.</param>
        /// <param name="fieldSize">The amount of space each element should take up in characters.  A positive number aligns the text
        /// to the right of the space, while a negative number aligns the text to the left.</param>
        /// <param name="begin">Character(s) that should precede the array.</param>
        /// <param name="beginRow">Character(s) that should precede each row.</param>
        /// <param name="elementStringifier">Function to use to get the string representation of each value.  Null uses the ToString function of type T.</param>
        /// <param name="rowSeparator">Character(s) to separate each row from the next.</param>
        /// <param name="elementSeparator">Character(s) to separate each element from the next.</param>
        /// <param name="endRow">Character(s) that should follow each row.</param>
        /// <param name="end">Character(s) that should follow the array.</param>
        /// <returns>A string representation of the 2D array, as if the array is a 2D grid-based map.</returns>
        public static string ExtendToStringGrid<T>(this T[,] array, int fieldSize, string begin = "", string beginRow = "", Func<T, string> elementStringifier = null,
                                                      string rowSeparator = "\n", string elementSeparator = " ", string endRow = "", string end = "")
        {
            if (elementStringifier == null)
                elementStringifier = (T obj) => obj.ToString();

            string result = begin;
            for (int y = 0; y < array.GetLength(1); y++)
            {
                result += beginRow;
                for (int x = 0; x < array.GetLength(0); x++)
                {
                    result += string.Format($"{{0, {fieldSize}}} ", elementStringifier(array[x, y]));
                    if (x != array.GetLength(0) - 1) result += elementSeparator;
                }

                result += endRow;
                if (y != array.GetLength(1) - 1) result += rowSeparator;
            }

            result += end;

            return result;
        }

        /// <summary>
        /// "Multiplies", aka repeats, a given string the given number of times.
        /// </summary>
        /// <param name="str">String to repeat.  Never specified manually since this is an extension method.</param>
        /// <param name="numTimes">The number of times to repeat the string.</param>
        /// <returns>The string str repeated numTimes times.</returns>
        public static string Multiply(this string str, int numTimes) => String.Concat(Enumerable.Repeat(str, numTimes));
    }
}