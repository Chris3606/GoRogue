using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.Messaging;

namespace GoRogue.PerformanceTests.Messaging
{
    public class OptimizedAndCacheSubsMessageBus
    {
        private readonly Dictionary<Type, List<(object subscriber, Action<object> handler)>> _subscriberRefs;
        private readonly Dictionary<Type, Type[]> _typeTreeCache;

        private readonly List<Action<object>> _currentSubscribers;

        public OptimizedAndCacheSubsMessageBus()
        {
            _subscriberRefs = new Dictionary<Type, List<(object subscriber, Action<object> handler)>>();
            _typeTreeCache = new Dictionary<Type, Type[]>();
            SubscriberCount = 0;
            _currentSubscribers = new List<Action<object>>();
        }

        public int SubscriberCount { get; private set; }

        public ISubscriber<TMessage> RegisterSubscriber<TMessage>(ISubscriber<TMessage> subscriber)
        {
            var messageType = typeof(TMessage);

            if (!_subscriberRefs.ContainsKey(messageType))
                _subscriberRefs[messageType] = new List<(object subscriber, Action<object> handler)>();
            else if (_subscriberRefs[messageType].Any(i => ReferenceEquals(i.subscriber, subscriber)))
                throw new ArgumentException("Subscriber added to message bus twice.", nameof(subscriber));

            _subscriberRefs[messageType].Add((subscriber, msg => subscriber.Handle((TMessage)msg)));
            SubscriberCount++;

            return subscriber;
        }

        public void UnregisterSubscriber<TMessage>(ISubscriber<TMessage> subscriber)
        {
            var messageType = typeof(TMessage);

            if (_subscriberRefs.TryGetValue(messageType, out List<(object subscriber, Action<object> handler)>? handlerRefs))
            {
                var item = handlerRefs.FindIndex(i => ReferenceEquals(i.subscriber, subscriber));

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

            // Cache list of subscribers so that subscribers can Register/Unregister freely without causing exception
            for (int i = 0; i < types.Length; i++)
            {
                if (!_subscriberRefs.TryGetValue(types[i], out var curSubCache)) continue;
                for (int j = 0; j < curSubCache.Count; j++)
                    _currentSubscribers.Add(curSubCache[j].handler);
            }

            // Call subscribers based on cache
            for (int i = 0; i < _currentSubscribers.Count; i++)
                _currentSubscribers[i](message);

            // Clear cache for next Send
            _currentSubscribers.Clear();
        }
    }
}
