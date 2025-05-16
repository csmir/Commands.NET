namespace Commands.Parsing;

/// <summary>
///     Provides a mechanism to mark command parameters with specified parsing logic, taking precedence over the default.
/// </summary>
/// <typeparam name="TConvertible">The type to parse into using this parser.</typeparam>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public abstract class TypeParserAttribute<TConvertible> : Attribute, IParser
{
    /// <inheritdoc />
    public Type Type { get; } = typeof(TConvertible);

    /// <inheritdoc />
    public abstract ValueTask<ParseResult> Parse(IContext context, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken);

    /// <inheritdoc />
    public ParseResult Error(string error)
    {
        Assert.NotNullOrEmpty(error, nameof(error));

        return ParseResult.FromError(new ParserException(this, error));
    }

    /// <inheritdoc />
    public ParseResult Success(object? value)
        => ParseResult.FromSuccess(value);
}