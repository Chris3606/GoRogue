using System;

namespace GoRogue
{
	/// <summary>
	/// Class representing different shapes that define the concept of a radius on a grid. You cannot
	/// create instances of this class using a constructor -- instead, this class contains static instances
	/// representing the various radius shapes.
	/// </summary>
	/// <remarks>
	/// Contains utility functions to work with radius shapes.  Instances of Radius are also implicitly
	/// convertible to both <see cref="Distance"/> and <see cref="AdjacencyRule"/> (since both a method
	/// of determining adjacent locations and a method of calculating distance are implied by a radius
	/// shape).
	/// </remarks>
	[Serializable]
	public class Radius : IEquatable<Radius>
	{
		/// <summary>
		/// Radius is a circle around the center point. CIRCLE would represent movement radius in
		/// an 8-way movement scheme with a ~1.41 cost multiplier for diagonal movement.
		/// </summary>
		[NonSerialized]
		public static readonly Radius CIRCLE = new Radius(Types.CIRCLE);

		/// <summary>
		/// Radius is a cube around the center point. Similar to SQUARE in 2d shape.
		/// </summary>
		[NonSerialized]
		public static readonly Radius CUBE = new Radius(Types.CUBE);

		/// <summary>
		/// Radius is a diamond around the center point. DIAMOND would represent movement radius
		/// in a 4-way movement scheme.
		/// </summary>
		[NonSerialized]
		public static readonly Radius DIAMOND = new Radius(Types.DIAMOND);

		/// <summary>
		/// Radius is an octahedron around the center point. Similar to DIAMOND in 2d shape.
		/// </summary>
		[NonSerialized]
		public static readonly Radius OCTAHEDRON = new Radius(Types.OCTAHEDRON);

		/// <summary>
		/// Radius is a sphere around the center point. Similar to CIRCLE in 2d shape.
		/// </summary>
		[NonSerialized]
		public static readonly Radius SPHERE = new Radius(Types.SPHERE);

		/// <summary>
		/// Radius is a square around the center point. SQUARE would represent movement radius in
		/// an 8-way movement scheme, where all 8 squares around an item are considered equal distance
		/// away.
		/// </summary>
		[NonSerialized]
		public static readonly Radius SQUARE = new Radius(Types.SQUARE);

		/// <summary>
		/// Enum value representing the radius shape -- useful for using Radius types in switch
		/// statements.
		/// </summary>
		public readonly Types Type;

		[NonSerialized]
		private static readonly string[] writeVals = Enum.GetNames(typeof(Types));

		private Radius(Types type)
		{
			Type = type;
		}

		/// <summary>
		/// Enum representing Radius types. Each Radius instance has a <see cref="Type"/> field
		/// which contains the corresponding value from this enum.  Useful for easy mapping of Radius
		/// types to a primitive type (for cases like a switch statement).
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
		/// Compares the current Radius to the object given.
		/// </summary>
		/// <param name="obj"/>
		/// <returns>True if the given object is a Radius with the same Type, false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Radius e)
				return Equals(e);

			return false;
		}

		/// <summary>
		/// Compares the current Radius to the one given.
		/// </summary>
		/// <param name="other"/>
		/// <returns>True if the given Radius has the same Type, false otherwise.</returns>
		public bool Equals(Radius other) => !ReferenceEquals(other, null) && Type == other.Type;

		/// <summary>
		/// Returns a hash-value for this object.
		/// </summary>
		/// <returns/>
		public override int GetHashCode() => Type.GetHashCode();

		/// <summary>
		/// Compares the two Radius instances.
		/// </summary>
		/// <param name="lhs"/>
		/// <param name="rhs"/>
		/// <returns>True if the two given Radius instances have the same Type, false otherwise.</returns>
		public static bool operator ==(Radius lhs, Radius rhs) => lhs?.Equals(rhs) ?? rhs is null;

		/// <summary>
		/// Compares the two Radius instances.
		/// </summary>
		/// <param name="lhs"/>
		/// <param name="rhs"/>
		/// <returns>True if the two given Radius instances do NOT have the same Type, false otherwise.</returns>
		public static bool operator !=(Radius lhs, Radius rhs) => !(lhs == rhs);

		/// <summary>
		/// Allows implicit casting to the <see cref="AdjacencyRule"/> type.
		/// </summary>
		/// <remarks>
		/// The rule corresponding to the proper definition of distance that creates the
		/// radius shape casted will be returned.
		/// </remarks>
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
		/// Allows implicit casting to the <see cref="Distance"/> type.
		/// </summary>
		/// <remarks>
		/// The <see cref="Distance"/> instance corresponding to the proper definition of
		/// distance that creates the radius shape casted will be returned.
		/// </remarks>
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
		/// Gets the Radius class instance representing the radius type specified.
		/// </summary>
		/// <param name="radiusType">The enum value for the radius shape.</param>
		/// <returns>The radius class representing the given radius shape.</returns>
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
