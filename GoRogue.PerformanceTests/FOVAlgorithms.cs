using BenchmarkDotNet.Attributes;
using GoRogue.FOV;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.PerformanceTests
{
    public class FOVAlgorithms
    {
        [UsedImplicitly]
        [Params(50, 100, 250, 500)]
        public int MapWidth;

        [UsedImplicitly]
        [Params(50, 100, 250, 500)]
        public int MapHeight;

        [UsedImplicitly]
        [Params(10, 20, 30)]
        public int FOVRadius;

        [UsedImplicitly]
        [Params(Radius.Types.Diamond, Radius.Types.Square, Radius.Types.Circle)]
        public Radius.Types FOVShape;

        private Point _center;

        private IFOV _fov = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Center FOV in middle of open map
            _center = (MapWidth / 2, MapHeight / 2);

            // Generate rectangular map
            var gen = new Generator(MapWidth, MapHeight);
            gen.ConfigAndGenerateSafe(g => g.AddSteps(DefaultAlgorithms.RectangleMapSteps()));

            // Extract wall-floor map which we can use as transparency view
            var transparencyView = gen.Context.GetFirst<IGridView<bool>>("WallFloor");

            // Create FOV structure to use
            _fov = new RecursiveShadowcastingFOV(transparencyView);
        }

        [Benchmark]
        public void CalculateFOV()
        {
            _fov.Calculate(_center, FOVRadius, (Radius)FOVShape);
        }
    }
}
