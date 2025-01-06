using Commands.Conversion;

namespace Commands.Builders;

/// <summary>
///     A builder model for a component configuration. This class cannot be inherited.
/// </summary>
public sealed class ComponentConfigurationBuilder : IConfigurationBuilder
{
    /// <inheritdoc />
    public IDictionary<Type, TypeParser> Parsers { get; set; } = new Dictionary<Type, TypeParser>();

    /// <inheritdoc />
    public IDictionary<object, object> Properties { get; set; } = new Dictionary<object, object>();

    /// <inheritdoc />
    public IConfigurationBuilder AddParser<TConvertable>(Func<ICallerContext, ICommandParameter, object?, IServiceProvider, ValueTask<ParseResult>> parseAction)
    {
        Assert.NotNull(parseAction, nameof(parseAction));

        var parser = new DelegateParser<TConvertable>(parseAction);

        Parsers[parser.Type] = parser;

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
    public IConfigurationBuilder AddParsers(params TypeParser[] parsers)
    {
        foreach (var parser in parsers)
        {
            Assert.NotNull(parser, nameof(parser));

            Parsers[parser.Type] = parser;
        }

        return this;
    }

    /// <inheritdoc />
    public IConfigurationBuilder WithParsers(params TypeParser[] parsers)
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
    public ComponentConfiguration Build()
        => new(Parsers.Values, Properties);
}
