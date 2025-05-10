using Commands.Parsing;

namespace Commands;

/// <summary>
///     
/// </summary>
public sealed class ComponentConfiguration
{
    /// <inheritdoc />
    public Dictionary<object, object> Properties { get; }

    /// <inheritdoc />
    public Dictionary<Type, TypeParser> Parsers { get; }

    /// <summary>
    ///     Creates a new -empty- instance of <see cref="ComponentConfiguration"/>.
    /// </summary>
    public ComponentConfiguration()
        : this([], []) { }

    /// <summary>
    ///     Creates a new instance of <see cref="ComponentConfiguration"/> with the provided parsers.
    /// </summary>
    /// <remarks>
    ///     This overload supports enumerable service injection in order to create a configuration from service definitions.
    /// </remarks>
    /// <param name="parsers">The parsers to add to the configuration.</param>
    public ComponentConfiguration(IEnumerable<TypeParser> parsers)
        : this(parsers.ToDictionary(x => x.Type), []) { }

    /// <summary>
    ///     Creates a new instance of <see cref="ComponentConfiguration"/> with the provided parsers and properties.
    /// </summary>
    /// <param name="parsers">The parsers to add to the configuration.</param>
    /// <param name="properties">The properties to add to the configuration.</param>
    public ComponentConfiguration(Dictionary<Type, TypeParser> parsers, Dictionary<object, object> properties)
    {
        var baseParsers = TypeParser.CreateDefaults().ToDictionary(x => x.Type);

        foreach (var parser in parsers)
            baseParsers[parser.Key] = parser.Value;

        Parsers = baseParsers;
        Properties = properties;
    }

    /// <summary>
    ///     Gets a property from the <see cref="Properties"/> collection, or returns the default value if the property does not exist.
    /// </summary>
    /// <typeparam name="T">The type with which the value returned from <see cref="Properties"/> should be compatible, and cast to.</typeparam>
    /// <param name="key">The key under which the properties should have a value.</param>
    /// <param name="defaultValue">A fallback value if <see cref="Properties"/> contains no value for the provided key, or if the value cannot be cast to <typeparamref name="T"/>.</param>
    /// <returns>The value returned by <paramref name="key"/> if it exists and can be cast to <typeparamref name="T"/>; Otherwise <paramref name="defaultValue"/>.</returns>
    public T? GetProperty<T>(object key, T? defaultValue = default)
        => Properties.TryGetValue(key, out var value) && value is T tValue ? tValue : defaultValue;

    // Defines a default configuration with no parsers or properties.
    internal static ComponentConfiguration Default = new([], []);
}
