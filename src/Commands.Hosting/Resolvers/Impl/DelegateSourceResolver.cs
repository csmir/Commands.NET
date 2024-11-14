using Commands.Results;

namespace Commands.Resolvers
{
    internal sealed class DelegateSourceResolver(
        Func<SourceResult> func) : SourceResolverBase
    {
        private readonly Func<SourceResult> _func = func;

        public override async ValueTask<SourceResult> Evaluate(CancellationToken cancellationToken)
        {
            await ValueTask.CompletedTask;

            return _func();
        }
    }
}
