using Microsoft.Extensions.Logging;

namespace Commands.Resolvers
{
    internal sealed class DefaultResultResolver(ILogger<DefaultResultResolver> logger) : ResultResolver
    {
        protected override ValueTask ArgumentMismatch(CallerContext consumer, MatchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            logger.Log(LogLevel.Warning, result.ToString(true));

            return base.ArgumentMismatch(consumer, result, services, cancellationToken);
        }

        protected override ValueTask CommandNotFound(CallerContext consumer, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            logger.Log(LogLevel.Warning, result.ToString(true));

            return base.CommandNotFound(consumer, result, services, cancellationToken);
        }

        protected override ValueTask ConditionUnmet(CallerContext consumer, ConditionResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            logger.Log(LogLevel.Error, result.ToString(true));

            return base.ConditionUnmet(consumer, result, services, cancellationToken);
        }

        protected override ValueTask ConversionFailed(CallerContext consumer, MatchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            logger.Log(LogLevel.Warning, result.ToString(true));

            return base.ConversionFailed(consumer, result, services, cancellationToken);
        }

        protected override ValueTask InvocationFailed(CallerContext consumer, InvokeResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            logger.Log(LogLevel.Error, result.ToString(true));

            return base.InvocationFailed(consumer, result, services, cancellationToken);
        }

        protected override ValueTask SearchIncomplete(CallerContext consumer, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            logger.Log(LogLevel.Warning, result.ToString(true));

            return base.SearchIncomplete(consumer, result, services, cancellationToken);
        }

        protected override ValueTask UnhandledFailure(CallerContext consumer, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            logger.Log(LogLevel.Critical, result.ToString());

            return base.UnhandledFailure(consumer, result, services, cancellationToken);
        }
    }
}
