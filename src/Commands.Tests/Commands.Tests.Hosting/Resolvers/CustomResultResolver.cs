namespace Commands.Tests
{
    internal class CustomResultResolver : ResultHandler
    {
        protected override Task HandleArgumentMismatch(ICallerContext consumer, MatchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.HandleArgumentMismatch(consumer, result, services, cancellationToken);
        }

        protected override Task HandleCommandNotFound(ICallerContext consumer, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.HandleCommandNotFound(consumer, result, services, cancellationToken);
        }

        protected override Task HandleConditionUnmet(ICallerContext consumer, ConditionResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.HandleConditionUnmet(consumer, result, services, cancellationToken);
        }

        protected override Task HandleConversionFailed(ICallerContext consumer, MatchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.HandleConversionFailed(consumer, result, services, cancellationToken);
        }

        protected override Task HandleInvocationFailed(ICallerContext consumer, InvokeResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.HandleInvocationFailed(consumer, result, services, cancellationToken);
        }

        protected override Task HandleSearchIncomplete(ICallerContext consumer, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.HandleSearchIncomplete(consumer, result, services, cancellationToken);
        }

        protected override Task HandleUnknownResult(ICallerContext consumer, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.HandleUnknownResult(consumer, result, services, cancellationToken);
        }
    }
}
