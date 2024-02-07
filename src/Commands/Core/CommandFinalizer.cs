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

        private ResolverBase[] _resolvers;

        public CommandFinalizer(IEnumerable<ResolverBase> resolvers)
        {
            _resolvers = resolvers.ToArray();
        }

        /// <summary>
        ///     Attempts to execute the result handler of a command.
        /// </summary>
        /// <param name="context">Context of the current execution.</param>
        /// <param name="result">The result of the command, being successful or containing failure information.</param>
        /// <param name="scope">The provider used to register modules and inject services.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>An awaitable <see cref="Task"/> that waits for the delegate to finish.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal async ValueTask FinalizeAsync(ICommandContext context, ICommandResult result, AsyncServiceScope scope, CancellationToken cancellationToken)
        {
            foreach (var resolver in _resolvers)
            {
                await resolver.EvaluateAsync(context, result, scope.ServiceProvider, cancellationToken);
            }

            await scope.DisposeAsync();
        }

        internal void AddResolver(ResolverBase resolver)
        {
            if (resolver == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resolver);
            }

            resolver.AddTo(ref _resolvers);
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
