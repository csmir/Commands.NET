using Commands.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.TypeConverters
{
    internal sealed class AsyncDelegateConverter<T>(
        [DisallowNull] Func<ConsumerBase, IArgument, string?, IServiceProvider, ValueTask<ConvertResult>> func)
        : TypeConverterBase<T>
    {
        private readonly Func<ConsumerBase, IArgument, string?, IServiceProvider, ValueTask<ConvertResult>> _func = func;

        public override ValueTask<ConvertResult> EvaluateAsync(ConsumerBase consumer, IArgument argument, string? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            return _func(consumer, argument, value, services);
        }
    }
}
