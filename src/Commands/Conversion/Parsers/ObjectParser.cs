namespace Commands.Conversion
{
    // This converter is used exclusively for enumerable conversion.
    internal sealed class ObjectParser : TypeParser<object>
    {
        public static ObjectParser Instance { get; } = new();

        public override async ValueTask<ConvertResult> Parse(ICallerContext caller, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            return Success(value);
        }
    }
}
