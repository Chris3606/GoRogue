using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;
using GoRogue.Pathing;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.PerformanceTests.Pathing
{
    /// <summary>
    /// Implements basic tests for flee maps on various sized open maps.
    /// </summary>
    public class FleeMapOpenMap
    {
        [UsedImplicitly]
        [ParamsAllValues]
        public Distance.Types DistanceCalc;

        [UsedImplicitly]
        [Params(10, 50, 100, 200)]
        public int MapSize;

        private GoalMap _singleGoalMap = null!;
        private FleeMap _singleFleeMap = null!;

        private GoalMap _dualGoalMap = null!;
        private FleeMap _dualFleeMap = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var map = new Generator(MapSize, MapSize)
                .ConfigAndGenerateSafe(gen => gen.AddSteps(DefaultAlgorithms.RectangleMapSteps()))
                .Context.GetFirst<IGridView<bool>>("WallFloor");

            var goalsBase =
                new LambdaTranslationGridView<bool, GoalState>(map, val => val ? GoalState.Clear : GoalState.Obstacle);

            var singleGoalView = new ArrayView<GoalState>(map.Width, map.Height);
            singleGoalView.ApplyOverlay(goalsBase);
            singleGoalView[map.Bounds().Center] = GoalState.Goal;

            _singleGoalMap = new GoalMap(singleGoalView, DistanceCalc);
            _singleFleeMap = new FleeMap(_singleGoalMap);

            var dualGoalView = new ArrayView<GoalState>(map.Width, map.Height);
            dualGoalView.ApplyOverlay(goalsBase);
            foreach (var rect in dualGoalView.Bounds().BisectVertically())
                dualGoalView[rect.Center] = GoalState.Goal;

            _dualGoalMap = new GoalMap(dualGoalView, DistanceCalc);
            _dualFleeMap = new FleeMap(_dualGoalMap);
        }

        [Benchmark]
        public FleeMap GoRogueSingleGoal()
        {
            // Flee map automatically updates when goal map does
            _singleGoalMap.Update();
            return _singleFleeMap;
        }

        [Benchmark]
        public FleeMap GoRogueDualGoal()
        {
            // Flee map automatically updates when goal map does
            _dualGoalMap.Update();
            return _dualFleeMap;
        }
    }
}
