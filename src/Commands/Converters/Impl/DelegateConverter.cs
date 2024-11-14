using Commands.Reflection;
using System.ComponentModel;

namespace Commands.Converters
{
    /// <summary>
    ///     Represents a converter that invokes a delegate when parameter conversion of its type <typeparamref name="T"/> occurs. This class cannot be inherited.
    /// </summary>
    /// <typeparam name="T">The convertible type that this converter should convert to.</typeparam>
    /// <param name="func">The delegate that is invoked when the conversion is requested.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DelegateConverter<T>(
        Func<ConsumerBase, IArgument, string?, IServiceProvider, ConvertResult> func)
        : TypeConverterBase<T>
    {
        private readonly Func<ConsumerBase, IArgument, string?, IServiceProvider, ConvertResult> _func = func;

        /// <inheritdoc />
        public override async ValueTask<ConvertResult> Evaluate(ConsumerBase consumer, IArgument argument, string? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            return _func(consumer, argument, value, services);
        }
    }
}
