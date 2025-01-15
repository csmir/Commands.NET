namespace Commands.Conditions;

public sealed class ExecuteConditionProperties
{
    public ExecuteConditionProperties()
    {

    }

    public ExecuteConditionProperties Delegate()
    {

    }

    public ExecuteCondition ToCondition()
    {

    }

    public static implicit operator ExecuteCondition(ExecuteConditionProperties properties)
        => properties.ToCondition();
}
