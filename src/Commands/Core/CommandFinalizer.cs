using Commands.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Commands.Core
{
    /// <summary>
    ///     A container that is responsible for finalizing the command scope and notifying post-execution processes.
    /// </summary>
    /// <param name="resolvers">A collection of <see cref="ResolverBase"/>'s to run on post-execution.</param>
    public sealed class CommandFinalizer(IEnumerable<ResolverBase> resolvers)
    {
        private static readonly Lazy<CommandFinalizer> _i = new(() => new CommandFinalizer([]));

        private readonly ResolverBase[] _resolvers = resolvers.ToArray();

        internal async ValueTask FinalizeAsync(ConsumerBase consumer, ICommandResult result, AsyncServiceScope scope, RequestContext context)
        {
            context.Logger.LogDebug("Finalizing execution...");

            foreach (var resolver in _resolvers)
            {
                await resolver.EvaluateAsync(consumer, result, scope.ServiceProvider, context.CancellationToken);
            }

            context.Logger.LogDebug("Disposing resources...");

            await scope.DisposeAsync();
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
