namespace Commands.Http;

/// <summary>
///     Represents the response of an HTTP request, containing the status code, content, and content type.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public struct HttpResponse : IHttpResult
{
    /// <inheritdoc />
    public HttpStatusCode StatusCode { get; set; }

    /// <inheritdoc />
    public string? ContentType { get; set; }

    /// <inheritdoc />
    public Encoding? ContentEncoding { get; set; }

    /// <inheritdoc />
    public byte[]? Content { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpResponse"/> class with a default status code of 204 No Content.
    /// </summary>
    public HttpResponse()
        => StatusCode = HttpStatusCode.NoContent;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpResponse"/> class with the specified status code.
    /// </summary>
    /// <param name="code">The status code of this result.</param>
    public HttpResponse(HttpStatusCode code)
        => StatusCode = code;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpResponse"/> class with the specified status code, content, and content type.
    /// </summary>
    /// <param name="code">The status code of this result.</param>
    /// <param name="content">The content of this result.</param>
    /// <param name="contentType">The content type of this result.</param>
    public HttpResponse(HttpStatusCode code, byte[] content, string? contentType = null)
    {
        Assert.NotNull(content, nameof(content));
        Assert.NotNullOrEmpty(contentType, nameof(contentType));

        Content = content;
        ContentType = contentType;
        ContentEncoding = Encoding.UTF8; // Default encoding is UTF-8

        StatusCode = code;
    }

    /// <summary>
    ///     Gets a string representation of this HTTP response, including the status code and description.
    /// </summary>
    /// <returns></returns>
    public override readonly string ToString()
        => $"Status = ({(int)StatusCode}) {StatusCode}";

    /// <summary>
    ///     Serializes the provided object to JSON and returns it as an HTTP response with the specified status code.
    /// </summary>
    /// <remarks>
    ///     When using Native-AOT, this method isn't implicitly supported. A custom <see cref="JsonSerializerContext"/> needs to be written with support of the provided type(s).
    /// </remarks>
    /// <typeparam name="TObject">The type of the object to serialize.</typeparam>
    /// <param name="content">The response to send</param>
    /// <param name="statusCode">The status code of the response.</param>
    /// <param name="serializerOptions">Additional options for serializing this response.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    [UnconditionalSuppressMessage("AOT", "IL3050")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "End user can define custom JsonSerializerContext that has the required TypeInfo for the target type.")]
    public static HttpResponse Json<TObject>(TObject content, HttpStatusCode statusCode = HttpStatusCode.OK, JsonSerializerOptions? serializerOptions = null)
    {
        Assert.NotNull(content, nameof(content));

        return new HttpResponse(statusCode, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(content, typeof(TObject), serializerOptions)), "application/json");
    }

    /// <summary>
    ///     Serializes the provided object to JSON and returns it as an HTTP response with the specified status code.
    /// </summary>
    /// <remarks>
    ///     When using Native-AOT, this method isn't implicitly supported. A custom <see cref="JsonSerializerContext"/> needs to be written with support of the provided type(s).
    /// </remarks>
    /// <param name="content">The response to send</param>
    /// <param name="statusCode">The status code of the response.</param>
    /// <param name="serializerOptions">Additional options for serializing this response.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    [UnconditionalSuppressMessage("AOT", "IL3050")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "End user can define custom JsonSerializerContext that has the required TypeInfo for the target type.")]
    public static HttpResponse Json(object content, HttpStatusCode statusCode = HttpStatusCode.OK, JsonSerializerOptions? serializerOptions = null)
    {
        Assert.NotNull(content, nameof(content));

        return new HttpResponse(statusCode, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(content, content.GetType(), serializerOptions)), "application/json");
    }

    /// <summary>
    ///     Creates a new HTTP response as 204 No Content.
    /// </summary>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse NoContent()
        => new(HttpStatusCode.NoContent);

    /// <summary>
    ///     Creates a new HTTP response as 200 OK, the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse Ok(string content, string contentType = "text/plain")
    {
        Assert.NotNull(content, nameof(content));
        Assert.NotNullOrEmpty(contentType, nameof(contentType));

        return new HttpResponse(HttpStatusCode.OK, Encoding.UTF8.GetBytes(content), contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 200 OK, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse Ok(byte[] content, string contentType)
    {
        Assert.NotNull(content, nameof(content));
        Assert.NotNullOrEmpty(contentType, nameof(contentType));

        return new HttpResponse(HttpStatusCode.OK, content, contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 400 Bad Request.
    /// </summary>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse BadRequest()
        => new(HttpStatusCode.BadRequest);

    /// <summary>
    ///     Creates a new HTTP response as 400 Bad Request, the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse BadRequest(string content, string contentType = "text/plain")
    {
        Assert.NotNull(content, nameof(content));
        Assert.NotNullOrEmpty(contentType, nameof(contentType));

        return new HttpResponse(HttpStatusCode.BadRequest, Encoding.UTF8.GetBytes(content), contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 400 Bad Request, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse BadRequest(byte[] content, string contentType)
    {
        Assert.NotNull(content, nameof(content));
        Assert.NotNullOrEmpty(contentType, nameof(contentType));

        return new HttpResponse(HttpStatusCode.BadRequest, content, contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 401 Unauthorized.
    /// </summary>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse Unauthorized()
        => new(HttpStatusCode.Unauthorized);

    /// <summary>
    ///     Creates a new HTTP response as 401 Unauthorized, the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse Unauthorized(string content, string contentType = "text/plain")
    {
        Assert.NotNull(content, nameof(content));
        Assert.NotNullOrEmpty(contentType, nameof(contentType));

        return new HttpResponse(HttpStatusCode.Unauthorized, Encoding.UTF8.GetBytes(content), contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 401 Unauthorized, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse Unauthorized(byte[] content, string contentType)
    {
        Assert.NotNull(content, nameof(content));
        Assert.NotNullOrEmpty(contentType, nameof(contentType));

        return new HttpResponse(HttpStatusCode.Unauthorized, content, contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 403 Forbidden.
    /// </summary>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse Forbidden()
        => new(HttpStatusCode.Forbidden);

    /// <summary>
    ///     Creates a new HTTP response as 403 Forbidden, the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse Forbidden(string content, string contentType = "text/plain")
    {
        Assert.NotNull(content, nameof(content));
        Assert.NotNullOrEmpty(contentType, nameof(contentType));

        return new HttpResponse(HttpStatusCode.Forbidden, Encoding.UTF8.GetBytes(content), contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 403 Forbidden, specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse Forbidden(byte[] content, string contentType)
    {
        Assert.NotNull(content, nameof(content));
        Assert.NotNullOrEmpty(contentType, nameof(contentType));

        return new HttpResponse(HttpStatusCode.Forbidden, content, contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 404 Not Found.
    /// </summary>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse NotFound()
        => new(HttpStatusCode.NotFound);

    /// <summary>
    ///     Creates a new HTTP response as 404 Not Found, the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse NotFound(string content, string contentType = "text/plain")
    {
        Assert.NotNull(content, nameof(content));
        Assert.NotNullOrEmpty(contentType, nameof(contentType));

        return new HttpResponse(HttpStatusCode.NotFound, Encoding.UTF8.GetBytes(content), contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 404 Not Found, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse NotFound(byte[] content, string contentType)
    {
        Assert.NotNull(content, nameof(content));
        Assert.NotNullOrEmpty(contentType, nameof(contentType));

        return new HttpResponse(HttpStatusCode.NotFound, content, contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 500 Internal Server Error.
    /// </summary>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse InternalServerError()
        => new(HttpStatusCode.InternalServerError);

    /// <summary>
    ///     Creates a new HTTP response as 500 Internal Server Error, the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse InternalServerError(string content, string contentType = "text/plain")
    {
        Assert.NotNull(content, nameof(content));
        Assert.NotNullOrEmpty(contentType, nameof(contentType));

        return new HttpResponse(HttpStatusCode.InternalServerError, Encoding.UTF8.GetBytes(content), contentType);
    }

    /// <summary>
    ///     Creates a new HTTP response as 500 Internal Server Error, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResponse InternalServerError(byte[] content, string contentType)
    {
        Assert.NotNull(content, nameof(content));
        Assert.NotNullOrEmpty(contentType, nameof(contentType));

        return new HttpResponse(HttpStatusCode.InternalServerError, content, contentType);
    }

    /// <summary>
    ///     Implicitly converts an <see cref="HttpResponse"/> to a <see cref="Task{HttpResult}"/> for asynchronous handling.
    /// </summary>
    /// <param name="result">The result to wrap in a task.</param>
    public static implicit operator Task<HttpResponse>(HttpResponse result)
        => Task.FromResult(result);
}
