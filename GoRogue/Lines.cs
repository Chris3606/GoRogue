using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue
{
    /// <summary>
    /// Provides implementations of various helper functions useful for gathering information from lines.
    /// </summary>
    [PublicAPI]
    public static class Lines
    {
        #region IEnumerable<Point> Extensions

        /// <summary>
        /// Gets the left-most point in a list that is on the given y-value.
        /// </summary>
        /// <param name="self"/>
        /// <param name="y">Y-value to find the left-most point on.</param>
        /// <returns/>
        public static int LeftAt(this IEnumerable<Point> self, int y) => self.Where(c => c.Y == y).OrderBy(c => c.X).First().X;
        /// <summary>
        /// Gets the right-most point in a list that is on the given y-value.
        /// </summary>
        /// <param name="self"/>
        /// <param name="y">Y-value to find the right-most point on.</param>
        /// <returns/>
        public static int RightAt(this IEnumerable<Point> self, int y) => self.Where(c => c.Y == y).OrderBy(c => -c.X).First().X;

        /// <summary>
        /// Gets the top-most point in a list that is on the given x-value.
        /// </summary>
        /// <param name="self"/>
        /// <param name="x">X-value to find the top-most point on.</param>
        /// <returns/>
        public static int TopAt(this IEnumerable<Point> self, int x) => Direction.YIncreasesUpward
            ? self.Where(c => c.X == x).OrderBy(c => -c.Y).First().Y
            : self.Where(c => c.X == x).OrderBy(c => c.Y).First().Y;

        /// <summary>
        /// Gets the top-most point in a list that is on the given x-value.
        /// </summary>
        /// <param name="self"/>
        /// <param name="x">X-value to find the top-most point on.</param>
        /// <returns/>
        public static int BottomAt(this IEnumerable<Point> self, int x) => Direction.YIncreasesUpward
            ? self.Where(c => c.X == x).OrderBy(c => c.Y).First().Y
            : self.Where(c => c.X == x).OrderBy(c => -c.Y).First().Y;

        #endregion
    }
}
