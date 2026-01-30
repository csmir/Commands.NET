namespace Commands.Http;

/// <summary>
///     A type containing constants for common HTTP header names.
/// </summary>
public static class HttpHeaderNames
{
    /// <summary>
    ///     The "Content-Type" HTTP header.
    /// </summary>
    public const string ContentType = "Content-Type";

    /// <summary>
    ///     The "Content-Length" HTTP header.
    /// </summary>
    public const string ContentLength = "Content-Length";

    /// <summary>
    ///     The "Authorization" HTTP header.
    /// </summary>
    public const string Authorization = "Authorization";

    /// <summary>
    ///     The "Accept" HTTP header.
    /// </summary>
    public const string Accept = "Accept";

    /// <summary>
    ///     A custom HTTP header that Commands.Http can use to convey error descriptions in development scenarios.  
    /// </summary>
    /// <remarks>
    ///     This header is included on erroneous responses when <see cref="IHostEnvironment"/> is in Development mode.
    /// </remarks>
    public const string LibraryErrorDescription = "CNET-Error-Description";

    /// <summary>
    ///     A custom HTTP header that Commands.Http can use to convey error origins in development scenarios.
    /// </summary>
    /// <remarks>
    ///     This header is included on erroneous responses when <see cref="IHostEnvironment"/> is in Development mode.
    /// </remarks>
    public const string LibraryErrorOrigin = "CNET-Error-Origin";
}

/// <summary>
///     Represents the response of an HTTP request, containing the status code, content, and content type.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class HttpResult : IHttpResult
{
    /// <inheritdoc />
    public HttpStatusCode StatusCode { get; }

    /// <inheritdoc />
    public object? Content { get; set; }

    /// <inheritdoc />
    public string? ContentType { get; set; }

    /// <inheritdoc />
    public Encoding? ContentEncoding { get; set; }

    /// <inheritdoc />
    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpResult"/> class with the specified status code.
    /// </summary>
    /// <param name="code">The numeric status code of this result. Converted to a value of <see cref="HttpStatusCode"/> on initialization.</param>
    public HttpResult(int code)
        : this((HttpStatusCode)code)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpResult"/> class with the specified status code.
    /// </summary>
    /// <param name="code">The status code of this result.</param>
    public HttpResult(HttpStatusCode code)
        => StatusCode = code;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpResult"/> class with the specified status code, content, and content type.
    /// </summary>
    /// <param name="code">The status code of this result. Converted to a value of <see cref="HttpStatusCode"/> on initialization.</param>
    /// <param name="content">The content of this result.</param>
    /// <param name="contentType">The content type of this result.</param>
    public HttpResult(int code, [DisallowNull] object content, string? contentType = null)
        : this((HttpStatusCode)code, content, contentType)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpResult"/> class with the specified status code, content, and content type.
    /// </summary>
    /// <param name="code">The status code of this result.</param>
    /// <param name="content">The content of this result.</param>
    /// <param name="contentType">The content type of this result.</param>
    public HttpResult(HttpStatusCode code, [DisallowNull] object content, string? contentType = null)
    {
        ArgumentNullException.ThrowIfNull(content);

        Content = content;
        ContentType = contentType;

        StatusCode = code;
    }

    /// <summary>
    ///     Gets a string representation of this HTTP response, including the status code and description.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => $"Status = ({(int)StatusCode}) {StatusCode}";

    /// <summary>
    ///     Implicitly converts an <see cref="HttpResult"/> to a <see cref="Task{HttpResult}"/> for asynchronous handling.
    /// </summary>
    /// <param name="result">The result to wrap in a task.</param>
    public static implicit operator Task<HttpResult>(HttpResult result)
        => Task.FromResult(result);

    /// <summary>
    ///     Implicitly converts an <see cref="HttpStatusCode"/> to an <see cref="HttpResult"/> with the status code set.
    /// </summary>
    /// <param name="code"></param>
    public static implicit operator HttpResult(HttpStatusCode code)
        => new(code);
}

/// <summary>
///     A specialized <see cref="HttpResult"/> that represents a JSON response.
/// </summary>
public sealed class JsonHttpResult<T> : HttpResult
{
    /// <summary>
    ///     Serializes the provided object to JSON and returns it as an HTTP response with the specified status code.
    /// </summary>
    /// <remarks>
    ///     When using Native-AOT, this method isn't implicitly supported. A custom <see cref="JsonSerializerContext"/> needs to be written with support of the provided type(s).
    /// </remarks>
    /// <param name="content">The response to send.</param>
    /// <param name="statusCode">The status code of the response.</param>
    public JsonHttpResult([DisallowNull] T content, HttpStatusCode statusCode = HttpStatusCode.OK)
        : base(statusCode, new Tuple<Type, object>(typeof(T), content), "application/json")
    {

    }
}