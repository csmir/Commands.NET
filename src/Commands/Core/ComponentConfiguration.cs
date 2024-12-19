using Commands.Builders;
using Commands.Conversion;
using System.Text.RegularExpressions;

namespace Commands
{
    /// <summary>
    ///     A read-only configuration class which is used by individual components to set up their own configuration. This class cannot be inherited.
    /// </summary>
    public sealed class ComponentConfiguration
    {
        /// <summary>
        ///     Gets a collection of properties that are used to store additional information explicitly important during the build process.
        /// </summary>
        public IReadOnlyDictionary<string, object> Properties { get; }

        /// <summary>
        ///     Gets a collection of type converters that are used to convert arguments.
        /// </summary>
        public IReadOnlyDictionary<Type, TypeParser> TypeConverters { get; }

        /// <summary>
        ///     Gets the naming convention used to identify command methods.
        /// </summary>
        public Regex? NamingPattern { get; }

        /// <summary>
        ///     Creates a new instance of <see cref="ComponentConfiguration"/>.
        /// </summary>
        /// <param name="converters">The range of type converters to match to command arguments.</param>
        /// <param name="namingPattern">The naming pattern which should determine how aliases are verified for their validity.</param>
        /// <param name="properties">The properties that are used to store additional information explicitly important during the build process.</param>"
        public ComponentConfiguration(IEnumerable<TypeParser> converters, string? namingPattern = @"^[a-z0-9_-]*$", Dictionary<string, object>? properties = null)
            : this(converters.ToDictionary(x => x.Type), properties ?? [], namingPattern is not null ? new Regex(namingPattern) : null) { }

        internal ComponentConfiguration(Dictionary<Type, TypeParser> converters, Dictionary<string, object> properties, Regex? namingPattern)
        {
            Properties = properties;
            TypeConverters = converters;
            NamingPattern = namingPattern;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ComponentConfigurationBuilder"/>, which can be built into an instance of <see cref="ComponentConfiguration"/>.
        /// </summary>
        /// <returns>A build model with a fluent API to configure how components should be registered within or at creation of the <see cref="ComponentTree"/>.</returns>
        public static IConfigurationBuilder CreateBuilder()
            => new ComponentConfigurationBuilder();
    }
}
