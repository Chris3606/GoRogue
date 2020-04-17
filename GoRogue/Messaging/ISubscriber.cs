namespace GoRogue.Messaging
{
    /// <summary>
    /// Interface representing subscribers to messages sent over a <see cref="MessageBus"/>.  Classes wishing to respond to one or more message types as they are sent across
    /// the bus should implement this interface.
    /// </summary>
    /// <remarks>
    /// It is possible to have one class handle two different event types, by having it implement multiple ISubscriber types, passing different types as TMessage.
    /// When this is performed, however, the compiler will be unable to automatically resolve the template parameter of <see cref="MessageBus.RegisterSubscriber{TMessage}(ISubscriber{TMessage})"/>,
    /// so it will need to be specified manually.  Further, the RegisterSubscriber function will need to be called once for each ISubscriber type the class implements.
    /// <example>
    /// In this example, MultipleSubscriber wants to respond to messages of both type string and string[], without using any component classes or any such method of splitting
    /// up the implementations.  Thus, we implement the appropriate ISubscriber interfaces, and call the message bus's
    /// <see cref="MessageBus.RegisterSubscriber{TMessage}(ISubscriber{TMessage})"/> function once for each interface, explicitly specifying the type.
    /// <code>
    /// class MultipleSubscriber : ISubscriber&lt;string&gt;, ISubscriber&lt;string[]&gt;
    /// {
    ///		/* Explicit interface definitions are not required but are recommened for code clarity
    ///		void ISubscriber&lt;string&gt;.Handle(string message) => Console.WriteLine(message);
    ///		void ISubscriber&lt;string[]&gt;.Handle(string[] message) => Console.WriteLine(message.ExtendToString());
    /// }
    /// 
    /// /* Later, when we add the subscriber to our message bus, we add each subscriber interface seperately */
    /// var messageBus = new MessageBus();
    /// var multiSubber = new MultipleSubscriber();
    /// messageBus.RegisterSubscriber&lt;string&gt;(multiSubber);
    /// messageBus.RegisterSubscriber&lt;string[]&gt;(multiSubber);
    /// </code>
    /// </example>
    /// </remarks>
    /// <typeparam name="TMessage">The type of message that the subscriber wants to handle.  Any and all messages sent over the event bus you subscribe to
    /// that can cast to this type will be passed the <see cref="Handle(TMessage)"/> function when they are sent.</typeparam>
    public interface ISubscriber<TMessage>
    {
        /// <summary>
        /// Function that should handle the specified type of message in whatever manner it needs to.  Called automatically any time a message is sent
        /// over an event bus this subscriber has been registered on.
        /// </summary>
        /// <param name="message">Message that was sent.</param>
        void Handle(TMessage message);
    }
}
