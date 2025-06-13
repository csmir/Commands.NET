namespace Commands.Parsing;

internal sealed class ArrayParser(TypeParser underlyingParser) : TypeParser
{
    private static readonly Dictionary<Type, ArrayParser> _parsers = [];

    public override Type TargetType => underlyingParser.TargetType;

#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("AotAnalysis", "IL3050", Justification = "The type is propagated from user-facing code, it is up to the user to make it available at compile-time.")]
#endif
    public override async ValueTask<ParseResult> Parse(IContext context, ICommandParameter parameter, object? argument, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (argument is not IEnumerable<object> input)
            return Error($"The provided value is not an array. Expected: '{TargetType.Name}', got: '{argument}'. At: '{parameter.Name}'");

        var instance = Array.CreateInstance(TargetType, input.Count());

        var counter = 0;
        foreach (var item in input)
        {
            var result = await underlyingParser.Parse(context, parameter, item, services, cancellationToken).ConfigureAwait(false);

            if (!result.Success)
                return Error($"Failed to convert an array element. Expected: '{underlyingParser.TargetType.Name}', got: '{item}'. At: '{parameter.Name}', Index: '{counter}'");

            instance.SetValue(result.Value, counter++);
        }

        return Success(instance);
    }

    internal static ArrayParser GetOrCreate(TypeParser underlyingConverter)
    {
        if (_parsers.TryGetValue(underlyingConverter.TargetType, out var parser))
            return parser;

        parser = new ArrayParser(underlyingConverter)!;

        _parsers.Add(underlyingConverter.TargetType, parser);

        return parser;
    }
}
