using System.Diagnostics.CodeAnalysis;

namespace Commands.Resolvers
{
    internal sealed class AsyncDelegateResolver(
        [DisallowNull] Func<ConsumerBase, ICommandResult, IServiceProvider, ValueTask> action)
        : ResultResolverBase
    {
        private readonly Func<ConsumerBase, ICommandResult, IServiceProvider, ValueTask> _action = action;

        public override async ValueTask EvaluateAsync(
            ConsumerBase consumer, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            await _action(consumer, result, services);
        }
    }
}
