namespace Commands.Parsing;

/// <inheritdoc />
/// <typeparam name="TConvertible">The type this <see cref="TypeParser{T}"/> should parse into.</typeparam>
public abstract class TypeParser<TConvertible> : TypeParser
{
    /// <inheritdoc />
    public override Type TargetType { get; } = typeof(TConvertible);

    /// <summary>
    ///     Creates a new <see cref="ParseResult"/> representing a successful parse operation.
    /// </summary>
    /// <param name="value">The value parsed from a raw argument into the target type of this parser.</param>
    /// <returns>A <see cref="ParseResult"/> representing the successful parse operation.</returns>
    public virtual ParseResult Success(TConvertible? value)
        => base.Success(value);
}

/// <summary>
///     An abstract type that can be implemented to create custom type parsing from a command query argument.
/// </summary>
public abstract class TypeParser : IParser
{
    /// <inheritdoc />
    public abstract Type TargetType { get; }

    /// <inheritdoc />
    public abstract ValueTask<ParseResult> Parse(
        IContext context, ICommandParameter parameter, object? argument, IServiceProvider services, CancellationToken cancellationToken);

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="error"/> is <see langword="null"/> or empty.</exception>
    public ParseResult Error(string error)
    {
        if (string.IsNullOrEmpty(error))
            throw new ArgumentNullException(nameof(error));

        return ParseResult.FromError(new ParserException(error));
    }

    /// <inheritdoc />
    public ParseResult Success(object? value)
        => ParseResult.FromSuccess(value);

    #region Internals

    internal static IEnumerable<TypeParser> CreateDefaults()
    {
        var list = TryParseParser.GetCommon();

        list.Add(new TimeSpanParser());
        list.Add(new ColorParser());
        list.Add(new ObjectParser());
        list.Add(new StringParser());

        return list;
    }

    #endregion
}
