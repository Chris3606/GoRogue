using GoRogue.MapViews;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoRogue
{
	/// <summary>
	/// Class responsible for caculating basic FOV (see SenseMap for more advanced lighting).
	/// Effectively a simplified, slightly faster interface compared to SenseMap, that supports only
	/// a single source and only shadowcasting. This is more conducive to the typical use case for
	/// FOV. It can calculate the FOV with a finite or infinite max radius, and can use a variety of
	/// radius types, as specified in Radius class (all the same ones that SenseMap supports). It
	/// also supports both 360 degree FOV and a "field of view" (cone) FOV. One may access this class
	/// like a 2D array of doubles (FOV values), wherein the values will range from 0.0 to 1.0, where
	/// 1.0 means the corresponding map grid coordinate is at maximum visibility, and 0.0 means the
	/// cooresponding coordinate is outside of FOV entirely (not visible).
	/// </summary>
	public class FOV : IReadOnlyFOV, IMapView<double>
	{
		private HashSet<Coord> currentFOV;
		private double[,] light;
		private HashSet<Coord> previousFOV;

		private IMapView<double> resMap;
		// Last light cached

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="resMap">
		/// The resistance map to use to calculate FOV. Values of 1.0 are considered blocking to FOV,
		/// while other (lower) values are considered to be not blocking.
		/// </param>
		public FOV(IMapView<double> resMap)
		{
			this.resMap = resMap;
			light = null;
			currentFOV = new HashSet<Coord>();
			previousFOV = new HashSet<Coord>();
		}

		/// <summary>
		/// IEnumerable of only positions currently in FOV.
		/// </summary>
		public IEnumerable<Coord> CurrentFOV { get => currentFOV; }

		/// <summary>
		/// Height of FOV map.
		/// </summary>
		public int Height { get => resMap.Height; }

		/// <summary>
		/// IEnumerable of positions that are in FOV as of the most current Calculate call, but were
		/// NOT in FOV afterthe previous time Calculate was called.
		/// </summary>
		public IEnumerable<Coord> NewlySeen { get => currentFOV.Where(pos => !previousFOV.Contains(pos)); }

		/// <summary>
		/// IEnumerable of positions that are NOT in FOV as of the most current Calculate call, but
		/// WERE in FOV after the previous time Calculate was called.
		/// </summary>
		public IEnumerable<Coord> NewlyUnseen { get => previousFOV.Where(pos => !currentFOV.Contains(pos)); }

		/// <summary>
		/// Width of FOV map.
		/// </summary>
		public int Width { get => resMap.Width; }

		/// <summary>
		/// Array-style indexer that takes a Coord as the index, and retrieves the FOV value at the
		/// given location.
		/// </summary>
		/// <param name="position">The position to retrieve the FOV value for.</param>
		/// <returns>The FOV value at the given location.</returns>
		public double this[Coord position]
		{
			get { return light[position.X, position.Y]; }
		}

		/// <summary>
		/// Array-style indexer that takes an x and y value as the index, and retrieves the FOV value
		/// at the given location.
		/// </summary>
		/// <param name="x">The x-coordinate of the position to retrieve the FOV value for.</param>
		/// <param name="y">The y-coordinate of the position to retrieve the FOV value for.</param>
		/// <returns>The FOV value at (x, y).</returns>
		public double this[int x, int y]
		{
			get { return light[x, y]; }
		}

		/// <summary>
		/// Returns a read-only representation of the fov.
		/// </summary>
		/// <returns>This fov object, exposed as an IReadOnlyFOV.</returns>
		public IReadOnlyFOV AsReadOnly() => this;

		// Since the values aren't compile-time constants, we have to do it this way (with overloads,
		// vs. default values).
		/// <summary>
		/// Calculates FOV, given an origin point of (startX, startY), with a given radius. If no
		/// radius is specified, simply calculates with a radius of maximum integer value, which is
		/// effectively infinite. Radius is computed as a circle around the source (type Radius.CIRCLE).
		/// </summary>
		/// <param name="startX">Coordinate x-value of the origin.</param>
		/// <param name="startY">Coordinate y-value of the origin.</param>
		/// <param name="radius">
		/// The maximum radius -- basically the maximum distance of FOV if completely unobstructed.
		/// If no radius is specified, it is effectively infinite.
		/// </param>
		public void Calculate(int startX, int startY, int radius = int.MaxValue) => Calculate(startX, startY, radius, Radius.CIRCLE);

		/// <summary>
		/// Calculates FOV, given an origin point, with a given radius. If no radius is specified,
		/// simply calculates with a radius of maximum integer value, which is effectively infinite.
		/// Radius is computed as a circle around the source (type Radius.CIRCLE).
		/// </summary>
		/// <param name="start">Position of FOV origin.</param>
		/// <param name="radius">
		/// The maximum radius -- basically the maximum distance of FOV if completely unobstructed.
		/// If no radius is specified, it is effectively infinite.
		/// </param>
		public void Calculate(Coord start, int radius = int.MaxValue) => Calculate(start.X, start.Y, radius, Radius.CIRCLE);

		/// <summary>
		/// Calculates FOV, given an origin point of (startX, startY), with the given radius and
		/// radius calculation strategy.
		/// </summary>
		/// <param name="startX">Coordinate x-value of the origin.</param>
		/// <param name="startY">Coordinate y-value of the origin.</param>
		/// <param name="radius">
		/// The maximum radius -- basically the maximum distance of FOV if completely unobstructed.
		/// </param>
		/// <param name="distanceCalc">
		/// The distance calculation used to determine what shape the radius has (or a type
		/// implicitly convertible to Distance, eg. Radius).
		/// </param>
		public void Calculate(int startX, int startY, int radius, Distance distanceCalc)
		{
			double rad = Math.Max(1, radius);
			double decay = 1.0 / (rad + 1);

			previousFOV = currentFOV;
			currentFOV = new HashSet<Coord>();

			initializeLightMap();
			light[startX, startY] = 1; // Full power to starting space
			currentFOV.Add(Coord.Get(startX, startY));

			foreach (Direction d in AdjacencyRule.DIAGONALS.DirectionsOfNeighbors())
			{
				shadowCast(1, 1.0, 0.0, 0, d.DeltaX, d.DeltaY, 0, (int)rad, startX, startY, decay, light, currentFOV, resMap, distanceCalc);
				shadowCast(1, 1.0, 0.0, d.DeltaX, 0, 0, d.DeltaY, (int)rad, startX, startY, decay, light, currentFOV, resMap, distanceCalc);
			}
		}

		/// <summary>
		/// Calculates FOV, given an origin point, with the given radius and radius calculation strategy.
		/// </summary>
		/// <param name="start">Coordinate of the origin.</param>
		/// <param name="radius">
		/// The maximum radius -- basically the maximum distance of FOV if completely unobstructed.
		/// </param>
		/// <param name="distanceCalc">
		/// The distance calculation used to determine what shape the radius has (or a type
		/// implicitly convertible to Distance, eg. Radius).
		/// </param>
		public void Calculate(Coord start, int radius, Distance distanceCalc) => Calculate(start.X, start.Y, radius, distanceCalc);

		/// <summary>
		/// Calculates FOV, given an origin point of (startX, startY), with the given radius and
		/// radius calculation strategy, and assuming FOV is restricted to the area specified by the
		/// given angle and span, in degrees. Provided that span is greater than 0, a conical section
		/// of the regular FOV radius will be actually in FOV.
		/// </summary>
		/// <param name="startX">Coordinate x-value of the origin.</param>
		/// <param name="startY">Coordinate y-value of the origin.</param>
		/// <param name="radius">
		/// The maximum radius -- basically the maximum distance of FOV if completely unobstructed.
		/// </param>
		/// <param name="distanceCalc">
		/// The distance calculation used to determine what shape the radius has (or a type
		/// implicitly convertible to Distance, eg. Radius).
		/// </param>
		/// <param name="angle">
		/// The angle in degrees that specifies the outermost center point of the FOV cone. 0 degrees
		/// points right.
		/// </param>
		/// <param name="span">
		/// The angle, in degrees, that specifies the full arc contained in the FOV cone -- angle/2
		/// degrees are included on either side of the span line.
		/// </param>
		public void Calculate(int startX, int startY, int radius, Distance distanceCalc, double angle, double span)
		{
			double rad = Math.Max(1, radius);
			double decay = 1.0 / (rad + 1);

			double angle2 = MathHelpers.ToRadian((angle > 360.0 || angle < 0.0) ? Math.IEEERemainder(angle + 720.0, 360.0) : angle);
			double span2 = MathHelpers.ToRadian(span);

			previousFOV = currentFOV;
			currentFOV = new HashSet<Coord>();

			initializeLightMap();
			light[startX, startY] = 1; // Starting space full light
			currentFOV.Add(Coord.Get(startX, startY));

			// TODO: There may be a glitch here with too large radius, may have to set to longest
			// possible straight-line Manhattan dist for map intsead. No falloff issue -- shadow is on/off.
			int ctr = 0;
			bool started = false;
			foreach (Direction d in AdjacencyRule.DIAGONALS.DirectionsOfNeighborsCounterClockwise(Direction.UP_RIGHT))
			{
				ctr %= 4;
				ctr++;
				if (angle <= Math.PI / 2.0 * ctr + span / 2.0)
					started = true;

				if (started)
				{
					if (ctr < 4 && angle < Math.PI / 2.0 * (ctr - 1) - span / 2.0)
						break;

					light = shadowCastLimited(1, 1.0, 0.0, 0, d.DeltaX, d.DeltaY, 0, (int)rad, startX, startY, decay, light, resMap, distanceCalc, angle2, span2);
					light = shadowCastLimited(1, 1.0, 0.0, d.DeltaX, 0, 0, d.DeltaY, (int)rad, startX, startY, decay, light, resMap, distanceCalc, angle2, span2);
				}
			}
		}

		/// <summary>
		/// Calculates FOV, given an origin point of (startX, startY), with the given radius and
		/// radius calculation strategy, and assuming FOV is restricted to the area specified by the
		/// given angle and span, in degrees. Provided that span is greater than 0, a conical section
		/// of the regular FOV radius will be actually in FOV.
		/// </summary>
		/// <param name="start">Coordinate of the origin.</param>
		/// <param name="radius">
		/// The maximum radius -- basically the maximum distance of FOV if completely unobstructed.
		/// </param>
		/// <param name="distanceCalc">
		/// The distance calculation used to determine what shape the radius has (or a type
		/// implicitly convertible to Distance, eg. Radius).
		/// </param>
		/// <param name="angle">
		/// The angle in degrees that specifies the outermost center point of the FOV cone. 0 degrees
		/// points right.
		/// </param>
		/// <param name="span">
		/// The angle, in degrees, that specifies the full arc contained in the FOV cone -- angle/2
		/// degrees are included on either side of the span line.
		/// </param>
		public void Calculate(Coord start, int radius, Distance distanceCalc, double angle, double span) => Calculate(start.X, start.Y, radius, distanceCalc, angle, span);

		// Warning intentionally disabled -- see SenseMap.ToString for details as to why this is not bad.
#pragma warning disable RECS0137

		/// <summary>
		/// ToString overload that customizes the characters used to represent the map.
		/// </summary>
		/// <param name="normal">The character used for any location not in FOV.</param>
		/// <param name="sourceValue">The character used for any location that is in FOV.</param>
		/// <returns>The string representation of FOV, using the specified characters.</returns>
		public string ToString(char normal = '-', char sourceValue = '+')
#pragma warning restore RECS0137
		{
			string result = "";

			for (int y = 0; y < resMap.Height; y++)
			{
				for (int x = 0; x < resMap.Width; x++)
				{
					result += (light[x, y] > 0.0) ? sourceValue : normal;
					result += " ";
				}

				result += '\n';
			}

			return result;
		}

		/// <summary>
		/// Returns a string representation of the map, with the actual values in the FOV, rounded to
		/// the given number of decimal places.
		/// </summary>
		/// <param name="decimalPlaces">The number of decimal places to round to.</param>
		/// <returns>A string representation of FOV, rounded to the given number of decimal places.</returns>
		public string ToString(int decimalPlaces) => light.ExtendToStringGrid(elementStringifier: (double obj) => obj.ToString("0." + "0".Multiply(decimalPlaces)));

		/// <summary>
		/// Returns a string representation of the map, where any location not in FOV is represented
		/// by a '-' character, and any position in FOV is represented by a '+'.
		/// </summary>
		/// <returns>A (multi-line) string representation of the FOV.</returns>
		public override string ToString() => ToString();

		// Returns value because its recursive
		private static double[,] shadowCast(int row, double start, double end, int xx, int xy, int yx, int yy,
									 int radius, int startX, int startY, double decay, double[,] lightMap, HashSet<Coord> fovSet,
									 IMapView<double> map, Distance distanceStrategy)
		{
			double newStart = 0;
			if (start < end)
				return lightMap;

			bool blocked = false;
			for (int distance = row; distance <= radius && distance < map.Width + map.Height && !blocked; distance++)
			{
				int deltaY = -distance;
				for (int deltaX = -distance; deltaX <= 0; deltaX++)
				{
					int currentX = startX + deltaX * xx + deltaY * xy;
					int currentY = startY + deltaX * yx + deltaY * yy;
					double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
					double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

					if (!(currentX >= 0 && currentY >= 0 && currentX < map.Width && currentY < map.Height) || start < rightSlope)
						continue;

					if (end > leftSlope)
						break;

					double deltaRadius = distanceStrategy.Calculate(deltaX, deltaY);
					// If within lightable area, light if needed
					if (deltaRadius <= radius)
					{
						double bright = 1 - decay * deltaRadius;
						lightMap[currentX, currentY] = bright;
						if (bright > 0.0)
							fovSet.Add(Coord.Get(currentX, currentY));
					}

					if (blocked) // Previous cell was blocked
					{
						if (map[currentX, currentY] >= 1) // Hit a wall...
							newStart = rightSlope;
						else
						{
							blocked = false;
							start = newStart;
						}
					}
					else
					{
						if (map[currentX, currentY] >= 1 && distance < radius) // Wall within sight line
						{
							blocked = true;
							lightMap = shadowCast(distance + 1, start, leftSlope, xx, xy, yx, yy, radius, startX, startY, decay, lightMap, fovSet, map, distanceStrategy);
							newStart = rightSlope;
						}
					}
				}
			}
			return lightMap;
		}

		private static double[,] shadowCastLimited(int row, double start, double end, int xx, int xy, int yx, int yy, int radius, int startX, int startY, double decay,
												   double[,] lightMap, IMapView<double> map, Distance distanceStrategy, double angle, double span)
		{
			double newStart = 0;
			if (start < end)
				return lightMap;

			bool blocked = false;
			for (int distance = row; distance <= radius && distance < map.Width + map.Height && !blocked; distance++)
			{
				int deltaY = -distance;
				for (int deltaX = -distance; deltaX <= 0; deltaX++)
				{
					int currentX = startX + deltaX * xx + deltaY * xy;
					int currentY = startY + deltaX * yx + deltaY * yy;
					double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
					double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

					if (!(currentX >= 0 && currentY >= 0 && currentX < map.Width && currentY < map.Height) || start < rightSlope)
						continue;

					if (end > leftSlope)
						break;

					double newAngle = Math.Atan2(currentY - startY, currentX - startX) + Math.PI * 2;
					if (Math.Abs(Math.IEEERemainder(angle - newAngle + Math.PI * 8, Math.PI * 2)) > span / 2.0)
						continue;

					double deltaRadius = distanceStrategy.Calculate(deltaX, deltaY);
					if (deltaRadius <= radius) // Check if within lightable area, light if needed
					{
						double bright = 1 - decay * deltaRadius;
						lightMap[currentX, currentY] = bright;
					}

					if (blocked) // Previous cell was blocking
					{
						if (map[currentX, currentY] >= 1) // We hit a wall
							newStart = rightSlope;
						else
						{
							blocked = false;
							start = newStart;
						}
					}
					else
					{
						if (map[currentX, currentY] >= 1 && distance < radius) // Wall within line of sight
						{
							blocked = true;
							lightMap = shadowCastLimited(distance + 1, start, leftSlope, xx, xy, yx, yy, radius, startX, startY, decay, lightMap, map, distanceStrategy, angle, span);
							newStart = rightSlope;
						}
					}
				}
			}
			return lightMap;
		}

		private void initializeLightMap()
		{
			if (light == null || light.GetLength(0) != resMap.Width || light.GetLength(1) != resMap.Height)
				light = new double[resMap.Width, resMap.Height];
			else
				Array.Clear(light, 0, light.Length);
		}
	}
}