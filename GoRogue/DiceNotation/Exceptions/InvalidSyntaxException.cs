using System;

namespace GoRogue.DiceNotation.Exceptions
{
	/// <summary>
	/// Exception that is thrown when a the syntax of a dice notation string is determined to be invalid.
	/// </summary>
	public class InvalidSyntaxException : Exception
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public InvalidSyntaxException()
		{
		}

		/// <summary>
		/// Constructor, taking a specified error message.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		public InvalidSyntaxException(string message)
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
		public InvalidSyntaxException(string message, Exception innerException)
		   : base(message, innerException)
		{
		}
	}
}