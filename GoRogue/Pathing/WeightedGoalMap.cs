using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// </remarks>
    [PublicAPI]
    public sealed class WeightedGoalMap : GridViewBase<double?>
    {
        /// <summary>
        /// The list of weighted goal maps. Can be used to add or remove goal maps, or change their weights.
        /// </summary>
        /// <remarks>
        /// When adding a new goal map, its <see cref="IGridView{T}.Width" /> and <see cref="IGridView{T}.Height" />
        /// should be identical to the WeightedGoalMap's <see cref="Width" /> and
        /// <see cref="Height" />.
        /// </remarks>
        public readonly Dictionary<IGridView<double?>, double> Weights;

        /// <summary>
        /// Constructor. Takes a single goal map and assigns it a weight of 1.0.
        /// </summary>
        /// <param name="map">The goal map.</param>
        public WeightedGoalMap(IGridView<double?> map)
        {
            Weights = new Dictionary<IGridView<double?>, double> { { map, 1 } };
            Width = map.Width;
            Height = map.Height;
        }

        /// <summary>
        /// Constructor. Takes a sequence of goal maps and assigns each one a weight of 1.0.
        /// </summary>
        /// <param name="maps">The goal maps. Each one should be of the same size.</param>
        public WeightedGoalMap(IEnumerable<IGridView<double?>> maps)
        {
            Weights = new Dictionary<IGridView<double?>, double>();
            foreach (var map in maps)
            {
                Weights.Add(map, 1);

                if (Height == 0)
                {
                    Width = map.Width;
                    Height = map.Height;
                }
                else
                    Debug.Assert(Height == map.Height && Width == map.Width);
            }
        }

        /// <summary>
        /// Constructor. Takes an existing goal map dictionary and copies it.
        /// </summary>
        /// <param name="maps">
        /// The goal maps. Each one should be of the same size, and all weights should have a nonzero value.
        /// </param>
        public WeightedGoalMap(IDictionary<IGridView<double?>, double> maps)
        {
            Weights = new Dictionary<IGridView<double?>, double>();

            foreach (var (key, value) in maps)
            {
                if (Math.Abs(value) <= 0.0000000001)
                    throw new ArgumentException(
                        $"No goal map used in a {nameof(WeightedGoalMap)} may have a weight of 0.0.", nameof(maps));

                if (Height == 0)
                {
                    Width = key.Width;
                    Height = key.Height;
                }
                else if (Height != key.Height || Width != key.Width)
                    throw new ArgumentException(
                        $"All goal maps used in a {nameof(WeightedGoalMap)} must have the same size.", nameof(maps));

                Weights.Add(key, value);
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
                var result = 0.0;
                var negResult = 0.0;
                foreach (var pair in Weights)
                {
                    var value = pair.Key[point];
                    if (!value.HasValue)
                        return null;
                    var weight = pair.Value;
                    var weighted = value.Value * weight;
                    if (weight > 0.0)
                        result = Math.Abs(result) < 0.0000000001 ? weighted : result * weighted;
                    else
                        negResult = Math.Abs(negResult) < 0.0000000001 ? weighted : negResult * weighted;
                }

                return result + negResult;
            }
        }

        /// <summary>
        /// Computes the entire aggregate goal map and returns it, effectively caching the result.
        /// This may be useful in situations where the goals are shared between many characters and do not change frequently.
        /// </summary>
        public ArrayView<double?> Combine()
        {
            var result = new ArrayView<double?>(Width, Height);
            for (var y = 0; y < Height; ++y)
                for (var x = 0; x < Width; ++x)
                    result[x, y] = this[x, y];
            return result;
        }
    }
}
