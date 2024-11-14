using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Commands.Resolvers
{
    internal sealed class DefaultResultResolver : ResultResolverBase
    {
#pragma warning disable CA2254 // Template should be a static expression
        protected override ValueTask ArgumentMismatch(ConsumerBase consumer, MatchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            var logger = services.GetRequiredService<ILogger<DefaultResultResolver>>();

            logger.Log(LogLevel.Warning, result.ToString(true));

            return base.ArgumentMismatch(consumer, result, services, cancellationToken);
        }

        protected override ValueTask CommandNotFound(ConsumerBase consumer, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            var logger = services.GetRequiredService<ILogger<DefaultResultResolver>>();

            logger.Log(LogLevel.Warning, result.ToString(true));

            return base.CommandNotFound(consumer, result, services, cancellationToken);
        }

        protected override ValueTask ConditionUnmet(ConsumerBase consumer, ConditionResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            var logger = services.GetRequiredService<ILogger<DefaultResultResolver>>();

            logger.Log(LogLevel.Error, result.ToString(true));

            return base.ConditionUnmet(consumer, result, services, cancellationToken);
        }

        protected override ValueTask ConversionFailed(ConsumerBase consumer, MatchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            var logger = services.GetRequiredService<ILogger<DefaultResultResolver>>();

            logger.Log(LogLevel.Warning, result.ToString(true));

            return base.ConversionFailed(consumer, result, services, cancellationToken);
        }

        protected override ValueTask InvocationFailed(ConsumerBase consumer, InvokeResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            var logger = services.GetRequiredService<ILogger<DefaultResultResolver>>();

            logger.Log(LogLevel.Error, result.ToString(true));

            return base.InvocationFailed(consumer, result, services, cancellationToken);
        }

        protected override ValueTask SearchIncomplete(ConsumerBase consumer, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            var logger = services.GetRequiredService<ILogger<DefaultResultResolver>>();

            logger.Log(LogLevel.Warning, result.ToString(true));

            return base.SearchIncomplete(consumer, result, services, cancellationToken);
        }

        protected override ValueTask UnhandledFailure(ConsumerBase consumer, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            var logger = services.GetRequiredService<ILogger<DefaultResultResolver>>();

            logger.Log(LogLevel.Critical, result.ToString());

            return base.UnhandledFailure(consumer, result, services, cancellationToken);
        }
#pragma warning restore CA2254 // Template should be a static expression
    }
}
