﻿namespace Commands.Conditions;

/// <summary>
///     An evaluator that contains a set of conditions based on OR operating logic, returning succesfully if any of the conditions are met. This class cannot be inherited.
/// </summary>
public sealed class OREvaluator : ConditionEvaluator
{
    /// <inheritdoc />
    public override async ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        var lastFailure = default(ConditionResult);

        foreach (var condition in Conditions)
        {
            var result = await condition.Evaluate(context, command, services, cancellationToken).ConfigureAwait(false);

            if (result.Success)
                return result;

            lastFailure = result;
        }

        return lastFailure;
    }
}
