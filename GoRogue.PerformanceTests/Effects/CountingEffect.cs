using GoRogue.Effects;

namespace GoRogue.PerformanceTests.Effects
{
    public class CountingEffect : Effect
    {
        public static int Count;

        public CountingEffect(int duration) : base("CountingEffect", duration)
        { }

        protected override void OnTrigger(out bool cancelTrigger)
        {
            Count++;
            cancelTrigger = false;
        }
    }
}
