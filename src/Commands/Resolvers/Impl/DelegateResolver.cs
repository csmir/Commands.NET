using Commands.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Resolvers
{
    internal sealed class DelegateResolver(
        [DisallowNull] Action<ConsumerBase, ICommandResult, IServiceProvider> action)
        : ResolverBase
    {
        private readonly Action<ConsumerBase, ICommandResult, IServiceProvider> _action = action;

        public override ValueTask EvaluateAsync(ConsumerBase consumer, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            _action(consumer, result, services);

            return ValueTask.CompletedTask;
        }
    }
}
