using Commands.Builders;
using Commands.Conversion;
using System.Reflection;

namespace Commands;

/// <summary>
///     A read-only configuration class which is used by individual components to set up their own configuration. This class cannot be inherited.
/// </summary>
public sealed class ComponentConfiguration
{
    /// <summary>
    ///     Gets a default configuration that can be used as a fallback when no configuration is provided.
    /// </summary>
    /// <remarks>
    ///     This instance contains no properties, and only implements the default parsers created by <see cref="TypeParser.CreateDefaults"/>.
    /// </remarks>
    public static ComponentConfiguration Default { get; } = new([], []);

    /// <summary>
    ///     Gets a collection of properties that are used to store additional information explicitly important during the build process.
    /// </summary>
    public IReadOnlyDictionary<object, object> Properties { get; }

    /// <summary>
    ///     Gets a collection of parsers that are used to convert arguments.
    /// </summary>
    public IReadOnlyDictionary<Type, TypeParser> Parsers { get; }

    internal ComponentConfiguration(IEnumerable<TypeParser> parsers, IEnumerable<KeyValuePair<object, object>> properties)
    {
        var baseParsers = TypeParser.CreateDefaults().ToDictionary(x => x.Type, x => x);

        foreach (var parser in parsers)
            baseParsers[parser.Type] = parser;

        Parsers = baseParsers;
        Properties = properties.ToDictionary(x => x.Key, x => x.Value);
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

    /// <summary>
    ///     Recursively searches through all the provided types and contained nested types to find all implementations of <see cref="CommandModule"/> or <see cref="CommandModule{T}"/>.
    /// </summary>
    /// <remarks>
    ///     The provided types do not have to be implementations of a module type, as this operation ignores types that are not. Supplying <see cref="Assembly.GetTypes"/> -or other implicitly sourced type collections- is valid.
    /// </remarks>
    /// <param name="types">A collection of <see cref="DynamicType"/> which accepts <see cref="Type"/>, which could, or should, contain command modules.</param>
    /// <returns>A lazily evaluated collection of <see cref="IComponent"/> implementations, being either <see cref="Command"/> or <see cref="CommandGroup"/> depending on if a method or a type was resolved.</returns>
    public IEnumerable<IComponent> CreateComponents(params DynamicType[] types)
        => ComponentUtilities.BuildGroups(this, types, null, false);

    /// <summary>
    ///     Creates a new instance of <see cref="ComponentConfiguration"/> with a range of default parsers, created from <see cref="TypeParser.CreateDefaults"/>.
    /// </summary>
    /// <inheritdoc cref="Create(IEnumerable{TypeParser}, IEnumerable{KeyValuePair{object, object}})"/>
    public static ComponentConfiguration Create(params TypeParser[] parsers)
        => Create(parsers);

    /// <summary>
    ///     Creates a new instance of <see cref="ComponentConfiguration"/> with the provided parsers and properties. 
    /// </summary>
    /// <remarks>
    ///     The provided parsers will be merged with the default parsers created by <see cref="TypeParser.CreateDefaults"/>. If a parser with the same type is provided, it will override the default parser.
    /// </remarks>
    /// <param name="parsers">A <see cref="TypeParser"/> collection which will parse input arguments provided through the <see cref="IExecutionProvider"/> into instances of the types by which methods are marked.</param>
    /// <param name="properties"></param>
    /// <returns>A new instance of <see cref="ComponentConfiguration"/> containing a <see cref="TypeParser"/> dictionary and a collection of properties which determine certain settings within the component creation process.</returns>
    public static ComponentConfiguration Create(IEnumerable<TypeParser> parsers, IEnumerable<KeyValuePair<object, object>>? properties = null)
    {
        foreach (var parser in parsers)
            Assert.NotNull(parser, nameof(parser));

        if (properties != null)
        {
            // Ensure that if a key is provided, it is not null. Otherwise, errors will occur in deeper processes.
            foreach (var property in properties)
                Assert.NotNull(property.Key, nameof(property.Key));
        }

        return new ComponentConfiguration(parsers, properties?.ToDictionary(x => x.Key, x => x.Value) ?? []);
    }

    /// <summary>
    ///     Creates a new instance of <see cref="ComponentConfigurationBuilder"/>, which can be built into an instance of <see cref="ComponentConfiguration"/>.
    /// </summary>
    /// <returns>A build model with a fluent API to configure how components should be registered within or at creation of the <see cref="IExecutionProvider"/>.</returns>
    public static IConfigurationBuilder CreateBuilder()
        => new ComponentConfigurationBuilder();
}
