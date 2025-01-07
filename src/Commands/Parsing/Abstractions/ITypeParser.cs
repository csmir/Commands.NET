namespace Commands.Parsing;

/// <summary>
///     Represents a parser that can convert an object to a specific type.
/// </summary>
public interface ITypeParser
{
    /// <summary>
    ///     Gets the type that should be parsed to. This value determines what command arguments will use this parser.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    ///     Evaluates the known data about the argument to be parsed into, as well as the raw value it should parse into a valid invocation parameter.
    /// </summary>
    /// <param name="caller">Context of the current execution.</param>
    /// <param name="services">The provider used to register modules and inject services.</param>
    /// <param name="argument">Information about the invocation argument this evaluation converts for.</param>
    /// <param name="value">The raw command query argument to parse.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
    public ValueTask<ParseResult> Parse(
        ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a new <see cref="ParseResult"/> representing a failed evaluation.
    /// </summary>
    /// <param name="exception">The exception that caused the evaluation to fail.</param>
    /// <returns>A <see cref="ParseResult"/> representing the failed evaluation.</returns>
    public ParseResult Error(Exception exception);

    /// <summary>
    ///     Creates a new <see cref="ParseResult"/> representing a failed evaluation.
    /// </summary>
    /// <param name="error">The error that caused the evaluation to fail.</param>
    /// <returns>A <see cref="ParseResult"/> representing the failed evaluation.</returns>
    public ParseResult Error(string error);

    /// <summary>
    ///     Creates a new <see cref="ParseResult"/> representing a successful evaluation.
    /// </summary>
    /// <param name="value">The value parsed from a raw argument into the target type of this parser.</param>
    /// <returns>A <see cref="ParseResult"/> representing the successful evaluation.</returns>
    public ParseResult Success(object? value);
}
