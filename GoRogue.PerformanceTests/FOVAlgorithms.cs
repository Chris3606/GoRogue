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
    public enum PointHashAlgorithm
    {
        Default,
        KnownSizeHasher
    };

    public enum FOVAlgorithmType
    {
        RSDoubleBased,
        RSBoolBased
    }

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
        public PointHashAlgorithm PointComparer;

        [UsedImplicitly]
        [ParamsAllValues]
        public FOVAlgorithmType FOVAlgorithm;

        [UsedImplicitly]
        [Params(Radius.Types.Diamond, Radius.Types.Square, Radius.Types.Circle)]
        public Radius.Types FOVShape;

        private Point _center;

        private IFOV _fov = null!;

        [GlobalSetup(Target = nameof(CalculateFOV))]
        public void GlobalSetupCalculate()
        {
            CreateFOVAlgorithm();
        }

        [GlobalSetup(Targets = new[] { nameof(SumDoubleResults), nameof(SumBoolResults) })]
        public void GlobalSetupResultInspection()
        {
            CreateFOVAlgorithm();
            _fov.Calculate(_center, FOVRadius, (Radius)FOVShape);
        }

        private void CreateFOVAlgorithm()
        {
            // Center FOV in middle of open map
            _center = (MapWidth / 2, MapHeight / 2);

            // Generate rectangular map
            var gen = new Generator(MapWidth, MapHeight);
            gen.ConfigAndGenerateSafe(g => g.AddSteps(DefaultAlgorithms.RectangleMapSteps()));

            // Extract wall-floor map which we can use as transparency view
            var transparencyView = gen.Context.GetFirst<IGridView<bool>>("WallFloor");

            // Create appropriate point hashing algorithm
            IEqualityComparer<Point>? pointHasher = PointComparer switch
            {
                PointHashAlgorithm.Default => null,
                PointHashAlgorithm.KnownSizeHasher => new KnownSizeHasher(transparencyView.Width),
                _ => throw new Exception("Unsupported hashing algorithm.")
            };

            // Create FOV structure to use
            _fov = FOVAlgorithm switch
            {
                FOVAlgorithmType.RSDoubleBased => new RecursiveShadowcastingDoubleBasedFOV(transparencyView, pointHasher),
                FOVAlgorithmType.RSBoolBased => new RecursiveShadowcastingFOV(
                    transparencyView, pointHasher),
                _ => throw new Exception("Unsupported FOV algorithm.")
            };
        }

        [Benchmark]
        public void CalculateFOV()
        {
            _fov.Calculate(_center, FOVRadius, (Radius)FOVShape);
        }

        [Benchmark]
        public double SumDoubleResults()
        {
            double sum = 0;
            for (int y = 0; y < _fov.DoubleResultView.Height; y++)
            {
                for (int x = 0; x < _fov.DoubleResultView.Width; x++)
                    sum += _fov.DoubleResultView[x, y];
            }

            return sum;
        }

        [Benchmark]
        public double SumBoolResults()
        {
            int sum = 0;
            for (int y = 0; y < _fov.DoubleResultView.Height; y++)
            {
                for (int x = 0; x < _fov.DoubleResultView.Width; x++)
                    sum += _fov.BooleanResultView[x, y] ? 2 : 1;
            }

            return sum;
        }
    }
}
