namespace Commands.Conditions;

/// <summary>
///     An evaluator that contains a set of conditions based on AND operating logic, returning succesfully if all of the conditions are met. This class cannot be inherited.
/// </summary>
public sealed class ANDEvaluator : ConditionEvaluator
{
    /// <inheritdoc />
    public override async ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        foreach (var condition in Conditions)
        {
            var result = await condition.Evaluate(caller, command, services, cancellationToken).ConfigureAwait(false);

            if (!result.Success)
                return result;
        }

        return ConditionResult.FromSuccess();
    }
}