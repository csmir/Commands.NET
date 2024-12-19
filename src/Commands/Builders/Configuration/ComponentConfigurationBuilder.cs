using Commands.Conversion;

namespace Commands.Builders
{
    /// <inheritdoc cref="IConfigurationBuilder"/>
    public class ComponentConfigurationBuilder : IConfigurationBuilder
    {
        /// <inheritdoc />
        public IDictionary<Type, TypeParser> Parsers { get; set; } = TypeParser.CreateDefaults();

        /// <inheritdoc />
        public IDictionary<string, object?> Properties { get; set; } = new Dictionary<string, object?>();

        /// <inheritdoc />
        public IConfigurationBuilder AddParser<TConvertable>(Func<ICallerContext, IArgument, object?, IServiceProvider, ConvertResult> convertAction)
        {
            if (convertAction == null)
                throw new ArgumentNullException(nameof(convertAction));

            var converter = new DelegateConverter<TConvertable>(convertAction);

            Parsers[converter.Type] = converter;

            return this;
        }

        /// <inheritdoc />
        public IConfigurationBuilder AddParser<TConvertable>(Func<ICallerContext, IArgument, object?, IServiceProvider, ValueTask<ConvertResult>> convertAction)
        {
            if (convertAction == null)
                throw new ArgumentNullException(nameof(convertAction));

            var converter = new AsyncDelegateConverter<TConvertable>(convertAction);

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
