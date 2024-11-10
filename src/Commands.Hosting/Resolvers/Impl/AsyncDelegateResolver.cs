using Commands.Results;

namespace Commands.Resolvers
{
    internal sealed class AsyncDelegateResolver(
        Func<ValueTask<SourceResult>> func) : SourceResolverBase
    {
        private readonly Func<ValueTask<SourceResult>> _func = func;

        public override ValueTask<SourceResult> Evaluate(CancellationToken cancellationToken)
        {
            return _func();
        }
    }
}
