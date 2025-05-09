﻿using Commands.Parsing;

namespace Commands;

/// <summary>
///     A set of properties that can be used to configure a component.
/// </summary>
public sealed class ComponentConfigurationBuilder
{
    private readonly Dictionary<Type, ITypeParserBuilder> _parsers;
    private readonly Dictionary<object, object> _properties;

    /// <summary>
    ///     Creates a new instance of <see cref="ComponentConfigurationBuilder"/>.
    /// </summary>
    public ComponentConfigurationBuilder()
    {
        _parsers = [];
        _properties = [];
    }

    /// <summary>
    ///     Adds a parser to the configuration.
    /// </summary>
    /// <param name="parser">The parser to add. This parser will replace existing parsers converting the same type.</param>
    /// <returns>The same <see cref="ComponentConfigurationBuilder"/> for call-chaining.</returns>
    public ComponentConfigurationBuilder AddParser(ITypeParserBuilder parser)
    {
        Assert.NotNull(parser, nameof(parser));

        _parsers[parser.GetParserType()] = parser;

        return this;
    }

    /// <summary>
    ///     Adds a parser to the configuration.
    /// </summary>
    /// <param name="parser">The parser to add. This parser will replace existing parsers converting the same type.</param>
    /// <returns>The same <see cref="ComponentConfigurationBuilder"/> for call-chaining.</returns>
    public ComponentConfigurationBuilder AddParser(TypeParser parser)
        => AddParser(new TypeParserBuilder(parser));

    /// <summary>
    ///     Adds a parser to the configuration.
    /// </summary>
    /// <typeparam name="TConvertible">The type to convert using this converter.</typeparam>
    /// <param name="parseDelegate">The delegate that will parse any incoming value that targets this type, and return the parsed output.</param>
    /// <returns>The same <see cref="ComponentConfigurationBuilder"/> for call-chaining.</returns>
    public ComponentConfigurationBuilder AddParser<TConvertible>(Func<ICallerContext, ICommandParameter, object?, IServiceProvider, ValueTask<ParseResult>> parseDelegate)
        => AddParser(new TypeParserBuilder(new DelegateTypeParser<TConvertible>(parseDelegate)));

    /// <summary>
    ///     Adds multiple parsers to the configuration.
    /// </summary>
    /// <param name="parsers">The parsers to add. Provided parsers will replace existing parsers converting the same type.</param>
    /// <returns>The same <see cref="ComponentConfigurationBuilder"/> for call-chaining.</returns>
    public ComponentConfigurationBuilder AddParsers(params ITypeParserBuilder[] parsers)
    {
        foreach (var parser in parsers)
            AddParser(parser);

        return this;
    }

    /// <summary>
    ///     Adds multiple parsers to the configuration.
    /// </summary>
    /// <param name="parsers">The parsers to add. Provided parsers will replace existing parsers converting the same type.</param>
    /// <returns>The same <see cref="ComponentConfigurationBuilder"/> for call-chaining.</returns>
    public ComponentConfigurationBuilder AddParsers(params TypeParser[] parsers)
    {
        foreach (var parser in parsers)
            AddParser(new TypeParserBuilder(parser));

        return this;
    }

    /// <summary>
    ///     Adds a property to the configuration.
    /// </summary>
    /// <param name="key">The identifier of the property. Properties with the same key as this one will replace the existing value.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The same <see cref="ComponentConfigurationBuilder"/> for call-chaining.</returns>
    public ComponentConfigurationBuilder AddProperty(object key, object value)
    {
        Assert.NotNull(key, nameof(key));

        _properties[key] = value;

        return this;
    }

    /// <summary>
    ///     Adds multiple properties to the configuration.
    /// </summary>
    /// <param name="properties">The properties to add. Properties with the same key as another will replace the existing value.</param>
    /// <returns>The same <see cref="ComponentConfigurationBuilder"/> for call-chaining.</returns>
    public ComponentConfigurationBuilder AddProperties(params KeyValuePair<object, object>[] properties)
    {
        foreach (var property in properties)
            AddProperty(property.Key, property.Value);

        return this;
    }

    /// <summary>
    ///     Converts the properties to a new instance of <see cref="ComponentConfiguration"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="ComponentConfiguration"/>.</returns>
    public ComponentConfiguration Build()
    {
        var baseParsers = new Dictionary<Type, TypeParser>();

        foreach (var kvp in _parsers)
            baseParsers[kvp.Key] = kvp.Value.Build();

        return new ComponentConfiguration(baseParsers, _properties);
    }

    #region Initializers

    /// <summary>
    ///     Defines a default set of properties which will serve as the implementation when no other configuration is provided to component creation.
    /// </summary>
    public static ComponentConfigurationBuilder Default { get; } = new();

    #endregion
}
