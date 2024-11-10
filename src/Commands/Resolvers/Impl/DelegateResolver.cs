namespace Commands.Resolvers
{
    internal sealed class DelegateResolver(
        Action<ConsumerBase, ICommandResult, IServiceProvider> action)
        : ResultResolverBase
    {
        private readonly Action<ConsumerBase, ICommandResult, IServiceProvider> _action = action;

        public override ValueTask Evaluate(
            ConsumerBase consumer, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            _action(consumer, result, services);

            return ValueTask.CompletedTask;
        }
    }
}
