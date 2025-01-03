namespace Commands.Conversion;

/// <inheritdoc />
/// <typeparam name="T">The type this <see cref="TypeParser{T}"/> should parse into.</typeparam>
public abstract class TypeParser<T> : TypeParser
{
    /// <summary>
    ///     Gets the type that should be parsed to.
    /// </summary>
    public override Type Type { get; } = typeof(T);

    /// <summary>
    ///     Creates a new <see cref="ParseResult"/> representing a successful parse operation.
    /// </summary>
    /// <param name="value">The value parsed from a raw argument into the target type of this parser.</param>
    /// <returns>A <see cref="ParseResult"/> representing the successful parse operation.</returns>
    public virtual ParseResult Success(T? value)
        => base.Success(value);
}

/// <summary>
///     An abstract type that can be implemented to create custom type parsing from a command query argument.
/// </summary>
public abstract class TypeParser
{
    /// <summary>
    ///     Gets the type that should be parsed to. This value determines what command arguments will use this parser.
    /// </summary>
    /// <remarks>
    ///     It is important to ensure this parser actually returns the specified type in <see cref="Success(object)"/>. If this is not the case, a critical exception will occur in runtime when the command is attempted to be executed.
    /// </remarks>
    public abstract Type Type { get; }

    /// <summary>
    ///     Evaluates the known data about the argument to be parsed into, as well as the raw value it should parse into a valid invocation parameter.
    /// </summary>
    /// <param name="caller">Context of the current execution.</param>
    /// <param name="services">The provider used to register modules and inject services.</param>
    /// <param name="argument">Information about the invocation argument this evaluation converts for.</param>
    /// <param name="value">The raw command query argument to parse.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
    public abstract ValueTask<ParseResult> Parse(
        ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a new <see cref="ParseResult"/> representing a failed evaluation.
    /// </summary>
    /// <param name="exception">The exception that caused the evaluation to fail.</param>
    /// <returns>A <see cref="ParseResult"/> representing the failed evaluation.</returns>
    protected ParseResult Error(Exception exception)
    {
        Assert.NotNull(exception, nameof(exception));

        if (exception is ParseException convertEx)
            return ParseResult.FromError(convertEx);

        return ParseResult.FromError(ParseException.ConvertFailed(Type, exception));
    }

    /// <summary>
    ///     Creates a new <see cref="ParseResult"/> representing a failed evaluation.
    /// </summary>
    /// <param name="error">The error that caused the evaluation to fail.</param>
    /// <returns>A <see cref="ParseResult"/> representing the failed evaluation.</returns>
    protected ParseResult Error(string error)
    {
        Assert.NotNullOrEmpty(error, nameof(error));

        return ParseResult.FromError(new ParseException(error));
    }

    /// <summary>
    ///     Creates a new <see cref="ParseResult"/> representing a successful evaluation.
    /// </summary>
    /// <param name="value">The value parsed from a raw argument into the target type of this parser.</param>
    /// <returns>A <see cref="ParseResult"/> representing the successful evaluation.</returns>
    protected ParseResult Success(object? value)
        => ParseResult.FromSuccess(value);

    /// <summary>
    ///     Creates a collection of default <see cref="TypeParser"/> implementations.
    /// </summary>
    /// <remarks>
    ///     This collection returns parsers for:
    ///     <list type="bullet">
    ///         <item>All BCL types (<see href="https://learn.microsoft.com/en-us/dotnet/standard/class-library-overview#system-namespace"/>).</item>
    ///         <item><see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="TimeSpan"/> and <see cref="Guid"/>.</item>
    ///         <item><see cref="Enum"/> implementations for which no custom parser exists.</item>
    ///     </list>
    ///     <i>Collections implementing <see cref="Array"/> are converted by their respective element types, and not the types themselves.</i>
    /// </remarks>
    /// <returns>An <see cref="IEnumerable{T}"/> containing a range of <see cref="TypeParser"/>'s for all types listed above.</returns>
    public static IEnumerable<TypeParser> CreateDefaults()
    {
        var list = TryParseParser.CreateBaseConverters();

        list.Add(new TimeSpanParser());
        list.Add(new ObjectParser());
        list.Add(new StringParser());

        return list;
    }
}
