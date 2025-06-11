using Commands.Conditions;

namespace Commands.Http;

/// <summary>
///     Represents an exception that is thrown when an HTTP condition fails to evaluate.
/// </summary>
public sealed class HttpConditionException : ConditionException
{
    /// <summary>
    ///     Gets the response that will be sent if this exception reaches the end of the pipeline.
    /// </summary>
    public HttpResult? Response { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="HttpConditionException"/> with the specified response, which will be sent if this exception reaches to the end of the pipeline.
    /// </summary>
    /// <param name="condition">The condition that failed.</param>
    /// <param name="response">The response which will be sent if this exception reaches to the end of the pipeline.</param>
    public HttpConditionException(ICondition condition, HttpResult response)
        : base(condition, "An error occurred during evaluation of an execution condition.")
        => Response = response;
}
