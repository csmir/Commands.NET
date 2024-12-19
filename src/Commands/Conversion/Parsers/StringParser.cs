namespace Commands.Conversion
{
    internal sealed class StringParser : TypeParser<string>
    {
        public static StringParser Instance { get; } = new();

        public override async ValueTask<ConvertResult> Parse(CallerContext consumer, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
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
