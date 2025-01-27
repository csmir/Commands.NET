namespace Commands;

/// <summary>
///     An asynchronous mechanism that contains metadata about, and includes logic to respond to a command request. 
///     This class implements an awaitable response operation.
/// </summary>
/// <remarks>
///     This class is intended to be implemented to provide custom behavior for sending responses to callers, or expanding the metadata that is sent with the entry of the pipeline.
/// </remarks>
public abstract class AsyncCallerContext : ICallerContext
{
    /// <inheritdoc />
    public ArgumentArray Arguments { get; }

    /// <summary>
    ///     Responds to the caller asynchronously with the provided message.
    /// </summary>
    /// <param name="message">The message to send to the caller.</param>
    /// <returns>An awaitable <see cref="Task"/> holding the response state.</returns>
    public abstract Task Respond(object? message);

    void ICallerContext.Respond(object? message)
        => Respond(message).Wait();
}
