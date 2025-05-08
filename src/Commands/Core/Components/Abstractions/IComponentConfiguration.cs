using Commands.Parsing;

namespace Commands;

/// <summary>
///     Contains configuration required by implementations of <see cref="IComponent"/> using extended, custom, or replacing type parsing or properties.
/// </summary>
public interface IComponentConfiguration
{
    /// <summary>
    ///     Gets a collection of properties that are used to store additional information explicitly important during the build process.
    /// </summary>
    public Dictionary<object, object> Properties { get; }

    /// <summary>
    ///     Gets a collection of parsers that are used to convert arguments.
    /// </summary>
    public Dictionary<Type, TypeParser> Parsers { get; }
}
