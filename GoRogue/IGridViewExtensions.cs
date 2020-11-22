using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.Random;
using JetBrains.Annotations;
using Troschuetz.Random;

// ReSharper disable once CheckNamespace
namespace SadRogue.Primitives.GridViews
{
    /// <summary>
    /// Extensions for <see cref="IGridView{T}" /> implementations that provide more basic utility functions
    /// for them.
    /// </summary>
    [PublicAPI]
    public static class GridViewExtensions
    {

        /// <summary>
        /// Gets the value at a random position in the IGridView.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="gridView" />
        /// <param name="rng">The rng to use. Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <returns>The item at a random position in the IGridView.</returns>
        public static T RandomItem<T>(this IGridView<T> gridView, IGenerator? rng = null)
            => gridView[RandomPosition(gridView, rng)];

        /// <summary>
        /// Gets the item at a random position in the grid view for which the selector returns true.
        /// Random positions will continuously be generated until one that qualifies is found.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="gridView" />
        /// <param name="selector">
        /// Function that takes a position, and the value at that position, and returns true if it is an
        /// acceptable selection, and false if not.
        /// </param>
        /// <param name="rng">The rng to use. Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <returns>
        /// The item at a random position in the IGridView for which the selector returns true.
        /// </returns>
        public static T RandomItem<T>(this IGridView<T> gridView, Func<Point, T, bool> selector, IGenerator? rng = null)
            => gridView[RandomPosition(gridView, selector, rng)];

        /// <summary>
        /// Gets a random position in the grid view, whose value in that grid view is the specified
        /// one. Random positions will continually be generated until one with the specified value is found.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="gridView" />
        /// <param name="validValue">
        /// A value to look for in the IGridView to determine whether or not a generated position is valid.
        /// </param>
        /// <param name="rng">The rng to use. Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <returns>A random position whose value in the current IGridView is equal to the one specified.</returns>
        public static Point RandomPosition<T>(this IGridView<T> gridView, T validValue, IGenerator? rng = null)
            => gridView.RandomPosition((c, i) => i?.Equals(validValue) ?? validValue == null, rng);

        /// <summary>
        /// Gets a random position in the grid view, whose value in grid view is one of the ones
        /// specified. Random positions will continually be generated until one that has one of the
        /// specified values is found.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="gridView" />
        /// <param name="validValues">
        /// A set of values to look for in the IGridView to determine whether or not a generated position
        /// is valid.
        /// </param>
        /// <param name="rng">The rng to use. Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <returns>
        /// A random position whose value in this IGridView is equal to one of the values specified.
        /// </returns>
        public static Point RandomPosition<T>(this IGridView<T> gridView, IEnumerable<T> validValues,
                                              IGenerator? rng = null)
            => gridView.RandomPosition((c, i) => validValues.Contains(i), rng);

        /// <summary>
        /// Gets a random position in the grid view, whose value in that grid view is one of the ones
        /// specified in the hash set. Random positions will continually be generated until one that
        /// has one of the specified values is found.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="gridView" />
        /// <param name="validValues">
        /// A set of values to look for in the IGridView to determine whether or not a generated position
        /// is valid.
        /// </param>
        /// <param name="rng">The rng to use. Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <returns>
        /// A random position whose value in this IGridView is equal to one of the values specified.
        /// </returns>
        public static Point RandomPosition<T>(this IGridView<T> gridView, HashSet<T> validValues, IGenerator? rng = null)
            => gridView.RandomPosition((c, i) => validValues.Contains(i), rng);

        /// <summary>
        /// Gets a random position in the grid view, whose value in that grid view is one of the ones specified in
        /// <paramref name="validValues" />. Random positions will continually be generated until one that has one of
        /// the specified values is found.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="gridView" />
        /// <param name="rng">The rng to use. Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <param name="validValues">
        /// A set of values to look for in the IGridView to determine whether or not a generated position
        /// is valid.
        /// </param>
        /// <returns>
        /// A random position whose value in this IGridView is equal to one of the values specified.
        /// </returns>
        public static Point RandomPosition<T>(this IGridView<T> gridView, IGenerator? rng = null, params T[] validValues)
            => RandomPosition(gridView, validValues, rng);

        /// <summary>
        /// Gets a random position in the grid view, for which the selector returns true. Random
        /// positions will continuously be generated until one that qualifies is found.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="gridView" />
        /// <param name="selector">
        /// Function that takes a position and the value at that position, and returns true if it is an
        /// acceptable selection, and false if not.
        /// </param>
        /// <param name="rng">The rng to use. Defaults to<see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <returns>A random position in the IGridView for which the selector returns true.</returns>
        public static Point RandomPosition<T>(this IGridView<T> gridView, Func<Point, T, bool> selector,
                                              IGenerator? rng = null)
        {
            rng ??= GlobalRandom.DefaultRNG;

            var c = new Point(rng.Next(gridView.Width), rng.Next(gridView.Height));

            while (!selector(c, gridView[c]))
                c = new Point(rng.Next(gridView.Width), rng.Next(gridView.Height));

            return c;
        }

        /// <summary>
        /// Gets a random position within the IGridView.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="gridView" />
        /// <param name="rng">The rng to use. Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <returns>A random position within the IGridView.</returns>
        public static Point RandomPosition<T>(this IGridView<T> gridView, IGenerator? rng = null)
        {
            rng ??= GlobalRandom.DefaultRNG;

            return new Point(rng.Next(gridView.Width), rng.Next(gridView.Height));
        }
    }
}
