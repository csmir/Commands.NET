using Commands.Core;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Resolvers
{
    internal sealed class DelegateResolver(
        [DisallowNull] Action<ConsumerBase, IRunResult, IServiceProvider> action)
        : ResultResolverBase
    {
        private readonly Action<ConsumerBase, IRunResult, IServiceProvider> _action = action;

        public override ValueTask EvaluateAsync(
            ConsumerBase consumer, IRunResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            _action(consumer, result, services);

            return ValueTask.CompletedTask;
        }
    }
}
