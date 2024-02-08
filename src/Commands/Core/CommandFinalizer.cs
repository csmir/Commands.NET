using Commands.Exceptions;
using Commands.Resolvers;
using Microsoft.Extensions.DependencyInjection;
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
            options.Logger.LogDebug("Finalizing execution...");

            if (result == null && !options.TryGetResult(out result))
            {
                result = new SearchResult(new SearchException("No commands were found with the provided input."));
            }

            foreach (var resolver in _resolvers)
            {
                await resolver.EvaluateAsync(consumer, result, options.Scope.ServiceProvider, options.CancellationToken);
            }

            options.Logger.LogDebug("Disposing resources...");

            await options.DisposeAsync();
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
