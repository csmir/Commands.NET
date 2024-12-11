using Commands.Converters;
using Commands.Resolvers;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Commands
{
    /// <summary>
    ///     A read-only configuration base class which is used by individual components to set up their own configuration.
    /// </summary>
    public class CommandConfiguration
    {
        /// <summary>
        ///     Gets a collection of type converters that are used to convert arguments.
        /// </summary>
        public Dictionary<Type, TypeConverterBase> TypeConverters { get; }

        /// <summary>
        ///     Gets the naming convention used to identify command methods.
        /// </summary>
        public Regex NamingRegex { get; }

        /// <summary>
        ///     Creates a new instance of <see cref="CommandConfiguration"/>.
        /// </summary>
        /// <param name="converters">The range of type converters to match to command arguments.</param>
        /// <param name="namingPattern">The naming pattern which should determine how aliases are verified for their validity.</param>
        public CommandConfiguration(IEnumerable<TypeConverterBase> converters, string namingPattern = @"^[a-z0-9_-]*$")
            : this(converters.ToDictionary(x => x.Type), new Regex(namingPattern))
        {

        }

        internal CommandConfiguration(Dictionary<Type, TypeConverterBase> converters, Regex namingPattern)
        {
            TypeConverters = converters;
            NamingRegex = namingPattern;
        }
    }
}
