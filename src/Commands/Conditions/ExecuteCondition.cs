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

    /// <summary>
    ///     Creates a new <see cref="ExecuteCondition"/> from the provided delegate.
    /// </summary>
    /// <typeparam name="TEval">The evaluator type which will group this evaluation together with other evaluations of the same <typeparamref name="TEval"/> implementation.</typeparam>
    /// <param name="evaluationDelegate">The delegate which will run when this condition is requested during pre-execution evaluation of a command.</param>
    /// <returns>A new implementation of <see cref="ExecuteCondition"/> representing the created condition.</returns>
    public static ExecuteCondition Create<TEval>(Func<ICallerContext, Command, IServiceProvider, ValueTask<ConditionResult>> evaluationDelegate)
        where TEval : ConditionEvaluator, new()
        => new DelegateExecuteCondition<TEval>(evaluationDelegate);
}