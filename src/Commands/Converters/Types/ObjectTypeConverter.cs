using Commands.Reflection;

namespace Commands.Converters
{
    // This converter is used exclusively for enumerable conversion.
    internal sealed class ObjectTypeConverter : TypeConverterBase<object>
    {
        public static ObjectTypeConverter Instance { get; } = new();

        public override async ValueTask<ConvertResult> Evaluate(ConsumerBase consumer, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            return Success(value!);
        }
    }
}
