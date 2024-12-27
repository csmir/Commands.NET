using Commands.Conversion;

namespace Commands.Builders
{
    /// <inheritdoc cref="IConfigurationBuilder"/>
    public class ComponentConfigurationBuilder : IConfigurationBuilder
    {
        /// <summary>
        ///     Gets the configuration builder that is used as a fallback when no configuration is provided. This builder is built every time <see cref="ComponentConfiguration.Default"/> is called.
        /// </summary>
        public static IConfigurationBuilder Default { get; } = new ComponentConfigurationBuilder();

        /// <inheritdoc />
        public IDictionary<Type, TypeParser> Parsers { get; set; } = TypeParser.CreateDefaults().ToDictionary(x => x.Type, x => x);

        /// <inheritdoc />
        public IDictionary<object, object> Properties { get; set; } = new Dictionary<object, object>();

        /// <inheritdoc />
        public IConfigurationBuilder AddParser<TConvertable>(Func<ICallerContext, IArgument, object?, IServiceProvider, Task<ConvertResult>> convertAction)
        {
            if (convertAction == null)
                throw new ArgumentNullException(nameof(convertAction));

            var converter = new DelegateParser<TConvertable>(convertAction);

            Parsers[converter.Type] = converter;

            return this;
        }

        /// <inheritdoc />
        public IConfigurationBuilder AddParser(TypeParser converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            Parsers[converter.Type] = converter;

            return this;
        }

        /// <inheritdoc />
        public IConfigurationBuilder WithParsers(params IEnumerable<TypeParser> converters)
        {
            Parsers = converters
                .ToDictionary(x => x.Type, x => x);

            return this;
        }

        /// <inheritdoc />
        public IConfigurationBuilder AddParsers(params IEnumerable<TypeParser> converters)
        {
            foreach (var converter in converters)
                Parsers[converter.Type] = converter;

            return this;
        }

        /// <inheritdoc />
        public ComponentConfiguration Build()
            => new(Parsers, Properties);
    }
}
