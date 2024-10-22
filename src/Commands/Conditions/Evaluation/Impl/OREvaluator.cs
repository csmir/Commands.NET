using Commands.Reflection;

namespace Commands.Conditions
{
    /// <summary>
    ///     An evaluator that contains a set of conditions based on OR operating logic, returning succesfully if any of the conditions are met.
    /// </summary>
    public sealed class OREvaluator : ConditionEvaluator
    {
        /// <inheritdoc />
        public override async ValueTask<ConditionResult> Evaluate(ConsumerBase consumer, CommandInfo command, IServiceProvider services, CancellationToken cancellationToken)
        {
            var lastFailure = default(ConditionResult);

            foreach (var condition in Conditions)
            {
                var result = await condition.Evaluate(consumer, command, services, cancellationToken);

                if (result.Success)
                {
                    return ConditionResult.FromSuccess();
                }

                lastFailure = result;
            }

            return lastFailure;
        }
    }
}
