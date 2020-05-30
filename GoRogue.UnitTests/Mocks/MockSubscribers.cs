using System;
using System.Collections.Generic;
using System.Text;
using GoRogue.Messaging;

namespace GoRogue.UnitTests.Mocks
{
    class MockSubscriber1 : ISubscriber<MockMessage1>
    {
        public static int HandleCount = 0;

        public void Handle(MockMessage1 message)
        {
            HandleCount++;
        }
    }

    class MockSubscriber2 : ISubscriber<MockMessage2>
    {
        public static int HandleCount = 0;

        public void Handle(MockMessage2 message)
        {
            HandleCount++;
        }
    }

    class MockSubscribers : ISubscriber<MockMessage1>, ISubscriber<MockMessage2>
    {
        public static int Handle1Count = 0;
        public static int Handle2Count = 0;

        void ISubscriber<MockMessage1>.Handle(MockMessage1 message)
        {
            Handle1Count++;
        }

        void ISubscriber<MockMessage2>.Handle(MockMessage2 message)
        {
            Handle2Count++;
        }
    }

    class MockIMessageSubscriber : ISubscriber<IMockMessage>
    {
        public static int HandleCount = 0;

        public void Handle(IMockMessage message)
        {
            HandleCount++;
        }
    }

    class MsgBaseSub : ISubscriber<MockMessageBase>
    {
        public static int HandleCount = 0;

        public void Handle(MockMessageBase message)
        {
            HandleCount++;
        }
    }
}
