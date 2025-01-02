namespace Commands.Conversion;

internal sealed class StringParser : TypeParser<string>
{
    public override ValueTask<ParseResult> Parse(ICallerContext caller, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (value is string str)
            return Success(str);

        return Success(value?.ToString());
    }
}
