using System;
using GoRogue.Effects;
using GoRogue.UnitTests.Mocks;
using Xunit;

namespace GoRogue.UnitTests.Effects
{
    public class AdvancedEffectTests
    {
        private const int Args = 5;

        [Fact]
        public void EffectDurationDecrement()
        {
            var effect = new AdvancedIntEffect("Test effect", 2, Args);
            Assert.Equal(2, effect.Duration);

            effect.Trigger(Args);
            Assert.Equal(1, effect.Duration);

            effect.Trigger(out bool _, Args);
            Assert.Equal(0, effect.Duration);

            effect.Trigger(Args);
            Assert.Equal(0, effect.Duration);

            effect.Trigger(out bool _, Args);
            Assert.Equal(0, effect.Duration);

            var effect2 = new AdvancedIntEffect("Test Effect", EffectDuration.Infinite, Args);
            Assert.Equal(EffectDuration.Infinite, effect2.Duration);

            effect2.Trigger(Args);
            Assert.Equal(EffectDuration.Infinite, effect2.Duration);

            effect2.Trigger(Args);
            Assert.Equal(EffectDuration.Infinite, effect2.Duration);

            effect2.Trigger(out bool _, Args);
            Assert.Equal(EffectDuration.Infinite, effect2.Duration);
        }

        [Fact]
        public void EffectToString()
        {
            const string name = "Int Effect 1";
            const int duration = 5;
            var intEffect = new AdvancedIntEffect(name, duration, Args);
            Assert.Equal(intEffect.ToString(), $"{name}: {duration} duration remaining");
        }

        [Fact]
        public void EffectTriggerAdd()
        {
            var effectTrigger = new AdvancedEffectTrigger<int>();
            Assert.Equal(0, effectTrigger.Effects.Count);

            effectTrigger.Add(new AdvancedIntEffect("Test Effect 1", 1, Args));
            Assert.Equal(1, effectTrigger.Effects.Count);

            effectTrigger.Add(new AdvancedIntEffect("Test Effect 2", 2, Args));
            Assert.Equal(2, effectTrigger.Effects.Count);

            effectTrigger.Add(new AdvancedIntEffect("Test Effect Inf", EffectDuration.Infinite, Args));
            Assert.Equal(3, effectTrigger.Effects.Count);

            Assert.Throws<ArgumentException>(() => effectTrigger.Add(new AdvancedIntEffect("Test Effect 0", 0, Args)));
        }

        [Fact]
        public void EffectTriggerEffects()
        {
            const int multiDuration = 3;
            var effectTrigger = new AdvancedEffectTrigger<int>();

            var effect1 = new AdvancedIntEffect("Int Effect 1", 1, Args);
            var effect2 = new AdvancedIntEffect("Int Effect 3", multiDuration, Args);

            var effectInf = new AdvancedIntEffect("Int Effect Inf", EffectDuration.Infinite, Args);

            effectTrigger.Add(effect2);
            effectTrigger.TriggerEffects(5);
            Assert.Equal(1, effectTrigger.Effects.Count);
            Assert.Equal(multiDuration - 1, effectTrigger.Effects[0].Duration);
            Assert.Equal(1, effect1.Duration);

            effectTrigger.Add(effect1);
            effectTrigger.Add(effectInf);
            Assert.Equal(3, effectTrigger.Effects.Count);

            effectTrigger.TriggerEffects(5);
            Assert.Equal(2, effectTrigger.Effects.Count);
            Assert.Equal(multiDuration - 2, effectTrigger.Effects[0].Duration);
            Assert.Equal(EffectDuration.Infinite, effectTrigger.Effects[1].Duration);

            var secEffectTrigger = new AdvancedEffectTrigger<int>();
            var testEffect = new AdvancedIntEffect("Int effect dummy", 1, Args);
            var cancelingEffect = new CancelingAdvancedIntEffect("Int effect 3", 1, Args);
            secEffectTrigger.Add(cancelingEffect);
            secEffectTrigger.Add(testEffect);
            Assert.Equal(2, secEffectTrigger.Effects.Count);

            secEffectTrigger.TriggerEffects(5);
            Assert.Equal(1, secEffectTrigger.Effects.Count);
            Assert.Equal(1, secEffectTrigger.Effects[0].Duration); // Must have cancelled
        }
    }
}
