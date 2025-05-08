namespace Commands.Conditions;

/// <summary>
///     A set of properties of an execute condition.
/// </summary>
/// <typeparam name="T">The evaluator type this condition should be evaluated by.</typeparam>
public sealed class ExecuteConditionBuilder<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
T> : IExecuteConditionBuilder
    where T : ConditionEvaluator, new()
{
    private Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>>? _delegate;

    /// <summary>
    ///     Creates a new <see cref="ExecuteConditionBuilder{T}"/> instance.
    /// </summary>
    public ExecuteConditionBuilder()
    {
        _delegate = null;
    }

    /// <summary>
    ///     Sets the delegate that will be executed when the condition is evaluated.
    /// </summary>
    /// <param name="executionDelegate">The delegate to set.</param>
    /// <returns>The same <see cref="ExecuteConditionBuilder{T}"/> for call-chaining.</returns>
    public ExecuteConditionBuilder<T> Delegate(Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>> executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _delegate = executionDelegate;

        return this;
    }

    /// <inheritdoc />
    public ExecuteCondition Build()
    {
        Assert.NotNull(_delegate, nameof(_delegate));

        return new DelegateExecuteCondition<T>(_delegate!);
    }
}

internal readonly struct ExecuteConditionBuilder : IExecuteConditionBuilder
{
    private readonly ExecuteCondition _condition;

    internal ExecuteConditionBuilder(ExecuteCondition condition)
    {
        _condition = condition;
    }

    public ExecuteCondition Build()
        => _condition;
}