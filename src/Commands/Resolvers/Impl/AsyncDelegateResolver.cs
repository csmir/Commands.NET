using Commands.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Resolvers
{
    internal sealed class AsyncDelegateResolver(
        [DisallowNull] Func<ICommandContext, ICommandResult, IServiceProvider, ValueTask> action)
        : ResolverBase
    {
        private readonly Func<ICommandContext, ICommandResult, IServiceProvider, ValueTask> _action = action;

        public override async ValueTask EvaluateAsync(ICommandContext context, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            await _action(context, result, services);
        }
    }
}
