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

        private EffectTrigger<EffectArgs?> _trigger = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _trigger = new EffectTrigger<EffectArgs?>();

            for (int i = 0; i < NumberOfEffectsTriggered; i++)
                _trigger.Add(new CountingEffect(CountingEffect.Infinite));
        }

        [Benchmark]
        public int TriggerEffects()
        {
            _trigger.TriggerEffects(new EffectArgs());
            return CountingEffect.Count;
        }

        [Benchmark]
        public int TriggerEffectsNullArgs()
        {
            _trigger.TriggerEffects(null);
            return CountingEffect.Count;
        }
    }
}
