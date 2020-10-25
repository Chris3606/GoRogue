using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.ConnectionPointSelectors
{
    /// <summary>
    /// Implements a the selection algorithm that selects the center points of the bounding boxes of the given
    /// <see cref="Area" /> instances as connection points.
    /// </summary>
    [PublicAPI]
    public class CenterBoundsConnectionPointSelector : IConnectionPointSelector
    {
        /// <inheritdoc />
        public AreaConnectionPointPair SelectConnectionPoints(IReadOnlyArea area1, IReadOnlyArea area2)
            => new AreaConnectionPointPair(area1.Bounds.Center, area2.Bounds.Center);
    }
}
