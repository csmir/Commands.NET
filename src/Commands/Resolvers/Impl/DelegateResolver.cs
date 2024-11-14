using System.ComponentModel;

namespace Commands.Resolvers
{
    /// <summary>
    ///     Represents a resolver that invokes a delegate when a result is encountered. This class cannot be inherited.
    /// </summary>
    /// <param name="action">The action to be invoked when receiving a result.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DelegateResolver(
        Action<ConsumerBase, ICommandResult, IServiceProvider> action)
        : ResultResolverBase
    {
        private readonly Action<ConsumerBase, ICommandResult, IServiceProvider> _action = action;

        /// <inheritdoc />
        public override ValueTask Evaluate(
            ConsumerBase consumer, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            _action(consumer, result, services);

            return ValueTask.CompletedTask;
        }
    }
}
