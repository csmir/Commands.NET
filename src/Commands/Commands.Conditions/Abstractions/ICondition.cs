using System.ComponentModel;

namespace Commands.Conditions;

/// <summary>
///     A condition that is evaluated within the command execution pipeline, determining if a command can succeed or not.
/// </summary>
public interface ICondition
{
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
    public ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken);

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

/// <inheritdoc/>
internal interface IInternalCondition : ICondition
{
    /// <summary>
    ///     Gets the name of the evaluator type for this condition.
    /// </summary>
    public string EvaluatorName { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="ConditionEvaluator"/> that is used to evaluate this condition.
    /// </summary>
    /// <returns>A new instance implementation of <see cref="ConditionEvaluator"/> that will be used to evaluate the given condition for a given command.</returns>
    public ConditionEvaluator CreateEvaluator();
}