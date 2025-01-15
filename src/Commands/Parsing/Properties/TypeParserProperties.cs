namespace Commands.Parsing;

public sealed class TypeParserProperties<T> : TypeParserProperties
{
    private Func<ICallerContext, ICommandParameter, object?, IServiceProvider, ValueTask<ParseResult>>? _delegate;
    private TryParseParser<T>.ParseDelegate? _tryParseDelegate;

    public TypeParserProperties()
        : base(null!) // Assign null as we do not use the underlying logic
    {
        _delegate = null;
    }

    public TypeParserProperties<T> Delegate(Func<ICallerContext, ICommandParameter, object?, IServiceProvider, ValueTask<ParseResult>> executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _delegate = executionDelegate;

        return this;
    }

    public TypeParserProperties<T> Delegate(TryParseParser<T>.ParseDelegate executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _tryParseDelegate = executionDelegate;

        return this;
    }

    public override TypeParser ToParser()
    {
        if (_tryParseDelegate is not null)
            return new TryParseParser<T>(_tryParseDelegate!);

        Assert.NotNull(_delegate, nameof(_delegate));

        return new DelegateTypeParser<T>(_delegate!);
    }

    internal override Type GetParserType()
        => typeof(T);
}

public class TypeParserProperties
{
    private readonly TypeParser _parser;

    internal TypeParserProperties(TypeParser parser)
    {
        _parser = parser;
    }

    public virtual TypeParser ToParser()
        => _parser;

    internal virtual Type GetParserType()
        => _parser.Type;
}
