using System;
using System.Collections.Generic;
using GoRogue.Random;

namespace GoRogue
{
    /// <summary>
    /// Static class full of miscellaneous helper methods.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Convenience function that swaps the values pointed to by x and y.
        /// </summary>
        /// <typeparam name="T">Type of values being swapped -- generally determinable implicitly by the compiler.</typeparam>
        /// <param name="lhs">Left-hand value.</param>
        /// <param name="rhs">Right-hand value.</param>
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        /// <summary>
        /// Extension method for List that implements a fisher-yates shuffle.  Modifies the list it is called on
        /// to randomly rearrange the elements therein.
        /// </summary>
        /// <remarks>
        /// Since this is an extension method, if we have a List&lt;T&gt; myList, we can simply call
        /// myList.FisherYatesShuffle(rng). However, do note that for the method to be detected properly, the namespace
        /// GoRogue (the namespace the Utility class is in) must be in a using statement.
        /// </remarks>
        /// <typeparam name="T">Type of elements in the list.</typeparam>
        /// <param name="list">List being operated on -- never specified manually as this is an extension method.</param>
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
        /// Extension method that selects and returns a random item from the list, using the rng specified.  Default
        /// for type T is returned if the list is empty.
        /// </summary>
        /// <typeparam name="T">Type of elements in the list.</typeparam>
        /// <param name="list">IList being operated on -- never specified manually as this is an extension method.</param>
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
        /// Extension method that selects and returns a random valid index from the list, using the rng specified.
        /// -1 is returned if the list is empty.
        /// </summary>
        /// <typeparam name="T">Type of elements in the list.</typeparam>
        /// <param name="list">IList being operated on -- never specified manually as this is an extension method.</param>
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
        /// Extension method for IEnumerable that converts the IEnumerable into a list.  This may be useful for any of the various methods in
        /// the GoRogue library that return IEnumerables, where you actually want to store the returned items for repeated use/iteration, etc.
        /// The function simply provides a shorter syntax to create a new list and pass in the IEnumerable to the constructor.
        /// </summary>
        /// <typeparam name="T">The type of elements in the IEnumerable.</typeparam>
        /// <param name="enumerable">The IEnumerable to convert to List -- never specified manually as this is an extension method.</param>
        /// <returns>A list containing all the items referenced by the IEnumerable value it is called on.</returns>
        static public List<T> ToList<T>(this IEnumerable<T> enumerable) => new List<T>(enumerable);
    }
}
