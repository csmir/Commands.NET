namespace Commands.Conditions;

/// <summary>
///     A set of properties of an execute condition.
/// </summary>
/// <typeparam name="T">The evaluator type this condition should be evaluated by.</typeparam>
public sealed class ExecuteConditionProperties<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
T> : IExecuteConditionProperties
    where T : ConditionEvaluator, new()
{
    private Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>>? _delegate;

    /// <summary>
    ///     Creates a new <see cref="ExecuteConditionProperties{T}"/> instance.
    /// </summary>
    public ExecuteConditionProperties()
    {
        _delegate = null;
    }

    /// <summary>
    ///     Sets the delegate that will be executed when the condition is evaluated.
    /// </summary>
    /// <param name="executionDelegate">The delegate to set.</param>
    /// <returns>The same <see cref="ExecuteConditionProperties{T}"/> for call-chaining.</returns>
    public ExecuteConditionProperties<T> Delegate(Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>> executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _delegate = executionDelegate;

        return this;
    }

    /// <inheritdoc />
    public ExecuteCondition Create()
    {
        Assert.NotNull(_delegate, nameof(_delegate));

        return new DelegateExecuteCondition<T>(_delegate!);
    }
}

internal readonly struct ExecuteConditionProperties : IExecuteConditionProperties
{
    private readonly ExecuteCondition _condition;

    internal ExecuteConditionProperties(ExecuteCondition condition)
    {
        _condition = condition;
    }

    public ExecuteCondition Create()
        => _condition;
}