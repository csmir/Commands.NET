using Commands.Conditions;

namespace Commands
{
    /// <summary>
    ///     An <see cref="CommandException"/> that is thrown when a command failed precondition validation.
    /// </summary>
    /// <param name="message">The message that represents the reason of the exception being thrown.</param>
    /// <param name="innerException">An exception thrown by an inner operation, if present.</param>
    public sealed class ConditionException(string message, Exception? innerException = null)
        : CommandException(message, innerException)
    {
        const string CONDITION_FAILED = "Condition evaluation failed. View inner exception for more details.";

        internal static ConditionException ConditionFailed(Exception innerException)
            => new(CONDITION_FAILED, innerException);
    }
}
