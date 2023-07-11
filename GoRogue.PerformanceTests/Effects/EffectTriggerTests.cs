using BenchmarkDotNet.Attributes;
using GoRogue.Effects;
using JetBrains.Annotations;

namespace GoRogue.PerformanceTests.Effects
{
    public class EffectTriggerTests
    {
        [UsedImplicitly]
        [Params(1, 5, 10, 25)]
        public int NumberOfEffectsTriggered;

        private EffectTrigger _trigger = null!;
        private AdvancedEffectTrigger<int> _advancedTrigger = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _trigger = new EffectTrigger();
            _advancedTrigger = new AdvancedEffectTrigger<int>();

            for (int i = 0; i < NumberOfEffectsTriggered; i++)
            {
                _trigger.Add(new CountingEffect(EffectDuration.Infinite));
                _advancedTrigger.Add(new AdvancedCountingEffect(EffectDuration.Infinite));
            }

        }

        [Benchmark]
        public int TriggerEffects()
        {
            _trigger.TriggerEffects();
            return CountingEffect.Count;
        }

        [Benchmark]
        public int TriggerAdvancedEffects()
        {
            _advancedTrigger.TriggerEffects(1);
            return AdvancedCountingEffect.Count;
        }
    }
}
