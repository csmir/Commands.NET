using Commands.Parsing;

namespace Commands;

/// <summary>
///     A set of properties that can be used to configure a component.
/// </summary>
public sealed class ComponentConfigurationProperties
{
    private readonly Dictionary<Type, ITypeParserProperties> _parsers;
    private readonly Dictionary<object, object> _properties;

    /// <summary>
    ///     Creates a new instance of <see cref="ComponentConfigurationProperties"/>.
    /// </summary>
    public ComponentConfigurationProperties()
    {
        _parsers    = [];
        _properties = [];
    }

    /// <summary>
    ///     Adds a parser to the configuration.
    /// </summary>
    /// <param name="parser">The parser to add. This parser will replace existing parsers converting the same type.</param>
    /// <returns>The same <see cref="ComponentConfigurationProperties"/> for call-chaining.</returns>
    public ComponentConfigurationProperties Parser(ITypeParserProperties parser)
    {
        Assert.NotNull(parser, nameof(parser));

        _parsers[parser.GetParserType()] = parser;

        return this;
    }

    /// <summary>
    ///     Adds a parser to the configuration.
    /// </summary>
    /// <param name="parser">The parser to add. This parser will replace existing parsers converting the same type.</param>
    /// <returns>The same <see cref="ComponentConfigurationProperties"/> for call-chaining.</returns>
    public ComponentConfigurationProperties Parser(TypeParser parser)
        => Parser(new TypeParserProperties(parser));

    /// <summary>
    ///     Adds multiple parsers to the configuration.
    /// </summary>
    /// <param name="parsers">The parsers to add. Provided parsers will replace existing parsers converting the same type.</param>
    /// <returns>The same <see cref="ComponentConfigurationProperties"/> for call-chaining.</returns>
    public ComponentConfigurationProperties Parsers(params ITypeParserProperties[] parsers)
    {
        foreach (var parser in parsers)
            Parser(parser);

        return this;
    }

    /// <summary>
    ///     Adds multiple parsers to the configuration.
    /// </summary>
    /// <param name="parsers">The parsers to add. Provided parsers will replace existing parsers converting the same type.</param>
    /// <returns>The same <see cref="ComponentConfigurationProperties"/> for call-chaining.</returns>
    public ComponentConfigurationProperties Parsers(params TypeParser[] parsers)
    {
        foreach (var parser in parsers)
            Parser(new TypeParserProperties(parser));

        return this;
    }

    /// <summary>
    ///     Adds a property to the configuration.
    /// </summary>
    /// <param name="key">The identifier of the property. Properties with the same key as this one will replace the existing value.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The same <see cref="ComponentConfigurationProperties"/> for call-chaining.</returns>
    public ComponentConfigurationProperties Property(object key, object value)
    {
        Assert.NotNull(key, nameof(key));

        _properties[key] = value;

        return this;
    }

    /// <summary>
    ///     Adds multiple properties to the configuration.
    /// </summary>
    /// <param name="properties">The properties to add. Properties with the same key as another will replace the existing value.</param>
    /// <returns>The same <see cref="ComponentConfigurationProperties"/> for call-chaining.</returns>
    public ComponentConfigurationProperties Properties(params KeyValuePair<object, object>[] properties)
    {
        foreach (var property in properties)
            Property(property.Key, property.Value);

        return this;
    }

    /// <summary>
    ///     Converts the properties to a new instance of <see cref="ComponentConfiguration"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="ComponentConfiguration"/>.</returns>
    public ComponentConfiguration Create()
    {
        var baseParsers = TypeParser.CreateDefaults().ToDictionary(x => x.Type);

        foreach (var kvp in _parsers)
            baseParsers[kvp.Key] = kvp.Value.Create();

        return new ComponentConfiguration(baseParsers, _properties);
    }

    #region Initializers

    /// <summary>
    ///     Defines a default set of properties which will serve as the implementation when no other configuration is provided to component creation.
    /// </summary>
    public static ComponentConfigurationProperties Default { get; } = new();

    #endregion
}
