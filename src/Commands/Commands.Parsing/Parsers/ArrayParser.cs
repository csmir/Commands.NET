﻿namespace Commands.Parsing;

internal sealed class ArrayParser(TypeParser underlyingParser) : TypeParser
{
    private static readonly Dictionary<Type, ArrayParser> _parsers = [];

    public override Type Type => underlyingParser.Type;

#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("AotAnalysis", "IL3050", Justification = "The type is propagated from user-facing code, it is up to the user to make it available at compile-time.")]
#endif
    public override async ValueTask<ParseResult> Parse(IContext context, ICommandParameter parameter, object? argument, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (argument is not IEnumerable<object> input)
            return Error($"The provided value is not an array. Expected: '{Type.Name}', got: '{argument}'. At: '{parameter.Name}'");

        var instance = Array.CreateInstance(Type, input.Count());

        var counter = 0;
        foreach (var item in input)
        {
            var result = await underlyingParser.Parse(context, parameter, item, services, cancellationToken).ConfigureAwait(false);

            if (!result.Success)
                return Error($"Failed to convert an array element. Expected: '{underlyingParser.Type.Name}', got: '{item}'. At: '{parameter.Name}', Index: '{counter}'");

            instance.SetValue(result.Value, counter++);
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
