using System.ComponentModel;

namespace Commands;

/// <summary>
///     A condition that is evaluated within the command execution pipeline, determining if a command can succeed or not.
/// </summary>
public interface ICondition
{
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
    ///     Gets the type of the evaluator implementation for a <see cref="ConditionAttribute{TEval}"/>.
    /// </summary>
    /// <returns>A type representing an implementation of <see cref="ConditionEvaluator"/>.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
#if NET8_0_OR_GREATER
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    public Type GetEvalType();
}