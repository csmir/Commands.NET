using Commands.Results;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Resolvers
{
    internal sealed class DelegateResolver(
        [DisallowNull] Func<SourceResult> func) : SourceResolverBase
    {
        private readonly Func<SourceResult> _func = func;

        public override async ValueTask<SourceResult> EvaluateAsync()
        {
            await ValueTask.CompletedTask;

            return _func();
        }
    }
}
