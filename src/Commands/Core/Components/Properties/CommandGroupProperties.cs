namespace Commands;

public sealed class CommandGroupProperties
{
    public CommandGroupProperties()
    {

    }

    public CommandGroupProperties Name()
    {

    }

    public CommandGroupProperties Names()
    {

    }

    public CommandGroupProperties Condition()
    {

    }

    public CommandGroupProperties Conditions()
    {

    }

    public CommandGroup ToComponent()
    {

    }

    public static implicit operator CommandGroup(CommandGroupProperties properties)
        => properties.ToComponent();
}
