namespace Commands.Parsing;

internal sealed class StringParser : TypeParser<string>
{
    public override ValueTask<ParseResult> Parse(IContext context, ICommandParameter parameter, object? argument, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (argument is string str)
            return Success(str);

        return Success(argument?.ToString());
    }
}
