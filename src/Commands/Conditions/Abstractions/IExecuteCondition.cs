namespace Commands.Conditions;

/// <summary>
///     A condition that is evaluated within the command execution pipeline, determining if a command can succeed or not.
/// </summary>
public interface IExecuteCondition
{
    /// <summary>
    ///     Gets the type of the evaluator implementation for this condition.
    /// </summary>
#if NET8_0_OR_GREATER
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    public Type EvaluatorType { get; }

    /// <summary>
    ///     Evaluates the provided state during execution to determine if the command method can be run or not.
    /// </summary>
    /// <param name="caller">The caller of the current execution.</param>
    /// <param name="command">The command currently being executed.</param>
    /// <param name="services">The provider used to register modules and inject services.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
    public ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a new <see cref="ConditionResult"/> representing a failed evaluation.
    /// </summary>
    /// <param name="error">The error that caused the evaluation to fail.</param>
    /// <returns>A <see cref="ConditionResult"/> representing the failed evaluation.</returns>
    public ConditionResult Error(string error);

    /// <summary>
    ///     Creates a new <see cref="ConditionResult"/> representing a successful evaluation.
    /// </summary>
    /// <returns>A <see cref="ConditionResult"/> representing the successful evaluation.</returns>
    public ConditionResult Success();
}