namespace Commands.Exceptions
{
    /// <summary>
    ///     An <see cref="CommandException"/> that is thrown when command source acquirement failed.
    /// </summary>
    /// <param name="message">The message that represents the reason of the exception being thrown.</param>
    /// <param name="innerException">An exception thrown by an inner operation, if present.</param>
    public sealed class SourceException(string message, Exception? innerException = null)
        : CommandException(message, innerException)
    {
        const string SOURCE_FAILED = "Failed to acquire source data. View inner exception for more details.";

        internal static SourceException SourceAcquirementFailed(Exception innerException)
        {
            return new SourceException(SOURCE_FAILED, innerException);
        }
    }
}
