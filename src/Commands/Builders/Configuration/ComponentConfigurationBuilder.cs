using Commands.Conversion;
using System.Text.RegularExpressions;

namespace Commands.Builders
{
    /// <inheritdoc cref="IConfigurationBuilder"/>
    public class ComponentConfigurationBuilder : IConfigurationBuilder
    {
        const string DEFAULT_REGEX = @"^[a-z0-9_-]*$";

        /// <inheritdoc />
        public Regex? NamingPattern { get; set; } = new Regex(DEFAULT_REGEX, RegexOptions.Compiled);

        /// <inheritdoc />
        public Dictionary<Type, TypeConverter> TypeConverters { get; set; } = TypeConverter.GetStandardTypeConverters();

        /// <inheritdoc />
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        /// <inheritdoc />
        public IConfigurationBuilder AddTypeConverter<TConvertable>(Func<CallerContext, IArgument, object?, IServiceProvider, ConvertResult> convertAction)
        {
            if (convertAction == null)
                throw new ArgumentNullException(nameof(convertAction));

            var converter = new DelegateConverter<TConvertable>(convertAction);

            TypeConverters[converter.Type] = converter;

            return this;
        }

        /// <inheritdoc />
        public IConfigurationBuilder AddTypeConverter<TConvertable>(Func<CallerContext, IArgument, object?, IServiceProvider, ValueTask<ConvertResult>> convertAction)
        {
            if (convertAction == null)
                throw new ArgumentNullException(nameof(convertAction));

            var converter = new AsyncDelegateConverter<TConvertable>(convertAction);

            TypeConverters[converter.Type] = converter;

            return this;
        }

        /// <inheritdoc />
        public IConfigurationBuilder AddTypeConverter(TypeConverter converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            TypeConverters[converter.Type] = converter;

            return this;
        }

        /// <inheritdoc />
        public IConfigurationBuilder WithTypeConverters(params IEnumerable<TypeConverter> converters)
        {
            TypeConverters = converters
                .ToDictionary(x => x.Type, x => x);

            return this;
        }

        /// <inheritdoc />
        public IConfigurationBuilder AddTypeConverters(params IEnumerable<TypeConverter> converters)
        {
            foreach (var converter in converters)
                TypeConverters[converter.Type] = converter;

            return this;
        }

        /// <inheritdoc />
        public ComponentConfiguration Build()
            => new(TypeConverters, Properties, NamingPattern);
    }
}
