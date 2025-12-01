namespace Commands.Http;

/// <summary>
///     Represents the response of an HTTP request, containing the status code, content, and content type.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class HttpResult : IHttpResult
{
    private readonly Dictionary<string, string> _headers = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public HttpStatusCode StatusCode { get; }

    /// <inheritdoc />
    public string? ContentType { get; }

    /// <inheritdoc />
    public Encoding? ContentEncoding { get; }

    /// <inheritdoc />
    public object? Content { get; }

    /// <inheritdoc />
    /// <remarks>
    ///     Add headers to this collection by using the <see cref="WithHeader(string, string)"/> method.
    /// </remarks>
    public IReadOnlyDictionary<string, string> Headers => _headers;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpResult"/> class with a default status code of 204 No Content.
    /// </summary>
    public HttpResult()
        => StatusCode = HttpStatusCode.NoContent;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpResult"/> class with the specified status code.
    /// </summary>
    /// <param name="code">The status code of this result.</param>
    public HttpResult(HttpStatusCode code)
        => StatusCode = code;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpResult"/> class with the specified status code, content, and content type.
    /// </summary>
    /// <param name="code">The status code of this result.</param>
    /// <param name="content">The content of this result.</param>
    /// <param name="contentType">The content type of this result.</param>
    public HttpResult(HttpStatusCode code, object content, string? contentType = null)
    {
        Assert.NotNull(content, nameof(content));

        Content = content;
        ContentType = contentType;

        StatusCode = code;
    }

    /// <summary>
    ///     Adds a header to the HTTP response if it does not already exist, or replaces the value if it does.
    /// </summary>
    /// <param name="key">The key of the header to add. For example: "X-API-KEY"</param>
    /// <param name="value">The value of the header to add.</param>
    /// <returns>The same <see cref="HttpResult"/> for call-chaining.</returns>
    public HttpResult WithHeader(string key, string value)
    {
        Assert.NotNullOrEmpty(key, nameof(key));
        Assert.NotNull(value, nameof(value));

        _headers[key] = value;

        return this;
    }

    /// <summary>
    ///     Gets a string representation of this HTTP response, including the status code and description.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => $"Status = ({(int)StatusCode}) {StatusCode}";

    /// <summary>
    ///     Serializes the provided object to JSON and returns it as an HTTP response with the specified status code.
    /// </summary>
    /// <remarks>
    ///     When using Native-AOT, this method isn't implicitly supported. A custom <see cref="JsonSerializerContext"/> needs to be written with support of the provided type(s).
    /// </remarks>
    /// <param name="content">The response to send.</param>
    /// <param name="statusCode">The status code of the response.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult Json<T>([DisallowNull] T content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Assert.NotNull(content, nameof(content));

        return new(statusCode, new Tuple<Type, object>(typeof(T), content), "application/json");
    }

    /// <summary>
    ///     Creates a new HTTP response with the specified JSON content and status code.
    /// </summary>
    /// <param name="content">The response to send.</param>
    /// <param name="statusCode">The status code of the response.</param>
    /// <returns>A </returns>
    public static HttpResult Json([StringSyntax(StringSyntaxAttribute.Json)] string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Assert.NotNullOrEmpty(content, nameof(content));

        return new(statusCode, Encoding.UTF8.GetBytes(content), "application/json");
    }

    /// <summary>
    ///     Creates a new HTTP response as 204 No Content.
    /// </summary>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult NoContent()
        => new(HttpStatusCode.NoContent);

    /// <summary>
    ///     Creates a new HTTP response as 200 OK, the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult Ok(string content, string contentType = "text/plain")
    {
        Assert.NotNull(content, nameof(content));

        return new(HttpStatusCode.OK, Encoding.UTF8.GetBytes(content), contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 200 OK, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult Ok(byte[] content, string contentType)
    {
        Assert.NotNull(content, nameof(content));

        return new(HttpStatusCode.OK, content, contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 400 Bad Request.
    /// </summary>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult BadRequest()
        => new(HttpStatusCode.BadRequest);

    /// <summary>
    ///     Creates a new HTTP response as 400 Bad Request, the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult BadRequest(string content, string contentType = "text/plain")
    {
        Assert.NotNull(content, nameof(content));

        return new(HttpStatusCode.BadRequest, Encoding.UTF8.GetBytes(content), contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 400 Bad Request, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult BadRequest(byte[] content, string contentType)
    {
        Assert.NotNull(content, nameof(content));

        return new(HttpStatusCode.BadRequest, content, contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 401 Unauthorized.
    /// </summary>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult Unauthorized()
        => new(HttpStatusCode.Unauthorized);

    /// <summary>
    ///     Creates a new HTTP response as 401 Unauthorized, the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult Unauthorized(string content, string contentType = "text/plain")
    {
        Assert.NotNull(content, nameof(content));

        return new(HttpStatusCode.Unauthorized, Encoding.UTF8.GetBytes(content), contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 401 Unauthorized, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult Unauthorized(byte[] content, string contentType)
    {
        Assert.NotNull(content, nameof(content));

        return new(HttpStatusCode.Unauthorized, content, contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 403 Forbidden.
    /// </summary>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult Forbidden()
        => new(HttpStatusCode.Forbidden);

    /// <summary>
    ///     Creates a new HTTP response as 403 Forbidden, the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult Forbidden(string content, string contentType = "text/plain")
    {
        Assert.NotNull(content, nameof(content));

        return new(HttpStatusCode.Forbidden, Encoding.UTF8.GetBytes(content), contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 403 Forbidden, specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult Forbidden(byte[] content, string contentType)
    {
        Assert.NotNull(content, nameof(content));

        return new(HttpStatusCode.Forbidden, content, contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 404 Not Found.
    /// </summary>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult NotFound()
        => new(HttpStatusCode.NotFound);

    /// <summary>
    ///     Creates a new HTTP response as 404 Not Found, the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult NotFound(string content, string contentType = "text/plain")
    {
        Assert.NotNull(content, nameof(content));

        return new(HttpStatusCode.NotFound, Encoding.UTF8.GetBytes(content), contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 404 Not Found, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult NotFound(byte[] content, string contentType)
    {
        Assert.NotNull(content, nameof(content));

        return new(HttpStatusCode.NotFound, content, contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 500 Internal Server Error.
    /// </summary>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult InternalServerError()
        => new(HttpStatusCode.InternalServerError);

    /// <summary>
    ///     Creates a new HTTP response as 500 Internal Server Error, the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult InternalServerError(string content, string contentType = "text/plain")
    {
        Assert.NotNull(content, nameof(content));

        return new(HttpStatusCode.InternalServerError, Encoding.UTF8.GetBytes(content), contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 500 Internal Server Error, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult InternalServerError(byte[] content, string contentType)
    {
        Assert.NotNull(content, nameof(content));

        return new(HttpStatusCode.InternalServerError, content, contentType);
    }

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
