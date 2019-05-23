using GoRogue.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace GoRogue_UnitTests
{
	#region MessageTypes
	interface IMsgInterface { }

	class BaseMessge : IMsgInterface { }

	class MyMessage1 : IMsgInterface { }
	class MyMessage2 : BaseMessge { }
	#endregion

	#region ConcreteSubscribers
	class Msg1Sub : ISubscriber<MyMessage1>
	{
		public static int HandleCount = 0;

		public void Handle(MyMessage1 message)
		{
			HandleCount++;
		}
	}

	class Msg2Sub : ISubscriber<MyMessage2>
	{
		public static int HandleCount = 0;

		public void Handle(MyMessage2 message)
		{
			HandleCount++;
		}
	}

	class MsgBothSub : ISubscriber<MyMessage1>, ISubscriber<MyMessage2>
	{
		public static int Handle1Count = 0;
		public static int Handle2Count = 0;

		void ISubscriber<MyMessage1>.Handle(MyMessage1 message)
		{
			Handle1Count++;
		}

		void ISubscriber<MyMessage2>.Handle(MyMessage2 message)
		{
			Handle2Count++;
		}
	}
	#endregion

	#region AbstractSubscribers
	class MsgInterfaceSub : ISubscriber<IMsgInterface>
	{
		public static int HandleCount = 0;

		public void Handle(IMsgInterface message)
		{
			HandleCount++;
		}
	}

	class MsgBaseSub : ISubscriber<BaseMessge>
	{
		public static int HandleCount = 0;

		public void Handle(BaseMessge message)
		{
			HandleCount++;
		}
	}
	#endregion

	[TestClass]
	public class MessageBusTests
	{
		private MessageBus bus;

		#region Setup/Teardown
		[TestInitialize]
		public void TestInit()
		{
			bus = new MessageBus();
			ResetHandleCounts();
		}
		#endregion

		#region Tests
		// Test that handlers of concrete types are dispatched appropriately
		[TestMethod]
		public void BasicHandlers()
		{
			bus.Send(new MyMessage1());
			AssertHandleCounts();

			// Add a single handler

			bus.RegisterSubscriber(new Msg1Sub());

			// Message should be handled
			bus.Send(new MyMessage1());
			AssertHandleCounts(m1: 1);

			// Message should NOT be handled, no handlers for this type
			bus.Send(new MyMessage2());
			AssertHandleCounts(m1: 1);

			ResetHandleCounts();

			bus.RegisterSubscriber(new Msg2Sub());
			AssertHandleCounts();

			// Handled by the respective handler
			bus.Send(new MyMessage1());
			AssertHandleCounts(m1: 1);

			ResetHandleCounts();
			bus.Send(new MyMessage2());
			AssertHandleCounts(m2: 1);
		}

		// Ensure handlers that are added are called and update appropriately
		[TestMethod]
		public void RegisterHandler()
		{
			bus.Send(new MyMessage1());
			AssertHandleCounts();

			Assert.AreEqual(0, bus.SubscriberCount);
			bus.RegisterSubscriber(new Msg1Sub());
			Assert.AreEqual(1, bus.SubscriberCount);
			AssertHandleCounts();

			bus.Send(new MyMessage1());
			AssertHandleCounts(m1: 1);
		}

		// Ensure that handlers that are unregistered aren't called anymore
		[TestMethod]
		public void UnregisterHandler()
		{
			var sub = new Msg1Sub();
			bus.RegisterSubscriber(sub);

			Assert.AreEqual(1, bus.SubscriberCount);
			bus.UnregisterSubscriber(sub);
			Assert.AreEqual(0, bus.SubscriberCount);

			bus.Send(new MyMessage1());
			AssertHandleCounts();
		}

		// Ensure classes that subscribe to multiple events don't cause issues
		[TestMethod]
		public void MultiHandler()
		{
			var handler = new MsgBothSub();

			// Not specifying type causes compiler error as it doesn't know which types to handle, since the subscriber wants multiple types
			//bus.RegisterSubscriber(handler);

			bus.RegisterSubscriber<MyMessage1>(handler);
			bus.RegisterSubscriber<MyMessage2>(handler);

			bus.Send(new MyMessage1());
			AssertHandleCounts(mb1: 1);
			ResetHandleCounts();
			
			bus.Send(new MyMessage2());
			AssertHandleCounts(mb2: 1);
		}

		// Ensure that if we have class B : A, and we pass message of type A whose runtime type is B, B handler gets called.  Similar for interfaces
		[TestMethod]
		public void TypeDetermination()
		{
			IMsgInterface msg = new MyMessage1();

			bus.RegisterSubscriber(new Msg1Sub());

			// Handler wants MyMessage1 types, msg is IMsgInterface but runtime type MyMessage1, so it should still get called
			bus.Send(msg);
			AssertHandleCounts(m1: 1);
			
			ResetHandleCounts();

			BaseMessge msg2 = new MyMessage2();
			bus.RegisterSubscriber(new Msg2Sub());

			// Similar here, Msg2Sub should still get called.
			bus.Send(msg2);
			AssertHandleCounts(m1: 0, m2: 1);
		}

		// Test that each handler that wants a given type gets called
		[TestMethod]
		public void MultipleHandlersSameType()
		{
			bus.RegisterSubscriber(new Msg1Sub());
			bus.RegisterSubscriber(new Msg1Sub());

			bus.Send(new MyMessage1());
			AssertHandleCounts(m1: 2);
		}

		// Test that a handler that wants an interface type gets called appropriately when concrete classes are passed in
		[TestMethod]
		public void InterfaceHandlers()
		{
			bus.RegisterSubscriber(new MsgInterfaceSub());

			bus.Send(new MyMessage1());
			AssertHandleCounts(mi: 1);
			bus.Send(new MyMessage2());
			AssertHandleCounts(mi: 2);
		}

		// Test that a handler that wants a base class type gets called appropriately when concrete classes are passed in
		[TestMethod]
		public void BaseClassHandlers()
		{
			bus.RegisterSubscriber(new MsgBaseSub());

			bus.Send(new MyMessage2());
			AssertHandleCounts(mbase: 1);
		}

		// Test that exception is thrown when the same subscriber is added twice
		[TestMethod]
		public void TestDoubleAdd()
		{
			Msg1Sub sub = new Msg1Sub();
			bus.RegisterSubscriber(sub);

			Assert.ThrowsException<System.ArgumentException>(() => bus.RegisterSubscriber(sub));
		}

		// Test that double/nonexistant removes throw ArgumentException
		[TestMethod]
		public void TestDoubleRemove()
		{
			Msg1Sub sub = new Msg1Sub();
			bus.RegisterSubscriber(sub);
			bus.UnregisterSubscriber(sub);

			Assert.ThrowsException<System.ArgumentException>(() => bus.UnregisterSubscriber(sub));
		}
		#endregion

		#region Helpers
		private void AssertHandleCounts(int m1 = 0, int m2 = 0, int mb1 = 0, int mb2 = 0, int mi = 0, int mbase = 0)
		{
			Assert.AreEqual(m1, Msg1Sub.HandleCount);
			Assert.AreEqual(m2, Msg2Sub.HandleCount);
			Assert.AreEqual(mb1, MsgBothSub.Handle1Count);
			Assert.AreEqual(mb2, MsgBothSub.Handle2Count);
			Assert.AreEqual(mi, MsgInterfaceSub.HandleCount);
			Assert.AreEqual(mbase,MsgBaseSub.HandleCount);
		}

		private void ResetHandleCounts()
		{
			Msg1Sub.HandleCount = 0;
			Msg2Sub.HandleCount = 0;
			MsgBothSub.Handle1Count = MsgBothSub.Handle2Count = 0;
			MsgInterfaceSub.HandleCount = 0;
			MsgBaseSub.HandleCount = 0;
		}
		#endregion
	}
}
