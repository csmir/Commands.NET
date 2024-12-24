using Commands.Builders;
using Commands.Conversion;

namespace Commands
{
    /// <summary>
    ///     A read-only configuration class which is used by individual components to set up their own configuration. This class cannot be inherited.
    /// </summary>
    public sealed class ComponentConfiguration
    {
        /// <summary>
        ///     Gets a default configuration that can be used as a fallback when no configuration is provided.
        /// </summary>
        /// <remarks>
        ///     When this property is accessed, a new instance of <see cref="ComponentConfiguration"/> is built using the <see cref="ComponentConfigurationBuilder.Default"/> builder.
        /// </remarks>
        public static ComponentConfiguration Default
            => ComponentConfigurationBuilder.Default.Build();

        /// <summary>
        ///     Gets a collection of properties that are used to store additional information explicitly important during the build process.
        /// </summary>
        public IReadOnlyDictionary<object, object> Properties { get; }

        /// <summary>
        ///     Gets a collection of parsers that are used to convert arguments.
        /// </summary>
        public IReadOnlyDictionary<Type, TypeParser> Parsers { get; }

        /// <summary>
        ///     Creates a new instance of <see cref="ComponentConfiguration"/> with the default parsers and naming pattern.
        /// </summary>
        /// <remarks>
        ///     This constructor initializes the <see cref="Parsers"/> collection with the default parsers for the most common types, using <see cref="TypeParser.CreateDefaults"/>.
        /// </remarks>
        public ComponentConfiguration()
            : this(TypeParser.CreateDefaults()) { }

        /// <summary>
        ///     Creates a new instance of <see cref="ComponentConfiguration"/> with the specified parsers and naming pattern.
        /// </summary>
        /// <param name="parsers">The range of parsers to match to command arguments.</param>
        public ComponentConfiguration(IEnumerable<TypeParser> parsers)
        {
            Properties = new Dictionary<object, object>();
            Parsers = parsers.ToDictionary(x => x.Type, x => x);
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ComponentConfiguration"/> with the specified parsers, properties and naming pattern.
        /// </summary>
        /// <param name="parsers">The range of parsers to match to command arguments.</param>
        /// <param name="properties">The properties that are used to store additional information explicitly important during the build process.</param>
        public ComponentConfiguration(IEnumerable<KeyValuePair<Type, TypeParser>> parsers, IEnumerable<KeyValuePair<object, object>> properties)
        {
            Properties = properties.ToDictionary(x => x.Key, x => x.Value);
            Parsers = parsers.ToDictionary(x => x.Key, x => x.Value);
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
        ///     Creates a new instance of <see cref="ComponentConfigurationBuilder"/>, which can be built into an instance of <see cref="ComponentConfiguration"/>.
        /// </summary>
        /// <returns>A build model with a fluent API to configure how components should be registered within or at creation of the <see cref="IComponentTree"/>.</returns>
        public static IConfigurationBuilder CreateBuilder()
            => new ComponentConfigurationBuilder();
    }
}
