namespace Commands.Parsing;

internal sealed class EnumParser(Type targetEnumType) : TypeParser
{
    private static readonly Dictionary<Type, EnumParser> _parsers = [];

    public override Type TargetType { get; } = targetEnumType;

    public override ValueTask<ParseResult> Parse(
        IContext context, ICommandParameter parameter, object? argument, IServiceProvider services, CancellationToken cancellationToken)
    {
        try
        {
            var @out = Enum.Parse(TargetType, argument?.ToString() ?? string.Empty, true);

            return Success(@out);
        }
        catch (ArgumentException)
        {
            return Error($"The provided value is not a part the enum specified. Expected: '{TargetType.Name}', got: '{argument}'. At: '{parameter.Name}'");
        }
    }

    internal static EnumParser GetOrCreate(Type type)
    {
        if (_parsers.TryGetValue(type, out var reader))
            return reader;

        _parsers.Add(type, reader = new EnumParser(type));

        return reader;
    }
}
