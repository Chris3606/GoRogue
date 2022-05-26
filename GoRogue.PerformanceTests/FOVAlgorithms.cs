using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using GoRogue.FOV;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using SadRogue.Primitives.PointHashers;

namespace GoRogue.PerformanceTests
{
    public enum PointHashAlgorithms
    {
        Default,
        KnownSizeHasher
    };

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
        [ParamsAllValues]
        public PointHashAlgorithms PointComparer;

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

            // Create appropriate point hashing algorithm
            var pointHasher = PointComparer switch
            {
                PointHashAlgorithms.Default => null,
                PointHashAlgorithms.KnownSizeHasher => new KnownSizeHasher(transparencyView.Width),
                _ => throw new Exception("Unsupported hashing algorithm.")
            };

            // Create FOV structure to use
            _fov = new RecursiveShadowcastingFOV(transparencyView, pointHasher);
        }

        [Benchmark]
        public void CalculateFOV()
        {
            _fov.Calculate(_center, FOVRadius, (Radius)FOVShape);
        }
    }
}
