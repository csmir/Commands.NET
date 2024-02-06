namespace Commands.Exceptions
{
    /// <summary>
    ///     An <see cref="ExecutionException"/> that is thrown when precondition validation or argument conversion failed.
    /// </summary>
    /// <param name="message">The message that represents the reason of the exception being thrown.</param>
    /// <param name="innerException">An exception thrown by an inner operation, if present.</param>
    public class MatchException(string message, Exception innerException = null)
        : ExecutionException(message, innerException)
    {

    }
}
