using System;
using GoRogue.Messaging;
using GoRogue.UnitTests.Mocks;
using Xunit;

namespace GoRogue.UnitTests.Messaging
{
    public class MessageBusTests
    {
        public MessageBusTests()
        {
            bus = new MessageBus();
            ResetHandleCounts();
        }

        private readonly MessageBus bus;

        private void AssertHandleCounts(int m1 = 0, int m2 = 0, int mb1 = 0, int mb2 = 0, int mi = 0, int mbase = 0)
        {
            Assert.Equal(m1, MockSubscriber1.HandleCount);
            Assert.Equal(m2, MockSubscriber2.HandleCount);
            Assert.Equal(mb1, MockSubscribers.Handle1Count);
            Assert.Equal(mb2, MockSubscribers.Handle2Count);
            Assert.Equal(mi, MockIMessageSubscriber.HandleCount);
            Assert.Equal(mbase, MsgBaseSub.HandleCount);
        }

        private void ResetHandleCounts()
        {
            MockSubscriber1.HandleCount = 0;
            MockSubscriber2.HandleCount = 0;
            MockSubscribers.Handle1Count = MockSubscribers.Handle2Count = 0;
            MockIMessageSubscriber.HandleCount = 0;
            MsgBaseSub.HandleCount = 0;
        }

        // Test that a handler that wants a base class type gets called appropriately when concrete classes are passed in
        [Fact]
        public void BaseClassHandlers()
        {
            bus.RegisterSubscriber(new MsgBaseSub());

            bus.Send(new MockMessage2());
            AssertHandleCounts(mbase: 1);
        }

        // Test that handlers of concrete types are dispatched appropriately
        [Fact]
        public void BasicHandlers()
        {
            bus.Send(new MockMessage1());
            AssertHandleCounts();

            // Add a single handler

            bus.RegisterSubscriber(new MockSubscriber1());

            // Message should be handled
            bus.Send(new MockMessage1());
            AssertHandleCounts(1);

            // Message should NOT be handled, no handlers for this type
            bus.Send(new MockMessage2());
            AssertHandleCounts(1);

            ResetHandleCounts();

            bus.RegisterSubscriber(new MockSubscriber2());
            AssertHandleCounts();

            // Handled by the respective handler
            bus.Send(new MockMessage1());
            AssertHandleCounts(1);

            ResetHandleCounts();
            bus.Send(new MockMessage2());
            AssertHandleCounts(m2: 1);
        }

        // Test that a handler that wants an interface type gets called appropriately when concrete classes are passed in
        [Fact]
        public void InterfaceHandlers()
        {
            bus.RegisterSubscriber(new MockIMessageSubscriber());

            bus.Send(new MockMessage1());
            AssertHandleCounts(mi: 1);
            bus.Send(new MockMessage2());
            AssertHandleCounts(mi: 2);
        }

        // Ensure classes that subscribe to multiple events don't cause issues
        [Fact]
        public void MultiHandler()
        {
            var handler = new MockSubscribers();

            // Not specifying type causes compiler error as it doesn't know which types to handle, since the subscriber wants multiple types
            //bus.RegisterSubscriber(handler);

            bus.RegisterSubscriber<MockMessage1>(handler);
            bus.RegisterSubscriber<MockMessage2>(handler);

            bus.Send(new MockMessage1());
            AssertHandleCounts(mb1: 1);
            ResetHandleCounts();

            bus.Send(new MockMessage2());
            AssertHandleCounts(mb2: 1);
        }

        // Test that each handler that wants a given type gets called
        [Fact]
        public void MultipleHandlersSameType()
        {
            bus.RegisterSubscriber(new MockSubscriber1());
            bus.RegisterSubscriber(new MockSubscriber1());

            bus.Send(new MockMessage1());
            AssertHandleCounts(2);
        }

        // Ensure handlers that are added are called and update appropriately
        [Fact]
        public void RegisterHandler()
        {
            bus.Send(new MockMessage1());
            AssertHandleCounts();

            Assert.Equal(0, bus.SubscriberCount);
            bus.RegisterSubscriber(new MockSubscriber1());
            Assert.Equal(1, bus.SubscriberCount);
            AssertHandleCounts();

            bus.Send(new MockMessage1());
            AssertHandleCounts(1);
        }

        // Test that exception is thrown when the same subscriber is added twice
        [Fact]
        public void TestDoubleAdd()
        {
            var sub = new MockSubscriber1();
            bus.RegisterSubscriber(sub);

            Assert.Throws<ArgumentException>(() => bus.RegisterSubscriber(sub));
        }

        // Test that double/nonexistant removes throw ArgumentException
        [Fact]
        public void TestDoubleRemove()
        {
            var sub = new MockSubscriber1();
            bus.RegisterSubscriber(sub);
            bus.UnregisterSubscriber(sub);

            Assert.Throws<ArgumentException>(() => bus.UnregisterSubscriber(sub));
        }

        // Ensure that if we have class B : A, and we pass message of type A whose runtime type is B, B handler gets called.  Similar for interfaces
        [Fact]
        public void TypeDetermination()
        {
            IMockMessage msg = new MockMessage1();

            bus.RegisterSubscriber(new MockSubscriber1());

            // Handler wants MyMessage1 types, msg is IMsgInterface but runtime type MyMessage1, so it should still get called
            bus.Send(msg);
            AssertHandleCounts(1);

            ResetHandleCounts();

            MockMessageBase msg2 = new MockMessage2();
            bus.RegisterSubscriber(new MockSubscriber2());

            // Similar here, Msg2Sub should still get called.
            bus.Send(msg2);
            AssertHandleCounts(0, 1);
        }

        // Ensure that handlers that are unregistered aren't called anymore
        [Fact]
        public void UnregisterHandler()
        {
            var sub = new MockSubscriber1();
            bus.RegisterSubscriber(sub);

            Assert.Equal(1, bus.SubscriberCount);
            bus.UnregisterSubscriber(sub);
            Assert.Equal(0, bus.SubscriberCount);

            bus.Send(new MockMessage1());
            AssertHandleCounts();
        }
    }
}
