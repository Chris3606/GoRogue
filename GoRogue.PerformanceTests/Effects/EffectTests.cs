using BenchmarkDotNet.Attributes;
using GoRogue.Effects;

namespace GoRogue.PerformanceTests.Effects
{
    public class EffectTests
    {
        private Effect _effect = null!;
        private AdvancedEffect<int> _advancedEffect = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _effect = new CountingEffect(EffectDuration.Instant);
            _advancedEffect = new AdvancedCountingEffect(EffectDuration.Instant);
        }

        [Benchmark]
        public int TriggerEffect()
        {
            _effect.Trigger(out bool _);
            return CountingEffect.Count;
        }

        [Benchmark]
        public int TriggerAdvancedEffect()
        {
            _advancedEffect.Trigger(out bool _, 1);
            return AdvancedCountingEffect.Count;
        }
    }
}
