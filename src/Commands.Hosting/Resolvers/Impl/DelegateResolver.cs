using Commands.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
