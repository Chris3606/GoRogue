using BenchmarkDotNet.Attributes;
using GoRogue.Effects;

namespace GoRogue.PerformanceTests.Effects
{
    public class EffectTests
    {
        private Effect<EffectArgs?> _effect = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _effect = new CountingEffect(CountingEffect.Instant);
        }

        [Benchmark]
        public int TriggerEffect()
        {
            _effect.Trigger(new EffectArgs());
            return CountingEffect.Count;
        }

        [Benchmark]
        public int TriggerEffectNullArgs()
        {
            _effect.Trigger(null);
            return CountingEffect.Count;
        }
    }
}
