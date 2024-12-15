using Commands.Reflection;

namespace Commands.Conditions
{
    /// <summary>
    ///     An evaluator that contains a set of conditions based on OR operating logic, returning succesfully if any of the conditions are met. This class cannot be inherited.
    /// </summary>
    public sealed class OREvaluator : ConditionEvaluator
    {
        /// <inheritdoc />
        public override async ValueTask<ConditionResult> Evaluate(CallerContext consumer, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken)
        {
            var lastFailure = default(ConditionResult);

            foreach (var condition in Conditions)
            {
                var result = await condition.Evaluate(consumer, command, trigger, services, cancellationToken);

                if (result.Success)
                    return ConditionResult.FromSuccess(trigger);

                lastFailure = result;
            }

            return ConditionResult.FromError(trigger, lastFailure.Exception!);
        }
    }
}
