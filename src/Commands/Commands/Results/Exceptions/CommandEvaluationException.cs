namespace Commands;

/// <summary>
///     Represents an exception that is created when a condition fails to evaluate.
/// </summary>
public class ConditionException(string reason)
    : Exception(reason)
{
}

/// <summary>
///     Represents an exception that is thrown when one or more conditions failed to evaluate.
/// </summary>
public sealed class CommandEvaluationException(Command command, Exception? innerException = null)
    : Exception(null, innerException)
{
    /// <summary>
    ///     Gets the command that caused the exception.
    /// </summary>
    public Command Command { get; } = command;
}