﻿using Commands.Results;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Resolvers
{
    internal sealed class DelegateResolver(
        [DisallowNull] Func<SourceResult> func) : SourceResolverBase
    {
        private readonly Func<SourceResult> _func = func;

        public override async ValueTask<SourceResult> Evaluate(CancellationToken cancellationToken)
        {
            await ValueTask.CompletedTask;

            return _func();
        }
    }
}
