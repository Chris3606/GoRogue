using GoRogue.Random;
using System.Collections.Generic;
using System;

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
        static public void FisherYatesShuffle<T>(this List<T> list, IRandom rng = null)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n);
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
        /// IList being operated on -- never specified manually as this is an extension method.
        /// </param>
        /// <param name="rng">RNG to use.</param>
        /// <returns>Index selected.</returns>
        static public int RandomIndex<T>(this IList<T> list, IRandom rng = null)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;

            if (list.Count == 0)
                return -1;

            return rng.Next(list.Count - 1);
        }

        /// <summary>
        /// Extension method that selects and returns a random valid index from the list, using the
        /// rng specified.
        /// -1 is returned if the list is empty.
        /// </summary>
        /// <typeparam name="T">Type of elements in the list.</typeparam>
        /// <param name="list">
        /// Read-only list being operated on -- never specified manually as this is an extension method.
        /// </param>
        /// <param name="rng">RNG to use.</param>
        /// <returns>Index selected.</returns>
        static public int RandomIndex<T>(this IReadOnlyList<T> list, IRandom rng = null)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;

            if (list.Count == 0)
                return -1;

            return rng.Next(list.Count - 1);
        }

        /// <summary>
        /// Extension method that selects and returns a random item from the list, using the rng
        /// specified. Default for type T is returned if the list is empty.
        /// </summary>
        /// <typeparam name="T">Type of elements in the list.</typeparam>
        /// <param name="list">
        /// IList being operated on -- never specified manually as this is an extension method.
        /// </param>
        /// <param name="rng">RNG to use.</param>
        /// <returns>Item selected.</returns>
        static public T RandomItem<T>(this IList<T> list, IRandom rng = null)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;

            if (list.Count == 0)
                return default(T);

            return list[rng.Next(list.Count - 1)];
        }

        
        /// <summary>
        /// Extension method that selects and returns a random item from the list, using the rng
        /// specified. Default for type T is returned if the list is empty.
        /// </summary>
        /// <typeparam name="T">Type of elements in the list.</typeparam>
        /// <param name="list">
        /// Read-only list being operated on -- never specified manually as this is an extension method.
        /// </param>
        /// <param name="rng">RNG to use.</param>
        /// <returns>Item selected.</returns> 
        static public T RandomItem<T>(this IReadOnlyList<T> list, IRandom rng = null)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;

            if (list.Count == 0)
                return default(T);

            return list[rng.Next(list.Count - 1)];
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
        /// Extension method for generic IEnumerable/list allowing printing the contents.  Takes the characters to surround the list in,
        /// the method to use to get the string representation of each element (defaulting to the ToString function of type T),
        /// and the characters to use to separate the list elements.
        /// </summary>
        /// <remarks> Defaults to a representation looking something like [elem1, elem2, elem3].</remarks>
        /// <typeparam name="T">Type of elements in the IEnumerable.</typeparam>
        /// <param name="enumerable">IEnumerable to print -- never specified manually as this is an extension method.</param>
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
        /// Extension method for read-only lists allowing printing the contents.  Takes the characters to surround the list in,
        /// the method to use to get the string representation of each element (defaulting to the ToString function of type T),
        /// and the characters to use to separate the list elements.
        /// </summary>
        /// <remarks> Defaults to a representation looking something like (elem1, elem2, elem3).</remarks>
        /// <typeparam name="T">Type of elements in the IEnumerable.</typeparam>
        /// <param name="list">Read-only list to print -- never specified manually as this is an extension method.</param>
        /// <param name="begin">Character(s) that should precede the list elements.</param>
        /// <param name="elementStringifier">Function to use to get the string representation of each element. Null uses the ToString function of type T.</param>
        /// <param name="separator">Characters to separate the list by.</param>
        /// <param name="end">Character(s) that should follow the list elements.</param>
        /// <returns>A string representation of the IReadOnlyList.</returns>
        public static string ExtendToString<T>(IReadOnlyList<T> list, string begin = "(", Func<T, string> elementStringifier = null, string separator = ", ", string end = ")")
            => ExtendToString((IEnumerable<T>)list, begin, elementStringifier, separator, end);


        /// <summary>
        /// Extension method for sets allowing printing the contents.  Takes the characters to surround the set elements in,
        /// the method to use to get the string representation of each element (defaulting to the ToString function of type T),
        /// and the characters to use to separate the set elements.
        /// </summary>
        /// <remarks> Defaults to a representation looking something like set(elem1, elem2, elem3).</remarks>
        /// <typeparam name="T">Type of elements in the IEnumerable.</typeparam>
        /// <param name="set">Set to print -- never specified manually as this is an extension method.</param>
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
        /// <remarks> Defaults to a representation looking something like set(elem1, elem2, elem3).</remarks>
        /// <typeparam name="K">Type of keys of the dictionary.</typeparam>
        /// <typeparam name="V">Type of values of the dictionary.</typeparam>
        /// <param name="dictionary">Dictionary to print -- never specified manually as this is an extension method.</param>
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
        /// Extension method for IEnumerable that converts the IEnumerable into a list. This may be
        /// useful for any of the various methods in the GoRogue library that return IEnumerables,
        /// where you actually want to store the returned items for repeated use/iteration, etc. The
        /// function simply provides a shorter syntax to create a new list and pass in the
        /// IEnumerable to the constructor.
        /// </summary>
        /// <typeparam name="T">The type of elements in the IEnumerable.</typeparam>
        /// <param name="enumerable">
        /// The IEnumerable to convert to List -- never specified manually as this is an extension method.
        /// </param>
        /// <returns>
        /// A list containing all the items referenced by the IEnumerable value it is called on.
        /// </returns>
        static public List<T> ToList<T>(this IEnumerable<T> enumerable) => new List<T>(enumerable);
    }
}