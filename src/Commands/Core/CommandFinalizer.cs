using Commands.Helpers;
using Commands.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Commands.Core
{
    /// <summary>
    ///     A container that implements an asynchronous functor to handle post-execution operations.
    /// </summary>
    public sealed class CommandFinalizer
    {
        private static readonly Lazy<CommandFinalizer> _i = new(() => new CommandFinalizer([]));

        private readonly ResolverBase[] _resolvers;

        public CommandFinalizer(IEnumerable<ResolverBase> resolvers)
        {
            _resolvers = resolvers.ToArray();
        }

        internal async ValueTask FinalizeAsync(ConsumerBase consumer, ICommandResult result, AsyncServiceScope scope, RequestContext context)
        {
            foreach (var resolver in _resolvers)
            {
                await resolver.EvaluateAsync(consumer, result, scope.ServiceProvider, context.CancellationToken);
            }

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
