namespace Commands;

/// <summary>
///     An evaluator that contains a set of conditions based on OR operating logic, returning succesfully if any of the conditions are met. This class cannot be inherited.
/// </summary>
public sealed class OREvaluator : ConditionEvaluator
{
    /// <inheritdoc />
    public override async ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        var lastFailure = default(ConditionResult);

        foreach (var condition in Conditions)
        {
            var result = await condition.Evaluate(caller, command, services, cancellationToken);

            if (result.Success)
                return result;

            lastFailure = result;
        }

        return lastFailure;
    }
}
