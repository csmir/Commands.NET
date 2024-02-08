namespace Commands.Exceptions
{
    /// <summary>
    ///     An <see cref="ExecutionException"/> that is thrown when a command failed precondition validation.
    /// </summary>
    /// <param name="message">The message that represents the reason of the exception being thrown.</param>
    /// <param name="innerException">An exception thrown by an inner operation, if present.</param>
    public sealed class ConditionException(string message, Exception innerException = null)
        : ExecutionException(message, innerException)
    {
        const string POSTCONDITION_FAILED = "Postcondition evaluation failed. View inner exception for more details.";
        const string PRECONDITION_FAILED = "Precondition evaluation failed. View inner exception for more details.";

        internal static ConditionException PreconditionFailed(Exception innerException)
        {
            return new(PRECONDITION_FAILED, innerException);
        }

        internal static ConditionException PostconditionFailed(Exception innerException)
        {
            return new(POSTCONDITION_FAILED, innerException);
        }
    }
}
