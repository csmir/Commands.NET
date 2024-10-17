using Commands.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace Commands.TypeConverters
{
    internal sealed class AsyncDelegateConverter<T>(
        [DisallowNull] Func<ConsumerBase, IArgument, string?, IServiceProvider, ValueTask<ConvertResult>> func)
        : TypeConverterBase<T>
    {
        private readonly Func<ConsumerBase, IArgument, string?, IServiceProvider, ValueTask<ConvertResult>> _func = func;

        public override ValueTask<ConvertResult> Evaluate(ConsumerBase consumer, IArgument argument, string? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            return _func(consumer, argument, value, services);
        }
    }
}
