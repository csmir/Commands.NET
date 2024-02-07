using Commands.Core;

namespace Commands.Resolvers
{
    /// <summary>
    ///     A handler for post-execution processes.
    /// </summary>
    /// <remarks>
    ///     Implementing this type allows you to treat result data and scope finalization, regardless on whether the command execution succeeded or not.
    /// </remarks>
    public abstract class ResolverBase
    {
        /// <summary>
        ///     Evaluates the post-execution data, carrying result data, consumer data and the scoped <see cref="IServiceProvider"/> for the current execution.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="result">The result of the command execution.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to populate and run modules in this scope.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>An awaitable <see cref="ValueTask"/>.</returns>
        public abstract ValueTask EvaluateAsync(ConsumerBase consumer, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken);
    }
}
