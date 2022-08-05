using JetBrains.Annotations;

namespace GoRogue.Messaging
{
    /// <summary>
    /// Interface representing subscribers to messages sent over a <see cref="MessageBus" />.  Classes wishing to respond to
    /// one or more message types as they are sent across the bus should implement this interface.
    /// </summary>
    /// <remarks>
    /// It is possible to have one class handle two different event types, by having it implement multiple ISubscriber types;
    /// however in this situation, you must either use the <see cref="MessageBus.RegisterAllSubscribers{T}"/> method (or its "Try" variant)
    /// to register the subscriber, or call the <see cref="MessageBus.RegisterSubscriber{TMessage}"/> method once for each type of message the subscriber
    /// subscribes to (specifying the TMessage type manually each time).  Although RegisterAllSubscribers will work for most use cases, when you want the fastest
    /// performance, you will want to prefer the RegisterSubscriber method, since RegisterAllSubscribers uses reflection.
    ///
    /// <example>
    /// In this example, MultipleSubscriber wants to respond to messages of both type string and string[], without using any
    /// component classes or any such method of splitting up the implementations.  Thus, we implement the appropriate ISubscriber interfaces,
    /// and call the message bus's <see cref="MessageBus.RegisterAllSubscribers{T}(T)" /> function to register the subscriber.
    ///
    /// <code>
    ///  class MultipleSubscriber : ISubscriber&lt;string&gt;, ISubscriber&lt;string[]&gt;
    ///  {
    /// 		/* Explicit interface definitions are not required but are recommend for code clarity
    /// 		void ISubscriber&lt;string&gt;.Handle(string message) => Console.WriteLine(message);
    /// 		void ISubscriber&lt;string[]&gt;.Handle(string[] message) => Console.WriteLine(message.ExtendToString());
    ///  }
    ///  
    ///  /* Later, when we add the subscriber to our message bus, we simply use the RegisterAllSubscribers method to ensure the subscriber is registered to both types. */
    ///  var messageBus = new MessageBus();
    ///  
    ///  var multiSubber = new MultipleSubscriber();
    ///  messageBus.RegisterAllSubscribers(multiSubber);
    ///  </code>
    ///
    /// Alternatively, we can use the RegisterSubscriber method to register the subscriber to each type individually.
    /// 
    /// <code>
    ///  /* Here, when we add the subscriber to our message bus, we add each subscriber interface separately */
    ///  var messageBus = new MessageBus();
    ///  
    ///  var multiSubber = new MultipleSubscriber();
    ///  messageBus.RegisterSubscriber&lt;string&gt;(multiSubber);
    ///  messageBus.RegisterSubscriber&lt;string[]&gt;(multiSubber);
    ///  </code>
    /// </example>
    /// </remarks>
    /// <typeparam name="TMessage">
    /// The type of message that the subscriber wants to handle.  Any and all messages sent over the event bus you subscribe to
    /// that can cast to this type will be passed the <see cref="Handle(TMessage)" /> function when they are sent.
    /// </typeparam>
    [PublicAPI]
    public interface ISubscriber<in TMessage>
    {
        /// <summary>
        /// Function that should handle the specified type of message in whatever manner it needs to.  Called automatically any
        /// time a message of the appropriate type is sent over an event bus this subscriber has been registered on.
        /// </summary>
        /// <param name="message">Message that was sent.</param>
        void Handle(TMessage message);
    }
}
