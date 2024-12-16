using Commands.Converters;
using Commands.Reflection;
using System.Text.RegularExpressions;

namespace Commands
{
    /// <summary>
    ///     A builder class used to create a new instance of <see cref="BuildConfiguration"/>.
    /// </summary>
    public class BuildConfigurationBuilder
    {
        const string DEFAULT_REGEX = @"^[a-z0-9_-]*$";

        /// <summary>
        ///     Gets or sets the naming convention of commands and groups being registered into the <see cref="CommandTree"/>.
        /// </summary>
        public Regex? NamingPattern { get; set; } = new Regex(DEFAULT_REGEX, RegexOptions.Compiled);

        /// <summary>
        ///     Gets or sets if registered modules should be made read-only at creation, meaning they cannot be modified at runtime.
        /// </summary>
        public bool SealModuleDefinitions { get; set; } = false;

        /// <summary>
        ///     Gets or sets a collection of <see cref="TypeConverter"/>'s representing predefined <see cref="Type"/> conversion. Conversion of <see langword="object"/>, <see langword="string"/>, implementations of <see langword="enum"/> and implementations of <see cref="IEnumerable{T}"/> are not necessary to implement, as they are handled internally by the <see cref="CommandTree"/>.
        /// </summary>
        /// <remarks>
        ///     This collection is set by default to the value of <see cref="TypeConverter.GetStandardTypeConverters"/>. 
        ///     Mutation to existing converters or adding new converters can be done through the methods provided by this class.
        ///     By default, any new converters added will replace existing converters with the same <see cref="Type"/>. 
        ///     <br />
        ///     <br />
        ///     The list of types that are converted by the default converters are:
        ///     <list type="bullet">
        ///         <item>String characters, being: <see langword="char"/>.</item>
        ///         <item>Integers variants, being: <see langword="bool"/>, <see langword="byte"/>, <see langword="sbyte"/>, <see langword="short"/>, <see langword="ushort"/>, <see langword="int"/>, <see langword="uint"/>, <see langword="long"/> and <see langword="ulong"/>.</item>
        ///         <item>Floating point numbers, being: <see langword="float"/>, <see langword="decimal"/> and <see langword="double"/>.</item>
        ///         <item>Commonly used structs, being: <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="TimeSpan"/> and <see cref="Guid"/>.</item>
        ///     </list>
        ///     <i>The converter <see cref="TimeSpan"/> does not implement the standard <see cref="TimeSpan.TryParse(string, out TimeSpan)"/>, instead having a custom implementation under <see cref="TimeSpanTypeConverter"/>.</i>
        /// </remarks>
        public Dictionary<Type, TypeConverter> TypeConverters { get; set; } = TypeConverter.GetStandardTypeConverters();

        /// <summary>
        ///     Gets or sets a collection of properties that can be used to store additional information applied during the build process.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = [];

        /// <summary>
        ///     Configures an action that will convert a raw argument into the target type, signified by <typeparamref name="TConvertable"/>.
        /// </summary>
        /// <typeparam name="TConvertable">The type for this converter to target.</typeparam>
        /// <param name="convertAction">The action that is responsible for the conversion process.</param>
        /// <returns>The same <see cref="BuildConfigurationBuilder"/> for call-chaining.</returns>
        public BuildConfigurationBuilder AddTypeConverter<TConvertable>(Func<CallerContext, IArgument, object?, IServiceProvider, ConvertResult> convertAction)
        {
            if (convertAction == null)
                throw new ArgumentNullException(nameof(convertAction));

            var converter = new DelegateConverter<TConvertable>(convertAction);

            TypeConverters[converter.Type] = converter;

            return this;
        }

        /// <summary>
        ///     Configures an asynchronous action that will convert a raw argument into the target type, signified by <typeparamref name="TConvertable"/>.
        /// </summary>
        /// <typeparam name="TConvertable">The type for this converter to target.</typeparam>
        /// <param name="convertAction">The action that is responsible for the conversion process.</param>
        /// <returns>The same <see cref="BuildConfigurationBuilder"/> for call-chaining.</returns>
        public BuildConfigurationBuilder AddTypeConverter<TConvertable>(Func<CallerContext, IArgument, object?, IServiceProvider, ValueTask<ConvertResult>> convertAction)
        {
            if (convertAction == null)
                throw new ArgumentNullException(nameof(convertAction));

            var converter = new AsyncDelegateConverter<TConvertable>(convertAction);

            TypeConverters[converter.Type] = converter;

            return this;
        }

        /// <summary>
        ///     Adds an implementation of <see cref="TypeConverter"/> to <see cref="TypeConverters"/>.
        /// </summary>
        /// <typeparam name="TConverter">The implementation type of <see cref="TypeConverter"/> to add.</typeparam>
        /// <param name="converter">The implementation of <see cref="TypeConverter"/> to add.</param>
        /// <returns>The same <see cref="BuildConfigurationBuilder"/> for call-chaining.</returns>
        public BuildConfigurationBuilder AddTypeConverter<TConverter>(TConverter converter)
            where TConverter : TypeConverter
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            TypeConverters[converter.Type] = converter;

            return this;
        }

        /// <summary>
        ///     Replaces the current collection of type converters with the specified converters.
        /// </summary>
        /// <param name="converters">A collection of converters to replace the existing converters in <see cref="TypeConverters"/>.</param>
        /// <returns>The same <see cref="BuildConfigurationBuilder"/> for call-chaining.</returns>
        public BuildConfigurationBuilder WithTypeConverters(params IEnumerable<TypeConverter> converters)
        {
            TypeConverters = converters
                .ToDictionary(x => x.Type, x => x);

            return this;
        }

        /// <summary>
        ///     Adds a collection of <see cref="TypeConverter"/>'s to the current <see cref="TypeConverters"/>, replacing any existing converters with the same <see cref="Type"/>.
        /// </summary>
        /// <param name="converters">A collection of converters to add or replace in <see cref="TypeConverters"/>.</param>
        /// <returns>The same <see cref="BuildConfigurationBuilder"/> for call-chaining.</returns>
        public BuildConfigurationBuilder AddTypeConverters(params IEnumerable<TypeConverter> converters)
        {
            foreach (var converter in converters)
                TypeConverters[converter.Type] = converter;

            return this;
        }

        /// <summary>
        ///     Builds the current <see cref="BuildConfigurationBuilder"/> into a new instance of <see cref="BuildConfiguration"/>.
        /// </summary>
        /// <returns>A newly created instance of <see cref="BuildConfiguration"/></returns>
        public BuildConfiguration Build()
            => new(TypeConverters, Properties, NamingPattern, SealModuleDefinitions);
    }
}
