using Commands.Reflection;

namespace Commands.Converters
{
    // This converter is used exclusively for enumerable conversion.
    internal sealed class ObjectTypeConverter : TypeConverter<object>
    {
        public static ObjectTypeConverter Instance { get; } = new();

        public override async ValueTask<ConvertResult> Evaluate(CallerContext consumer, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            return Success(value!);
        }
    }
}
