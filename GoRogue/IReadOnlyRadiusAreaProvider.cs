using System.Collections.Generic;

namespace GoRogue
{
    /// <summary>
    /// Read-only interface of a RadiusAreaProvider.
    /// </summary>
    public interface IReadOnlyRadiusAreaProvider
    {
        /// <summary>
        /// The bounds to constrain the returned Coords to. Set to Rectangle.EMPTY to indicate that
        /// there are no bounds.
        /// </summary>
        Rectangle Bounds { get; }

        /// <summary>
        /// The center point of the radius.
        /// </summary>
        Coord Center { get; }

        /// <summary>
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to Distance, eg. Radius).
        /// </summary>
        Distance DistanceCalc { get; }

        /// <summary>
        /// The length of the radius, eg. the number of tiles from the center point (as defined by
        /// the distance calculation/radius shape given) to which the radius extends.
        /// </summary>
        int Radius { get; }

        /// <summary>
        /// Calculates the new radius, and returns an IEnumerable of all unique Coords within that
        /// radius and bounds specified (as applicable). See RadiusAreaProvider class description for
        /// details on the ordering. Safe to expose in read-only class, since it does not modify the
        /// public interface of the RadiusAreaProvider.
        /// </summary>
        /// <returns>Enumerable of all unique Coords within the radius and bounds specified.</returns>
        IEnumerable<Coord> CalculatePositions();
    }
}