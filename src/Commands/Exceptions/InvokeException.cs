﻿namespace Commands.Exceptions
{
    /// <summary>
    ///     An <see cref="ExecutionException"/> that is thrown when the command being executed failed to finish invocation.
    /// </summary>
    /// <param name="message">The message that represents the reason of the exception being thrown.</param>
    /// <param name="innerException">An exception thrown by an inner operation, if present.</param>
    public sealed class InvokeException(string message, Exception? innerException = null)
        : ExecutionException(message, innerException)
    {
        const string RUN_FAILED = "Command failed to finalize execution. View inner exception for more details.";
        const string RETURN_UNRESOLVED = "Command failed to handle unknown return type. Implement either void, ValueTask or Task.";

        internal static InvokeException InvokeFailed(Exception innerException)
        {
            return new(RUN_FAILED, innerException);
        }

        internal static InvokeException ReturnUnresolved()
        {
            return new(RETURN_UNRESOLVED);
        }
    }
}
