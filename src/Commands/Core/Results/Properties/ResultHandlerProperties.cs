namespace Commands;

public sealed class ResultHandlerProperties
{
    public ResultHandlerProperties()
    {

    }

    public ResultHandlerProperties Delegate()
    {

    }

    public ResultHandler ToHandler()
    {

    }

    public static implicit operator ResultHandler(ResultHandlerProperties properties)
        => properties.ToHandler();
}
