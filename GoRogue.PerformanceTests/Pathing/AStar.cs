using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;
using GoRogue.Pathing;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.PerformanceTests.Pathing
{
    /// <summary>
    /// Implements tests of various AStar methods.
    /// </summary>
    public class AStar
    {
        [UsedImplicitly]
        [ParamsAllValues]
        public Distance.Types DistanceCalc;

        [UsedImplicitly]
        [Params(10, 50, 100)]
        public int PathDistance;

        private Point _p1;
        private Point _p2;

        private GoRogue.Pathing.AStar _aStar = null!;
        private FastAStar _fastAStar = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var map = new Generator(PathDistance + 5, PathDistance + 5)
                .ConfigAndGenerateSafe(gen => gen.AddSteps(DefaultAlgorithms.RectangleMapSteps()))
                .Context.GetFirst<IGridView<bool>>("WallFloor");

            // A couple points exactly PathDistance apart.  We keep the paths on the same y-line so that they are
            // PathDistance apart regardless of the DistanceCalc.
            // Path performance checks will independently path both from p1 to p2 and from p2 to p1, in order to
            // account for one direction likely being faster (first direction checked in the neighbors loop)
            _p1 = (1, 5);
            _p2 = (PathDistance + 1, 5);

            // An AStar instance to use for pathing
            _aStar = new GoRogue.Pathing.AStar(map, DistanceCalc);

            // Equivalent FastAStar instance
            _fastAStar = new FastAStar(map, DistanceCalc);
        }

        [Benchmark]
        public (Path?, Path?) AStarGoRogueOpenMap()
        {
            return (_aStar.ShortestPath(_p1, _p2), _aStar.ShortestPath(_p2, _p1));
        }

        [Benchmark]
        public (Path?, Path?) FastAStarGoRogueOpenMap()
        {
            return (_fastAStar.ShortestPath(_p1, _p2), _fastAStar.ShortestPath(_p2, _p1));
        }
    }
}
