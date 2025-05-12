using Commands.Parsing;

namespace Commands;

/// <summary>
///     A set of options that can be used to configure command, group and tree creation.
/// </summary>
public sealed class CreationOptions
{
    /// <summary>
    ///     A collection of parsers that can be used to parse command arguments into the target type. When creating this object, the default parsers are registered for the most common types.
    /// </summary>
    public Dictionary<Type, TypeParser> Parsers { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CreationOptions"/> class containing default values.
    /// </summary>
    public CreationOptions()
    {
        Parsers = TypeParser.CreateDefaults()
            .ToDictionary(x => x.Type);
    }

    /// <summary>
    ///     Gets the default <see cref="CreationOptions"/> instance. When creating a command, group or tree, this instance is used by default if no other options are provided.
    /// </summary>
    public static CreationOptions Default { get; } = new CreationOptions();

    #region Internals

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

    #endregion
}
