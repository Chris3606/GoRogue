using GoRogue.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		/// Gets a rectangle representing the bounds of the MapView.
		/// </summary>
		/// <typeparam name="T">Type of items being exposed by the MapView.</typeparam>
		/// <param name="mapView">
		/// Map view to get bounds for -- never specified manually as this is an extension method
		/// </param>
		/// <returns>A rectangle representing the MapView's bounds.</returns>
		public static Rectangle Bounds<T>(this IMapView<T> mapView) => new Rectangle(0, 0, mapView.Width, mapView.Height);

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
		/// Gets the value at a random position in the MapView.
		/// </summary>
		/// <typeparam name="T">Type of items being exposed by the MapView.</typeparam>
		/// <param name="mapView">
		/// Map view to select from -- never specified manually as this is an extension method.
		/// </param>
		/// <param name="rng">The rng to use. Defaults to SingletonRandom.DefaultRNG.</param>
		/// <returns>The item at a random position in the MapView.</returns>
		public static T RandomItem<T>(this IMapView<T> mapView, IGenerator rng = null)
			=> mapView[RandomPosition(mapView, rng)];

		/// <summary>
		/// Gets the item at a random position in the map view, for which the selector returns true. Random
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
		/// <returns>The item at a random position in the MapView for which the selector returns true.</returns>
		public static T RandomItem<T>(this IMapView<T> mapView, Func<Coord, T, bool> selector, IGenerator rng = null)
			=> mapView[RandomPosition(mapView, selector, rng)];
		

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
		/// Gets a random position in the map view, whose value in map view is one of the ones
		/// specified in the HashSet. Random positions will continually be generated until one that has one of the
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
		public static Coord RandomPosition<T>(this IMapView<T> mapView, HashSet<T> validValues, IGenerator rng = null)
			=> mapView.RandomPosition((c, i) => validValues.Contains(i), rng);

		/// <summary>
		/// Gets a random position in the map view, whose value in map view is one of the ones
		/// specified in validValues. Random positions will continually be generated until one that has one of the
		/// specified values is found.  
		/// </summary>
		/// <typeparam name="T">Type of items being exposed by the MapView.</typeparam>
		/// <param name="mapView">
		/// Map view to select from -- never specified manually as this is an extension method.
		/// </param>
		/// <param name="rng">The rng to use. Defaults to SingletonRandom.DefaultRNG.</param>
		/// <param name="validValues">
		/// A set of values to look for in the MapView to determine whether or not a generated Coord
		/// is valid.
		/// </param>
		/// <returns>
		/// A random position whose value in this MapView is equal to one of the values specified.
		/// </returns>
		public static Coord RandomPosition<T>(this IMapView<T> mapView, IGenerator rng = null, params T[] validValues)
			=> RandomPosition(mapView, (IEnumerable<T>)validValues, rng);

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
		/// Extension method for IMapViews allowing printing the contents. Takes characters
		/// to surround the map printout, and each row, the method used to get the string representation of
		/// each element (defaulting to the ToString function of type T), and separation characters
		/// for each element and row.
		/// </summary>
		/// <typeparam name="T">Type of elements in the IMapView.</typeparam>
		/// <param name="map">
		/// The IMapView to stringify -- never specified manually as this is an extension method.
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
		/// <returns>
		/// A string representation of the map, as viewd by the given map view.
		/// </returns>
		public static string ExtendToString<T>(this IMapView<T> map, string begin = "", string beginRow = "", Func<T, string> elementStringifier = null,
													  string rowSeparator = "\n", string elementSeparator = " ", string endRow = "", string end = "")
		{
			if (elementStringifier == null)
				elementStringifier = (T obj) => obj.ToString();

			var result = new StringBuilder(begin);
			for (int y = 0; y < map.Height; y++)
			{
				result.Append(beginRow);
				for (int x = 0; x < map.Width; x++)
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
		/// Extension method for IMapViews allowing printing the contents. Takes characters
		/// to surround the map, and each row, the method used to get the string representation of
		/// each element (defaulting to the ToString function of type T), and separation characters
		/// for each element and row. Takes the size
		/// of the field to give each element, characters to surround the MapView printout, and each row, the
		/// method used to get the string representation of each element (defaulting to the ToString
		/// function of type T), and separation characters for each element and row.
		/// </summary>
		/// <typeparam name="T">Type of elements in the 2D array.</typeparam>	
		/// <param name="map">
		/// The IMapView to stringify -- never specified manually as this is an extension method.
		/// </param>
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
		/// <returns>
		/// A string representation of the map, as viewd by the given map view.
		/// </returns>
		public static string ExtendToString<T>(this IMapView<T> map, int fieldSize, string begin = "", string beginRow = "", Func<T, string> elementStringifier = null,
													  string rowSeparator = "\n", string elementSeparator = " ", string endRow = "", string end = "")
		{
			if (elementStringifier == null)
				elementStringifier = (T obj) => obj.ToString();

			var result = new StringBuilder(begin);
			for (int y = 0; y < map.Height; y++)
			{
				result.Append(beginRow);
				for (int x = 0; x < map.Width; x++)
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
		/// Extension method that applies values of the overlay to the current one -- effectively
		/// sets all the values of the current map to be corresponding to the one you pass in.
		/// </summary>
		/// <param name="self">The current ISettableMapView.  Never specified manually as this is an extension method.</param>
		/// <param name="overlay">
		/// The data apply to the map. Must have identical dimensions to the current map.
		/// </param>
		public static void ApplyOverlay<T>(this ISettableMapView<T> self, ISettableMapView<T> overlay)
		{
			if (self.Height != overlay.Height || self.Width != overlay.Width)
				throw new ArgumentException("Overlay size must match current map size.");

			for (int y = 0; y < self.Height; ++y)
				for (int x = 0; x < self.Width; ++x)
					self[x, y] = overlay[x, y];
		}
	}
}