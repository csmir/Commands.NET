namespace Commands;

/// <summary>
///     The result of a convert operation within the command execution pipeline.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public readonly struct ParseResult : IValueResult
{
    /// <inheritdoc />
    public object? Value { get; }

    /// <inheritdoc />
    public Exception? Exception { get; }

    /// <inheritdoc />
    public bool Success
        => Exception == null;

    private ParseResult(Exception? exception, object? value)
    {
        Exception = exception;
        Value = value;
    }

    /// <summary>
    ///     Creates a new <see cref="ParseResult"/> resembling a successful conversion operation.
    /// </summary>
    /// <remarks>
    ///     This overload is called when conversion succeeds with a value.
    /// </remarks>
    /// <param name="value">The converted value of the operation.</param>
    /// <returns>A new result containing information about the operation.</returns>
    public static ParseResult FromSuccess(object? value)
        => new(null, value);

    /// <summary>
    ///     Creates a new <see cref="ParseResult"/> resembling a failed conversion operation.
    /// </summary>
    /// <param name="exception">The exception that occurred during the conversion operation.</param>
    /// <returns>A new result containing information about the operation.</returns>
    public static ParseResult FromError(Exception exception)
        => new(exception, null);

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
    ///     Implicitly converts a <see cref="ParseResult"/> to a <see cref="ValueTask{TResult}"/>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator ValueTask<ParseResult>(ParseResult result)
        => new(result);
}
