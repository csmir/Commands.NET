using Commands.Resolvers;
using Microsoft.Extensions.Logging;

namespace Commands.Core
{
    /// <summary>
    ///     A container that is responsible for finalizing the command scope and notifying post-execution processes.
    /// </summary>
    /// <param name="resolvers">A collection of <see cref="ResultResolverBase"/>'s to run on post-execution.</param>
    public sealed class CommandFinalizer(IEnumerable<ResultResolverBase> resolvers)
    {
        private static readonly Lazy<CommandFinalizer> _i = new(() => new CommandFinalizer([]));

        private readonly ResultResolverBase[] _resolvers = resolvers.ToArray();

        internal async ValueTask FinalizeAsync(
            ConsumerBase consumer, IRunResult result, CommandOptions options)
        {
            foreach (var resolver in _resolvers)
            {
                await resolver.EvaluateAsync(consumer, result, options.Scope!.ServiceProvider, options.CancellationToken);
            }
            options.Scope!.Dispose();
        }

        internal static CommandFinalizer Default
        {
            get
            {
                return _i.Value;
            }
        }
    }
}
