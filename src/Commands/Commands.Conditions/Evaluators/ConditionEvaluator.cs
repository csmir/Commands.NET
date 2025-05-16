namespace Commands.Conditions;

/// <summary>
///     An evaluator that contains a set of conditions grouped by the evaluator type. This evaluator determines if the contained set of conditions is met.
/// </summary>
public abstract class ConditionEvaluator
{
    /// <summary>
    ///     Gets or sets the conditions that are being evaluated.
    /// </summary>
    public ICondition[] Conditions { get; set; } = [];

    /// <summary>
    ///     Determines if the contained set of conditions is met.
    /// </summary>
    /// <param name="context">The context of the current scope.</param>
    /// <param name="command">The command currently being targetted for execution.</param>
    /// <param name="services">The provider used to register modules and inject services.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
    public abstract ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken);

    internal static ConditionEvaluator[] CreateEvaluators(IEnumerable<ICondition> conditions)
    {
        if (!conditions.Any())
            return [];

        var evaluatorGroups = conditions.GroupBy(x => x.EvaluatorType);

        var arr = new ConditionEvaluator[evaluatorGroups.Count()];

        var i = 0;

        foreach (var group in evaluatorGroups)
        {
            // Unable to access group.Key in this context as IGrouping does not have dynamic access to required type info.
            // Rather, it does, but the compiler doesn't know it does.

#if NET8_0_OR_GREATER
            var instance = (ConditionEvaluator)Activator.CreateInstance(group.First().EvaluatorType)!;
#else
            var instance = (ConditionEvaluator)Activator.CreateInstance(group.Key);
#endif
            instance.Conditions = [.. group];
            arr[i++] = instance;
        }

        return arr;
    }
}
