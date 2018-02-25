using System;

namespace GoRogue
{
    // TODO: Potentially a crapton more utility stuff to add here. Probably Get around to it closer
    //       to FOV/area of effect libs.
    /// <summary>
    /// Class representing different radius types. Similar in architecture to Coord in architecture
    /// -- it cannot be instantiated. Instead it simply has pre-allocated static variables for each
    /// type of radius, that should be used whenever a variable of type Radius is required.
    /// </summary>
    /// <remarks>
    /// Also contains utility functions to work with radius types, and is also implicitly convertible
    /// to the Distance class.
    /// </remarks>
    public class Radius
    {
        private static readonly string[] writeVals = Enum.GetNames(typeof(Types));
        /// <summary>
        /// Radius is a circle around the center point. Shape that would represent movement radius in
        /// an 8-way movement scheme, with all movement cost the same based upon distance from the source.
        /// </summary>
        public static readonly Radius CIRCLE = new Radius(Types.CIRCLE);

        /// <summary>
        /// Radius is a cube around the center point. Similar to SQUARE in 2d shape.
        /// </summary>
        public static readonly Radius CUBE = new Radius(Types.CUBE);

        /// <summary>
        /// Radius is a diamond around the center point. Shape that would represent movement radius
        /// in a 4-way movement scheme.
        /// </summary>
        public static readonly Radius DIAMOND = new Radius(Types.DIAMOND);

        /// <summary>
        /// Radius is an octahedron around the center point. Similar to DIAMOND in 2d shape.
        /// </summary>
        public static readonly Radius OCTAHEDRON = new Radius(Types.OCTAHEDRON);

        /// <summary>
        /// Radius is a sphere around the center point. Similar to CIRCLE in 2d shape.
        /// </summary>
        public static readonly Radius SPHERE = new Radius(Types.SPHERE);

        /// <summary>
        /// Radius is a square around the center point. Shape that would represent movement radius in
        /// an 8-way movement scheme, with no additional cost on diagonal movement.
        /// </summary>
        public static readonly Radius SQUARE = new Radius(Types.SQUARE);

        /// <summary>
        /// Enum type corresponding to radius type being represented.
        /// </summary>
        public readonly Types Type;

        private Radius(Types type)
        {
            Type = type;
        }

        /// <summary>
        /// Enum representing Radius types. Useful for easy mapping of radius types to a primitive
        /// type (for cases like a switch statement).
        /// </summary>
        public enum Types
        {
            /// <summary>
            /// Type for Radius.SQUARE.
            /// </summary>
            SQUARE,

            /// <summary>
            /// Type for Radius.DIAMOND.
            /// </summary>
            DIAMOND,

            /// <summary>
            /// Type for Radius.CIRCLE.
            /// </summary>
            CIRCLE,

            /// <summary>
            /// Type for Radius.CUBE.
            /// </summary>
            CUBE,

            /// <summary>
            /// Type for Radius.OCTAHEDRON.
            /// </summary>
            OCTAHEDRON,

            /// <summary>
            /// Type for Radius.SPHERE.
            /// </summary>
            SPHERE
        };

        /// <summary>
        /// Allows implicit casting to AdjacencyRule type. The rule corresponding to the proper
        /// definition of distance to create the Radius casted will be retrieved.
        /// </summary>
        /// <param name="radius">Radius type being casted.</param>
        public static implicit operator AdjacencyRule(Radius radius)
        {
            switch (radius.Type)
            {
                case Types.CIRCLE:
                case Types.SPHERE:
                case Types.SQUARE:
                case Types.CUBE:
                    return AdjacencyRule.EIGHT_WAY;

                case Types.DIAMOND:
                case Types.OCTAHEDRON:
                    return AdjacencyRule.CARDINALS;

                default:
                    return null; // Will not occur
            }
        }

        /// <summary>
        /// Allows implicit casting to Distance type. The distance corresponding to the proper
        /// definition of distance to create the Radius casted will be retrieved.
        /// </summary>
        /// <param name="radius">Radius type being casted.</param>
        public static implicit operator Distance(Radius radius)
        {
            switch (radius.Type)
            {
                case Types.CIRCLE:
                case Types.SPHERE:
                    return Distance.EUCLIDEAN;

                case Types.DIAMOND:
                case Types.OCTAHEDRON:
                    return Distance.MANHATTAN;

                case Types.SQUARE:
                case Types.CUBE:
                    return Distance.CHEBYSHEV;

                default:
                    return null; // Will not occur
            }
        }

        /// <summary>
        /// Gets the Radius class instance representing the distance type specified.
        /// </summary>
        /// <param name="radiusType">The enum value for the distance method.</param>
        /// <returns>The radius class representing the given distance calculation.</returns>
        public static Radius ToRadius(Types radiusType)
        {
            switch (radiusType)
            {
                case Types.CIRCLE:
                    return CIRCLE;

                case Types.CUBE:
                    return CUBE;

                case Types.DIAMOND:
                    return DIAMOND;

                case Types.OCTAHEDRON:
                    return OCTAHEDRON;

                case Types.SPHERE:
                    return SPHERE;

                case Types.SQUARE:
                    return SQUARE;

                default:
                    return null; // Will never occur
            }
        }

        /// <summary>
        /// Returns a string representation of the Radius.
        /// </summary>
        /// <returns>A string representation of the Radius.</returns>
        public override string ToString() => writeVals[(int)Type];
    }
}