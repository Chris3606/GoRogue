using System;

namespace GoRogue.DiceNotation.Exceptions
{
    /// <summary>
    /// Exception that is thrown when a dice term is constructed with a negative number of dice.
    /// </summary>
    public class InvalidMultiplicityException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public InvalidMultiplicityException()
        {
        }

        /// <summary>
        /// Constructor, taking a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public InvalidMultiplicityException(string message)
           : base(message)
        {
        }

        /// <summary>
        /// Constructor, taking a specified error message and a reference to the inner exception that
        /// is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception. If the innerException parameter
        /// is not a null reference, the current exception is raised in a catch block that handles
        /// the inner exception.
        /// </param>
        public InvalidMultiplicityException(string message, Exception innerException)
           : base(message, innerException)
        {
        }
    }
}