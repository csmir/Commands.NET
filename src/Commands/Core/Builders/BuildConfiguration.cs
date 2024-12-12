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
        // The following property is only used when configuring the command manager.
        internal Action<ISearchable[], bool>? N_NotifyTopLevelMutation;

        /// <summary>
        ///     Gets a collection of type converters that are used to convert arguments.
        /// </summary>
        public IReadOnlyDictionary<Type, TypeConverterBase> TypeConverters { get; }

        /// <summary>
        ///     Gets the naming convention used to identify command methods.
        /// </summary>
        public Regex NamingRegex { get; }

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
        public BuildConfiguration(IEnumerable<TypeConverterBase> converters, string namingPattern = @"^[a-z0-9_-]*$", bool sealModuleDefinitions = false)
            : this(converters.ToDictionary(x => x.Type), new Regex(namingPattern), sealModuleDefinitions)
        {

        }

        internal BuildConfiguration(Dictionary<Type, TypeConverterBase> converters, Regex namingPattern, bool sealModuleDefinitions)
        {
            SealModuleDefinitions = sealModuleDefinitions;
            TypeConverters = converters;
            NamingRegex = namingPattern;
        }
    }
}
