using System.Runtime.CompilerServices;

namespace Commands.Conditions;

/// <summary>
///     An evaluator that contains a set of conditions grouped by the evaluator type. This evaluator determines if the contained set of conditions is met.
/// </summary>
public abstract class ConditionEvaluator
{
    /// <summary>
    ///     Gets how many conditions can be evaluated within the same evaluator. When not specified, there is no limit.
    /// </summary>
    public virtual int? MaximumAllowedConditions { get; }

    /// <summary>
    ///     Gets the order in which this evaluator should be executed relative to other evaluators.
    /// </summary>
    public virtual int Order { get; } = 0;

    /// <summary>
    ///     Gets or sets the conditions that are being evaluated.
    /// </summary>
    public ICondition[] Conditions { get; set; } = [];

    /// <summary>+
    ///     Determines if the contained set of conditions is met.
    /// </summary>
    /// <param name="context">The context of the current scope.</param>
    /// <param name="command">The command currently being targetted for execution.</param>
    /// <param name="services">The provider used to register modules and inject services.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
    public abstract ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken);

    #region Internals

    internal static ConditionEvaluator[] CreateEvaluators(IEnumerable<ICondition> conditions)
    {
        static IEnumerable<ConditionEvaluator> YieldEvaluators(IEnumerable<IGrouping<string, ICondition>> groups)
        {
            foreach (var group in groups)
            {
                var groupArr = group.ToArray();

                var evaluator = Unsafe.As<IInternalCondition>(groupArr[0]).CreateEvaluator();

                if (evaluator.MaximumAllowedConditions.HasValue && groupArr.Length > evaluator.MaximumAllowedConditions.Value)
                    throw new ComponentFormatException($"The evaluator {evaluator.GetType()} specifies that only {evaluator.MaximumAllowedConditions.Value} conditions of its scope are permitted per signature, but it discovered {groupArr.Length} conditions.");

                evaluator.Conditions = groupArr;

                yield return evaluator;
            }
        }

        if (!conditions.Any())
            return [];

        var evaluatorGroups = conditions
            .GroupBy(x => Unsafe.As<IInternalCondition>(x).EvaluatorName);

        return [.. YieldEvaluators(evaluatorGroups).OrderBy(x => x.Order)];
    }

    #endregion
}
