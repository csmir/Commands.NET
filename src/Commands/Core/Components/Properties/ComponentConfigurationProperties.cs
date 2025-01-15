using Commands.Parsing;

namespace Commands;

public sealed class ComponentConfigurationProperties
{
    private readonly Dictionary<Type, TypeParserProperties> _parsers;
    private readonly Dictionary<object, object> _properties;

    public static ComponentConfigurationProperties Default { get; } = new();

    public ComponentConfigurationProperties()
    {
        _parsers = [];
        _properties = [];
    }

    public ComponentConfigurationProperties Parser(TypeParserProperties parser)
    {
        Assert.NotNull(parser, nameof(parser));

        _parsers[parser.GetParserType()] = parser;

        return this;
    }

    public ComponentConfigurationProperties Parsers(params TypeParserProperties[] parsers)
    {
        foreach (var parser in parsers)
            Parser(parser);

        return this;
    }

    public ComponentConfigurationProperties Property(object key, object value)
    {
        Assert.NotNull(key, nameof(key));

        _properties[key] = value;

        return this;
    }

    public ComponentConfigurationProperties Properties(params KeyValuePair<object, object>[] properties)
    {
        foreach (var property in properties)
            Property(property.Key, property.Value);

        return this;
    }

    public ComponentConfiguration ToConfiguration()
    {
        var baseParsers = TypeParser.CreateDefaults().ToDictionary(x => x.Type);

        foreach (var kvp in _parsers)
            baseParsers[kvp.Key] = kvp.Value.ToParser();

        return new ComponentConfiguration(baseParsers, _properties);
    }
}
