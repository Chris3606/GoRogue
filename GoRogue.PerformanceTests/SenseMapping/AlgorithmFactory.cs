using System;
using GoRogue.SenseMapping.Sources;
using SadRogue.Primitives;

namespace GoRogue.PerformanceTests.SenseMapping
{
    public enum SourceType
    {
        Ripple,
        RippleLoose,
        RippleTight,
        RippleVeryLoose,
        Shadow
    }

    internal static class AlgorithmFactory
    {
        public static ISenseSource CreateSenseSource(SourceType algo, Point position, double radius, Radius shape)
            => algo switch
            {
                // ReSharper disable once RedundantArgumentDefaultValue
                SourceType.Ripple => new RippleSenseSource(position, radius, shape, RippleType.Regular),
                SourceType.RippleLoose => new RippleSenseSource(position, radius, shape, RippleType.Loose),
                SourceType.RippleTight => new RippleSenseSource(position, radius, shape, RippleType.Tight),
                SourceType.RippleVeryLoose => new RippleSenseSource(position, radius, shape, RippleType.VeryLoose),
                SourceType.Shadow => new RecursiveShadowcastingSenseSource(position, radius, shape),
                _ => throw new Exception($"Unsupported source type: {algo}")
            };
    }
}
