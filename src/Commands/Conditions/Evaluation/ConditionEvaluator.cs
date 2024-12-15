using Commands.Reflection;

namespace Commands.Conditions
{
    /// <summary>
    ///     An evaluator that containerizes a set of conditions, determining the result of the evaluation.
    /// </summary>
    public abstract class ConditionEvaluator()
    {
        /// <summary>
        ///     Gets or sets the trigger that determines when the condition is evaluated.
        /// </summary>
        public ConditionTrigger Trigger { get; set; }

        /// <summary>
        ///     Gets or sets the conditions that are being evaluated.
        /// </summary>
        public IExecuteCondition[] Conditions { get; set; } = [];

        /// <summary>
        ///     Evaluates the known data about a command at the pre or post stage in execution, in order to determine if command execution can succeed or not.
        /// </summary>
        /// <param name="consumer">The consumer of the current execution.</param>
        /// <param name="command">The result of the execution.</param>
        /// <param name="trigger">The trigger that determines when the condition is evaluated.</param>
        /// <param name="services">The provider used to register modules and inject services.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> that contains the result of the evaluation.</returns>
        public abstract ValueTask<ConditionResult> Evaluate(
            CallerContext consumer, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken);

        internal static IEnumerable<ConditionEvaluator> CreateEvaluators(IEnumerable<IExecuteCondition> conditions)
        {
            foreach (var conditionTypeGroup in conditions.GroupBy(x => x.GetGroupId()))
            {
                var evaluator = conditionTypeGroup.First().GetEvaluator();

                evaluator.Conditions = [.. conditionTypeGroup];

                yield return evaluator;
            }
        }
    }
}
