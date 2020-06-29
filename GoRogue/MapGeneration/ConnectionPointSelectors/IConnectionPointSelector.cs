using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.ConnectionPointSelectors
{
    /// <summary>
    /// Interface for implementing an algorithm for selecting the positions to connect in order to
    /// connect two given areas.
    /// </summary>
    [PublicAPI]
    public interface IConnectionPointSelector
    {
        /// <summary>
        /// Implements the algorithm. Returns a tuple of two positions -- the first position is the
        /// position in <paramref name="area1" /> to use, the second position is the position in
        /// <paramref name="area2" /> to use.
        /// </summary>
        /// <param name="area1">First <see cref="Area" /> to connect.</param>
        /// <param name="area2">Second <see cref="Area" /> to connect.</param>
        /// <returns>
        /// A tuple containing the Coordinates from each <see cref="Area" /> to connect -- the first
        /// item in the tuple is the position in area1, the second is the position in area2.
        /// </returns>
        (Point area1Position, Point area2Position) SelectConnectionPoints(IReadOnlyArea area1, IReadOnlyArea area2);
    }
}
