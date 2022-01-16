using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.Random;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

// Intentionally disabled so extensions correspond to IEnhancedRandom location
// ReSharper disable once CheckNamespace
namespace ShaiRandom.Generators
{
    /// <summary>
    /// Class containing some extension methods for <see cref="IEnhancedRandom" /> instances.
    /// </summary>
    [PublicAPI]
    public static class GoRogueEnhancedRandomExtensions
    {
        #region Percentage Checks
        /// <summary>
        /// Performs a percentage check that has the specified chance to succeed.  The percentage should be in range
        /// [0, 100] (inclusive).
        /// </summary>
        /// <param name="rng" />
        /// <param name="percentage">Percentage chance (out of 100) that this check will succeed.  Must be in range [0, 100].</param>
        /// <returns></returns>
        public static bool PercentageCheck(this IEnhancedRandom rng, float percentage)
        {
            if (percentage > 100 || percentage < 0)
                throw new ArgumentException($"Percentage given to {nameof(PercentageCheck)} must be in range [0, 100].",
                    nameof(percentage));

            return rng.NextBool(percentage * 0.01f);
        }
        #endregion

        # region Random Selection - Area
        /// <summary>
        /// Extension method that selects and returns a random valid index of a position in a (non-empty) Area.
        /// </summary>
        /// <exception cref="ArgumentException">An empty Area was provided.</exception>
        /// <param name="rng"/>
        /// <param name="area">The area to select from.  Must be non-empty.</param>
        /// <returns>The index selected.</returns>
        public static int RandomIndex(this IEnhancedRandom rng, IReadOnlyArea area)
        {
            int count = area.Count;
            if (count == 0)
                throw new ArgumentException("Cannot select random index from empty area.", nameof(area));

            return rng.NextInt(count);
        }

        /// <summary>
        /// Extension method that selects and returns a random valid index for some position in the (non-empty)
        /// <see cref="IReadOnlyArea"/> for which the selector function given returns true, using the rng specified.
        /// Indices are repeatedly selected until a qualifying index is found.
        /// </summary>
        /// <remarks>
        /// This function will never return if no indices in the area return true for the given selector, and could take
        /// a very long time to execute if the area is large and the selector returns true for very few of those indices.
        /// For a more reliable termination, use the overload taking a maxTries parameter instead:
        /// <see cref="RandomIndex(ShaiRandom.Generators.IEnhancedRandom, SadRogue.Primitives.IReadOnlyArea, Func{int, bool}, int)"/>
        /// </remarks>
        /// <exception cref="ArgumentException">An empty Area was provided.</exception>
        /// <param name="rng" />
        /// <param name="area">The area to select from.  Must be non-empty.</param>
        /// <param name="selector">Function that should return true if the given index is valid selection, false otherwise.</param>
        /// <returns>Index selected.</returns>
        public static int RandomIndex(this IEnhancedRandom rng, IReadOnlyArea area, Func<int, bool> selector)
        {
            int index = rng.RandomIndex(area);
            while (!selector(index))
                index = rng.RandomIndex(area);

            return index;
        }

        public static int RandomIndex(this IEnhancedRandom rng, IReadOnlyArea area, Func<int, bool> selector,
                                      int maxTries)
        {
            if (maxTries <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxTries),
                    $"Value must be > 0; for infinite retries, use the overload without a {nameof(maxTries)} parameter.");

            int curTries = 0;
            while (curTries < maxTries)
            {
                int idx = rng.RandomIndex(area);
                if (selector(idx))
                    return idx;

                curTries++;
            }

            throw new MaxAttemptsReachedException();
        }

        /// <summary>
        /// Extension method that selects and returns a random position from the Area, using the rng
        /// specified. An exception is thrown if the area is empty.
        /// </summary>
        /// <exception cref="ArgumentException">An empty Area was provided.</exception>
        /// <param name="rng" />
        /// <param name="area">The area to select from.  Must be non-empty.</param>
        /// <returns>Item selected.</returns>
        public static Point RandomPosition(this IEnhancedRandom rng, IReadOnlyArea area)
        {
            int count = area.Count;
            if (count == 0)
                throw new ArgumentException("Cannot select random item from empty area.", nameof(area));

            return area[rng.NextInt(count)];
        }

        /// <summary>
        /// Extension method that selects and returns a random position from the given (non-empty)
        /// <see cref="IReadOnlyArea"/> for which the selector function given returns true, using the rng specified.
        /// Items are repeatedly selected until a qualifying index is found.
        /// </summary>
        /// <remarks>
        /// This function will never return if no positions in the area return true for the given selector, and could take
        /// a very long time to execute if the area is large and the selector returns true for very few of its positions.
        /// For a more reliable termination, use the overload taking a maxTries parameter instead:
        /// <see cref="RandomPosition(ShaiRandom.Generators.IEnhancedRandom, SadRogue.Primitives.IReadOnlyArea, Func{Point, bool}, int)"/>
        /// </remarks>
        /// <exception cref="ArgumentException">An empty Area was provided.</exception>
        /// <param name="rng" />
        /// <param name="area">The area to select from.  Must be non-empty.</param>
        /// <param name="selector">Function that should return true if the given index is valid selection, false otherwise.</param>
        /// <returns>Index selected.</returns>
        public static Point RandomPosition(this IEnhancedRandom rng, IReadOnlyArea area, Func<Point, bool> selector)
        {
            var item = rng.RandomPosition(area);
            while (!selector(item))
                item = rng.RandomPosition(area);

            return item;
        }

        public static Point RandomPosition(this IEnhancedRandom rng, IReadOnlyArea area, Func<Point, bool> selector,
                                          int maxTries)
        {
            if (maxTries <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxTries),
                    $"Value must be > 0; for infinite retries, use the overload without a {nameof(maxTries)} parameter.");

            int curTries = 0;
            while (curTries < maxTries)
            {
                Point point = rng.RandomPosition(area);
                if (selector(point))
                    return point;

                curTries++;
            }

            throw new MaxAttemptsReachedException();
        }
        #endregion

        #region Random Selection - Grid Views
        /// <summary>
        /// Gets the value at a random position in the IGridView.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="gridView" />
        /// <returns>The item at a random position in the IGridView.</returns>
        public static T RandomElement<T>(this IEnhancedRandom rng, IGridView<T> gridView)
            => gridView[rng.RandomPosition(gridView)];

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
        /// <returns>
        /// The item at a random position in the IGridView for which the selector returns true.
        /// </returns>
        public static T RandomElement<T>(this IEnhancedRandom rng, IGridView<T> gridView, Func<Point, T, bool> selector)
            => gridView[rng.RandomPosition(gridView, selector)];

        /// <summary>
        /// Gets a random position in the grid view, whose value in that grid view is the specified
        /// one. Random positions will continually be generated until one with the specified value is found.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="gridView" />
        /// <param name="validValue">
        /// A value to look for in the IGridView to determine whether or not a generated position is valid.
        /// </param>
        /// <returns>A random position whose value in the current IGridView is equal to the one specified.</returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView, T validValue)
            => rng.RandomPosition(gridView, (c, i) => i?.Equals(validValue) ?? validValue == null);

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
        /// <returns>
        /// A random position whose value in this IGridView is equal to one of the values specified.
        /// </returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView, IEnumerable<T> validValues)
            => rng.RandomPosition(gridView, (c, i) => validValues.Contains(i));

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
        /// <returns>
        /// A random position whose value in this IGridView is equal to one of the values specified.
        /// </returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView, HashSet<T> validValues)
            => rng.RandomPosition(gridView, (c, i) => validValues.Contains(i));

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
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView, params T[] validValues)
            => rng.RandomPosition(gridView, (IEnumerable<T>)validValues);

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
        /// <returns>A random position in the IGridView for which the selector returns true.</returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView,
                                              Func<Point, T, bool> selector)
        {
            var c = new Point(rng.NextInt(gridView.Width), rng.NextInt(gridView.Height));

            while (!selector(c, gridView[c]))
                c = new Point(rng.NextInt(gridView.Width), rng.NextInt(gridView.Height));

            return c;
        }

        /// <summary>
        /// Gets a random position within the IGridView.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="gridView" />
        /// <returns>A random position within the IGridView.</returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView)
        {
            return new Point(rng.NextInt(gridView.Width), rng.NextInt(gridView.Height));
        }
        #endregion
        // TODO: Add maxTries overloads
    }
}
