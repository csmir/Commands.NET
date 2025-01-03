namespace Commands.Conversion;

/// <summary>
///     Represents a converter that invokes a delegate when parameter conversion of its type <typeparamref name="T"/> occurs. This class cannot be inherited.
/// </summary>
/// <typeparam name="T">The convertible type that this converter should convert to.</typeparam>
/// <param name="func">The delegate that is invoked when the conversion is requested.</param>
public sealed class DelegateParser<T>(
    Func<ICallerContext, ICommandParameter, object?, IServiceProvider, ValueTask<ParseResult>> func)
    : TypeParser<T>
{
    /// <inheritdoc />
    public override ValueTask<ParseResult> Parse(ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        => func(caller, argument, value, services);
}
