namespace Commands.Conditions
{
    /// <summary>
    ///     An evaluator that contains a set of conditions based on AND operating logic, returning succesfully if all of the conditions are met. This class cannot be inherited.
    /// </summary>
    public sealed class ANDEvaluator : ConditionEvaluator
    {
        /// <inheritdoc />
        public override async ValueTask<ConditionResult> Evaluate(CallerContext consumer, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken)
        {
            foreach (var condition in Conditions)
            {
                var result = await condition.Evaluate(consumer, command, trigger, services, cancellationToken);

                if (!result.Success)
                    return ConditionResult.FromError(trigger, result.Exception!);
            }

            return ConditionResult.FromSuccess(trigger);
        }
    }
}
