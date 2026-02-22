using Commands.Parsing;

namespace Commands;

/// <summary>
///     Reveals information about an invocation parameter of a command or any complex member.
/// </summary>
public interface ICommandParameter : ICommandSegment, IParameter
{
    /// <summary>
    ///     Gets if this parameter is the query remainder or not.
    /// </summary>
    public bool IsRemainder { get; }

    /// <summary>
    ///     Gets if this parameter is a collection type or not.
    /// </summary>
    public bool IsCollection { get; }

    /// <summary>
    ///     Gets if this parameter is a resource type or not.
    /// </summary>
    public bool IsResource { get; }

    /// <summary>
    ///     Gets the parser for this parameter.
    /// </summary>
    /// <remarks>
    ///     Will be <see langword="null"/> if this parameter is <see cref="ConstructibleParameter"/>.
    /// </remarks>
    public IParser? Parser { get; }

    /// <summary>
    ///     Attempts to convert the provided value to the target type of this parameter.
    /// </summary>
    /// <param name="context">The context of the current execution.</param>
    /// <param name="value">The value which the <see cref="Parser"/> should parse.</param>
    /// <param name="services">The provider used to register modules and inject services.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask{ConvertResult}"/> holding the result of the convert operation.</returns>
    public ValueTask<ParseResult> Parse(IContext context, object? value, IServiceProvider services, CancellationToken cancellationToken);
}
