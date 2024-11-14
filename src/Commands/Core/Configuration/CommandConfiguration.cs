using Commands.Converters;
using Commands.Resolvers;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Commands
{
    /// <summary>
    ///     A read-only configuration entity which is used by the <see cref="CommandManager"/> to construct commands and prepare them for execution. This class cannot be inherited.
    /// </summary>
    public sealed class CommandConfiguration
    {
        /// <summary>
        ///     Gets a collection of assemblies that are used to search for command types.
        /// </summary>
        public List<Assembly> Assemblies { get; } 

        /// <summary>
        ///     Gets a collection of type converters that are used to convert arguments.
        /// </summary>
        public Dictionary<Type, TypeConverterBase> TypeConverters { get; } 

        /// <summary>
        ///     Gets a collection of result resolvers that are used to handle command results.
        /// </summary>
        public List<ResultResolverBase> ResultResolvers { get; }

        /// <summary>
        ///     Gets a collection of commands based on delegates that are added to the manager at runtime.
        /// </summary>
        public List<CommandBase> Commands { get; } 

        /// <summary>
        ///     Gets the naming convention used to identify command methods.
        /// </summary>
        public Regex NamingRegex { get; } 

        internal CommandConfiguration(ConfigurationBuilder configuration)
        {
            Assemblies = configuration.Assemblies;
            TypeConverters = configuration.TypeConverters;
            ResultResolvers = configuration.ResultResolvers;
            Commands = configuration.Commands;
            NamingRegex = configuration.NamingRegex;
        }
    }
}
