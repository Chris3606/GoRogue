﻿using System;
using BenchmarkDotNet.Attributes;
using GoRogue.MapGeneration;
using GoRogue.SenseMapping;
using GoRogue.SenseMapping.Sources;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.PerformanceTests
{
    public enum SourceType
    {
        Ripple,
        RippleLoose,
        RippleTight,
        RippleVeryLoose,
        Shadow
    }

    public class SenseMapAlgorithms
    {
        [UsedImplicitly]
        [Params(50, 100, 250, 500)]
        public int MapSize;

        [UsedImplicitly]
        [Params(10, 20, 30)]
        public int SourceRadius;

        [UsedImplicitly]
        [ParamsAllValues]
        public Radius.Types SourceShape;

        [UsedImplicitly]
        [ParamsAllValues]
        public SourceType SourceAlgo;

        private SenseMap _senseMap = null!;

        [GlobalSetup(Target = nameof(CalculateSingleSource))]
        public void GlobalSetupSingleSource()
        {
            CreateSenseMap();

            // Create single source at center
            ISenseSource source = SourceAlgo switch
            {
                SourceType.Ripple => new RippleSenseSource(_senseMap.ResultView.Bounds().Center, SourceRadius,
                    // ReSharper disable once RedundantArgumentDefaultValue
                    (Radius)SourceShape, RippleType.Regular),
                SourceType.RippleLoose => new RippleSenseSource(_senseMap.ResultView.Bounds().Center, SourceRadius,
                    (Radius)SourceShape, RippleType.Loose),
                SourceType.RippleVeryLoose => new RippleSenseSource(_senseMap.ResultView.Bounds().Center, SourceRadius,
                    (Radius)SourceShape, RippleType.VeryLoose),
                SourceType.RippleTight => new RippleSenseSource(_senseMap.ResultView.Bounds().Center, SourceRadius,
                    (Radius)SourceShape, RippleType.Tight),
                SourceType.Shadow => new RecursiveShadowcastingSenseSource(_senseMap.ResultView.Bounds().Center, SourceRadius, (Radius)SourceShape),
                _ => throw new Exception($"Unsupported SenseSource type encountered: {SourceAlgo}")
            };
            _senseMap.AddSenseSource(source);
        }

        [GlobalSetup(Target = nameof(CalculateDoubleSource))]
        public void GlobalSetupDoubleSource()
        {
            CreateSenseMap();

            // Create two sources, equidistant on x axis
            foreach (var rect in _senseMap.ResultView.Bounds().BisectVertically())
            {
                ISenseSource source = SourceAlgo switch
                {
                    // ReSharper disable once RedundantArgumentDefaultValue
                    SourceType.Ripple => new RippleSenseSource(rect.Center, SourceRadius, (Radius)SourceShape, RippleType.Regular),
                    SourceType.RippleLoose => new RippleSenseSource(rect.Center, SourceRadius, (Radius)SourceShape, RippleType.Loose),
                    SourceType.RippleVeryLoose => new RippleSenseSource(rect.Center, SourceRadius, (Radius)SourceShape, RippleType.VeryLoose),
                    SourceType.RippleTight => new RippleSenseSource(rect.Center, SourceRadius, (Radius)SourceShape, RippleType.Tight),
                    SourceType.Shadow => new RecursiveShadowcastingSenseSource(_senseMap.ResultView.Bounds().Center, SourceRadius, (Radius)SourceShape),
                    _ => throw new Exception($"Unsupported SenseSource type encountered: {SourceAlgo}")
                };

                _senseMap.AddSenseSource(source);
            }
        }

        [Benchmark]
        public SenseMap CalculateSingleSource()
        {
            _senseMap.Calculate();
            return _senseMap;
        }

        [Benchmark]
        public SenseMap CalculateDoubleSource()
        {
            _senseMap.Calculate();
            return _senseMap;
        }

        private void CreateSenseMap()
        {
            // Create sense map of rectangular area
            var wallFloor = new Generator(MapSize, MapSize)
                .ConfigAndGenerateSafe(gen => gen.AddSteps(DefaultAlgorithms.RectangleMapSteps()))
                .Context.GetFirst<IGridView<bool>>("WallFloor");

            var resMap = new ArrayView<double>(wallFloor.Width, wallFloor.Height);
            resMap.ApplyOverlay(pos => wallFloor[pos] ? 0.0 : 1.0);
            _senseMap = new SenseMap(resMap);
        }
    }
}
