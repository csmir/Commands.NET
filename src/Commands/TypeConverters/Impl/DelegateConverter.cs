using Commands.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace Commands.TypeConverters
{
    internal sealed class DelegateConverter<T>(
        [DisallowNull] Func<ConsumerBase, IArgument, string?, IServiceProvider, ConvertResult> func)
        : TypeConverterBase<T>
    {
        private readonly Func<ConsumerBase, IArgument, string?, IServiceProvider, ConvertResult> _func = func;

        public override async ValueTask<ConvertResult> Evaluate(ConsumerBase consumer, IArgument argument, string? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            return _func(consumer, argument, value, services);
        }
    }
}
