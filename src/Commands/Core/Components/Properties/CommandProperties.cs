using Commands.Conditions;

namespace Commands;

public sealed class CommandProperties
{
    public CommandProperties()
    {

    }

    public CommandProperties Name(string name)
    {

    }

    public CommandProperties Names(params string[] names)
    {

    }

    public CommandProperties Condition(ExecuteCondition condition)
    {

    }

    public CommandProperties Conditions(params ExecuteCondition[] conditions)
    {

    }

    public CommandProperties Delegate(Delegate executionDelegate)
    {

    }

    public Command ToComponent()
    {

    }

    public static implicit operator Command(CommandProperties properties)
        => properties.ToComponent();
}
