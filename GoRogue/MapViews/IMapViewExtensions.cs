using System.Collections.Generic;

namespace GoRogue.MapViews
{
    /// <summary>
    /// Extensions for the IMapView class that effectively act as methods with default implementations for them.
    /// </summary>
    public static class IMapViewExtensions
    {
        /// <summary>
        /// Iterates through each position in the map view. Equivalent to nested for loop
        /// for (y = 0...) for (x = 0...)
        /// </summary>
        /// <typeparam name="T">Type of elements in the map view.</typeparam>
        /// <param name="mapView">Map view to iterate over positions for.  Never specified
        /// manually since this is an extension method.</param>
        /// <returns>All positions in the IMapView.</returns>
        public static IEnumerable<Coord> Positions<T>(this IMapView<T> mapView)
        {
            for (int y = 0; y < mapView.Height; y++)
                for (int x = 0; x < mapView.Width; x++)
                    yield return Coord.Get(x, y);
        }
    }
}
