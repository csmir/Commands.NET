namespace Commands
{
    /// <summary>
    ///     Represents a metadata object that serves as a binding between the executing user of the command, and the logic that is the source of a response.
    /// </summary>
    /// <remarks>
    ///     This class is intended to be overridden to provide custom behavior for sending responses to callers, or expanding the metadata that is sent with the entry of the pipeline.
    /// </remarks>
    public class CallerContext
    {
        /// <summary>
        ///     Sends a response to the caller. This method is virtual and can be overridden to send the response to a different location.
        /// </summary>
        /// <param name="response">The response to send.</param>
        /// <returns>An awaitable <see cref="Task"/> containing the state of the response. This call does not need to be awaited, running async if not.</returns>
        public virtual Task Respond(object response)
        {
            Console.WriteLine(response);

            return Task.CompletedTask;
        }
    }
}
