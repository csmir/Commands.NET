using Commands.Core;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Resolvers
{
    public abstract class ResolverBase
    {
        public abstract ValueTask EvaluateAsync(ICommandContext context, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken);
    }
}
