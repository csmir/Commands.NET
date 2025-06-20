using Commands.Conditions;

namespace Commands.Http;

/// <summary>
///     An evaluator that checks if the authority (host and port) of an HTTP request matches the specified condition.
/// </summary>
public sealed class HttpAuthorityEvaluator : OREvaluator
{
    /// <summary>
    ///     Creates a new instance of the <see cref="HttpAuthorityEvaluator"/> class.
    /// </summary>
    public HttpAuthorityEvaluator()
    {
        Order = ExecuteFirst;
    }
}
