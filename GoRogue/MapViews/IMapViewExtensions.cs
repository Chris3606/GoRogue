using GoRogue.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using Troschuetz.Random;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Extensions for the IMapView class that effectively act as methods with default
    /// implementations for them.
    /// </summary>
    public static class IMapViewExtensions
    {
        /// <summary>
        /// Iterates through each position in the map view. Equivalent to nested for loop for (y =
        /// 0...) for (x = 0...)
        /// </summary>
        /// <typeparam name="T">Type of elements in the map view.</typeparam>
        /// <param name="mapView">
        /// Map view to iterate over positions for. Never specified manually since this is an
        /// extension method.
        /// </param>
        /// <returns>All positions in the IMapView.</returns>
        public static IEnumerable<Coord> Positions<T>(this IMapView<T> mapView)
        {
            for (int y = 0; y < mapView.Height; y++)
                for (int x = 0; x < mapView.Width; x++)
                    yield return Coord.Get(x, y);
        }

        /// <summary>
        /// Gets a random position in the map view, whose value in that map view is the specified
        /// one. Random positions will continually be generated until one with the specified value is found.
        /// </summary>
        /// <typeparam name="T">Type of items being exposed by the MapView.</typeparam>
        /// <param name="mapView">
        /// Map view to select from -- never specified manually as this is an extension method.
        /// </param>
        /// <param name="validValue">
        /// A value to look for in the MapView to determine whether or not a generated Coord is valid.
        /// </param>
        /// <param name="rng">The rng to use. Defaults to SingletonRandom.DefaultRNG.</param>
        /// <returns>A random position whose value in this MapView is equal to the one specified.</returns>
        public static Coord RandomPosition<T>(this IMapView<T> mapView, T validValue, IGenerator rng = null)
            => mapView.RandomPosition((c, i) => i.Equals(validValue), rng);

        /// <summary>
        /// Gets a random position in the map view, whose value in map view is one of the ones
        /// specified. Random positions will continually be generated until one that has one of the
        /// specified values is found.
        /// </summary>
        /// <typeparam name="T">Type of items being exposed by the MapView.</typeparam>
        /// <param name="mapView">
        /// Map view to select from -- never specified manually as this is an extension method.
        /// </param>
        /// <param name="validValues">
        /// A set of values to look for in the MapView to determine whether or not a generated Coord
        /// is valid.
        /// </param>
        /// <param name="rng">The rng to use. Defaults to SingletonRandom.DefaultRNG.</param>
        /// <returns>
        /// A random position whose value in this MapView is equal to one of the values specified.
        /// </returns>
        public static Coord RandomPosition<T>(this IMapView<T> mapView, IEnumerable<T> validValues, IGenerator rng = null)
            => mapView.RandomPosition((c, i) => validValues.Contains(i), rng);

        /// <summary>
        /// Gets a random position in the map view, for which the selector returns true. Random
        /// positions will continuously be generated until one that qualifies is found.
        /// </summary>
        /// <typeparam name="T">Type of items being exposed by the MapView.</typeparam>
        /// <param name="mapView">
        /// Map view to select from -- never specified manually as this is an extension method.
        /// </param>
        /// <param name="selector">
        /// Function that takes a Coord and the value at that Coord, and returns true if it is an
        /// acceptable selection, and false if not.
        /// </param>
        /// <param name="rng">The rng to use. Defaults to SingletonRandom.DefaultRNG.</param>
        /// <returns>A random position in the MapView for which the selector returns true.</returns>
        public static Coord RandomPosition<T>(this IMapView<T> mapView, Func<Coord, T, bool> selector, IGenerator rng = null)
        {
            if (rng == null)
                rng = SingletonRandom.DefaultRNG;

            var c = Coord.Get(rng.Next(mapView.Width), rng.Next(mapView.Height));

            while (!selector(c, mapView[c]))
                c = Coord.Get(rng.Next(mapView.Width), rng.Next(mapView.Height));

            return c;
        }

        /// <summary>
        /// Gets a random position within the MapView.
        /// </summary>
        /// <typeparam name="T">Type of items being exposed by the MapView.</typeparam>
        /// <param name="mapView">
        /// Map view to select from -- never specified manually as this is an extension method.
        /// </param>
        /// <param name="rng">The rng to use. Defaults to SingletonRandom.DefaultRNG.</param>
        /// <returns>A random position within the MapView.</returns>
        public static Coord RandomPosition<T>(this IMapView<T> mapView, IGenerator rng = null)
        {
            if (rng == null)
                rng = SingletonRandom.DefaultRNG;

            return Coord.Get(rng.Next(mapView.Width), rng.Next(mapView.Height));
        }

        /// <summary>
        /// Gets a rectangle representing the bounds of the MapView.
        /// </summary>
        /// <typeparam name="T">Type of items being exposed by the MapView.</typeparam>
        /// <param name="mapView">Map view to select from -- never specified manually as this is an extension method</param>
        /// <returns>A rectangle representing the MapView's bounds.</returns>
        public static Rectangle Bounds<T>(this IMapView<T> mapView) => new Rectangle(0, 0, mapView.Width, mapView.Height);
    }
}