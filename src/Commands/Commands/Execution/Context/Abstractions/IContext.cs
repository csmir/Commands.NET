namespace Commands;

/// <summary>
///     A mechanism that contains metadata about, and includes logic to respond to a command request. 
/// </summary>
/// <remarks>
///     This interface is intended to be implemented to provide custom behavior for sending responses, or expanding the metadata that is sent with the entry of the pipeline.
/// </remarks>
public interface IContext
{
    /// <summary>
    ///     The arguments that were provided to the command for which this context was created.
    /// </summary>
    public Arguments Arguments { get; }

    /// <summary>
    ///     Sends a response using the given context as the metadata that can determine where to send said response.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>An awaitable <see cref="Task"/> containing the state of the response.</returns>
    public void Respond(object? message);
}
