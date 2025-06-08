using Commands.Conditions;
using System.ComponentModel;

namespace Commands.Http;

/// <summary>
///     An evaluator that checks if the HTTP method of a command matches the specified method condition.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class HttpMethodEvaluator : ConditionEvaluator
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpMethodEvaluator"/> class.
    /// </summary>
    public HttpMethodEvaluator() 
        => MaximumAllowedConditions = 1; // Only one HTTP method condition can exist for a command at a time.

    /// <inheritdoc />
    public override ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken) 
        => Conditions[0].Evaluate(context, command, services, cancellationToken);
}
