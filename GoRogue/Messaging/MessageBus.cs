using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace GoRogue.Messaging
{
    /// <summary>
    /// A messaging system that can have subscribers added to it, and send messages.  When messages are sent, it will call any
    /// handlers that requested to handle messages
    /// of the proper types, based on the type-tree/interface-tree of the messages.
    /// </summary>
    [PublicAPI]
    public class MessageBus
    {
        private readonly Dictionary<Type, List<(object subscriber, Action<object> handler)>> _subscriberRefs;
        private readonly Dictionary<Type, Type[]> _typeTreeCache;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageBus()
        {
            _subscriberRefs = new Dictionary<Type, List<(object subscriber, Action<object> handler)>>();
            _typeTreeCache = new Dictionary<Type, Type[]>();
            SubscriberCount = 0;
        }

        /// <summary>
        /// Number of subscribers currently listening on this message bus.
        /// </summary>
        public int SubscriberCount { get; private set; }

        /// <summary>
        /// Adds the given subscriber to the message bus's handlers list, so its Handle function will be called when any messages
        /// that can cast to <typeparamref name="TMessage" /> are sent via the <see cref="Send{TMessage}(TMessage)" /> function.
        /// Particularly when a handler is intended to have a shorter lifespan than the message bus, they MUST be unregistered via
        /// <see cref="UnregisterSubscriber{TMessage}(ISubscriber{TMessage})" /> when they are disposed of, to avoid the bus
        /// preventing
        /// the handler from being garbage collected.
        /// </summary>
        /// <typeparam name="TMessage">
        /// Type of message the subscriber is handling.  This can typically be inferred by the compiler,
        /// barring the case detailed in the <see cref="ISubscriber{TMessage}" /> remarks where one class subscribes to multiple
        /// message types.
        /// </typeparam>
        /// <param name="subscriber">Subscriber to add.</param>
        /// <returns>
        /// The subscriber that was added, in case a reference is needed to later call
        /// <see cref="UnregisterSubscriber{TMessage}(ISubscriber{TMessage})" />.
        /// </returns>
        public ISubscriber<TMessage> RegisterSubscriber<TMessage>(ISubscriber<TMessage> subscriber)
        {
            if (!TryRegisterSubscriber(subscriber))
                throw new ArgumentException("Subscriber added to message bus twice.", nameof(subscriber));

            return subscriber;
        }

        /// <summary>
        /// Tries to add the given subscriber to the message bus's handlers list, so its Handle function will be called when any messages
        /// that can cast to <typeparamref name="TMessage" /> are sent via the <see cref="Send{TMessage}(TMessage)" /> function.
        /// Particularly when a handler is intended to have a shorter lifespan than the message bus, they MUST be unregistered via
        /// <see cref="UnregisterSubscriber{TMessage}(ISubscriber{TMessage})" /> when they are disposed of, to avoid the bus
        /// preventing the handler from being garbage collected.
        /// </summary>
        /// <typeparam name="TMessage">
        /// Type of message the subscriber is handling.  This can typically be inferred by the compiler,
        /// barring the case detailed in the <see cref="ISubscriber{TMessage}" /> remarks where one class subscribes to multiple
        /// message types.
        /// </typeparam>
        /// <param name="subscriber">Subscriber to add.</param>
        /// <returns>
        /// True if the subscriber was added; false if it was already identically registered.
        /// </returns>
        public bool TryRegisterSubscriber<TMessage>(ISubscriber<TMessage> subscriber)
        {
            var messageType = typeof(TMessage);

            if (!_subscriberRefs.ContainsKey(messageType))
                _subscriberRefs[messageType] = new List<(object subscriber, Action<object> handler)>();
            else if (_subscriberRefs[messageType].Any(i => ReferenceEquals(i.subscriber, subscriber)))
                return false;

            _subscriberRefs[messageType].Add((subscriber, msg => subscriber.Handle((TMessage)msg)));
            SubscriberCount++;

            return true;
        }

        /// <summary>
        /// Removes the given subscriber from the message bus's handlers list.  Particularly when a subscriber is intended to have
        /// a shorter lifetime than the
        /// MessageBus object it subscribed with, handlers MUST be removed when disposed of so they can be garbage collected -- an
        /// object cannot be garbage-collected
        /// so long as it is registered as a subscriber to a message bus (unless the bus is also being garbage-collected).
        /// </summary>
        /// <typeparam name="TMessage">
        /// Type of message the subscriber is handling.  This can typically be inferred by the compiler,
        /// barring the case detailed in the <see cref="ISubscriber{TMessage}" /> remarks where one class subscribes to multiple
        /// message types.
        /// </typeparam>
        /// <param name="subscriber">Subscriber to remove.</param>
        public void UnregisterSubscriber<TMessage>(ISubscriber<TMessage> subscriber)
        {
            if (!TryUnregisterSubscriber(subscriber))
                throw new ArgumentException(
                    $"Tried to remove a subscriber from a {nameof(MessageBus)} that was never added.");
        }

        /// <summary>
        /// Tries to remove the given subscriber from the message bus's handlers list.  Particularly when a subscriber is intended to have
        /// a shorter lifetime than the
        /// MessageBus object it subscribed with, handlers MUST be removed when disposed of so they can be garbage collected -- an
        /// object cannot be garbage-collected
        /// so long as it is registered as a subscriber to a message bus (unless the bus is also being garbage-collected).
        /// </summary>
        /// <typeparam name="TMessage">
        /// Type of message the subscriber is handling.  This can typically be inferred by the compiler,
        /// barring the case detailed in the <see cref="ISubscriber{TMessage}" /> remarks where one class subscribes to multiple
        /// message types.
        /// </typeparam>
        /// <param name="subscriber">Subscriber to remove.</param>
        /// <returns>True if the subscriber was successfully removed; false if it was never registered.</returns>
        public bool TryUnregisterSubscriber<TMessage>(ISubscriber<TMessage> subscriber)
        {
            var messageType = typeof(TMessage);

            if (_subscriberRefs.TryGetValue(messageType,
                    out List<(object subscriber, Action<object> handler)>? handlerRefs))
            {
                var item = handlerRefs.FindIndex(i => ReferenceEquals(i.subscriber, subscriber));

                if (item == -1)
                    return false;

                handlerRefs.RemoveAt(item);
                if (handlerRefs.Count == 0)
                    _subscriberRefs.Remove(messageType);

                SubscriberCount--;
            }
            else
                return false;

            return true;
        }

        /// <summary>
        /// Sends the specified message on the message bus, automatically calling any appropriate registered handlers.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        public void Send<TMessage>(TMessage message) where TMessage : notnull
        {
            var runtimeMessageType = message.GetType();
            if (!_typeTreeCache.TryGetValue(runtimeMessageType, out Type[]? types))
                types = _typeTreeCache[runtimeMessageType] = ReflectionAddons.GetTypeTree(runtimeMessageType).ToArray();

            // Cache all subscriber lists we are about to iterate over, to ensure that calls to registration functions
            // don't interfere with the current sending
            var cache = new (object subscriber, Action<object> handler)[]?[types.Length];
            for (int i = 0; i < types.Length; i++)
                if (_subscriberRefs.TryGetValue(types[i],
                    out List<(object subscriber, Action<object> handler)>? handlerRefs))
                    cache[i] = handlerRefs.ToArray();

            // Call all handlers, from cache generated
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < cache.Length; i++)
            {
                var curCache = cache[i];
                if (curCache == null) continue;

                for (int j = 0; j < curCache.Length; j++)
                    curCache[j].handler(message);
            }
        }
    }
}
