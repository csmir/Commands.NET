namespace Commands
{
    /// <summary>
    ///     Represents a metadata object that serves as a binding between the executing user of the command, and the logic that is the source of a response.
    /// </summary>
    /// <remarks>
    ///     This interface is intended to be implemented to provide custom behavior for sending responses to callers, or expanding the metadata that is sent with the entry of the pipeline.
    /// </remarks>
    public interface ICallerContext
    {
        /// <summary>
        ///     Sends a response to the caller.
        /// </summary>
        /// <param name="response">The response to send.</param>
        /// <returns>An awaitable <see cref="Task"/> containing the state of the response.</returns>
        public Task Respond(object? response);
    }
}
