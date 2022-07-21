using System;
using GoRogue.Messaging;

namespace GoRogue.PerformanceTests.Messaging
{
    internal interface ISubscriberRef
    {
        object Subscriber { get; }
        Action<object> Handler { get; }
    }

    internal class SubscriberRef<TMessage> : ISubscriberRef
    {
        public SubscriberRef(ISubscriber<TMessage> subscriber)
        {
            Subscriber = subscriber;
            Handler = o => subscriber.Handle((TMessage)o);
        }

        public object Subscriber { get; }
        public Action<object> Handler { get; }
    }
}
