namespace Commands;

/// <summary>
///     The result of an invocation operation within the command execution pipeline.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public readonly struct InvokeResult : IResult
{
    /// <inheritdoc />
    public Exception? Exception { get; }

    /// <inheritdoc />
    public bool Success
        => Exception == null;

    /// <summary>
    ///     Gets the command responsible for the invocation.
    /// </summary>
    public Command Command { get; }

    /// <summary>
    ///     Gets the value returned by the method execution.
    /// </summary>
    public object? ReturnValue { get; }

    internal InvokeResult(Command command, object? value, Exception? exception)
    {
        ReturnValue = value;
        Command = command;
        Exception = exception;
    }

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
}
