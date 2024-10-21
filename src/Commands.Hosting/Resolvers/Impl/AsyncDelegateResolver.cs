using Commands.Results;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Resolvers
{
    internal sealed class AsyncDelegateResolver(
        [DisallowNull] Func<ValueTask<SourceResult>> func) : SourceResolverBase
    {
        private readonly Func<ValueTask<SourceResult>> _func = func;

        public override ValueTask<SourceResult> Evaluate(CancellationToken cancellationToken)
        {
            return _func();
        }
    }
}
