using Commands.Components;

namespace Commands.Conversion
{
    internal sealed class StringTypeConverter : TypeConverter<string>
    {
        public static StringTypeConverter Instance { get; } = new();

        public override async ValueTask<ConvertResult> Evaluate(CallerContext consumer, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            if (value is string str)
            {
                return Success(str);
            }

            return Success(value?.ToString()!);
        }
    }
}
