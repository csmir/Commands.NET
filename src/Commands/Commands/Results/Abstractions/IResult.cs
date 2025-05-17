namespace Commands;

/// <summary>
///     The result of an operation within the command execution pipeline.
/// </summary>
public interface IResult
{
    /// <summary>
    ///     Gets the exception that represents the reason of a failed operation, if any.
    /// </summary>
    /// <remarks>
    ///     Will be <see langword="null"/> if <see cref="Success"/> returns <see langword="true"/>.
    /// </remarks>
    public Exception? Exception { get; }

    /// <summary>
    ///     Gets the result of the preceding operation. Returns <see langword="true"/> if the operation was successful, <see langword="false"/> otherwise.
    /// </summary>
    public bool Success { get; }
}
