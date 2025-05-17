namespace Commands;

/// <summary>
///     An asynchronous mechanism that contains metadata about, and includes logic to respond to a command request. 
///     This class implements an awaitable response operation.
/// </summary>
/// <remarks>
///     This class is intended to be implemented to provide custom behavior for sending responses, or expanding the metadata that is sent with the entry of the pipeline.
/// </remarks>
public abstract class AsyncContext : IContext
{
    /// <inheritdoc />
    public abstract Arguments Arguments { get; }

    /// <summary>
    ///     Responds asynchronously with the provided message.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>An awaitable <see cref="Task"/> holding the response state.</returns>
    public abstract Task Respond(object? message);

    void IContext.Respond(object? message)
        => Respond(message).Wait();
}
