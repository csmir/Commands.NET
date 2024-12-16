using Commands.Converters;
using Commands.Reflection;
using System.Text.RegularExpressions;

namespace Commands
{
    /// <summary>
    ///     A read-only configuration class which is used by individual components to set up their own configuration.
    /// </summary>
    public class BuildConfiguration
    {
        /// <summary>
        ///     Gets a filter that is used to determine if a component should be registered. This property is only used when building declared signatures.
        /// </summary>
        /// <remarks>
        ///     This filter is used to determine if a component should be registered. If the filter returns <see langword="false"/>, the component will not be registered.
        /// </remarks>
        public Func<IComponent, bool>? ComponentRegistrationFilter { get; }

        /// <summary>
        ///     Gets a collection of properties that are used to store additional information explicitly important during the build process.
        /// </summary>
        public IReadOnlyDictionary<string, object> Properties { get; }

        /// <summary>
        ///     Gets a collection of type converters that are used to convert arguments.
        /// </summary>
        public IReadOnlyDictionary<Type, TypeConverter> TypeConverters { get; }

        /// <summary>
        ///     Gets the naming convention used to identify command methods.
        /// </summary>
        public Regex? NamingPattern { get; }

        /// <summary>
        ///     Gets if module definitions created with this configuration should be sealed, making them readonly and unable to be modified at runtime.
        /// </summary>
        public bool SealModuleDefinitions { get; }

        /// <summary>
        ///     Creates a new instance of <see cref="BuildConfiguration"/>.
        /// </summary>
        /// <param name="converters">The range of type converters to match to command arguments.</param>
        /// <param name="namingPattern">The naming pattern which should determine how aliases are verified for their validity.</param>
        /// <param name="sealModuleDefinitions">Defines if modules registered by this configuration will be read-only, making them unable to be modified.</param>
        public BuildConfiguration(IEnumerable<TypeConverter> converters, string? namingPattern = @"^[a-z0-9_-]*$", bool sealModuleDefinitions = false)
            : this(converters.ToDictionary(x => x.Type), [], namingPattern is not null ? new Regex(namingPattern) : null, sealModuleDefinitions) { }

        internal BuildConfiguration(Dictionary<Type, TypeConverter> converters, Dictionary<string, object> properties, Regex? namingPattern, bool enforceReadonly, Func<IComponent, bool>? filter = null)
        {
            Properties = properties;
            ComponentRegistrationFilter = filter;
            SealModuleDefinitions = enforceReadonly;
            TypeConverters = converters;
            NamingPattern = namingPattern;
        }
    }
}
