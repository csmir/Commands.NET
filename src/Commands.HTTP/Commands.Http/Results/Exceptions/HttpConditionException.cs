using Commands.Conditions;

namespace Commands.Http;

/// <summary>
///     Represents an exception that is thrown when an HTTP condition fails to evaluate.
/// </summary>
public sealed class HttpConditionException : ConditionException
{
    /// <summary>
    ///     Gets the HTTP status code that was returned when the condition failed to evaluate.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    ///     Gets the content type of the response that was returned when the condition failed to evaluate.
    /// </summary>
    public string? ContentType { get; }

    /// <summary>
    ///     Gets the content of the response that was returned when the condition failed to evaluate.
    /// </summary>
    public object? Content { get; }

    /// <summary>
    ///     /Creates a new instance of <see cref="HttpConditionException"/> with the specified condition, status code, content, and content type.
    /// </summary>
    /// <param name="condition">The condition that failed.</param>
    /// <param name="statusCode">The status code that represents the failure.</param>
    /// <param name="content">The content that should be sent back to the caller on failure.</param>
    /// <param name="contentType">The content type of <paramref name="content"/>.</param>
    public HttpConditionException(ICondition condition, HttpStatusCode statusCode, object? content, string? contentType = null)
        : base(condition, "An error occurred during evaluation of an execution condition.")
    {
        StatusCode = statusCode;
        ContentType = contentType;
        Content = content;
    }
}
