namespace Commands.Conditions;

/// <summary>
///     An evaluator that contains a set of conditions grouped by their name, evaluator type and trigger. This evaluator determines if the contained set of conditions is met.
/// </summary>
public abstract class ConditionEvaluator
{
    /// <summary>
    ///     Gets or sets the trigger that determines when the condition is evaluated.
    /// </summary>
    public ConditionTrigger Trigger { get; set; }

    /// <summary>
    ///     Gets or sets the conditions that are being evaluated.
    /// </summary>
    public ICondition[] Conditions { get; set; } = [];

    /// <summary>
    ///     Determines if the contained set of conditions is met.
    /// </summary>
    /// <param name="caller">The caller of the current scope.</param>
    /// <param name="command">The command currently being targetted for execution.</param>
    /// <param name="trigger">The trigger that determines when the condition is evaluated, being 1 or more points in the execution pipeline.</param>
    /// <param name="services">The provider used to register modules and inject services.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
    public abstract ValueTask<ConditionResult> Evaluate(
        ICallerContext caller, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken);

    internal static IEnumerable<ConditionEvaluator> CreateEvaluators(IEnumerable<ICondition> conditions)
    {
        foreach (var conditionTypeGroup in conditions.GroupBy(x => x.GetGroupId()))
        {
            var evaluator = conditionTypeGroup.First().GetEvaluator();

            evaluator.Conditions = [.. conditionTypeGroup];

            yield return evaluator;
        }
    }
}
