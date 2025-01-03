using Commands.Conversion;

namespace Commands.Builders;

/// <summary>
///     A builder model for a component configuration. This class cannot be inherited.
/// </summary>
public sealed class ComponentConfigurationBuilder : IConfigurationBuilder
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
    public IConfigurationBuilder AddParser<TConvertable>(Func<ICallerContext, ICommandParameter, object?, IServiceProvider, ValueTask<ParseResult>> parseAction)
    {
        Assert.NotNull(parseAction, nameof(parseAction));

        var converter = new DelegateParser<TConvertable>(parseAction);

        Parsers[converter.Type] = converter;

        return this;
    }

    /// <inheritdoc />
    public IConfigurationBuilder AddParser(TypeParser parser)
    {
        Assert.NotNull(parser, nameof(parser));

        Parsers[parser.Type] = parser;

        return this;
    }

    /// <inheritdoc />
    public IConfigurationBuilder WithParsers(params IEnumerable<TypeParser> parsers)
    {
        Parsers = parsers
            .ToDictionary(x => x.Type, parser =>
            {
                Assert.NotNull(parser, nameof(parser));
                return parser;
            });

        return this;
    }

    /// <inheritdoc />
    public IConfigurationBuilder AddParsers(params IEnumerable<TypeParser> parsers)
    {
        foreach (var converter in parsers)
        {
            Assert.NotNull(converter, nameof(converter));

            Parsers[converter.Type] = converter;
        }

        return this;
    }

    /// <inheritdoc />
    public ComponentConfiguration Build()
        => new(Parsers, Properties);
}
