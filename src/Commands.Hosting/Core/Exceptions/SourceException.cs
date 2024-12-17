namespace Commands
{
    /// <summary>
    ///     An <see cref="CommandException"/> that is thrown when command source acquirement failed. This class cannot be inherited.
    /// </summary>
    /// <param name="message">The message that represents the reason of the exception being thrown.</param>
    /// <param name="innerException">An exception thrown by an inner operation, if present.</param>
    public sealed class SourceException(string message, Exception? innerException = null)
        : CommandException(message, innerException)
    {
        const string SOURCE_FAILED = "Failed to acquire source data. View inner exception for more details.";

        internal static SourceException GetSourceFailed(Exception? innerException = null)
            => new(SOURCE_FAILED, innerException);
    }
}
