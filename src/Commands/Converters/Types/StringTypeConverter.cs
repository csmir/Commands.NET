using Commands.Reflection;

namespace Commands.Converters
{
    internal sealed class StringTypeConverter : TypeConverterBase<string>
    {
        public static StringTypeConverter Instance { get; } = new();

        public override ValueTask<ConvertResult> Evaluate(ConsumerBase consumer, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (value is string str)
            {
                return ValueTask.FromResult(Success(str));
            }

            return ValueTask.FromResult(Success(value?.ToString()!));
        }
    }
}
