using System.ComponentModel;

namespace Commands.Conditions;

public sealed class ExecuteConditionProperties<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    T> : ExecuteConditionProperties
    where T : ConditionEvaluator, new()
{
    private Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>>? _delegate;

    public ExecuteConditionProperties()
        : base(null!) // Assign null as we do not use the underlying logic
    {
        _delegate = null;
    }

    public ExecuteConditionProperties<T> Delegate(Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>> executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _delegate = executionDelegate;

        return this;
    }

    public override ExecuteCondition ToCondition()
    {
        Assert.NotNull(_delegate, nameof(_delegate));

        return new DelegateExecuteCondition<T>(_delegate!);
    }
}

public class ExecuteConditionProperties
{
    private readonly ExecuteCondition _condition;

    internal ExecuteConditionProperties(ExecuteCondition condition)
    {
        _condition = condition;
    }

    public virtual ExecuteCondition ToCondition()
        => _condition;
}
