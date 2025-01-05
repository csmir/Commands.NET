namespace Commands.Conversion;

internal sealed class ArrayParser(TypeParser underlyingParser) : TypeParser
{
    private static readonly Dictionary<Type, ArrayParser> _parsers = [];

    public override Type Type => underlyingParser.Type;

#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("AotAnalysis", "IL3050", Justification = "The type is propagated from user-facing code, it is up to the user to make it available at compile-time.")]
#endif
    public override async ValueTask<ParseResult> Parse(ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (value is not object[] array)
            return Error($"The provided value is not an array. Expected: '{Type.Name}', got: '{value}'. At: '{argument.Name}'");

        var instance = Array.CreateInstance(Type, array.Length);

        for (var i = 0; i < array.Length; i++)
        {
            var item = array.GetValue(i);

            var result = await underlyingParser.Parse(caller, argument, item, services, cancellationToken);

            if (!result.Success)
                return Error($"Failed to convert an array element. Expected: '{underlyingParser.Type.Name}', got: '{item}'. At: '{argument.Name}', Index: '{i}'");

            instance.SetValue(result.Value, i);
        }

        return Success(instance);
    }

    internal static ArrayParser GetOrCreate(TypeParser underlyingConverter)
    {
        if (_parsers.TryGetValue(underlyingConverter.Type, out var parser))
            return parser;

        parser = new ArrayParser(underlyingConverter)!;

        _parsers.Add(underlyingConverter.Type, parser);

        return parser;
    }
}
