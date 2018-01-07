using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GoRogue;

namespace GoRogue_UnitTests
{
    public class IntEffect : Effect<EffectArgs>
    {
        public IntEffect(string name, int startingDuration)
            : base(name, startingDuration) { }

        protected override void OnTrigger(EffectArgs e) => Console.WriteLine($"Effect {Name} triggered.");
    }

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
    [TestClass]
    public class EffectTests
    {
        [TestMethod]
        public void EffectToString()
        {
            string NAME = "Int Effect 1";
            int DURATION = 5;
            var intEffect = new IntEffect(NAME, DURATION);
            Assert.AreEqual(intEffect.ToString(), $"{NAME}: {DURATION} duration remaining");
        }

        [TestMethod]
        public void EffectDurationDecrement()
        {
            var effect = new IntEffect("Test effect", 2);
            Assert.AreEqual(effect.Duration, 2);

            effect.Trigger(null);
            Assert.AreEqual(effect.Duration, 1);

            effect.Trigger(null);
            Assert.AreEqual(effect.Duration, 0);

            effect.Trigger(null);
            Assert.AreEqual(effect.Duration, 0);

            var effect2 = new IntEffect("Test Effect", IntEffect.INFINITE);
            Assert.AreEqual(effect2.Duration, IntEffect.INFINITE);

            effect2.Trigger(null);
            Assert.AreEqual(effect2.Duration, IntEffect.INFINITE);

            effect2.Trigger(null);
            Assert.AreEqual(effect2.Duration, IntEffect.INFINITE);
        }

        [TestMethod]
        public void EffectTriggerAdd()
        {
            var effectTrigger = new EffectTrigger<EffectArgs>();
            Assert.AreEqual(0, effectTrigger.Effects.Count);

            effectTrigger.Add(new IntEffect("Test Effect 1", 1));
            Assert.AreEqual(1, effectTrigger.Effects.Count);

            effectTrigger.Add(new IntEffect("Test Effect 2", 2));
            Assert.AreEqual(2, effectTrigger.Effects.Count);

            effectTrigger.Add(new IntEffect("Test Effect Inf", IntEffect.INFINITE));
            Assert.AreEqual(3, effectTrigger.Effects.Count);

            bool exception = false;
            try
            {
                effectTrigger.Add(new IntEffect("Test Effect 0", 0));
            }
            catch (ArgumentException)
            {
                exception = true;
            }
            Assert.AreEqual(true, exception);
        }

        [TestMethod]
        public void EffectTriggerEffects()
        {
            int MULTI_DURATION = 3;
            var effectTrigger = new EffectTrigger<EffectArgs>();

            var effect1 = new IntEffect("Int Effect 1", 1);
            var effect2 = new IntEffect("Int Effect 3", MULTI_DURATION);

            var effectInf = new IntEffect("Int Effect Inf", IntEffect.INFINITE);

            effectTrigger.Add(effect2);
            effectTrigger.TriggerEffects(null); // Test with null arguments
            Assert.AreEqual(1, effectTrigger.Effects.Count);
            Assert.AreEqual(MULTI_DURATION - 1, effectTrigger.Effects[0].Duration);
            Assert.AreEqual(1, effect1.Duration);

            effectTrigger.Add(effect1);
            effectTrigger.Add(effectInf);
            Assert.AreEqual(3, effectTrigger.Effects.Count);

            effectTrigger.TriggerEffects(null);
            Assert.AreEqual(2, effectTrigger.Effects.Count);
            Assert.AreEqual(MULTI_DURATION - 2, effectTrigger.Effects[0].Duration);
            Assert.AreEqual(IntEffect.INFINITE, effectTrigger.Effects[1].Duration);

            var secEffectTrigger = new EffectTrigger<EffectArgs>();
            var testEffect = new IntEffect("Int effect dummy", 1);
            var cancelingEffect = new CancelingIntEffect("Int effect 3", 1);
            secEffectTrigger.Add(cancelingEffect);
            secEffectTrigger.Add(testEffect);
            Assert.AreEqual(2, secEffectTrigger.Effects.Count);

            secEffectTrigger.TriggerEffects(new EffectArgs());
            Assert.AreEqual(1, secEffectTrigger.Effects.Count);
            Assert.AreEqual(1, secEffectTrigger.Effects[0].Duration); // Must have cancelled
        }
    }
}
