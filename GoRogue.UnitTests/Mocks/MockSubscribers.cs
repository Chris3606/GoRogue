using GoRogue.Messaging;

namespace GoRogue.UnitTests.Mocks
{
    internal class MockSubscriber1 : ISubscriber<MockMessage1>
    {
        public static int HandleCount;

        public void Handle(MockMessage1 message) => HandleCount++;
    }

    internal class MockSubscriber2 : ISubscriber<MockMessage2>
    {
        public static int HandleCount;

        public void Handle(MockMessage2 message) => HandleCount++;
    }

    internal class MockSubscribers : ISubscriber<MockMessage1>, ISubscriber<MockMessage2>
    {
        public static int Handle1Count;
        public static int Handle2Count;

        void ISubscriber<MockMessage1>.Handle(MockMessage1 message) => Handle1Count++;

        void ISubscriber<MockMessage2>.Handle(MockMessage2 message) => Handle2Count++;
    }

    internal class MockIMessageSubscriber : ISubscriber<IMockMessage>
    {
        public static int HandleCount;

        public void Handle(IMockMessage message) => HandleCount++;
    }

    internal class MsgBaseSub : ISubscriber<MockMessageBase>
    {
        public static int HandleCount;

        public void Handle(MockMessageBase message) => HandleCount++;
    }
}
