using Commands.Reflection;
using Commands.Resolvers;
using Commands.TypeConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Commands
{
    /// <summary>
    ///     A set of options determining the build process for modules and commands.
    /// </summary>
    public interface ICommandBuilder
    {
        /// <summary>
        ///     Gets or sets a collection of assemblies that are to be used to discover created modules.
        /// </summary>
        public List<Assembly> Assemblies { get; set; }

        /// <summary>
        ///     Gets or sets a collection of <see cref="TypeConverterBase"/>'s representing predefined <see cref="Type"/> conversion.
        /// </summary>
        public Dictionary<Type, TypeConverterBase> TypeConverters { get; set; }

        /// <summary>
        ///     Gets or sets a collection of <see cref="ResultResolverBase"/>'s that serve as post-execution handlers.
        /// </summary>
        public List<ResultResolverBase> ResultResolvers { get; set; }

        /// <summary>
        ///     Gets or sets a collection of <see cref="CommandInfo"/>'s that are manually created before the registration process runs.
        /// </summary>
        public List<CommandInfo> Commands { get; set; }

        /// <summary>
        ///     Gets or sets the naming convention of commands and groups being registered into the <see cref="CommandManager"/>.
        /// </summary>
        public Regex NamingRegex { get; set; }
    }
}
