using System;

namespace GoRogue.UnitTests.Mocks
{
    public class IntEffect : Effect<EffectArgs>
    {
        public IntEffect(string name, int startingDuration)
            : base(name, startingDuration)
        { }

        protected override void OnTrigger(EffectArgs? e)
        { }
    }

    public class CancelingIntEffect : IntEffect
    {
        public CancelingIntEffect(string name, int startingDuration)
            : base(name, startingDuration)
        { }

        protected override void OnTrigger(EffectArgs? e)
        {
            e = e ?? throw new ArgumentException($"Effect arguments for {nameof(CancelingIntEffect)} may not be null",
                nameof(e));
            e.CancelTrigger = true;
        }
    }
}
