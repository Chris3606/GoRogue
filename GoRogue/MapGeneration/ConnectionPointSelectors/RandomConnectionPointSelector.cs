using GoRogue.Random;
using JetBrains.Annotations;
using SadRogue.Primitives;
using ShaiRandom.Generators;

namespace GoRogue.MapGeneration.ConnectionPointSelectors
{
    /// <summary>
    /// Implements a the selection algorithm that simply selects random points from the given
    /// areas' positions lists, using the RNG specified, or the default rng if null is given.
    /// </summary>
    [PublicAPI]
    public class RandomConnectionPointSelector : IConnectionPointSelector
    {
        private readonly IEnhancedRandom _rng;

        /// <summary>
        /// Constructor. Specifies the RNG to use, or null if the default RNG should be used.
        /// </summary>
        /// <param name="rng">The RNG to use, or null if the default RNG should be used.</param>
        public RandomConnectionPointSelector(IEnhancedRandom? rng = null) => _rng = rng ?? GlobalRandom.DefaultRNG;

        /// <inheritdoc />
        public AreaConnectionPointPair SelectConnectionPoints(IReadOnlyArea area1, IReadOnlyArea area2)
            => new AreaConnectionPointPair(_rng.RandomElement(area1), _rng.RandomElement(area2));
    }
}
