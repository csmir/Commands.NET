using Commands.Core;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Resolvers
{
    internal sealed class DelegateResolver(
        [DisallowNull] Action<ConsumerBase, ICommandResult, IServiceProvider> action)
        : ResolverBase
    {
        private readonly Action<ConsumerBase, ICommandResult, IServiceProvider> _action = action;

        public override ValueTask EvaluateAsync(
            ConsumerBase consumer, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            _action(consumer, result, services);

            return ValueTask.CompletedTask;
        }
    }
}
