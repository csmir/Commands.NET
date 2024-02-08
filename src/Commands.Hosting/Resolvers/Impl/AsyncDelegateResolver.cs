using Commands.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Resolvers
{
    internal sealed class AsyncDelegateResolver(
        [DisallowNull] Func<ValueTask<SourceResult>> func) : SourceResolverBase
    {
        private readonly Func<ValueTask<SourceResult>> _func = func;

        public override ValueTask<SourceResult> EvaluateAsync()
        {
            return _func();
        }
    }
}
