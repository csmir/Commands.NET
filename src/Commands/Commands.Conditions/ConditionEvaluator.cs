namespace Commands.Conditions;

/// <summary>
///     An evaluator that contains a set of conditions grouped by the evaluator type. This evaluator determines if the contained set of conditions is met.
/// </summary>
public abstract class ConditionEvaluator : IEvaluator
{
    /// <summary>
    ///     Defines that this handler should be executed last in the command execution pipeline, after all other handlers have been executed.
    /// </summary>
    public const int ExecuteLast = int.MaxValue;

    /// <summary>
    ///     Defines that this handler should be executed first in the command execution pipeline, before all other handlers.
    /// </summary>
    public const int ExecuteFirst = int.MinValue;

    /// <summary>
    ///     Gets the order in which this evaluator should be executed relative to other evaluators.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    ///     Gets how many conditions can be evaluated within the same evaluator. When not specified, there is no limit.
    /// </summary>
    public int? MaximumAllowedConditions { get; set; }

    /// <summary>
    ///     Gets or sets the conditions that are being evaluated.
    /// </summary>
    public ExecuteConditionAttribute[] Conditions { get; set; } = null!;

    /// <summary>+
    ///     Determines if the contained set of conditions is met.
    /// </summary>
    /// <param name="context">The context of the current scope.</param>
    /// <param name="command">The command currently being targetted for execution.</param>
    /// <param name="services">The provider used to register modules and inject services.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
    public abstract ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken);
}
