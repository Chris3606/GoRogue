using GoRogue.Effects;

namespace GoRogue.PerformanceTests.Effects
{
    public class AdvancedCountingEffect : AdvancedEffect<int>
    {
        public static int Count;

        public AdvancedCountingEffect(int duration) : base("CountingEffect", duration)
        { }

        protected override void OnTrigger(out bool cancelTrigger, int args)
        {
            Count += args;
            cancelTrigger = false;
        }
    }
}
