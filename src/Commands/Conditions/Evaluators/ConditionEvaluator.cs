namespace Commands.Conditions;

/// <summary>
///     An evaluator that contains a set of conditions grouped by the evaluator type. This evaluator determines if the contained set of conditions is met.
/// </summary>
public abstract class ConditionEvaluator
{
    /// <summary>
    ///     Gets or sets the conditions that are being evaluated.
    /// </summary>
    public IExecuteCondition[] Conditions { get; set; } = [];

    /// <summary>
    ///     Determines if the contained set of conditions is met.
    /// </summary>
    /// <param name="caller">The caller of the current scope.</param>
    /// <param name="command">The command currently being targetted for execution.</param>
    /// <param name="services">The provider used to register modules and inject services.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
    public abstract ValueTask<ConditionResult> Evaluate(
        ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken);

    internal static IEnumerable<ConditionEvaluator> CreateEvaluators(IEnumerable<IExecuteCondition> conditions)
    {
        if (!conditions.Any())
            yield break;

        // Group conditions by their evaluator implementation.
        foreach (var conditionTypeGroup in conditions.GroupBy(x => x.EvaluatorType))
        {
            // We would prefer to take the group Key as the evaluator type, but thanks to DynamicallyAccessedMembers not being present on IGrouping.Key, we cannot.
            var instance = (ConditionEvaluator)Activator.CreateInstance(conditionTypeGroup.First().EvaluatorType)!;

            instance.Conditions = [.. conditionTypeGroup];

            yield return instance;
        }
    }
}
