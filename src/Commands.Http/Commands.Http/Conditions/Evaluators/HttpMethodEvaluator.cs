using Commands.Conditions;

namespace Commands.Http;

// An evaluator for HTTP method conditions.
internal sealed class HttpMethodEvaluator : ConditionEvaluator
{
    public HttpMethodEvaluator() 
        => MaximumAllowedConditions = 1; // Only one HTTP method condition can exist for a command at a time.

    public override ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken) 
        => Conditions[0].Evaluate(context, command, services, cancellationToken);
}
