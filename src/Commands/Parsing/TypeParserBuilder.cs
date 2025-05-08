namespace Commands.Parsing;

/// <summary>
///     A set of properties for a type parser.
/// </summary>
/// <typeparam name="T">The target type this parser should return on invocation.</typeparam>
public sealed class TypeParserBuilder<T> : ITypeParserBuilder
{
    private Func<ICallerContext, ICommandParameter, object?, IServiceProvider, ValueTask<ParseResult>>? _delegate;
    private TryParseParser<T>.ParseDelegate? _tryParseDelegate;

    /// <summary>
    ///     Creates a new instance of <see cref="TypeParserBuilder{T}"/>.
    /// </summary>
    public TypeParserBuilder()
    {
        _delegate = null;
    }

    /// <summary>
    ///     Sets the delegate that will be executed when the type parser is invoked.
    /// </summary>
    /// <param name="executionDelegate">The delegate to set.</param>
    /// <returns>The same <see cref="TypeParserBuilder{T}"/> for call-chaining.</returns>
    public TypeParserBuilder<T> AddDelegate(Func<ICallerContext, ICommandParameter, object?, IServiceProvider, ValueTask<ParseResult>> executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _delegate = executionDelegate;

        return this;
    }

    /// <summary>
    ///     Sets the delegate that will be executed when the type parser is invoked.
    /// </summary>
    /// <remarks>
    ///     This delegate grants access to TryParse methods for the target type as delegate implementations. For example, <see cref="int.TryParse(string, out int)"/>.
    /// </remarks>
    /// <param name="executionDelegate">The delegate to set.</param>
    /// <returns>The same <see cref="TypeParserBuilder{T}"/> for call-chaining.</returns>
    public TypeParserBuilder<T> AddDelegate(TryParseParser<T>.ParseDelegate executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _tryParseDelegate = executionDelegate;

        return this;
    }

    /// <inheritdoc />
    public TypeParser Build()
    {
        if (_tryParseDelegate is not null)
            return new TryParseParser<T>(_tryParseDelegate!);

        Assert.NotNull(_delegate, nameof(_delegate));

        return new DelegateTypeParser<T>(_delegate!);
    }

    Type ITypeParserBuilder.GetParserType()
        => typeof(T);
}

internal readonly struct TypeParserBuilder : ITypeParserBuilder
{
    private readonly TypeParser _parser;

    internal TypeParserBuilder(TypeParser parser)
    {
        _parser = parser;
    }

    public TypeParser Build()
        => _parser;

    Type ITypeParserBuilder.GetParserType()
        => _parser.Type;
}