using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Extension method that selects and returns a random valid index for some position in the (non-empty)
        /// <see cref="IReadOnlyArea"/> for which the selector function given returns true, using the rng specified.
        /// Indices are repeatedly selected until a qualifying index is found, or the specified <paramref name="maxTries"/>
        /// value is reached.
        /// </summary>
        /// <exception cref="ArgumentException">An empty Area was provided.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A <paramref name="maxTries"/> value that was less than or equal to 0 was provided.</exception>
        /// <exception cref="MaxAttemptsReachedException">A value was selected <paramref name="maxTries"/> times, and none of the selections returned true from <paramref name="selector"/></exception>
        /// <param name="rng" />
        /// <param name="area">The area to select from.  Must be non-empty.</param>
        /// <param name="selector">Function that should return true if the given index is valid selection, false otherwise.</param>
        /// <param name="maxTries">Maximum number of selections to make before giving up and throwing an exception.</param>
        /// <returns>Index selected.</returns>
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
        public static Point RandomElement(this IEnhancedRandom rng, IReadOnlyArea area)
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
        /// <see cref="RandomElement(ShaiRandom.Generators.IEnhancedRandom, SadRogue.Primitives.IReadOnlyArea, Func{Point, bool}, int)"/>
        /// </remarks>
        /// <exception cref="ArgumentException">An empty Area was provided.</exception>
        /// <param name="rng" />
        /// <param name="area">The area to select from.  Must be non-empty.</param>
        /// <param name="selector">Function that should return true if the given index is valid selection, false otherwise.</param>
        /// <returns>Item selected.</returns>
        public static Point RandomElement(this IEnhancedRandom rng, IReadOnlyArea area, Func<Point, bool> selector)
        {
            var item = rng.RandomElement(area);
            while (!selector(item))
                item = rng.RandomElement(area);

            return item;
        }

        /// <summary>
        /// Extension method that selects and returns a random position from the given (non-empty)
        /// <see cref="IReadOnlyArea"/> for which the selector function given returns true, using the rng specified.
        /// Items are repeatedly selected until a qualifying index is found, or the specified <paramref name="maxTries"/>
        /// value is reached.
        /// </summary>
        /// <exception cref="ArgumentException">An empty Area was provided.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A <paramref name="maxTries"/> value that was less than or equal to 0 was provided.</exception>
        /// <exception cref="MaxAttemptsReachedException">A value was selected <paramref name="maxTries"/> times, and none of the selections returned true from <paramref name="selector"/></exception>
        /// <param name="rng" />
        /// <param name="area">The area to select from.  Must be non-empty.</param>
        /// <param name="selector">Function that should return true if the given index is valid selection, false otherwise.</param>
        /// <param name="maxTries">Maximum number of selections to make before giving up and throwing an exception.</param>
        /// <returns>Position selected.</returns>
        public static Point RandomElement(this IEnhancedRandom rng, IReadOnlyArea area, Func<Point, bool> selector,
                                          int maxTries)
        {
            if (maxTries <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxTries),
                    $"Value must be > 0; for infinite retries, use the overload without a {nameof(maxTries)} parameter.");

            int curTries = 0;
            while (curTries < maxTries)
            {
                Point point = rng.RandomElement(area);
                if (selector(point))
                    return point;

                curTries++;
            }

            throw new MaxAttemptsReachedException();
        }
        #endregion

        #region Random Selection - Grid Views
        /// <summary>
        /// Gets the value at a random position in the IGridView, using the rng given.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="rng"/>
        /// <param name="gridView">The grid view to select from.</param>
        /// <returns>The item at a random position in the IGridView.</returns>
        public static T RandomElement<T>(this IEnhancedRandom rng, IGridView<T> gridView)
            => gridView[rng.RandomPosition(gridView)];

        /// <summary>
        /// Extension method that selects and returns a random item from the given (non-empty)
        /// <see cref="IGridView{T}"/> for which the selector function given returns true, using the rng specified.
        /// Items are repeatedly selected until a qualifying value is found.
        /// </summary>
        /// <remarks>
        /// This function will never return if no positions in the view return true for the given selector, and could take
        /// a very long time to execute if the view is large and the selector returns true for very few of its positions.
        /// For a more reliable termination, use the overload taking a maxTries parameter instead.
        /// </remarks>
        /// <exception cref="ArgumentException">An empty grid view was provided.</exception>
        /// <typeparam name="T" />
        /// <param name="rng"/>
        /// <param name="gridView">The grid view to select from.</param>
        /// <param name="selector">
        /// Function that takes a position, and the value at that position, and returns true if it is an
        /// acceptable selection, and false if not.
        /// </param>
        /// <returns>
        /// The item at the first random position in the IGridView selected for which the selector returns true.
        /// </returns>
        public static T RandomElement<T>(this IEnhancedRandom rng, IGridView<T> gridView, Func<Point, T, bool> selector)
            => gridView[rng.RandomPosition(gridView, selector)];

        /// <summary>
        /// Extension method that selects and returns a random item from the given (non-empty)
        /// <see cref="IGridView{T}"/> for which the selector function given returns true, using the rng specified.
        /// Items are repeatedly selected until a qualifying value is found, or the specified <paramref name="maxTries"/>
        /// value is reached.
        /// </summary>
        /// <exception cref="ArgumentException">An empty grid view was provided.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A <paramref name="maxTries"/> value that was less than or equal to 0 was provided.</exception>
        /// <exception cref="MaxAttemptsReachedException">A value was selected <paramref name="maxTries"/> times, and none of the selections returned true from <paramref name="selector"/></exception>
        /// <param name="rng" />
        /// <param name="gridView">The grid view to select from.</param>
        /// <param name="selector">
        /// Function that takes a position, and the value at that position, and returns true if it is an
        /// acceptable selection, and false if not.
        /// </param>
        /// <param name="maxTries">Maximum number of selections to make before giving up and throwing an exception.</param>
        /// <returns>
        /// The item at the first random position in the IGridView selected for which the selector returns true.
        /// </returns>
        public static T RandomElement<T>(this IEnhancedRandom rng, IGridView<T> gridView, Func<Point, T, bool> selector,
                                         int maxTries)
            => gridView[rng.RandomPosition(gridView, selector, maxTries)];

        /// <summary>
        /// Extension method that selects and returns a random position from the given (non-empty)
        /// <see cref="IGridView{T}"/> whose value in that grid view is the one that is specified.
        /// Positions are repeatedly selected until one with the specified value is found.
        /// </summary>
        /// <remarks>
        /// This function will never return if no positions in the view have the value given, and could take
        /// a very long time to execute if the view is large and very few of its positions have the specified value.
        /// For a more reliable termination, use the overload taking a maxTries parameter instead.
        /// </remarks>
        /// <exception cref="ArgumentException">An empty grid view was provided.</exception>
        /// <typeparam name="T" />
        /// <param name="rng"/>
        /// <param name="gridView">The grid view to select from.</param>
        /// <param name="validValue">
        /// A value to look for in the IGridView to determine whether or not a generated position is valid.
        /// </param>
        /// <returns>A random position whose value in the current IGridView is equal to the one specified.</returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView, T validValue)
            => rng.RandomPosition(gridView, (c, i) => i?.Equals(validValue) ?? validValue == null);

        /// <summary>
        /// Extension method that selects and returns a random position from the given (non-empty)
        /// <see cref="IGridView{T}"/> whose value in that grid view is the one that is specified.
        /// Positions are repeatedly selected until one with the specified value is found, or the specified <paramref name="maxTries"/>
        /// value is reached.
        /// </summary>
        /// <exception cref="ArgumentException">An empty grid view was provided.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A <paramref name="maxTries"/> value that was less than or equal to 0 was provided.</exception>
        /// <exception cref="MaxAttemptsReachedException">A value was selected <paramref name="maxTries"/> times, and none of the selected positions had a value of <paramref name="validValue"/>.</exception>
        /// <param name="rng" />
        /// <param name="gridView">The grid view to select from.</param>
        /// <param name="validValue">
        /// A value to look for in the IGridView to determine whether or not a generated position is valid.
        /// </param>
        /// <param name="maxTries">Maximum number of selections to make before giving up and throwing an exception.</param>
        /// <returns>A random position whose value in the current IGridView is equal to the one specified.</returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView, T validValue, int maxTries)
            => rng.RandomPosition(gridView, (c, i) => i?.Equals(validValue) ?? validValue == null, maxTries);

        /// <summary>
        /// Extension method that selects and returns a random position from the given (non-empty)
        /// <see cref="IGridView{T}"/> whose value in that grid view is is one of the ones specified.
        /// Random positions are repeatedly selected until one that has one of the
        /// specified values is found.
        /// </summary>
        /// <remarks>
        /// This function will never return if no positions in the view have one of the values given, and could take
        /// a very long time to execute if the view is large and very few of its positions have one of the specified values.
        /// For a more reliable termination, use the overload taking a maxTries parameter instead.
        /// </remarks>
        /// <exception cref="ArgumentException">An empty grid view was provided.</exception>
        /// <typeparam name="T" />
        /// <param name="rng"/>
        /// <param name="gridView">The grid view to select from.</param>
        /// <param name="validValues">
        /// A set of values to look for in the IGridView to determine whether or not a generated position
        /// is valid.
        /// </param>
        /// <returns>A random position whose value in this IGridView is equal to one of the values specified.</returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView, IEnumerable<T> validValues)
            => rng.RandomPosition(gridView, (c, i) => validValues.Contains(i));

        /// <summary>
        /// Extension method that selects and returns a random position from the given (non-empty)
        /// <see cref="IGridView{T}"/> whose value in that grid view is is one of the ones specified.
        /// Positions are repeatedly selected until one that has one of the specified values is found, or the specified <paramref name="maxTries"/>
        /// value is reached.
        /// </summary>
        /// <exception cref="ArgumentException">An empty grid view was provided.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A <paramref name="maxTries"/> value that was less than or equal to 0 was provided.</exception>
        /// <exception cref="MaxAttemptsReachedException">A value was selected <paramref name="maxTries"/> times, and none of the selected positions had one of the given values.</exception>
        /// <param name="rng" />
        /// <param name="gridView">The grid view to select from.</param>
        /// <param name="validValues">
        /// A set of values to look for in the IGridView to determine whether or not a generated position
        /// is valid.
        /// </param>
        /// <param name="maxTries">Maximum number of selections to make before giving up and throwing an exception.</param>
        /// <returns>A random position whose value in this IGridView is equal to one of the values specified.</returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView, IEnumerable<T> validValues, int maxTries)
            => rng.RandomPosition(gridView, (c, i) => validValues.Contains(i), maxTries);

        /// <summary>
        /// Extension method that selects and returns a random position from the given (non-empty)
        /// <see cref="IGridView{T}"/> whose value in that grid view is is one of the ones in the specified hash set.
        /// Random positions are repeatedly selected until one that has one of the
        /// specified values is found.
        /// </summary>
        /// <remarks>
        /// This function will never return if no positions in the view have one of the values given, and could take
        /// a very long time to execute if the view is large and very few of its positions have one of the specified values.
        /// For a more reliable termination, use the overload taking a maxTries parameter instead.
        /// </remarks>
        /// <exception cref="ArgumentException">An empty grid view was provided.</exception>
        /// <typeparam name="T" />
        /// <param name="rng"/>
        /// <param name="gridView">The grid view to select from.</param>
        /// <param name="validValues">
        /// A set of values to look for in the IGridView to determine whether or not a generated position
        /// is valid.
        /// </param>
        /// <returns>A random position whose value in this IGridView is equal to one of the values specified.</returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView, HashSet<T> validValues)
            => rng.RandomPosition(gridView, (c, i) => validValues.Contains(i));

        /// <summary>
        /// Extension method that selects and returns a random position from the given (non-empty)
        /// <see cref="IGridView{T}"/> whose value in that grid view is is one of the ones in the specified hash set.
        /// Positions are repeatedly selected until one that has one of the specified values is found, or the specified <paramref name="maxTries"/>
        /// value is reached.
        /// </summary>
        /// <exception cref="ArgumentException">An empty grid view was provided.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A <paramref name="maxTries"/> value that was less than or equal to 0 was provided.</exception>
        /// <exception cref="MaxAttemptsReachedException">A value was selected <paramref name="maxTries"/> times, and none of the selected positions had one of the given values.</exception>
        /// <param name="rng" />
        /// <param name="gridView">The grid view to select from.</param>
        /// <param name="validValues">
        /// A set of values to look for in the IGridView to determine whether or not a generated position
        /// is valid.
        /// </param>
        /// <param name="maxTries">Maximum number of selections to make before giving up and throwing an exception.</param>
        /// <returns>A random position whose value in this IGridView is equal to one of the values specified.</returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView, HashSet<T> validValues, int maxTries)
            => rng.RandomPosition(gridView, (c, i) => validValues.Contains(i), maxTries);

        /// <summary>
        /// Extension method that selects and returns a random position from the given (non-empty)
        /// <see cref="IGridView{T}"/> whose value in that grid view is is one of the ones specified.
        /// Random positions are repeatedly selected until one that has one of the
        /// specified values is found.
        /// </summary>
        /// <remarks>
        /// This function will never return if no positions in the view have one of the values given, and could take
        /// a very long time to execute if the view is large and very few of its positions have one of the specified values.
        /// For a more reliable termination, use the overload taking a maxTries parameter instead.
        /// </remarks>
        /// <exception cref="ArgumentException">An empty grid view was provided.</exception>
        /// <typeparam name="T" />
        /// <param name="rng"/>
        /// <param name="gridView">The grid view to select from.</param>
        /// <param name="validValues">
        /// A set of values to look for in the IGridView to determine whether or not a generated position
        /// is valid.
        /// </param>
        /// <returns>A random position whose value in this IGridView is equal to one of the values specified.</returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView, params T[] validValues)
            => rng.RandomPosition(gridView, (IEnumerable<T>)validValues);

        /// <summary>
        /// Extension method that selects and returns a random position from the given (non-empty)
        /// <see cref="IGridView{T}"/> whose value in that grid view is is one of the ones specified.
        /// Positions are repeatedly selected until one that has one of the specified values is found, or the specified <paramref name="maxTries"/>
        /// value is reached.
        /// </summary>
        /// <exception cref="ArgumentException">An empty grid view was provided.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A <paramref name="maxTries"/> value that was less than or equal to 0 was provided.</exception>
        /// <exception cref="MaxAttemptsReachedException">A value was selected <paramref name="maxTries"/> times, and none of the selected positions had one of the given values.</exception>
        /// <param name="rng" />
        /// <param name="gridView">The grid view to select from.</param>
        /// <param name="validValues">
        /// A set of values to look for in the IGridView to determine whether or not a generated position
        /// is valid.
        /// </param>
        /// <param name="maxTries">Maximum number of selections to make before giving up and throwing an exception.</param>
        /// <returns>A random position whose value in this IGridView is equal to one of the values specified.</returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView, int maxTries, params T[] validValues)
            => rng.RandomPosition(gridView, validValues, maxTries);

        /// <summary>
        /// Gets a random position in the grid view, for which the selector returns true. Random
        /// positions will continuously be generated until one that qualifies is found.
        /// </summary>
        /// <remarks>
        /// This function will never return if no positions in the view return true for the given selector, and could take
        /// a very long time to execute if the view is large and the selector returns true for very few of its positions.
        /// For a more reliable termination, use the overload taking a maxTries parameter instead.
        /// </remarks>
        /// <exception cref="ArgumentException">An empty grid view was given.</exception>
        /// <typeparam name="T" />
        /// <param name="rng" />
        /// <param name="gridView">The grid view to select from.</param>
        /// <param name="selector">
        /// Function that takes a position and the value at that position, and returns true if it is an
        /// acceptable selection, and false if not.
        /// </param>
        /// <returns>A random position in the IGridView for which the selector returns true.</returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView,
                                              Func<Point, T, bool> selector)
        {
            if (gridView.Width == 0 || gridView.Height == 0)
                throw new ArgumentException("Cannot select random position from empty grid view.", nameof(gridView));

            var pos = rng.RandomPosition(gridView);
            while (!selector(pos, gridView[pos]))
                pos = rng.RandomPosition(gridView);
            return pos;
        }

        /// <summary>
        /// Gets a random position in the grid view, for which the selector returns true. Random
        /// positions will continuously be generated until one that qualifies is found, or <see cref="maxTries"/>
        /// selections occur.
        /// </summary>
        /// <exception cref="ArgumentException">An empty grid view was provided.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A <paramref name="maxTries"/> value that was less than or equal to 0 was provided.</exception>
        /// <exception cref="MaxAttemptsReachedException">A value was selected <paramref name="maxTries"/> times, and none of the selected positions returned true from <paramref name="selector"/>.</exception>
        /// <typeparam name="T" />
        /// <param name="rng" />
        /// <param name="gridView">The grid view to select from.</param>
        /// <param name="selector">
        /// Function that takes a position and the value at that position, and returns true if it is an
        /// acceptable selection, and false if not.
        /// </param>
        /// <param name="maxTries">Maximum number of selections to make before giving up and throwing an exception.</param>
        /// <returns>A random position in the IGridView for which the selector returns true.</returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView,
                                              Func<Point, T, bool> selector, int maxTries)
        {
            if (maxTries <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxTries),
                    $"Value must be > 0; for infinite retries, use the overload without a {nameof(maxTries)} parameter.");

            int curTries = 0;
            while (curTries < maxTries)
            {
                var pos = rng.RandomPosition(gridView);
                if (selector(pos, gridView[pos]))
                    return pos;

                curTries++;
            }

            throw new MaxAttemptsReachedException();
        }

        /// <summary>
        /// Randomly selects a position within the IGridView.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="rng"/>
        /// <param name="gridView">Grid view to select a position from.</param>
        /// <returns>A random position within the IGridView.</returns>
        public static Point RandomPosition<T>(this IEnhancedRandom rng, IGridView<T> gridView)
        {
            if (gridView.Width == 0 || gridView.Height == 0)
                throw new ArgumentException("Cannot select random position from empty grid view.", nameof(gridView));

            return new Point(rng.NextInt(gridView.Width), rng.NextInt(gridView.Height));
        }
        #endregion
    }
}
