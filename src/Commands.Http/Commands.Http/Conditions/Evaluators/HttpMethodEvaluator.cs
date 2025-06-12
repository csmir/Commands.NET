using Commands.Conditions;

namespace Commands.Http;

/// <summary>
///     An evaluator that checks if the HTTP method of a command matches the specified method condition.
/// </summary>
public sealed class HttpMethodEvaluator : ConditionEvaluator
{
    /// <inheritdoc />
    public override int? MaximumAllowedConditions => 1; // Only one HTTP method condition can exist for a command at a time.

    /// <inheritdoc />
    public override int Order => -1;

    /// <inheritdoc />
    public override ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken)
        => Conditions[0].Evaluate(context, command, services, cancellationToken);
}