namespace Commands.Exceptions
{
    /// <summary>
    ///     An exception thrown anywhere in the command execution pipeline.
    /// </summary>
    public class ExecutionException : Exception
    {
        /// <summary>
        ///     Creates a new <see cref="ExecutionException"/>.
        /// </summary>
        /// <param name="message">The message that represents the reason of the exception being thrown.</param>
        public ExecutionException(string message)
            : base(message)
        {

        }

        /// <summary>
        ///     Creates a new <see cref="ExecutionException"/>.
        /// </summary>
        /// <param name="message">The message that represents the reason of the exception being thrown.</param>
        /// <param name="innerException">An exception thrown by an inner operation, if present.</param>
        public ExecutionException(string message, Exception innerException = null)
            : base(message, innerException)
        {

        }
    }
}
