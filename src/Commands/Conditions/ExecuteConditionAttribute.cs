namespace Commands.Conditions;

/// <summary>
///     An attribute that contains an evaluation method called when marked on top of a command signature.
/// </summary>
/// <remarks>
///     The <see cref="Evaluate(ICallerContext, Command, IServiceProvider, CancellationToken)"/> method is responsible for doing this evaluation. 
///     Custom implementations of <see cref="ExecuteConditionAttribute{T}"/> can be placed at module or command level, with each being ran in top-down order when a target is checked. 
///     If multiple commands are found during matching, multiple sequences of preconditions will be ran to find a match that succeeds.
/// </remarks>
/// <typeparam name="TEvaluator">The type of evaluator that will be used to determine the result of the evaluation.</typeparam>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public abstract class ExecuteConditionAttribute<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
# endif
TEvaluator>() : Attribute, IExecuteCondition
    where TEvaluator : ConditionEvaluator, new()
{
    /// <inheritdoc />
#if NET8_0_OR_GREATER
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    public Type EvaluatorType { get; } = typeof(TEvaluator);

    /// <inheritdoc />
    /// <remarks>
    ///     Make use of <see cref="Error(string)"/> and <see cref="Success"/> to safely create the intended result.
    /// </remarks>
    public abstract ValueTask<ConditionResult> Evaluate(
        ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken);

    /// <inheritdoc />
    public ConditionResult Error(string error)
    {
        Assert.NotNullOrEmpty(error, nameof(error));

        return ConditionResult.FromError(new ConditionException(this, error));
    }

    /// <inheritdoc />
    public ConditionResult Success()
        => ConditionResult.FromSuccess();
}
