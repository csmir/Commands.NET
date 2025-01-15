namespace Commands.Parsing;

public sealed class TypeParserProperties
{
    public TypeParserProperties()
    {

    }

    public TypeParserProperties Delegate()
    {

    }

    public TypeParser ToParser()
    {

    }

    public static implicit operator TypeParser(TypeParserProperties properties)
        => properties.ToParser();
}
