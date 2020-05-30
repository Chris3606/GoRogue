using GoRogue;
using System;
using Xunit;

namespace GoRogue.UnitTests
{
    public class CancelingIntEffect : IntEffect
    {
        public CancelingIntEffect(string name, int startingDuration)
            : base(name, startingDuration) { }

        protected override void OnTrigger(EffectArgs e)
        {
            Console.WriteLine($"Effect {Name} triggered, cancelling further triggers:");
            e.CancelTrigger = true;
        }
    }

    public class EffectTests
    {
        [Fact]
        public void EffectDurationDecrement()
        {
            var effect = new IntEffect("Test effect", 2);
            Assert.Equal(2, effect.Duration);

            effect.Trigger(null);
            Assert.Equal(1, effect.Duration);

            effect.Trigger(null);
            Assert.Equal(0, effect.Duration);

            effect.Trigger(null);
            Assert.Equal(0, effect.Duration);

            var effect2 = new IntEffect("Test Effect", IntEffect.INFINITE);
            Assert.Equal(IntEffect.INFINITE, effect2.Duration);

            effect2.Trigger(null);
            Assert.Equal(IntEffect.INFINITE, effect2.Duration);

            effect2.Trigger(null);
            Assert.Equal(IntEffect.INFINITE, effect2.Duration);
        }

        [Fact]
        public void EffectToString()
        {
            string NAME = "Int Effect 1";
            int DURATION = 5;
            var intEffect = new IntEffect(NAME, DURATION);
            Assert.Equal(intEffect.ToString(), $"{NAME}: {DURATION} duration remaining");
        }

        [Fact]
        public void EffectTriggerAdd()
        {
            var effectTrigger = new EffectTrigger<EffectArgs>();
            Assert.Equal(0, effectTrigger.Effects.Count);

            effectTrigger.Add(new IntEffect("Test Effect 1", 1));
            Assert.Equal(1, effectTrigger.Effects.Count);

            effectTrigger.Add(new IntEffect("Test Effect 2", 2));
            Assert.Equal(2, effectTrigger.Effects.Count);

            effectTrigger.Add(new IntEffect("Test Effect Inf", IntEffect.INFINITE));
            Assert.Equal(3, effectTrigger.Effects.Count);

            Assert.Throws<ArgumentException>(() => effectTrigger.Add(new IntEffect("Test Effect 0", 0)));
        }

        [Fact]
        public void EffectTriggerEffects()
        {
            int MULTI_DURATION = 3;
            var effectTrigger = new EffectTrigger<EffectArgs>();

            var effect1 = new IntEffect("Int Effect 1", 1);
            var effect2 = new IntEffect("Int Effect 3", MULTI_DURATION);

            var effectInf = new IntEffect("Int Effect Inf", IntEffect.INFINITE);

            effectTrigger.Add(effect2);
            effectTrigger.TriggerEffects(null); // Test with null arguments
            Assert.Equal(1, effectTrigger.Effects.Count);
            Assert.Equal(MULTI_DURATION - 1, effectTrigger.Effects[0].Duration);
            Assert.Equal(1, effect1.Duration);

            effectTrigger.Add(effect1);
            effectTrigger.Add(effectInf);
            Assert.Equal(3, effectTrigger.Effects.Count);

            effectTrigger.TriggerEffects(null);
            Assert.Equal(2, effectTrigger.Effects.Count);
            Assert.Equal(MULTI_DURATION - 2, effectTrigger.Effects[0].Duration);
            Assert.Equal(IntEffect.INFINITE, effectTrigger.Effects[1].Duration);

            var secEffectTrigger = new EffectTrigger<EffectArgs>();
            var testEffect = new IntEffect("Int effect dummy", 1);
            var cancelingEffect = new CancelingIntEffect("Int effect 3", 1);
            secEffectTrigger.Add(cancelingEffect);
            secEffectTrigger.Add(testEffect);
            Assert.Equal(2, secEffectTrigger.Effects.Count);

            secEffectTrigger.TriggerEffects(new EffectArgs());
            Assert.Equal(1, secEffectTrigger.Effects.Count);
            Assert.Equal(1, secEffectTrigger.Effects[0].Duration); // Must have cancelled
        }
    }

    public class IntEffect : Effect<EffectArgs>
    {
        public IntEffect(string name, int startingDuration)
            : base(name, startingDuration) { }

        protected override void OnTrigger(EffectArgs e) => Console.WriteLine($"Effect {Name} triggered.");
    }
}
