using GoRogue.Effects;

namespace GoRogue.PerformanceTests.Effects
{
    public class CountingEffect : Effect<EffectArgs?>
    {
        public static int Count;

        public CountingEffect(int duration) : base("CountingEffect", duration)
        { }

        protected override void OnTrigger(EffectArgs? e) => Count++;
    }
}
