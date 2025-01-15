namespace Commands.Conditions;

internal sealed class DelegateExecuteCondition<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
TEvaluator>(Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>> func) : ExecuteCondition<TEvaluator>
    where TEvaluator : ConditionEvaluator, new()
{
    /// <inheritdoc />
    public override ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken)
        => func(caller, command, services);
}

/// <inheritdoc />
public abstract class ExecuteCondition<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
# endif
TEvaluator> : ExecuteCondition
    where TEvaluator : ConditionEvaluator, new()
{
    /// <inheritdoc />
#if NET8_0_OR_GREATER
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    public override Type EvaluatorType { get; } = typeof(TEvaluator);
}

/// <summary>
///     Represents a condition that determines whether a command can execute or not.
/// </summary>
public abstract class ExecuteCondition : IExecuteCondition
{
    /// <inheritdoc />
#if NET8_0_OR_GREATER
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    public abstract Type EvaluatorType { get; }

    /// <inheritdoc />
    public abstract ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken);

    /// <inheritdoc />
    public ConditionResult Error(string error)
    {
        Assert.NotNullOrEmpty(error, nameof(error));

        return ConditionResult.FromError(new ConditionException(this, error));
    }

    /// <inheritdoc />
    public ConditionResult Success()
        => ConditionResult.FromSuccess();

    public static ExecuteConditionProperties<T> From<T>()
        where T : ConditionEvaluator, new()
        => new();

    public static ExecuteConditionProperties<T> From<T>(Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>> executionDelegate)
        where T : ConditionEvaluator, new()
        => new ExecuteConditionProperties<T>().Delegate(executionDelegate);

    public static ExecuteConditionProperties From(ExecuteCondition condition)
        => new(condition);
}