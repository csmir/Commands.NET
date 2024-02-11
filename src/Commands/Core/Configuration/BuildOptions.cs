using Commands.Reflection;
using Commands.TypeConverters;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Commands.Core
{
    /// <summary>
    ///     A set of options determining the build process for modules and commands.
    /// </summary>
    public class BuildOptions()
    {
        const string DEFAULT_REGEX = @"^[a-z0-9_-]*$";

        private Dictionary<Type, TypeConverterBase>? _keyedConverters;

        /// <summary>
        ///     Gets or sets a collection of assemblies that are to be used to discover created modules.
        /// </summary>
        /// <remarks>
        ///     Default: <see cref="Assembly.GetEntryAssembly"/>
        /// </remarks>
        public List<Assembly> Assemblies { get; set; } = [Assembly.GetEntryAssembly()!]; // never null in managed context.

        /// <summary>
        ///     Gets or sets a collection of <see cref="TypeConverterBase"/>'s representing predefined <see cref="Type"/> conversion.
        /// </summary>
        /// <remarks>
        ///     This dictionary can be changed to remove base converters that should be replaced by local implementations.
        ///     <br/>
        ///     <br/>
        ///     Default: <see cref="TypeConverterBase.BuildDefaults"/>.
        /// </remarks>
        public List<TypeConverterBase> TypeConverters { get; set; } = [.. TypeConverterBase.BuildDefaults()];

        /// <summary>
        ///     Gets or sets a collection of <see cref="IComponent"/>'s that are manually created before the registration process runs.
        /// </summary>
        public List<CommandInfo> Commands { get; set; } = [];

        /// <summary>
        ///     Gets or sets the naming convention of commands and groups being registered into the <see cref="CommandManager"/>.
        /// </summary>
        /// <remarks>
        ///     Default: <c>@"^[a-z0-9_-]*$"</c>
        /// </remarks>
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'. We don't do this because it can be changed in source.
        public Regex NamingRegex { get; set; } = new(DEFAULT_REGEX, RegexOptions.Compiled);
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

        internal Dictionary<Type, TypeConverterBase> KeyedConverters
        {
            get
            {
                if (_keyedConverters == null)
                {
                    return SetKeyedConverters(null);
                }

                return _keyedConverters;
            }
        }

        internal Dictionary<Type, TypeConverterBase> SetKeyedConverters(IEnumerable<TypeConverterBase>? converters)
        {
            _keyedConverters = TypeConverters
                .UnionBy(converters ?? [], x => x.Type)
                .ToDictionary(x => x.Type, x => x);

            return _keyedConverters;
        }
    }
}
