namespace Commands.Parsing;

internal sealed class ObjectParser : TypeParser<object>
{
    public override ValueTask<ParseResult> Parse(IContext context, ICommandParameter parameter, object? argument, IServiceProvider services, CancellationToken cancellationToken)
        => Success(argument);
}
