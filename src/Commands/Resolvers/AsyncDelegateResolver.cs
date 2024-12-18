using System.ComponentModel;

namespace Commands.Resolvers
{
    /// <summary>
    ///     Represents a resolver that invokes a delegate when a result is encountered. This class cannot be inherited.
    /// </summary>
    /// <param name="action">The action to be invoked when receiving a result.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class AsyncDelegateResolver(
        Func<CallerContext, IExecuteResult, IServiceProvider, ValueTask> action)
        : ResultResolver
    {
        private readonly Func<CallerContext, IExecuteResult, IServiceProvider, ValueTask> _action = action;

        /// <inheritdoc />
        public override async ValueTask EvaluateResult(
            CallerContext consumer, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            await _action(consumer, result, services);
        }
    }
}
