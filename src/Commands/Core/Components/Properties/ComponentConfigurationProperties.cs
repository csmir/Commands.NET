using Commands.Parsing;

namespace Commands;

public sealed class ComponentConfigurationProperties
{
    public ComponentConfigurationProperties()
    {

    }

    public ComponentConfigurationProperties Parser(TypeParser parser)
    {

    }

    public ComponentConfigurationProperties Parsers(params TypeParser[] parsers)
    {

    }

    public ComponentConfiguration ToConfiguration()
    {

    }

    public static implicit operator ComponentConfiguration(ComponentConfigurationProperties properties)
        => properties.ToConfiguration();
}
