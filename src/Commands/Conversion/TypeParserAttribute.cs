namespace Commands;

/// <summary>
///     Provides a mechanism to mark command parameters with specified parsing logic, taking precedence over the default.
/// </summary>
/// <typeparam name="T">The type to parse into using this parser.</typeparam>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public abstract class TypeParserAttribute<T> : Attribute, ITypeParser
{
    /// <inheritdoc />
    public Type Type { get; } = typeof(T);

    /// <inheritdoc />
    public abstract ValueTask<ParseResult> Parse(
        ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken);

    /// <inheritdoc />
    public ParseResult Error(Exception exception)
    {
        Assert.NotNull(exception, nameof(exception));

        if (exception is ParseException convertEx)
            return ParseResult.FromError(convertEx);

        return ParseResult.FromError(ParseException.ParseFailed(Type, exception));
    }

    /// <inheritdoc />
    public ParseResult Error(string error)
    {
        Assert.NotNullOrEmpty(error, nameof(error));

        return ParseResult.FromError(new ParseException(error));
    }

    /// <inheritdoc />
    public ParseResult Success(object? value)
        => ParseResult.FromSuccess(value);
}