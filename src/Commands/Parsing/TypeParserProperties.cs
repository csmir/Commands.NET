namespace Commands.Parsing;

/// <summary>
///     A set of properties for a type parser.
/// </summary>
/// <typeparam name="T">The target type this parser should return on invocation.</typeparam>
public sealed class TypeParserProperties<T> : ITypeParserProperties
{
    private Func<ICallerContext, ICommandParameter, object?, IServiceProvider, ValueTask<ParseResult>>? _delegate;
    private TryParseParser<T>.ParseDelegate? _tryParseDelegate;

    /// <summary>
    ///     Creates a new instance of <see cref="TypeParserProperties{T}"/>.
    /// </summary>
    public TypeParserProperties()
    {
        _delegate = null;
    }

    /// <summary>
    ///     Sets the delegate that will be executed when the type parser is invoked.
    /// </summary>
    /// <param name="executionDelegate">The delegate to set.</param>
    /// <returns>The same <see cref="TypeParserProperties{T}"/> for call-chaining.</returns>
    public TypeParserProperties<T> AddDelegate(Func<ICallerContext, ICommandParameter, object?, IServiceProvider, ValueTask<ParseResult>> executionDelegate)
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
    /// <returns>The same <see cref="TypeParserProperties{T}"/> for call-chaining.</returns>
    public TypeParserProperties<T> AddDelegate(TryParseParser<T>.ParseDelegate executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _tryParseDelegate = executionDelegate;

        return this;
    }

    /// <inheritdoc />
    public TypeParser Create()
    {
        if (_tryParseDelegate is not null)
            return new TryParseParser<T>(_tryParseDelegate!);

        Assert.NotNull(_delegate, nameof(_delegate));

        return new DelegateTypeParser<T>(_delegate!);
    }

    Type ITypeParserProperties.GetParserType()
        => typeof(T);
}

internal readonly struct TypeParserProperties : ITypeParserProperties
{
    private readonly TypeParser _parser;

    internal TypeParserProperties(TypeParser parser)
    {
        _parser = parser;
    }

    public TypeParser Create()
        => _parser;

    Type ITypeParserProperties.GetParserType()
        => _parser.Type;
}