using System;

namespace GoRogue
{
	/// <summary>
	/// Class representing methods of calculating distance on a grid. You cannot create instances of this
	/// class using a constructor -- instead this class contains static instances representing the
	/// various distance calculations.
	/// </summary>
	/// <remarks>
	/// Provides functions that calculate the distance between two points according to the distance
	/// measurement being used. Instances of Distance are also implicitly convertible to <see cref="AdjacencyRule"/>
	/// types (since an adjacency method is implied by a distance calculation), and they may also be explicitly
	/// casted to the appropriate 2D <see cref="Radius"/> types.
	/// </remarks>
	public class Distance
	{
		/// <summary>
		/// Represents chebyshev distance (equivalent to 8-way movement with no extra cost for diagonals).
		/// </summary>
		public static Distance CHEBYSHEV = new Distance(Types.CHEBYSHEV);

		/// <summary>
		/// Represents euclidean distance (equivalent to 8-way movement with ~1.41 movement cost for diagonals).
		/// </summary>
		public static Distance EUCLIDEAN = new Distance(Types.EUCLIDEAN);

		/// <summary>
		/// Represents manhattan distance (equivalent to 4-way, cardinal-only movement).
		/// </summary>
		public static Distance MANHATTAN = new Distance(Types.MANHATTAN);

		/// <summary>
		/// Enum value representing the method of calcuating distance -- useful for using
		/// Distance types in switch statements.
		/// </summary>
		public readonly Types Type;

		private static readonly string[] writeVals = Enum.GetNames(typeof(Types));

		private Distance(Types type)
		{
			Type = type;
		}

		/// <summary>
		/// Enum representing Distance types. Each Distance instance has a <see cref="Type"/> field
		/// which contains the corresponding value from this enum.  Useful for easy mapping of Distance
		/// types to a primitive type (for cases like a switch statement).
		/// </summary>
		public enum Types
		{
			/// <summary>
			/// Enum type for <see cref="Distance.MANHATTAN"/>.
			/// </summary>
			MANHATTAN,

			/// <summary>
			/// Enum type for <see cref="Distance.EUCLIDEAN"/>.
			/// </summary>
			EUCLIDEAN,

			/// <summary>
			/// Enum type for <see cref="Distance.CHEBYSHEV"/>.
			/// </summary>
			CHEBYSHEV
		};

		/// <summary>
		/// Allows explicit casting to <see cref="Radius"/> type.
		/// </summary>
		/// <remarks>
		/// The 2D radius shape corresponding to the definition of a radius according to the distance calculation
		/// casted will be returned.
		/// </remarks>
		/// <param name="distance"/>
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
		/// Allows implicit casting to the <see cref="AdjacencyRule"/> type. 
		/// </summary>
		/// <remarks>
		/// The adjacency rule corresponding to the definition of a adjacency according to the
		/// distance calculation casted will be returned.
		/// </remarks>
		/// <param name="distance"/>
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
		/// <param name="distanceType">The enum value for the distance calculation method.</param>
		/// <returns>The Distance class representing the given distance calculation.</returns>
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
		public double Calculate(int startX, int startY, int startZ, int endX, int endY, int endZ)
		{
			return Calculate((double)startX, (double)startY, (double)startZ, (double)endX, (double)endY, (double)endZ);
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
		public double Calculate(double startX, double startY, double startZ, double endX, double endY, double endZ)
		{
			double dx = Math.Abs(startX - endX);
			double dy = Math.Abs(startY - endY);
			double dz = Math.Abs(startZ - endZ);
			return Calculate(dx, dy, dz);
		}

		/// <summary>
		/// Returns the distance between the two (2D) points specified.
		/// </summary>
		/// <param name="startX">X-Coordinate of the starting point.</param>
		/// <param name="startY">Y-Coordinate of the starting point.</param>
		/// <param name="endX">X-Coordinate of the ending point.</param>
		/// <param name="endY">Y-Coordinate of the ending point.</param>
		/// <returns>The distance between the two points.</returns>
		public double Calculate(int startX, int startY, int endX, int endY) => Calculate((double)startX, (double)startY, (double)endX, (double)endY);

		/// <summary>
		/// Returns the distance between the two (2D) points specified.
		/// </summary>
		/// <param name="start">Starting point.</param>
		/// <param name="end">Ending point.</param>
		/// <returns>The distance between the two points.</returns>
		public double Calculate(Coord start, Coord end) => Calculate((double)start.X, (double)start.Y, (double)end.X, (double)end.Y);

		/// <summary>
		/// Returns the distance between the two (2D) points specified.
		/// </summary>
		/// <param name="startX">X-Coordinate of the starting point.</param>
		/// <param name="startY">Y-Coordinate of the starting point.</param>
		/// <param name="endX">X-Coordinate of the ending point.</param>
		/// <param name="endY">Y-Coordinate of the ending point.</param>
		/// <returns>The distance between the two points.</returns>
		public double Calculate(double startX, double startY, double endX, double endY)
		{
			double dx = startX - endX;
			double dy = startY - endY;
			return Calculate(dx, dy);
		}

		/// <summary>
		/// Returns the distance between two locations, given the change in X and change in Y value
		/// (specified by the X and Y values of the given vector).
		/// </summary>
		/// <param name="deltaChange">The delta-x and delta-y between the two locations.</param>
		/// ///
		/// <returns>The distance between two locations withe the given delta-change values.</returns>
		public double Calculate(Coord deltaChange) => Calculate((double)deltaChange.X, (double)deltaChange.Y);

		/// <summary>
		/// Returns the distance between two locations, given the change in X and change in Y value.
		/// </summary>
		/// <param name="dx">The delta-x between the two locations.</param>
		/// <param name="dy">The delta-y between the two locations.</param>
		/// <returns>The distance between two locations with the given delta-change values.</returns>
		public double Calculate(int dx, int dy) => Calculate((double)dx, (double)dy);

		/// <summary>
		/// Returns the distance between two locations, given the change in X and change in Y value.
		/// </summary>
		/// <param name="dx">The delta-x between the two locations.</param>
		/// <param name="dy">The delta-y between the two locations.</param>
		/// <returns>The distance between two locations with the given delta-change values.</returns>
		public double Calculate(double dx, double dy) => Calculate(dx, dy, 0);

		/// <summary>
		/// Returns the distance between two locations, given the change in X, Y, and Z value.
		/// </summary>
		/// <param name="dx">The delta-x between the two locations.</param>
		/// <param name="dy">The delta-y between the two locations.</param>
		/// <param name="dz">The delta-z between the two locations.</param>
		/// <returns>The distance between the locations with the given delta-change values.</returns>
		public double Calculate(int dx, int dy, int dz) => Calculate((float)dx, (float)dy, (float)dz);

		/// <summary>
		/// Returns the distance between two locations, given the change in X, Y, and Z value.
		/// </summary>
		/// <param name="dx">The delta-x between the two locations.</param>
		/// <param name="dy">The delta-y between the two locations.</param>
		/// <param name="dz">The delta-z between the two locations.</param>
		/// <returns>The distance between two locations with the given delta-change values.</returns>
		public double Calculate(double dx, double dy, double dz)
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
		/// Returns a string representation of the distance calculation method represented.
		/// </summary>
		/// <returns>A string representation of the distance method represented.</returns>
		public override string ToString() => writeVals[(int)Type];
	}
}
