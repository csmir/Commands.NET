using Commands.Parsing;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Commands;

/// <summary>
///     A set of options that can be used to configure command, group and tree creation. This class cannot be inherited.
/// </summary>
public sealed class ComponentOptions
{
    /// <summary>
    ///     Gets or sets an action that will be invoked when a component is built. This action can be used to perform additional setup or logging on the component after it has been created.
    /// </summary>
    public Action<IComponent>? BuildCompleted { get; set; } = null;

    /// <summary>
    ///     Gets or sets a dictionary of parsers that will be used to parse command arguments into the target type by all components using these options.
    /// </summary>
    /// <remarks>
    ///     By modifying the key value pairs in this dictionary, you can add custom parsers for specific types or override the default parsers. When accessing the default, or creating a new instance of <see cref="ComponentOptions"/>, the following types are prepopulated:
    ///     <list type="bullet">
    ///         <item>
    ///             BCL types: <see cref="ulong"/>, <see cref="long"/>, <see cref="uint"/>, <see cref="int"/>, <see cref="ushort"/>, <see cref="short"/>, <see cref="sbyte"/>, <see cref="byte"/>, <see cref="bool"/>,
    ///             <see cref="decimal"/>, <see cref="double"/>, <see cref="float"/>,
    ///             <see cref="string"/> and <see cref="char"/>.
    ///         </item>
    ///         <item>
    ///             Common language structs: <see cref="Color"/>, <see cref="Guid"/> <see cref="DateTime"/>, <see cref="DateTimeOffset"/> and <see cref="TimeSpan"/>.
    ///         </item>
    ///         <item>
    ///             Array implementations of all above types.
    ///             <br/>
    ///             <i>By default, array parsers are created by wrapping defined parsers. For example, if you add a custom parser for <see cref="int"/>, the <see cref="Array"/> parser for <see cref="int"/>[] will be created by wrapping the custom parser.</i>
    ///         </item>
    ///         <item>
    ///             All <see cref="Enum"/> implementations.
    ///         </item>
    ///     </list>
    /// </remarks>
    public IDictionary<Type, TypeParser> Parsers { get; set; }

    /// <summary>
    ///     Gets or sets a regular expression that will be used to validate the names of commands and command groups.
    /// </summary>
    /// <remarks>
    ///     By default, this value is <see langword="null"/>. It can -for example- be implemented to validate against restrictions on component naming in certain API environments.
    /// </remarks>
    public Regex? NameValidation { get; set; } = null;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ComponentOptions"/> class containing default values for <see cref="Parsers"/>.
    /// </summary>
    public ComponentOptions()
        : this(false) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ComponentOptions"/> class.
    /// </summary>
    /// <param name="skipDefaults">Defines whether defaults should be configured in this configuration. If set, it will circumvent</param>
    public ComponentOptions(bool skipDefaults)
    {
        if (skipDefaults)
            Parsers = new Dictionary<Type, TypeParser>();
        else
            Parsers = TypeParser.CreateDefaults()
                .ToDictionary(x => x.TargetType);
    }

    /// <summary>
    ///     Gets the default <see cref="ComponentOptions"/> instance. When creating a command, group or tree, this instance is used by default if no other options are provided.
    /// </summary>
    public static ComponentOptions Default { get; } = new ComponentOptions();

    #region Internals

    internal TypeParser GetParser(Type type)
    {
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

        throw new ComponentFormatException($"The provided component options defines no parser implementation for parameter with type {type}.");
    }

    #endregion
}
