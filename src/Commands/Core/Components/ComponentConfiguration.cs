using Commands.Parsing;

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
    public static ComponentConfiguration Default
        => ComponentConfigurationProperties.Default.ToConfiguration();

    /// <summary>
    ///     Gets a collection of properties that are used to store additional information explicitly important during the build process.
    /// </summary>
    public IReadOnlyDictionary<object, object> Properties { get; }

    /// <summary>
    ///     Gets a collection of parsers that are used to convert arguments.
    /// </summary>
    public IReadOnlyDictionary<Type, TypeParser> Parsers { get; }

    internal ComponentConfiguration(Dictionary<Type, TypeParser> parsers, Dictionary<object, object> properties)
    {
        Parsers = parsers;
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

    /// <summary>
    ///     Defines a collection of properties to configure and convert into a new instance of <see cref="ComponentConfiguration"/>.
    /// </summary>
    /// <returns>A fluent-pattern property object that can be converted into an instance when configured.</returns>
    public static ComponentConfigurationProperties From()
        => new();
}
