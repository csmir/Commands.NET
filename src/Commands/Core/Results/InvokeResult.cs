namespace Commands;

/// <summary>
///     The result of an invocation operation within the command execution pipeline.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public readonly struct InvokeResult : IValueResult
{
    /// <summary>
    ///     Gets the command responsible for the invocation.
    /// </summary>
    public Command Command { get; }

    /// <inheritdoc />
    public object? Value { get; }

    /// <inheritdoc />
    public Exception? Exception { get; }

    /// <inheritdoc />
    public bool Success
        => Exception == null;

    private InvokeResult(Command command, object? value, Exception? exception)
    {
        Command = command;
        Value = value;

        Exception = exception;
    }

    /// <summary>
    ///     Creates a new <see cref="InvokeResult"/> resembling a successful invocation operation.
    /// </summary>
    /// <param name="command">The command that was invoked.</param>
    /// <param name="value">The result of command execution, being the returned value by the executed method.</param>
    /// <returns>A new result containing information about the operation.</returns>
    public static InvokeResult FromSuccess(Command command, object? value)
        => new(command, value, null);

    /// <summary>
    ///     Creates a new <see cref="InvokeResult"/> resembling a failed invocation operation.
    /// </summary>
    /// <param name="command">The command that failed to be invoked</param>
    /// <param name="exception">The exception that occurred during command invocation.</param>
    /// <returns>A new result containing information about the operation.</returns>
    public static InvokeResult FromError(Command command, Exception exception)
        => new(command, null, exception);

    /// <inheritdoc />
    public override string ToString()
        => $"Command = {Command} \nSuccess = {(Exception == null ? "True" : $"False \nException = {Exception.Message}")}";

    /// <summary>
    ///     Gets a string representation of this result.
    /// </summary>
    /// <param name="inline">Sets whether the string representation should be inlined or not.</param>
    /// <returns>A string containing a formatted value of the result.</returns>
    public string ToString(bool inline)
        => inline ? $"Success = {(Exception == null ? "True" : $"False")}" : ToString();

    /// <summary>
    ///     Implicitly converts a <see cref="InvokeResult"/> to a <see cref="ValueTask{TResult}"/>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator ValueTask<InvokeResult>(InvokeResult result)
        => new(result);
}
