using Commands.Core;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Resolvers
{
    internal sealed class AsyncDelegateResolver(
        [DisallowNull] Func<ConsumerBase, IRunResult, IServiceProvider, ValueTask> action)
        : ResultResolverBase
    {
        private readonly Func<ConsumerBase, IRunResult, IServiceProvider, ValueTask> _action = action;

        public override async ValueTask EvaluateAsync(
            ConsumerBase consumer, IRunResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            await _action(consumer, result, services);
        }
    }
}
