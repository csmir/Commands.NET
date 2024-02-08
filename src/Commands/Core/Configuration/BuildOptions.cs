using Commands.TypeConverters;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Commands.Core
{
    /// <summary>
    ///     A set of options determining the build process for modules and commands.
    /// </summary>
    public class BuildOptions()
    {
        private Dictionary<Type, TypeConverterBase> _keyedConverters;

        /// <summary>
        ///     Gets or sets a collection of assemblies that are to be used to discover created modules.
        /// </summary>
        /// <remarks>
        ///     Default: <see cref="Assembly.GetEntryAssembly"/>
        /// </remarks>
        public Assembly[] Assemblies { get; set; } = [Assembly.GetEntryAssembly()];

        /// <summary>
        ///     Gets or sets a collection of <see cref="TypeConverterBase"/>'s representing predefined <see cref="ValueType"/> conversion.
        /// </summary>
        /// <remarks>
        ///     This dictionary can be changed to remove base converters that should be replaced by local implementations.
        ///     <br/>
        ///     <br/>
        ///     Default: <see cref="TypeConverterBase.BuildDefaults"/>.
        /// </remarks>
        public TypeConverterBase[] TypeConverters { get; set; } = TypeConverterBase.BuildDefaults();

        /// <summary>
        ///     Gets or sets the naming convention of commands and groups being registered into the <see cref="CommandManager"/>.
        /// </summary>
        /// <remarks>
        ///     Default: <c>@"^[a-z0-9_-]*$"</c>
        /// </remarks>
        public Regex NamingRegex { get; set; } = new(@"^[a-z0-9_-]*$", RegexOptions.Compiled);

        /// <summary>
        ///     Gets or sets whether commands not matching <see cref="NamingRegex"/> will throw during command registration.
        /// </summary>
        /// <remarks>
        ///     Default: <see langword="true"/>
        /// </remarks>
        public bool ThrowOnMatchFailure { get; set; } = true;

        internal Dictionary<Type, TypeConverterBase> KeyedConverters
        {
            get
            {
                if (_keyedConverters == null)
                {
                    SetKeyedConverters(null);
                }

                return _keyedConverters;
            }
        }

        internal void SetKeyedConverters(IEnumerable<TypeConverterBase> converters)
        {
            _keyedConverters = TypeConverters
                .UnionBy(converters, x => x.Type)
                .ToDictionary(x => x.Type, x => x);
        }
    }
}
