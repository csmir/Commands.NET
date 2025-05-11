using Commands.Parsing;

namespace Commands;

/// <summary>
///     A set of options that can be used to configure command, group and tree creation.
/// </summary>
public sealed class BuildOptions
{
    /// <summary>
    ///     A collection of parsers that can be used to parse command arguments into the target type. When creating this object, the default parsers are registered for the most common types.
    /// </summary>
    public Dictionary<Type, TypeParser> Parsers { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BuildOptions"/> class containing default values.
    /// </summary>
    public BuildOptions()
    {
        Parsers = TypeParser.CreateDefaults()
            .ToDictionary(x => x.Type);
    }

    internal TypeParser GetParser(Type type)
    {
        Assert.NotNull(type, nameof(type));

        if (Parsers.TryGetValue(type, out var parser))
            return parser;

        if (type.IsEnum)
            return EnumParser.GetOrCreate(type);

        if (type.IsArray)
        {
            type = type.GetElementType()!;

            if (Parsers.TryGetValue(type, out parser))
                return ArrayParser.GetOrCreate(parser);

            if (type.IsEnum)
                return EnumParser.GetOrCreate(type);
        }

        throw new NotSupportedException($"No parser is known for type {type}.");
    }

    /// <summary>
    ///     Gets the default <see cref="BuildOptions"/> instance.
    /// </summary>
    public static BuildOptions Default { get; } = new BuildOptions();
}
