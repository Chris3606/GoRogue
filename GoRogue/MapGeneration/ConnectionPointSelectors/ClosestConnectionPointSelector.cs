using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.ConnectionPointSelectors
{
    /// <summary>
    /// Implements a the selection algorithm that selects the two points closest to each other in the given <see cref="Area" />
    /// instances.
    /// </summary>
    [PublicAPI]
    public class ClosestConnectionPointSelector : IConnectionPointSelector
    {
        /// <summary>
        /// Distance calculation to use to determine closeness.
        /// </summary>
        public readonly Distance DistanceCalculation;

        /// <summary>
        /// Creates a new point selector.
        /// </summary>
        /// <param name="distanceCalculation">Distance calculation to use to determine closeness.</param>
        public ClosestConnectionPointSelector(Distance distanceCalculation)
            => DistanceCalculation = distanceCalculation;

        /// <inheritdoc />
        public AreaConnectionPointPair SelectConnectionPoints(
            IReadOnlyArea area1, IReadOnlyArea area2)
        {
            var c1 = Point.None;
            var c2 = Point.None;
            var minDist = double.MaxValue;

            foreach (var point1 in area1.Positions)
                foreach (var point2 in area2.Positions)
                {
                    var distance = DistanceCalculation.Calculate(point1, point2);
                    if (distance < minDist)
                    {
                        c1 = point1;
                        c2 = point2;
                        minDist = distance;
                    }
                }

            return new AreaConnectionPointPair(c1, c2);
        }
    }
}
