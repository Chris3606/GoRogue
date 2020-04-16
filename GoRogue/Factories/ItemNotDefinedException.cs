using System;
using System.Runtime.Serialization;

namespace GoRogue.Factories
{
    /// <summary>
    /// Exception thrown by <see cref="AdvancedFactory{TBlueprintConfig, TProduced}"/> or <see cref="Factory{TProduced}"/> objects when a blueprint that doesn't exist is used.
    /// </summary>
    [Serializable]
    public class ItemNotDefinedException : Exception
    {
        /// <summary>
        /// Creates an exception with no message.
        /// </summary>
        public ItemNotDefinedException()
            : base()
        { }

        /// <summary>
        /// Creates an exception with the specified inner exception and message.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that caused this exception</param>
        public ItemNotDefinedException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Creates an exception based on serialization context.
        /// </summary>
        /// <param name="info"/>
        /// <param name="context"/>
        protected ItemNotDefinedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
        
        /// <summary>
        /// Creates an exception with a message based on the specified factory ID.
        /// </summary>
        /// <param name="factoryId">Factory id that caused the error.</param>
        public ItemNotDefinedException(string factoryId)
            : base($"The item '{factoryId}' has not been added to this factory.")
        { }
    }
}
