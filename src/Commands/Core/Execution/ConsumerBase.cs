namespace Commands
{
    /// <summary>
    ///     Represents the data about the consumer of the command.
    /// </summary>
    public class ConsumerBase
    {
        /// <summary>
        ///     Sends a response to the consumer. This method is virtual and can be overridden to send the response to a different location.
        /// </summary>
        /// <param name="response">The response to send.</param>
        /// <returns>An awaitable <see cref="Task"/> containing the state of the response. This call does not need to be awaited, running async if not.</returns>
        public virtual Task SendAsync(object response)
        {
            Console.WriteLine(response);

            return Task.CompletedTask;
        }
    }
}
