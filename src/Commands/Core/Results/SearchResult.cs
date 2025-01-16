namespace Commands;

/// <summary>
///     The result of a search operation within the command execution pipeline.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public readonly struct SearchResult : IExecuteResult
{
    /// <inheritdoc />
    public Exception Exception { get; }

    /// <inheritdoc />
    public bool Success { get; } = false;

    internal SearchResult(Exception exception)
        => Exception = exception;

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
}
