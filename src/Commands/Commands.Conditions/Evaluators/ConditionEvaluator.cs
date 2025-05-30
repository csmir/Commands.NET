﻿namespace Commands.Conditions;

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

#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("AotAnalysis", "IL2072", Justification = "IGrouping<>.Key is a value of ICondition.EvaluatorType, and is marked as dynamically accessible, allowing it to be accessible to the activator.")]
#endif
    internal static ConditionEvaluator[] CreateEvaluators(IEnumerable<ICondition> conditions)
    {
        if (!conditions.Any())
            return [];

        var evaluatorGroups = conditions.GroupBy(x => x.EvaluatorType);

        var arr = new ConditionEvaluator[evaluatorGroups.Count()];

        var i = 0;

        foreach (var group in evaluatorGroups)
        {
            var instance = (ConditionEvaluator)Activator.CreateInstance(group.Key)!;
            instance.Conditions = [.. group];
            arr[i++] = instance;
        }

        return arr;
    }
}
