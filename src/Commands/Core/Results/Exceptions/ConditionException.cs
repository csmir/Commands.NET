using Commands.Conditions;

namespace Commands;

/// <summary>
///     Represents an exception that is thrown when a condition fails to evaluate.
/// </summary>
public sealed class ConditionException(IExecuteCondition condition, string reason, Exception? innerException = null)
    : Exception(reason, innerException)
{
    /// <summary>
    ///     Gets the condition that caused the exception.
    /// </summary>
    public IExecuteCondition Condition { get; } = condition;
}

/// <summary>
///     Represents an exception that is thrown when one or more conditions failed to evaluate.
/// </summary>
public sealed class PipelineConditionException(Command command, Exception? innerException = null)
    : Exception(MESSAGE, innerException)
{
    const string MESSAGE = "One or more conditions failed during evaluation.";

    /// <summary>
    ///     Gets the command that caused the exception.
    /// </summary>
    public Command Command { get; } = command;
}