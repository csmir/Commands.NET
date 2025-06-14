namespace Commands.Conditions;

/// <summary>
///     Represents an attribute that defines a condition to be evaluated during command execution, using <typeparamref name="TEvaluator"/> as the evaluator implementation.
/// </summary>
/// <typeparam name="TEvaluator">The evaluator which will be used to </typeparam>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Delegate, AllowMultiple = true)]
public abstract class ExecuteConditionAttribute<TEvaluator>() : ExecuteConditionAttribute(typeof(TEvaluator))
    where TEvaluator : IEvaluator, new()
{
    internal override IEvaluator CreateEvaluator()
        => new TEvaluator();
}

/// <summary>
///     Represents an attribute that defines a condition to be evaluated during command execution.
/// </summary>
/// <param name="evaluatorType">The type of <see cref="IEvaluator"/> by which the current condition should be evaluated.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Delegate, AllowMultiple = true)]
public abstract class ExecuteConditionAttribute(
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    Type evaluatorType) : Attribute
{
    internal string EvaluatorName { get; } = evaluatorType.FullName ?? evaluatorType.Name;

    /// <summary>
    ///     Evaluates the provided state during execution to determine if the command method can be run or not.
    /// </summary>
    /// <remarks>
    ///     Make use of <see cref="Error(string)"/> and <see cref="Success"/> to safely create the intended result.
    /// </remarks>
    /// <param name="context">The context of the current execution.</param>
    /// <param name="command">The command currently being executed.</param>
    /// <param name="services">The provider used to register modules and inject services.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
    public abstract ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a <see cref="ConditionResult"/> that indicates an erroneous evaluation of the condition.
    /// </summary>
    /// <param name="error">The error by which this condition failed.</param>
    /// <returns>A new <see cref="ConditionResult"/> holding a failed evaluation result.</returns>
    public virtual ConditionResult Error(string error)
    {
        Assert.NotNullOrEmpty(error, nameof(error));
        return ConditionResult.FromError(new ConditionException(error));
    }

    /// <summary>
    ///     Creates a <see cref="ConditionResult"/> that indicates a successful evaluation of the condition.
    /// </summary>
    /// <returns>A new <see cref="ConditionResult"/> holding a successful evaluation result.</returns>
    public virtual ConditionResult Success()
        => ConditionResult.FromSuccess();

    internal virtual IEvaluator CreateEvaluator()
    {
        if (!typeof(IEvaluator).IsAssignableFrom(evaluatorType))
            throw new ArgumentException($"The provided type '{evaluatorType.FullName}' does not implement {nameof(IEvaluator)}.", nameof(evaluatorType));
    ConditionEvaluator ICondition.CreateEvaluator() 
        => new TEvaluator();
    ConditionEvaluator ICondition.CreateEvaluator() 
        => new TEvaluator();

        return (IEvaluator)Activator.CreateInstance(evaluatorType)!;
    }
}