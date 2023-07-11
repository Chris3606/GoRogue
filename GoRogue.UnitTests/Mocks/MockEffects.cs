using GoRogue.Effects;
using Xunit;

namespace GoRogue.UnitTests.Mocks
{
    public class IntEffect : Effect
    {
        public IntEffect(string name, int startingDuration)
            : base(name, startingDuration)
        { }

        protected override void OnTrigger(out bool cancelTrigger)
        {
            cancelTrigger = false;
        }
    }

    public class CancelingIntEffect : IntEffect
    {
        public CancelingIntEffect(string name, int startingDuration)
            : base(name, startingDuration)
        { }

        protected override void OnTrigger(out bool cancelTrigger)
        {
            cancelTrigger = true;
        }
    }

    public class AdvancedIntEffect : AdvancedEffect<int>
    {
        public readonly int ExpectedArgs;

        public AdvancedIntEffect(string name, int startingDuration, int expectedArgs)
            : base(name, startingDuration)
        {
            ExpectedArgs = expectedArgs;
        }

        protected override void OnTrigger(out bool cancelTrigger, int args)
        {
            Assert.Equal(ExpectedArgs, args);
            cancelTrigger = false;
        }
    }

    public class CancelingAdvancedIntEffect : AdvancedIntEffect
    {
        public CancelingAdvancedIntEffect(string name, int startingDuration, int expectedArgs)
            : base(name, startingDuration, expectedArgs)
        { }

        protected override void OnTrigger(out bool cancelTrigger, int args)
        {
            Assert.Equal(ExpectedArgs, args);
            cancelTrigger = true;
        }
    }
}
