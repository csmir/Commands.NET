using Commands.Components;
using System.ComponentModel;

namespace Commands.Conversion
{
    /// <summary>
    ///     Represents a converter that invokes a delegate when parameter conversion of its type <typeparamref name="T"/> occurs. This class cannot be inherited.
    /// </summary>
    /// <typeparam name="T">The convertible type that this converter should convert to.</typeparam>
    /// <param name="func">The delegate that is invoked when the conversion is requested.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class AsyncDelegateConverter<T>(
        Func<CallerContext, IArgument, object?, IServiceProvider, ValueTask<ConvertResult>> func)
        : TypeConverter<T>
    {
        private readonly Func<CallerContext, IArgument, object?, IServiceProvider, ValueTask<ConvertResult>> _func = func;

        /// <inheritdoc />
        public override ValueTask<ConvertResult> Evaluate(CallerContext consumer, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            return _func(consumer, argument, value, services);
        }
    }
}
