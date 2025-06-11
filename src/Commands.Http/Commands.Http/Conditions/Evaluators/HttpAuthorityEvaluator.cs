using Commands.Conditions;

namespace Commands.Http;

/// <summary>
///     An evaluator that checks if the authority (host and port) of an HTTP request matches the specified condition.
/// </summary>
[Priority(-2)] // Run this evaluator before self-defined attributes and method evaluator.
public sealed class HttpAuthorityEvaluator : OREvaluator
{
}
