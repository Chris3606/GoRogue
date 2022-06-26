using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Pathing
{
    /// <summary>
    /// Implementation of the second half of the goal map system described in
    /// <a href="http://www.roguebasin.com/index.php?title=The_Incredible_Power_of_Dijkstra_Maps">this article</a> --
    /// the ability to combine multiple goal maps with different weights.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the work of building one overall goal map out of multiple existing
    /// maps. It holds references to one or more maps, each with its own "weight". The higher the
    /// weight value, the more strongly an AI will attempt to pursue this goal. A negative weight
    /// inverts the map, turning its goal into something to avoid.  Inverting the weight does not
    /// create a "safety map" as described in the article, as the resulting goal map will show no
    /// concept of global vs. local avoidance.  For that functionality, see <see cref="FleeMap" />.
    ///
    /// Note that all the grid views held by this class must have the same width and height.  If their width and height
    /// change after they are added such that this is not the case, unexpected behavior will occur.
    /// </remarks>
    [PublicAPI]
    public sealed class WeightedGoalMap : GridViewBase<double?>
    {
        /// <summary>
        /// The list of weighted goal maps. Can be used to add or remove goal maps, or change their weights.
        /// </summary>
        /// <remarks>
        /// When adding a new goal map, its <see cref="SadRogue.Primitives.GridViews.IGridView{T}.Width" /> and <see cref="SadRogue.Primitives.GridViews.IGridView{T}.Height" />
        /// should be identical to the WeightedGoalMap's <see cref="Width" /> and
        /// <see cref="Height" />.
        /// </remarks>
        public readonly List<GoalMapWeightPair> Weights;

        /// <summary>
        /// Constructor. Takes a single goal map and assigns it a weight of 1.0.
        /// </summary>
        /// <param name="map">The goal map.</param>
        public WeightedGoalMap(IGridView<double?> map)
        {
            Weights = new List<GoalMapWeightPair> { new GoalMapWeightPair(map, 1) };
            Width = map.Width;
            Height = map.Height;
        }

        /// <summary>
        /// Constructor. Takes a sequence of goal maps and assigns each one a weight of 1.0.
        /// </summary>
        /// <param name="maps">The goal maps. Each one must be of the same size.</param>
        public WeightedGoalMap(IEnumerable<IGridView<double?>> maps)
        {
            Weights = new List<GoalMapWeightPair>();
            foreach (var map in maps)
            {
                Weights.Add(new GoalMapWeightPair(map, 1));

                if (Height == 0)
                {
                    Width = map.Width;
                    Height = map.Height;
                }
                else if (Height != map.Height || Width != map.Width)
                    throw new ArgumentException(
                        $"All goal maps used in a {nameof(WeightedGoalMap)} must have the same size.", nameof(maps));
            }
        }

        /// <summary>
        /// Constructor. Takes an existing goal map dictionary and copies it.
        /// </summary>
        /// <param name="maps">
        /// The goal maps. Each one must be of the same size, and all weights should have a nonzero value.
        /// </param>
        public WeightedGoalMap(IEnumerable<GoalMapWeightPair> maps)
        {
            Weights = new List<GoalMapWeightPair>();

            foreach (var pair in maps)
            {
                if (Height == 0)
                {
                    Width = pair.GoalMap.Width;
                    Height = pair.GoalMap.Height;
                }
                else if (Height != pair.GoalMap.Height || Width != pair.GoalMap.Width)
                    throw new ArgumentException(
                        $"All goal maps used in a {nameof(WeightedGoalMap)} must have the same size.", nameof(maps));

                Weights.Add(pair);
            }
        }

        /// <summary>
        /// The height of the goal map, and the goal maps that compose it.
        /// </summary>
        public override int Height { get; }


        /// <summary>
        /// The width of the goal map, and the goal maps that compose it.
        /// </summary>
        public override int Width { get; }

        /// <summary>
        /// Returns the value of the combined goal maps at any given point.
        /// </summary>
        public override double? this[Point point]
        {
            get
            {
                double result = 0.0;
                int length = Weights.Count;
                for (int i = 0; i < length; i++)
                {
                    var (map, weight) = Weights[i];
                    var mapValue = map[point];
                    if (!mapValue.HasValue)
                        return null;

                    var weighted = mapValue.Value * weight;
                    result += weighted;

                }

                return result;
            }
        }

        /// <summary>
        /// Computes the entire aggregate goal map and returns it, effectively caching the result.
        /// This may be useful in situations where the goals are shared between many characters and do not change frequently.
        /// </summary>
        public ArrayView<double?> Combine()
        {
            var result = new ArrayView<double?>(Width, Height);
            result.ApplyOverlay(this);

            return result;
        }
    }
}
