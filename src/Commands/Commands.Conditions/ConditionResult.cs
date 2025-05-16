namespace Commands.Conditions;

/// <summary>
///     The result of a check operation within the command execution pipeline.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public readonly struct ConditionResult : IResult
{
    /// <inheritdoc />
    public Exception? Exception { get; }

    /// <inheritdoc />
    public bool Success
        => Exception == null;

    private ConditionResult(Exception? exception)
        => Exception = exception;

    /// <summary>
    ///     Creates a new <see cref="ConditionResult"/> resembling a successful check operation.
    /// </summary>
    /// <returns>A new result containing information about the succeeded operation.</returns>
    public static ConditionResult FromSuccess()
        => new(null);

    /// <summary>
    ///     Creates a new <see cref="ConditionResult"/> resembling a failed check operation.
    /// </summary>
    /// <param name="exception">The exception that contains the failure reason of this check.</param>
    /// <returns>A new result containing information about the failed operation.</returns>
    public static ConditionResult FromError(Exception exception)
        => new(exception);

    /// <inheritdoc />
    public override string ToString()
        => $"Success = {(Exception == null ? "True" : $"False \nException = {Exception.Message}")}";

    /// <summary>
    ///     Gets a string representation of this result.
    /// </summary>
    /// <param name="inline">Sets whether the string representation should be inlined or not.</param>
    /// <returns>A string containing a formatted value of the result.</returns>
    public string ToString(bool inline)
        => inline ? $"Success = {(Exception == null ? "True" : $"False")}" : ToString();

    /// <summary>
    ///     Implicitly converts a <see cref="ConditionResult"/> to a <see cref="ValueTask{TResult}"/>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator ValueTask<ConditionResult>(ConditionResult result)
        => new(result);
}
