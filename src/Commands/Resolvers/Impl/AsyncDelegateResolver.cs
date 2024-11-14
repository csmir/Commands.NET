using System.ComponentModel;

namespace Commands.Resolvers
{
    /// <summary>
    ///     Represents a resolver that invokes a delegate when a result is encountered. This class cannot be inherited.
    /// </summary>
    /// <param name="action">The action to be invoked when receiving a result.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class AsyncDelegateResolver(
        Func<ConsumerBase, ICommandResult, IServiceProvider, ValueTask> action)
        : ResultResolverBase
    {
        private readonly Func<ConsumerBase, ICommandResult, IServiceProvider, ValueTask> _action = action;

        /// <inheritdoc />
        public override async ValueTask Evaluate(
            ConsumerBase consumer, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            await _action(consumer, result, services);
        }
    }
}
