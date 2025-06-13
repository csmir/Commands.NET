using Commands.Conditions;

namespace Commands.Http;

/// <summary>
///     An evaluator that checks if the HTTP method of a command matches the specified method condition.
/// </summary>
public sealed class HttpMethodEvaluator : OREvaluator
{
    /// <summary>
    ///     Creates a new instance of the <see cref="HttpMethodEvaluator"/> class.
    /// </summary>
    public HttpMethodEvaluator()
    {
        // Set the order to run this evaluator before self-defined attributes and method evaluator.
        Order = -2;

        // Set the maximum allowed conditions to 1, as this evaluator is typically used to check a single method.
        MaximumAllowedConditions = 1;
    }
}