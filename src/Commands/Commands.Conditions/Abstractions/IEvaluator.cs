namespace Commands.Conditions;

/// <summary>
///     An evaluator that is used to evaluate a set of conditions within the command execution pipeline given the specified approach.
/// </summary>
public interface IEvaluator
{
    /// <summary>
    ///     Gets how many conditions can be evaluated within the same evaluator. When not specified, there is no limit.
    /// </summary>
    public int? MaximumAllowedConditions { get; set; }

    /// <summary>
    ///     Gets the order in which this evaluator should be executed relative to other evaluators.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    ///     Gets or sets the conditions that are being evaluated by the current evaluator.
    /// </summary>
    public ExecuteConditionAttribute[] Conditions { get; set; }

    /// <summary>
    ///     Determines if the contained set of conditions is met.
    /// </summary>
    /// <param name="context">The context of the current scope.</param>
    /// <param name="command">The command currently being targetted for execution.</param>
    /// <param name="services">The provider used to register modules and inject services.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
    public ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken);
}
