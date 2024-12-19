using Commands.Resolvers;

namespace Commands.Tests
{
    internal class CustomResultResolver : ResultResolver
    {
        protected override ValueTask ArgumentMismatch(ICallerContext consumer, MatchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.ArgumentMismatch(consumer, result, services, cancellationToken);
        }

        protected override ValueTask CommandNotFound(ICallerContext consumer, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.CommandNotFound(consumer, result, services, cancellationToken);
        }

        protected override ValueTask ConditionUnmet(ICallerContext consumer, ConditionResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.ConditionUnmet(consumer, result, services, cancellationToken);
        }

        protected override ValueTask ConversionFailed(ICallerContext consumer, MatchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.ConversionFailed(consumer, result, services, cancellationToken);
        }

        protected override ValueTask InvocationFailed(ICallerContext consumer, InvokeResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.InvocationFailed(consumer, result, services, cancellationToken);
        }

        protected override ValueTask SearchIncomplete(ICallerContext consumer, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.SearchIncomplete(consumer, result, services, cancellationToken);
        }

        protected override ValueTask UnhandledFailure(ICallerContext consumer, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.UnhandledFailure(consumer, result, services, cancellationToken);
        }
    }
}
