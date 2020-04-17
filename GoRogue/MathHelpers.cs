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
        /// Rounds the given number up (toward highest number), to the nearest multiple of the
        /// specified value.
        /// </summary>
        /// <param name="number">Number to round.</param>
        /// <param name="toMultipleOf">Number given is rounded up to nearest multiple of this number.</param>
        /// <returns>The number parameter, rouded up to the nearest multiple of <paramref name="toMultipleOf"/>.</returns>
        public static int RoundToMultiple(int number, int toMultipleOf)
        {
            int isPositive = (number >= 0) ? 1 : 0;
            return ((number + isPositive * (toMultipleOf - 1)) / toMultipleOf) * toMultipleOf;
        }

        // Basically modulo for array indices, solves - num issues. (-1, 3) is 2.
        /// <summary>
        /// A modified modulo operator, which practically differs from <paramref name="num"/> / <paramref name="wrapTo"/>
        /// in that it wraps from 0 to <paramref name="wrapTo"/> - 1, as well as from <paramref name="wrapTo"/> - 1 to 0.
        /// </summary>
        /// <remarks>
        /// A modified modulo operator. Returns the result of  the formula
        /// (<paramref name="num"/> % <paramref name="wrapTo"/> + <paramref name="wrapTo"/>) % <paramref name="wrapTo"/>.
        /// 
        /// Practically it differs from regular modulo in that the values it returns when negative values for <paramref name="num"/>
        /// are wrapped around like one would want an array index to (if wrapTo is list.length, -1 wraps to list.length - 1). For example,
        /// 0 % 3 = 0, -1 % 3 = -1, -2 % 3 = -2, -3 % 3 = 0, and so forth, but WrapTo(0, 3) = 0,
        /// WrapTo(-1, 3) = 2, WrapTo(-2, 3) = 1, WrapTo(-3, 3) = 0, and so forth. This can be useful if you're
        /// trying to "wrap" a number around at both ends, for example wrap to 3, such that 3 wraps
        /// to 0, and -1 wraps to 2. This is common if you are wrapping around an array index to the
        /// length of the array and need to ensure that positive numbers greater than or equal to the
        /// length of the array wrap to the beginning of the array (index 0), AND that negative
        /// numbers (under 0) wrap around to the end of the array (Length - 1).
        /// </remarks>
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

        /// <summary>
        /// Approximation of the Atan2 function that scales the returned value to the range [0.0, 1.0], in order to remain
        /// agnostic of units (radius vs degrees).  It will never return a negative number, so is also useful to avoid floating-point
        /// modulus.  Credit to the SquidLib java RL library and <a href="https://math.stackexchange.com/a/1105038">this suggestion
        /// from user njuffa</a> for this math.
        /// </summary>
        /// <param name="y">Y-component of point to find angle towards.</param>
        /// <param name="x">X-component of point to find angle towards.</param>
        /// <returns>A value representing the angle to the given point, scaled to range [0.0, 1.0].</returns>
        public static double ScaledAtan2Approx(double y, double x)
        {
            if (y == 0.0 && x >= 0.0)
                return 0.0;

            double ax = Math.Abs(x);
            double ay = Math.Abs(y);

            if (ax < ay)
            {
                double a = ax / ay;
                double s = a * a;
                double r = 0.25 - (((-0.0464964749 * s + 0.15931422) * s - 0.327622764) * s * a + a) * 0.15915494309189535;
                return (x < 0.0) ? (y < 0.0) ? 0.5 + r : 0.5 - r : (y < 0.0) ? 1.0 - r : r;
            }
            else
            {
                double a = ay / ax;
                double s = a * a;
                double r = (((-0.0464964749 * s + 0.15931422) * s - 0.327622764) * s * a + a) * 0.15915494309189535;
                return (x < 0.0) ? (y < 0.0) ? 0.5 + r : 0.5 - r : (y < 0.0) ? 1.0 - r : r;
            }
        }
    }
}
