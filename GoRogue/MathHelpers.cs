using System;

namespace GoRogue
{
	/// <summary>
	/// Static class consisting of mathematical "helper" functions and constants -- things like angle
	/// unit conversions, and other helpful functions.
	/// </summary>
	public static class MathHelpers
	{
		/// <summary>
		/// Result of 1/360; represents in decimal form a percent of a circle that a degree constitutes.
		/// </summary>
		public const double DEGREE_PCT_OF_CIRCLE = 0.002777777777777778;

		/// <summary>
		/// Rounds the given number up (toward highest number), to the nearest multiple of the
		/// specified value.
		/// </summary>
		/// <param name="number">Number to round.</param>
		/// <param name="toMultipleOf">Number given is rounded up to nearest multiple of this number.</param>
		/// <returns>The number parameter, rouded up to the nearest multiple of toMultipleOf.</returns>
		public static int RoundToMultiple(int number, int toMultipleOf)
		{
			int isPositive = (number >= 0) ? 1 : 0;
			return ((number + isPositive * (toMultipleOf - 1)) / toMultipleOf) * toMultipleOf;
		}

		/// <summary>
		/// Converts given angle from radians to degrees.
		/// </summary>
		/// <param name="radAngle">Angle in radians.</param>
		/// <returns>The given angle in degrees.</returns>
		public static double ToDegree(double radAngle) => radAngle * (180.0 / Math.PI);

		/// <summary>
		/// Converts given angle from degrees to radians.
		/// </summary>
		/// <param name="degAngle">Angle in degrees.</param>
		/// <returns>The given angle in radians.</returns>
		public static double ToRadian(double degAngle) => Math.PI * degAngle / 180.0;

		// Basically modulo for array indices, solves - num issues. (-1, 3) is 2.
		/// <summary>
		/// A modified modulo operator. Computes similar to num % wrapTo, following the formula (num
		/// % wrapTo + wrapTo) % wrapTo. Practically it differs from regular modulo in that the
		/// values it returns when negative values for num are given wrap around like one would want
		/// an array index to (if wrapTo is list.length, -1 wraps to list.length - 1). For example, 0
		/// % 3 = 0, -1 % 3 = -1, -2 % 3 = -2, -3 % 3 = 0, and so forth, but WrapTo(0, 3) = 0,
		/// WrapTo(-1, 3) = 2, -2 % 3 = 1, -3 % 3 = 0, and so forth. This can be useful if you're
		/// trying to "wrap" a number around at both ends, for example wrap to 3, such that 3 wraps
		/// to 0, and 0 wraps to 2. This is common if you are wrapping around an array index to the
		/// length of the array and need to ensure that positive numbers greater than or equal to the
		/// length of the array wrap to the beginning of the array (index 0), AND that negative
		/// numbers (under 0) wrap around to the end of the array (Length - 1). In that example to
		/// wrap a number i you would call WrapTo(i, array.Length).
		/// </summary>
		/// <param name="num">The number to wrap.</param>
		/// <param name="wrapTo">
		/// The number to wrap to -- the result of the function is as outlined in function
		/// description, and guaranteed to be between [0, wrapTo - 1], inclusive.
		/// </param>
		/// <returns>
		/// The wrapped result, as outlined in function description. Guaranteed to lie in range [0,
		/// wrapTo - 1], inclusive.
		/// </returns>
		public static int WrapAround(int num, int wrapTo) => (num % wrapTo + wrapTo) % wrapTo;
	}
}