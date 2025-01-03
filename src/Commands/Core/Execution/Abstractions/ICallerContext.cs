namespace Commands;

/// <summary>
///     A mechanism that contains metadata about, and includes logic to respond to a command request. 
/// </summary>
/// <remarks>
///     This interface is intended to be implemented to provide custom behavior for sending responses to callers, or expanding the metadata that is sent with the entry of the pipeline.
/// </remarks>
public interface ICallerContext
{
    /// <summary>
    ///     Sends a response to the caller.
    /// </summary>
    /// <param name="message">The response to send.</param>
    /// <returns>An awaitable <see cref="Task"/> containing the state of the response.</returns>
    public void Respond(object? message);
}
