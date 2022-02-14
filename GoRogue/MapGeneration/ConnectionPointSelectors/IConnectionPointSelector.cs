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
        /// Implements the algorithm. Returns pair of positions -- one position in <paramref name="area1" />
        /// to use, and on position in <paramref name="area2" /> to use.
        /// </summary>
        /// <param name="area1">First <see cref="SadRogue.Primitives.Area" /> to connect.</param>
        /// <param name="area2">Second <see cref="SadRogue.Primitives.Area" /> to connect.</param>
        /// <returns>
        /// A pair of positions (one from each <see cref="SadRogue.Primitives.Area" />) to connect.
        /// </returns>
        AreaConnectionPointPair SelectConnectionPoints(IReadOnlyArea area1, IReadOnlyArea area2);
    }
}
