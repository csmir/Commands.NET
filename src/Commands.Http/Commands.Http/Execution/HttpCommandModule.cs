namespace Commands.Http;

/// <summary>
///     Represents a command module for handling HTTP commands, providing methods to respond with various HTTP responses.
/// </summary>
/// <typeparam name="T">The type implementing <see cref="HttpCommandContext"/> implemented by this module.</typeparam>
public abstract class HttpCommandModule<T> : CommandModule<T>
    where T : HttpCommandContext
{
    /// <summary>
    ///     Closes the HTTP response, indicating that no further data will be sent and sending the data subsequently. This method should be called after setting the response headers and content.
    /// </summary>
    /// <remarks>
    ///     Additional calls to this method after the response has been sent will throw an exception.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the response has already been sent or closed.</exception>
    public virtual void Respond()
        => Context.Respond();

    /// <summary>
    ///     Responds to the HTTP request with the specified <see cref="HttpResult"/> object, closing and disposing the used resources. 
    /// </summary>
    /// <remarks>
    ///     Additional calls to this method after the response has been sent will throw an exception.
    /// </remarks>
    /// <param name="response">The <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</param>
    /// <exception cref="InvalidOperationException">Thrown when the response has already been sent or closed.</exception>
    public virtual void Respond(IHttpResult response)
        => Context.Respond(response);

    /// <summary>
    ///     Serializes the provided object to JSON and returns it as an HTTP response with the specified status code.
    /// </summary>
    /// <remarks>
    ///     When using Native-AOT, this method isn't implicitly supported. A custom <see cref="JsonSerializerContext"/> needs to be written with support of the provided type(s).
    /// </remarks>
    /// <param name="content">The response to send.</param>
    /// <param name="statusCode">The status code of the response.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult Json<TObject>([DisallowNull] TObject content, HttpStatusCode statusCode = HttpStatusCode.OK)
        => new JsonHttpResult<TObject>(content, statusCode);

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
        => new(HttpStatusCode.OK, Encoding.UTF8.GetBytes(content), contentType);

    /// <summary>
    ///     Creates a new HTTP response as 200 OK, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult Ok(byte[] content, string contentType)
        => new(HttpStatusCode.OK, content, contentType);

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
        => new(HttpStatusCode.BadRequest, Encoding.UTF8.GetBytes(content), contentType);

    /// <summary>
    ///     Creates a new HTTP response as 400 Bad Request, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult BadRequest(byte[] content, string contentType)
        => new(HttpStatusCode.BadRequest, content, contentType);

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
        => new(HttpStatusCode.Unauthorized, Encoding.UTF8.GetBytes(content), contentType);

    /// <summary>
    ///     Creates a new HTTP response as 401 Unauthorized, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult Unauthorized(byte[] content, string contentType)
        => new(HttpStatusCode.Unauthorized, content, contentType);

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
        => new(HttpStatusCode.Forbidden, Encoding.UTF8.GetBytes(content), contentType);

    /// <summary>
    ///     Creates a new HTTP response as 403 Forbidden, specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult Forbidden(byte[] content, string contentType)
        => new(HttpStatusCode.Forbidden, content, contentType);

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
        => new(HttpStatusCode.NotFound, Encoding.UTF8.GetBytes(content), contentType);

    /// <summary>
    ///     Creates a new HTTP response as 404 Not Found, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult NotFound(byte[] content, string contentType)
        => new(HttpStatusCode.NotFound, content, contentType);

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
        => new(HttpStatusCode.InternalServerError, Encoding.UTF8.GetBytes(content), contentType);

    /// <summary>
    ///     Creates a new HTTP response as 500 Internal Server Error, the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResult"/> containing the values to be served to the caller invoking this operation.</returns>
    public static HttpResult InternalServerError(byte[] content, string contentType)
        => new(HttpStatusCode.InternalServerError, content, contentType);
}
