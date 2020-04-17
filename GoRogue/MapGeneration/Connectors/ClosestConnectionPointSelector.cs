using System;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Point selector that selects the two points from the <see cref="Area"/> instances that are closest to each other,
    /// according to the given distance calculation.
    /// </summary>
    public class ClosestConnectionPointSelector : IAreaConnectionPointSelector
    {
        /// <summary>
        /// <see cref="Distance"/> calculation to use to determine closeness.
        /// </summary>
        public Distance DistanceCalculation { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="distanceCalculation"><see cref="Distance"/> calculation to use to determine closeness.</param>
        public ClosestConnectionPointSelector(Distance distanceCalculation) => DistanceCalculation = distanceCalculation;

        /// <summary>
        /// Determines the two points in the given map areas that are closest to each other, and returns
        /// them as a tuple of two positions.  The first position is the position in <paramref name="area1"/>to use,
        /// and the second is the position in <paramref name="area2"/> to use.
        /// </summary>
        /// <remarks>
        /// This algorithm can be relatively time-complex -- O(m*n) where m is the number of Pointinates in area1, and n
        /// is the number of Pointintes in area2.
        /// </remarks>
        /// <param name="area1">First <see cref="Area"/> to connect.</param>
        /// <param name="area2">Second <see cref="Area"/> to connect.</param>
        /// <returns>
        /// A tuple containing the Pointinates from each <see cref="Area"/> to connect -- the first item in the
        /// tuple is the position in <paramref name="area1"/>, the second is the position in <paramref name="area2"/>.
        /// </returns>
        public Tuple<Point, Point> SelectConnectionPoints(IReadOnlyArea area1, IReadOnlyArea area2)
        {
            Point c1 = Point.None;
            Point c2 = Point.None;
            double minDist = double.MaxValue;

            foreach (var point1 in area1.Positions)
                foreach (var point2 in area2.Positions)
                {
                    double distance = DistanceCalculation.Calculate(point1, point2);
                    if (distance < minDist)
                    {
                        c1 = point1;
                        c2 = point2;
                        minDist = distance;
                    }
                }

            return new Tuple<Point, Point>(c1, c2);
        }
    }
}
