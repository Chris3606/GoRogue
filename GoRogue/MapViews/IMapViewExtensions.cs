using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoRogue.Random;
using JetBrains.Annotations;
using SadRogue.Primitives;
using Troschuetz.Random;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Extensions for <see cref="IMapView{T}" /> implementations that provide basic utility functions
    /// for them.
    /// </summary>
    /// <remarks>
    /// By providing these as extension methods, they effectively act as interface methods that have implementations
    /// already defined.  If these were regular interface implementations, all interface implementers would be forced
    /// to implement them manually, which is undesirable as the implementation should clearly be the same and is based
    /// only on functions/properties defined in IMapView.
    /// </remarks>
    [PublicAPI]
    public static class MapViewExtensions
    {
        /// <summary>
        /// Sets all the values of the current map to be equal to the corresponding values from
        /// the map you pass in.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="self" />
        /// <param name="overlay">
        /// The data apply to the map. Must have identical dimensions to the current map.
        /// </param>
        public static void ApplyOverlay<T>(this ISettableMapView<T> self, IMapView<T> overlay)
        {
            if (self.Height != overlay.Height || self.Width != overlay.Width)
                throw new ArgumentException("Overlay size must match current map size.");

            for (var y = 0; y < self.Height; ++y)
                for (var x = 0; x < self.Width; ++x)
                    self[x, y] = overlay[x, y];
        }

        /// <summary>
        /// Gets a rectangle representing the bounds of the current map view.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="mapView" />
        /// <returns>A rectangle representing the map view's bounds.</returns>
        public static Rectangle Bounds<T>(this IMapView<T> mapView)
            => new Rectangle(0, 0, mapView.Width, mapView.Height);

        /// <summary>
        /// Returns whether or not the given position is contained withing the current map view or not.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="mapView" />
        /// <param name="x">X-value of the position to check.</param>
        /// <param name="y">Y-value of the position to check.</param>
        /// <returns>True if the given position is contained within this map view, false otherwise.</returns>
        public static bool Contains<T>(this IMapView<T> mapView, int x, int y)
            => x >= 0 && y >= 0 && x < mapView.Width && y < mapView.Height;

        /// <summary>
        /// Returns whether or not the given position is contained withing the current map view or not.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="mapView" />
        /// <param name="position">The position to check.</param>
        /// <returns>True if the given position is contained within this map view, false otherwise.</returns>
        public static bool Contains<T>(this IMapView<T> mapView, Point position)
            => position.X >= 0 && position.Y >= 0 && position.X < mapView.Width && position.Y < mapView.Height;

        /// <summary>
        /// Allows stringifying the contents of a map view. Takes characters to
        /// surround the map printout, and each row, the method used to get the string representation
        /// of each element (defaulting to the ToString function of type T), and separation
        /// characters for each element and row.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="map" />
        /// <param name="begin">Character(s) that should precede the IMapView printout.</param>
        /// <param name="beginRow">Character(s) that should precede each row.</param>
        /// <param name="elementStringifier">
        /// Function to use to get the string representation of each value. null uses the ToString
        /// function of type T.
        /// </param>
        /// <param name="rowSeparator">Character(s) to separate each row from the next.</param>
        /// <param name="elementSeparator">Character(s) to separate each element from the next.</param>
        /// <param name="endRow">Character(s) that should follow each row.</param>
        /// <param name="end">Character(s) that should follow the IMapView printout.</param>
        /// <returns>A string representation of the map, as viewed by the given map view.</returns>
        public static string ExtendToString<T>(this IMapView<T> map, string begin = "", string beginRow = "",
                                               Func<T, string>? elementStringifier = null,
                                               string rowSeparator = "\n", string elementSeparator = " ",
                                               string endRow = "", string end = "")
        {
            elementStringifier ??= obj => obj?.ToString() ?? "null";

            var result = new StringBuilder(begin);
            for (var y = 0; y < map.Height; y++)
            {
                result.Append(beginRow);
                for (var x = 0; x < map.Width; x++)
                {
                    result.Append(elementStringifier(map[x, y]));
                    if (x != map.Width - 1) result.Append(elementSeparator);
                }

                result.Append(endRow);
                if (y != map.Height - 1) result.Append(rowSeparator);
            }

            result.Append(end);

            return result.ToString();
        }

        /// <summary>
        /// Allows stringifying the contents of a map view. Takes characters to
        /// surround the map, and each row, the method used to get the string representation of each
        /// element (defaulting to the ToString function of type T), and separation characters for
        /// each element and row. Takes the size of the field to give each element, characters to
        /// surround the MapView printout, and each row, the method used to get the string
        /// representation of each element (defaulting to the ToString function of type T), and
        /// separation characters for each element and row.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="map" />
        /// <param name="fieldSize">
        /// The amount of space each element should take up in characters. A positive number aligns
        /// the text to the right of the space, while a negative number aligns the text to the left.
        /// </param>
        /// <param name="begin">Character(s) that should precede the IMapView printout.</param>
        /// <param name="beginRow">Character(s) that should precede each row.</param>
        /// <param name="elementStringifier">
        /// Function to use to get the string representation of each value. Null uses the ToString
        /// function of type T.
        /// </param>
        /// <param name="rowSeparator">Character(s) to separate each row from the next.</param>
        /// <param name="elementSeparator">Character(s) to separate each element from the next.</param>
        /// <param name="endRow">Character(s) that should follow each row.</param>
        /// <param name="end">Character(s) that should follow the IMapView printout.</param>
        /// <returns>A string representation of the map, as viewed by the given map view.</returns>
        public static string ExtendToString<T>(this IMapView<T> map, int fieldSize, string begin = "",
                                               string beginRow = "", Func<T, string>? elementStringifier = null,
                                               string rowSeparator = "\n", string elementSeparator = " ",
                                               string endRow = "", string end = "")
        {
            elementStringifier ??= obj => obj?.ToString() ?? "null";

            var result = new StringBuilder(begin);
            for (var y = 0; y < map.Height; y++)
            {
                result.Append(beginRow);
                for (var x = 0; x < map.Width; x++)
                {
                    result.Append(string.Format($"{{0, {fieldSize}}} ", elementStringifier(map[x, y])));
                    if (x != map.Width - 1) result.Append(elementSeparator);
                }

                result.Append(endRow);
                if (y != map.Height - 1) result.Append(rowSeparator);
            }

            result.Append(end);

            return result.ToString();
        }

        /// <summary>
        /// Iterates through each position in the map view. Equivalent to nested for loop for (y =
        /// 0...) for (x = 0...)
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="mapView" />
        /// <returns>All positions in the IMapView.</returns>
        public static IEnumerable<Point> Positions<T>(this IMapView<T> mapView)
        {
            for (var y = 0; y < mapView.Height; y++)
                for (var x = 0; x < mapView.Width; x++)
                    yield return new Point(x, y);
        }

        /// <summary>
        /// Gets the value at a random position in the IMapView.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="mapView" />
        /// <param name="rng">The rng to use. Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <returns>The item at a random position in the IMapView.</returns>
        public static T RandomItem<T>(this IMapView<T> mapView, IGenerator? rng = null)
            => mapView[RandomPosition(mapView, rng)];

        /// <summary>
        /// Gets the item at a random position in the map view for which the selector returns true.
        /// Random positions will continuously be generated until one that qualifies is found.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="mapView" />
        /// <param name="selector">
        /// Function that takes a position, and the value at that position, and returns true if it is an
        /// acceptable selection, and false if not.
        /// </param>
        /// <param name="rng">The rng to use. Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <returns>
        /// The item at a random position in the IMapView for which the selector returns true.
        /// </returns>
        public static T RandomItem<T>(this IMapView<T> mapView, Func<Point, T, bool> selector, IGenerator? rng = null)
            => mapView[RandomPosition(mapView, selector, rng)];

        /// <summary>
        /// Gets a random position in the map view, whose value in that map view is the specified
        /// one. Random positions will continually be generated until one with the specified value is found.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="mapView" />
        /// <param name="validValue">
        /// A value to look for in the IMapView to determine whether or not a generated position is valid.
        /// </param>
        /// <param name="rng">The rng to use. Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <returns>A random position whose value in the current IMapView is equal to the one specified.</returns>
        public static Point RandomPosition<T>(this IMapView<T> mapView, T validValue, IGenerator? rng = null)
            => mapView.RandomPosition((c, i) => i?.Equals(validValue) ?? validValue == null, rng);

        /// <summary>
        /// Gets a random position in the map view, whose value in map view is one of the ones
        /// specified. Random positions will continually be generated until one that has one of the
        /// specified values is found.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="mapView" />
        /// <param name="validValues">
        /// A set of values to look for in the IMapView to determine whether or not a generated position
        /// is valid.
        /// </param>
        /// <param name="rng">The rng to use. Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <returns>
        /// A random position whose value in this IMapView is equal to one of the values specified.
        /// </returns>
        public static Point RandomPosition<T>(this IMapView<T> mapView, IEnumerable<T> validValues,
                                              IGenerator? rng = null)
            => mapView.RandomPosition((c, i) => validValues.Contains(i), rng);

        /// <summary>
        /// Gets a random position in the map view, whose value in map view is one of the ones
        /// specified in the hash set. Random positions will continually be generated until one that
        /// has one of the specified values is found.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="mapView" />
        /// <param name="validValues">
        /// A set of values to look for in the IMapView to determine whether or not a generated position
        /// is valid.
        /// </param>
        /// <param name="rng">The rng to use. Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <returns>
        /// A random position whose value in this IMapView is equal to one of the values specified.
        /// </returns>
        public static Point RandomPosition<T>(this IMapView<T> mapView, HashSet<T> validValues, IGenerator? rng = null)
            => mapView.RandomPosition((c, i) => validValues.Contains(i), rng);

        /// <summary>
        /// Gets a random position in the map view, whose value in map view is one of the ones
        /// specified in <paramref name="validValues" />. Random positions will continually be generated until one that
        /// has one of the specified values is found.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="mapView" />
        /// <param name="rng">The rng to use. Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <param name="validValues">
        /// A set of values to look for in the IMapView to determine whether or not a generated position
        /// is valid.
        /// </param>
        /// <returns>
        /// A random position whose value in this IMapView is equal to one of the values specified.
        /// </returns>
        public static Point RandomPosition<T>(this IMapView<T> mapView, IGenerator? rng = null, params T[] validValues)
            => RandomPosition(mapView, validValues, rng);

        /// <summary>
        /// Gets a random position in the map view, for which the selector returns true. Random
        /// positions will continuously be generated until one that qualifies is found.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="mapView" />
        /// <param name="selector">
        /// Function that takes a position and the value at that position, and returns true if it is an
        /// acceptable selection, and false if not.
        /// </param>
        /// <param name="rng">The rng to use. Defaults to<see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <returns>A random position in the IMapView for which the selector returns true.</returns>
        public static Point RandomPosition<T>(this IMapView<T> mapView, Func<Point, T, bool> selector,
                                              IGenerator? rng = null)
        {
            rng ??= GlobalRandom.DefaultRNG;

            var c = new Point(rng.Next(mapView.Width), rng.Next(mapView.Height));

            while (!selector(c, mapView[c]))
                c = new Point(rng.Next(mapView.Width), rng.Next(mapView.Height));

            return c;
        }

        /// <summary>
        /// Gets a random position within the IMapView.
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="mapView" />
        /// <param name="rng">The rng to use. Defaults to <see cref="GlobalRandom.DefaultRNG" />.</param>
        /// <returns>A random position within the IMapView.</returns>
        public static Point RandomPosition<T>(this IMapView<T> mapView, IGenerator? rng = null)
        {
            rng ??= GlobalRandom.DefaultRNG;

            return new Point(rng.Next(mapView.Width), rng.Next(mapView.Height));
        }
    }
}
