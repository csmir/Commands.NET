using System.Diagnostics.CodeAnalysis;

namespace Commands.Core
{
    public abstract class ResolverBase
    {
        public abstract ValueTask EvaluateAsync(ICommandContext context, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken);

        internal sealed class AsyncDelegateResolver(
            [DisallowNull] Func<ICommandContext, ICommandResult, IServiceProvider, ValueTask> action)
            : ResolverBase
        {
            private readonly Func<ICommandContext, ICommandResult, IServiceProvider, ValueTask> _action = action;

            public override async ValueTask EvaluateAsync(ICommandContext context, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
            {
                await _action(context, result, services);
            }
        }

        internal sealed class DelegateResolver(
            [DisallowNull] Action<ICommandContext, ICommandResult, IServiceProvider> action)
            : ResolverBase
        {
            private readonly Action<ICommandContext, ICommandResult, IServiceProvider> _action = action;

            public override ValueTask EvaluateAsync(ICommandContext context, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
            {
                _action(context, result, services);

                return ValueTask.CompletedTask;
            }
        }
    }
}
