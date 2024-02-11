using Commands.Core;
using Commands.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Tests
{
    internal class CustomResultResolver : ResultResolverBase
    {
        protected override ValueTask ArgumentMismatch(ConsumerBase consumer, MatchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.ArgumentMismatch(consumer, result, services, cancellationToken);
        }

        protected override ValueTask CommandNotFound(ConsumerBase consumer, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.CommandNotFound(consumer, result, services, cancellationToken);
        }

        protected override ValueTask ConditionUnmet(ConsumerBase consumer, ConditionResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.ConditionUnmet(consumer, result, services, cancellationToken);
        }

        protected override ValueTask ConversionFailed(ConsumerBase consumer, MatchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.ConversionFailed(consumer, result, services, cancellationToken);
        }

        protected override ValueTask InvocationFailed(ConsumerBase consumer, InvokeResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.InvocationFailed(consumer, result, services, cancellationToken);
        }

        protected override ValueTask SearchIncomplete(ConsumerBase consumer, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.SearchIncomplete(consumer, result, services, cancellationToken);
        }

        protected override ValueTask UnhandledFailure(ConsumerBase consumer, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            return base.UnhandledFailure(consumer, result, services, cancellationToken);
        }
    }
}
