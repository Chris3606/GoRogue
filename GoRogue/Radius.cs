namespace GoRogue
{
    /// <summary>
    /// Enum representing types.  Useful for easy mapping of radius types to a primitive type (for cases like a switch statement).
    /// </summary>
    public enum RadiusType
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

    // TODO: Potentially a crapton more utility stuff to add here.  Probably Get around to it closer to LOS/area of effect libs.
    /// <summary>
    /// Class representing different radius types.  Similar in architecture to Coord in architecture -- it cannot be instantiated. Instead it simply has pre-allocated static variables
    /// for each type of radius, that should be used whenever a variable of type Radius is required.
    /// </summary>
    /// <remarks>
    /// Also contains utility functions to work with radius types, and is also implicitly convertible to the Distance class.
    /// </remarks>
    public class Radius
    {
        //static readonly double PI2 = Math.PI * 2;

        /// <summary>
        /// Enum type corresponding to radius type being represented.
        /// </summary>
        public readonly RadiusType Type;

        /// <summary>
        /// Radius is a square around the center point.  Shape that would represent movement radius in an 8-way
        /// movement scheme, with no additional cost on diagonal movement.
        /// </summary>
        public static readonly Radius SQUARE = new Radius(RadiusType.SQUARE);

        /// <summary>
        /// Radius is a diamond around the center point.  Shape that would represent movement radius in a 4-way
        /// movement scheme.
        /// </summary>
        public static readonly Radius DIAMOND = new Radius(RadiusType.DIAMOND);

        /// <summary>
        /// Radius is a circle around the center point.  Shape that would represent movement radius in an 8-way
        /// movement scheme, with all movement cost the same based upon distance from the source.
        /// </summary>
        public static readonly Radius CIRCLE = new Radius(RadiusType.CIRCLE);

        /// <summary>
        /// Radius is a cube around the center point.  Similar to SQUARE in 2d shape.
        /// </summary>
        public static readonly Radius CUBE = new Radius(RadiusType.CUBE);

        /// <summary>
        /// Radius is an octahedron around the center point.  Similar to DIAMOND in 2d shape.
        /// </summary>
        public static readonly Radius OCTAHEDRON = new Radius(RadiusType.OCTAHEDRON);

        /// <summary>
        /// Radius is a sphere around the center point.  Similar to CIRCLE in 2d shape.
        /// </summary>
        public static readonly Radius SPHERE = new Radius(RadiusType.SPHERE);

        /// <summary>
        /// Gets the Radius class instance representing the distance type specified.
        /// </summary>
        /// <param name="radiusType">The enum value for the distance method.</param>
        /// <returns>The radius class representing the given distance calculation.</returns>
        public static Radius ToRadius(RadiusType radiusType)
        {
            switch (radiusType)
            {
                case RadiusType.CIRCLE:
                    return CIRCLE;

                case RadiusType.CUBE:
                    return CUBE;

                case RadiusType.DIAMOND:
                    return DIAMOND;

                case RadiusType.OCTAHEDRON:
                    return OCTAHEDRON;

                case RadiusType.SPHERE:
                    return SPHERE;

                case RadiusType.SQUARE:
                    return SQUARE;

                default:
                    return null; // Will never occur
            }
        }

        /// <summary>
        /// Allows explicit casting to Distance type.  The distance corresponding to the proper definition of distance to create
        /// the Radius casted will be retrieved.
        /// </summary>
        /// <param name="radius">Radius type being casted.</param>
        public static explicit operator Distance(Radius radius)
        {
            switch (radius.Type)
            {
                case RadiusType.CIRCLE:
                case RadiusType.SPHERE:
                    return Distance.EUCLIDIAN;

                case RadiusType.DIAMOND:
                case RadiusType.OCTAHEDRON:
                    return Distance.MANHATTAN;

                case RadiusType.SQUARE:
                case RadiusType.CUBE:
                    return Distance.CHEBYSHEV;

                default:
                    return null; // Will not occur
            }
        }

        private Radius(RadiusType type)
        {
            Type = type;
        }
    }
}