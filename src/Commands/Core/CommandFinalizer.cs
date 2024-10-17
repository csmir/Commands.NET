using Commands.Resolvers;

namespace Commands
{
    /// <summary>
    ///     A container that is responsible for finalizing the command scope and notifying post-execution processes.
    /// </summary>
    /// <param name="resolvers">A collection of <see cref="ResultResolverBase"/>'s to run on post-execution.</param>
    public sealed class CommandFinalizer(IEnumerable<ResultResolverBase> resolvers)
    {
        private readonly ResultResolverBase[] _resolvers = resolvers.ToArray();

        internal async ValueTask Finalize(
            ConsumerBase consumer, ICommandResult result, CommandOptions options)
        {
            foreach (var resolver in _resolvers)
            {
                await resolver.Evaluate(consumer, result, options.Services, options.CancellationToken);
            }
        }
    }
}
