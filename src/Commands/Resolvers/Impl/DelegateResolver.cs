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
        [DisallowNull] Action<ICommandContext, ICommandResult, IServiceProvider> action)
        : ResolverBase
    {
        private readonly Action<ICommandContext, ICommandResult, IServiceProvider> _action = action;

        public override ValueTask EvaluateAsync(ICommandContext context, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            _action(context, result, services);

            return ValueTask.CompletedTask;
        }
    }
}
