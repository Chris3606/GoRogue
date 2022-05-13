using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.Messaging;

namespace GoRogue.PerformanceTests.Messaging
{
    public class NoForEachMessageBus
    {
        private readonly Dictionary<Type, List<ISubscriberRef>> _subscriberRefs;
        private readonly Dictionary<Type, Type[]> _typeTreeCache;

        public NoForEachMessageBus()
        {
            _subscriberRefs = new Dictionary<Type, List<ISubscriberRef>>();
            _typeTreeCache = new Dictionary<Type, Type[]>();
            SubscriberCount = 0;
        }

        public int SubscriberCount { get; private set; }

        public ISubscriber<TMessage> RegisterSubscriber<TMessage>(ISubscriber<TMessage> subscriber)
        {
            var messageType = typeof(TMessage);

            if (!_subscriberRefs.ContainsKey(messageType))
                _subscriberRefs[messageType] = new List<ISubscriberRef>();
            else if (_subscriberRefs[messageType].Any(i => ReferenceEquals(i.Subscriber, subscriber)))
                throw new ArgumentException("Subscriber added to message bus twice.", nameof(subscriber));

            _subscriberRefs[messageType].Add(new SubscriberRef<TMessage>(subscriber));
            SubscriberCount++;

            return subscriber;
        }

        public void UnregisterSubscriber<TMessage>(ISubscriber<TMessage> subscriber)
        {
            var messageType = typeof(TMessage);

            if (_subscriberRefs.TryGetValue(messageType, out List<ISubscriberRef>? handlerRefs))
            {
                var item = handlerRefs.FindIndex(i => ReferenceEquals(i.Subscriber, subscriber));

                if (item == -1)
                    throw new ArgumentException(
                        $"Tried to remove a subscriber from a {nameof(MessageBus)} that was never added.");

                handlerRefs.RemoveAt(item);
                if (handlerRefs.Count == 0)
                    _subscriberRefs.Remove(messageType);

                SubscriberCount--;
            }
            else
                throw new ArgumentException(
                    $"Tried to remove a subscriber from a {nameof(MessageBus)} that was never added.");
        }

        public void Send<TMessage>(TMessage message) where TMessage : notnull
        {
            var runtimeMessageType = message.GetType();
            if (!_typeTreeCache.TryGetValue(runtimeMessageType, out Type[]? types))
                types = _typeTreeCache[runtimeMessageType] = ReflectionAddons.GetTypeTree(runtimeMessageType).ToArray();


            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (_subscriberRefs.TryGetValue(type, out List<ISubscriberRef>? handlerRefs))
                    for (int j = 0; j < handlerRefs.Count; j++)
                        handlerRefs[j].Handler(message);
            }
        }
    }
}
