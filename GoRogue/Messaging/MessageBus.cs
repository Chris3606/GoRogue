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
        private readonly Dictionary<Type, List<ISubscriberRef>> _subscriberRefs;
        private readonly Dictionary<Type, Type[]> _typeTreeCache;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageBus()
        {
            _subscriberRefs = new Dictionary<Type, List<ISubscriberRef>>();
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
            var messageType = typeof(TMessage);

            if (!_subscriberRefs.ContainsKey(messageType))
                _subscriberRefs[messageType] = new List<ISubscriberRef>();
            else if (_subscriberRefs[messageType].Any(i => ReferenceEquals(i.Subscriber, subscriber)))
                throw new ArgumentException("Subscriber added to message bus twice.", nameof(subscriber));

            _subscriberRefs[messageType].Add(new SubscriberRef<TMessage>(subscriber));
            SubscriberCount++;

            return subscriber;
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

            foreach (var type in types)
                if (_subscriberRefs.TryGetValue(type, out List<ISubscriberRef>? handlerRefs))
                    foreach (var handlerRef in handlerRefs)
                        handlerRef.Handler(message);
        }

        // Non-template interface for object-level handler functions
        internal interface ISubscriberRef
        {
            // Used for reference-equals comparison, so it can be safely stored as type object
            object Subscriber { get; }
            Action<object> Handler { get; }
        }

        // Creates handler based on ISubscriber passed in that casts the object and calls the subscribers handle.
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
}
