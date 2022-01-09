using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;
using GoRogue.Pathing;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.PerformanceTests.Pathing
{
    /// <summary>
    /// Implements basic tests for weighted goal maps on various sized open maps.
    /// </summary>
    public class WeightedGoalMapOpenMap
    {
        [UsedImplicitly]
        [ParamsAllValues]
        public Distance.Types DistanceCalc;

        [UsedImplicitly]
        [Params(50, 100, 200)]
        public int MapSize;

        [UsedImplicitly]
        [Params(1, 3, 5)]
        public int NumGoalMaps;

        private WeightedGoalMap _desireMap = null!;
        private Point _threeQuarters;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var map = new Generator(MapSize, MapSize)
                .ConfigAndGenerateSafe(gen => gen.AddSteps(DefaultAlgorithms.RectangleMapSteps()))
                .Context.GetFirst<IGridView<bool>>("WallFloor");

            var goalView = new ArrayView<GoalState>(map.Width, map.Height);
            goalView.ApplyOverlay(pos => map[pos] ? GoalState.Clear : GoalState.Obstacle);
            goalView[map.Bounds().Center] = GoalState.Goal;

            // Create the goal maps used as the desires.  They all use the same goals and weights, which in general is not
            // realistic, but will provide a mathematically solid test base nonetheless.
            var maps = new List<GoalMap>();
            for (int i = 0; i < NumGoalMaps; i++)
            {
                var desireMap = new GoalMap(goalView, DistanceCalc);
                maps.Add(desireMap);
            }

            // Create overall weighted goal map
            _desireMap = new WeightedGoalMap(maps);

            // Pre-calculate the point we'll access
            _threeQuarters = new Point(map.Width, map.Height) * 0.75;
        }

        [Benchmark]
        public double? ThisIndexer()
        {
            return _desireMap[_threeQuarters];
        }

        [Benchmark]
        public Direction DirectionOfMinValue()
        {
            return _desireMap.GetDirectionOfMinValue(_threeQuarters, (Distance)DistanceCalc);
        }
    }
}
