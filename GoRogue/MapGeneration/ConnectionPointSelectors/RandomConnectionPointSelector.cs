using GoRogue.Random;
using SadRogue.Primitives;
using Troschuetz.Random;

namespace GoRogue.MapGeneration.ConnectionPointSelectors
{
    /// <summary>
    /// Implements a the selection algorithm that simply selects random points from the given
    /// areas' positions lists, using the RNG specified, or the default rng if null is given.
    /// </summary>
    public class RandomConnectionPointSelector : IConnectionPointSelector
    {
        private readonly IGenerator _rng;

        /// <summary>
        /// Constructor. Specifies the RNG to use, or null if the default RNG should be used.
        /// </summary>
        /// <param name="rng">The RNG to use, or null if the default RNG should be used.</param>
        public RandomConnectionPointSelector(IGenerator? rng = null)
        {
            if (rng == null)
                _rng = GlobalRandom.DefaultRNG;
            else
                _rng = rng;
        }

        /// <inheritdoc/>
        public (Point area1Position, Point area2Position) SelectConnectionPoints(IReadOnlyArea area1, IReadOnlyArea area2) =>
            (area1.Positions.RandomItem(_rng), area2.Positions.RandomItem(_rng));
    }
}
