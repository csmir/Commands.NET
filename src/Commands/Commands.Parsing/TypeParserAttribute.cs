namespace Commands.Parsing;

/// <summary>
///     Provides a mechanism to mark command parameters with specified parsing logic, taking precedence over the default.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public abstract class TypeParserAttribute : Attribute, IParser
{
    /// <inheritdoc />
    public virtual Type Type { get; } = null!;

    /// <inheritdoc />
    public abstract ValueTask<ParseResult> Parse(IContext context, ICommandParameter parameter, object? argument, IServiceProvider services, CancellationToken cancellationToken);

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