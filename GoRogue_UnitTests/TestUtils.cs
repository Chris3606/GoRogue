using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GoRogue_UnitTests
{
    /// <summary>
    /// Static/extension methods to help with creating test variables/enumerables for XUnit
    /// </summary>
    public static class TestUtils
    {
        /// <summary>
        /// Returns the object as a single-element enumerable.
        /// </summary>
        /// <typeparam name="T"/>
        /// <param name="obj"/>
        /// <returns>The given object as a single-element IEnumerable</returns>
        public static IEnumerable<T> ToEnumerable<T>(this T obj)
        {
            yield return obj;
        }

        /// <summary>
        /// Creates a tuple of each possible pairing of elements in the enumerables.
        /// </summary>
        /// <typeparam name="T1"/>
        /// <typeparam name="T2"/>
        /// <param name="l1"/>
        /// <param name="l2"/>
        /// <returns>Tuples containing every possible (unique) pairing of elements between the two enumerables.</returns>
        public static IEnumerable<(T1, T2)> Combinate<T1, T2>(this IEnumerable<T1> l1, IEnumerable<T2> l2)
        {
            foreach (T1 x in l1)
            {
                foreach (T2 y in l2)
                    yield return (x, y);
            }
        }

        /// <summary>
        /// Creates a tuple for each pairing of the tuples with the elements from <paramref name="l2"/>.
        /// </summary>
        /// <typeparam name="T1"/>
        /// <typeparam name="T2"/>
        /// <typeparam name="T3"/>
        /// <param name="tuples"/>
        /// <param name="l2"/>
        /// <returns>An enumerable of 3-element tuples that collectively represent every unique pairing of the initial tuples with the new values.</returns>
        public static IEnumerable<(T1, T2, T3)> Combinate<T1, T2, T3>(this IEnumerable<(T1 i1, T2 i2)> tuples, IEnumerable<T3> l2)
        {
            foreach ((T1 i1, T2 i2) in tuples)
            {
                foreach (T3 y in l2)
                    yield return (i1, i2, y);
            }
        }

        /// <summary>
        /// Creates an enumerable of the input parameters.
        /// </summary>
        /// <typeparam name="T"/></typeparam>
        /// <param name="objs"/>Input values to have within the resulting enumerable.
        /// <returns>An enumerable containing all the input parameters, in order.</returns>
        public static IEnumerable<T> Enumerable<T>(params T[] objs) => objs;
    }
}
