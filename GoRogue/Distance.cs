using System;

namespace GoRogue
{
    /// <summary>
    /// Class representing a method of calculating distance. You cannot create instances of this
    /// class using a constructor -- instead this class contains static instances representing the
    /// applicable types of distance calculations.
    /// </summary>
    /// <remarks>
    /// Provides functions that calculate the distance between two points according to the distance
    /// measurement being used. Distance instances can also be explicitly casted to Radius instances
    /// -- the 2D radius shape that corresponds to the shape of a "radius" according to the casted
    /// distance calculation is retrieved (eg, EUCLIDEAN casts to CIRCLE, MANHATTAN to DIAMOND, etc.)
    /// </remarks>
    public class Distance
    {
        private static readonly string[] writeVals = Enum.GetNames(typeof(Types));
        
        /// <summary>
        /// CHEBYSHEV distance (equivalent to 8-way movement with no extra cost for diagonals).
        /// </summary>
        public static Distance CHEBYSHEV = new Distance(Types.CHEBYSHEV);

        /// <summary>
        /// EUCLIDEAN distance (equivalent to 8-way movement with extra cost for diagonals).
        /// </summary>
        public static Distance EUCLIDEAN = new Distance(Types.EUCLIDEAN);

        /// <summary>
        /// MANHATTAN distance (equivalent to 4-way, cardinal-only movement).
        /// </summary>
        public static Distance MANHATTAN = new Distance(Types.MANHATTAN);

        /// <summary>
        /// Enum type for the distance calculation. Useful for conducting a switch statement on
        /// distance instances.
        /// </summary>
        public readonly Types Type;

        private Distance(Types type)
        {
            Type = type;
        }

        /// <summary>
        /// Enum for types used under the hood in Direction classes -- allows for convenient switch
        /// statements to be used with respect to directions.
        /// </summary>
        public enum Types
        {
            /// <summary>
            /// Enum type for Distance.MANHATTAN.
            /// </summary>
            MANHATTAN,

            /// <summary>
            /// Enum type for Distance.EUCLIDEAN.
            /// </summary>
            EUCLIDEAN,

            /// <summary>
            /// Enum type for Distance.CHEBYSHEV.
            /// </summary>
            CHEBYSHEV
        };

        /// <summary>
        /// Allows explicit casting to Radius type. The 2D radius shape corresponding to the
        /// definition of a radius according to the distance calculation casted will be retrieved.
        /// </summary>
        /// <param name="distance">Distance type being casted.</param>
        public static explicit operator Radius(Distance distance)
        {
            switch (distance.Type)
            {
                case Types.MANHATTAN:
                    return Radius.DIAMOND;

                case Types.EUCLIDEAN:
                    return Radius.CIRCLE;

                case Types.CHEBYSHEV:
                    return Radius.SQUARE;

                default:
                    return null; // Will not occur
            }
        }

        /// <summary>
        /// Allows implicit casting to the AdjacencyRule type. The adjacency rule corresponding to
        /// the definition of a radius according to the distance calculation casted will be retrieved.
        /// </summary>
        /// <param name="distance">Distance type being casted.</param>
        public static implicit operator AdjacencyRule(Distance distance)
        {
            switch (distance.Type)
            {
                case Types.MANHATTAN:
                    return AdjacencyRule.CARDINALS;

                case Types.CHEBYSHEV:
                case Types.EUCLIDEAN:
                    return AdjacencyRule.EIGHT_WAY;

                default:
                    return null; // Will not occur
            }
        }

        /// <summary>
        /// Gets the Distance class instance representing the distance type specified.
        /// </summary>
        /// <param name="distanceType">The enum value for the distance method.</param>
        /// <returns>The distance class representing the given distance calculation.</returns>
        public static Distance ToDistance(Types distanceType)
        {
            switch (distanceType)
            {
                case Types.MANHATTAN:
                    return MANHATTAN;

                case Types.EUCLIDEAN:
                    return EUCLIDEAN;

                case Types.CHEBYSHEV:
                    return CHEBYSHEV;

                default:
                    return null; // Will never occur
            }
        }

        /// <summary>
        /// Returns the distance between the two (3D) points specified.
        /// </summary>
        /// <param name="startX">X-Coordinate of the starting point.</param>
        /// <param name="startY">Y-Coordinate of the starting point.</param>
        /// <param name="startZ">Z-Coordinate of the starting point.</param>
        /// <param name="endX">X-Coordinate of the ending point.</param>
        /// <param name="endY">Y-Coordinate of the ending point.</param>
        /// <param name="endZ">Z-Coordinate of the ending point.</param>
        /// <returns>The distance between the two points.</returns>
        public double DistanceBetween(int startX, int startY, int startZ, int endX, int endY, int endZ)
        {
            return DistanceBetween((double)startX, (double)startY, (double)startZ, (double)endX, (double)endY, (double)endZ);
        }

        /// <summary>
        /// Returns the distance between the two (3D) points specified. Points are floating point
        /// instead of integer-values.
        /// </summary>
        /// <param name="startX">X-Coordinate of the starting point.</param>
        /// <param name="startY">Y-Coordinate of the starting point.</param>
        /// <param name="startZ">Z-Coordinate of the starting point.</param>
        /// <param name="endX">X-Coordinate of the ending point.</param>
        /// <param name="endY">Y-Coordinate of the ending point.</param>
        /// <param name="endZ">Z-Coordinate of the ending point.</param>
        /// <returns>The distance between the two points.</returns>
        public double DistanceBetween(double startX, double startY, double startZ, double endX, double endY, double endZ)
        {
            double dx = Math.Abs(startX - endX);
            double dy = Math.Abs(startY - endY);
            double dz = Math.Abs(startZ - endZ);
            return DistanceOf(dx, dy, dz);
        }

        /// <summary>
        /// Returns the distance between the two (2D) points specified.
        /// </summary>
        /// <param name="startX">X-Coordinate of the starting point.</param>
        /// <param name="startY">Y-Coordinate of the starting point.</param>
        /// <param name="endX">X-Coordinate of the ending point.</param>
        /// <param name="endY">Y-Coordinate of the ending point.</param>
        /// <returns>The distance between the two points.</returns>
        public double DistanceBetween(int startX, int startY, int endX, int endY) => DistanceBetween((double)startX, (double)startY, (double)endX, (double)endY);

        /// <summary>
        /// Returns the distance between the two (2D) points specified.
        /// </summary>
        /// <param name="start">Starting point.</param>
        /// <param name="end">Ending point.</param>
        /// <returns>The distance between the two points.</returns>
        public double DistanceBetween(Coord start, Coord end) => DistanceBetween((double)start.X, (double)start.Y, (double)end.X, (double)end.Y);

        /// <summary>
        /// Returns the distance between the two (2D) points specified. Points are floating point
        /// instead of integer-values.
        /// </summary>
        /// <param name="startX">X-Coordinate of the starting point.</param>
        /// <param name="startY">Y-Coordinate of the starting point.</param>
        /// <param name="endX">X-Coordinate of the ending point.</param>
        /// <param name="endY">Y-Coordinate of the ending point.</param>
        /// <returns>The distance between the two points.</returns>
        public double DistanceBetween(double startX, double startY, double endX, double endY)
        {
            double dx = startX - endX;
            double dy = startY - endY;
            return DistanceOf(dx, dy);
        }

        /// <summary>
        /// Returns the distance between two locations, given the change in X and change in Y value
        /// (specified by the X and Y values of the coordinate.
        /// </summary>
        /// <param name="end">The delta-x and delta-y between the two locations.</param>
        /// ///
        /// <returns>The distance between the two locations.</returns>
        public double DistanceOf(Coord end) => DistanceOf((double)end.X, (double)end.Y);

        /// <summary>
        /// Returns the distance between two locations, given the change in X and change in Y value.
        /// </summary>
        /// <param name="dx">The delta-x between the two locations.</param>
        /// <param name="dy">The delta-y between the two locations.</param>
        /// <returns>The distance between the two locations.</returns>
        public double DistanceOf(int dx, int dy) => DistanceOf((double)dx, (double)dy);

        /// <summary>
        /// Returns the distance between two locations, given the change in X and change in Y value.
        /// The change in X and Y are specified as floating point values rather than integers.
        /// </summary>
        /// <param name="dx">The delta-x between the two locations.</param>
        /// <param name="dy">The delta-y between the two locations.</param>
        /// <returns>The distance between the two locations.</returns>
        public double DistanceOf(double dx, double dy) => DistanceOf(dx, dy, 0);

        /// <summary>
        /// Returns the distance between two locations, given the change in X, Y, and Z value.
        /// </summary>
        /// <param name="dx">The delta-x between the two locations.</param>
        /// <param name="dy">The delta-y between the two locations.</param>
        /// <param name="dz">The delta-z between the two locations.</param>
        /// <returns>The distance between the two locations.</returns>
        public double DistanceOf(int dx, int dy, int dz) => DistanceOf((float)dx, (float)dy, (float)dz);

        /// <summary>
        /// Returns the distance between two locations, given the change in X, Y, and Z value. The
        /// change in X, Y, and Z are specified as floating point values rather than integers.
        /// </summary>
        /// <param name="dx">The delta-x between the two locations.</param>
        /// <param name="dy">The delta-y between the two locations.</param>
        /// <param name="dz">The delta-z between the two locations.</param>
        /// <returns>The distance between the two locations.</returns>
        public double DistanceOf(double dx, double dy, double dz)
        {
            dx = Math.Abs(dx);
            dy = Math.Abs(dy);
            dz = Math.Abs(dz);

            double radius = 0;
            switch (Type)
            {
                case Types.CHEBYSHEV:
                    radius = Math.Max(dx, Math.Max(dy, dz)); // Radius is the longest axial distance
                    break;

                case Types.MANHATTAN:
                    radius = dx + dy + dz; // Simply manhattan distance
                    break;

                case Types.EUCLIDEAN:
                    radius = Math.Sqrt(dx * dx + dy * dy + dz * dz); // Spherical radius
                    break;
            }
            return radius;
        }

        /// <summary>
        /// Returns a string representation of the Distance.
        /// </summary>
        /// <returns>A string representation of the Distance.</returns>
        public override string ToString() => writeVals[(int)Type];
    }
}