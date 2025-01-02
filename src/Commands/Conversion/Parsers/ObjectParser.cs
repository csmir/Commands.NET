namespace Commands.Conversion;

internal sealed class ObjectParser : TypeParser<object>
{
    public override ValueTask<ParseResult> Parse(ICallerContext caller, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        => Success(value);
}
