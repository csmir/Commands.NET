namespace Commands.Http;

/// <summary>
///     Represents a command module for handling HTTP commands, providing methods to respond with various HTTP responses.
/// </summary>
/// <typeparam name="T">The type implementing <see cref="HttpCommandContext"/> implemented by this module.</typeparam>
public abstract class HttpCommandModule<T> : CommandModule<T>
    where T : HttpCommandContext
{
    /// <summary>
    ///     Serializes the provided object to JSON and returns it as an HTTP response with the specified status code.
    /// </summary>
    /// <remarks>
    ///     When using AOT, this method isn't implicitly supported.
    /// </remarks>
    /// <typeparam name="TObject">The type of the object to serialize.</typeparam>
    /// <param name="content">The response to send</param>
    /// <param name="statusCode">The status code of the response.</param>
    /// <param name="serializerOptions">Additional options for serializing this response.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    [UnconditionalSuppressMessage("AOT", "IL3050")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "End user can define custom JsonSerializerContext that has the required TypeInfo for the target type.")]
    public virtual HttpResponse Json<TObject>(TObject content, HttpStatusCode statusCode = HttpStatusCode.OK, JsonSerializerOptions? serializerOptions = null) 
        => new(statusCode, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(content, typeof(TObject), serializerOptions)), "application/json");

    /// <summary>
    ///     Creates a new HTTP response with the status code 204 No Content and an optional reason phrase.
    /// </summary>
    /// <param name="reasonPhrase">The reason phrase containing information about the status code, if necessary.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public virtual HttpResponse NoContent(string? reasonPhrase = null) 
        => new(HttpStatusCode.NoContent, reasonPhrase);

    /// <summary>
    ///     Creates a new HTTP response with the status code 200 OK and an optional reason phrase.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public virtual HttpResponse Ok(string content, string contentType = "text/plain")
        => new(HttpStatusCode.OK, Encoding.UTF8.GetBytes(content), contentType);

    /// <summary>
    ///     Creates a new HTTP response with the status code 200 OK and the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public virtual HttpResponse Ok(byte[] content, string contentType) 
        => new(HttpStatusCode.OK, content, contentType);

    /// <summary>
    ///     Creates a new HTTP response with the status code 400 Bad Request and an optional reason phrase.
    /// </summary>
    /// <param name="reasonPhrase">The reason phrase containing information about the status code, if necessary.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public virtual HttpResponse BadRequest(string? reasonPhrase = null)
        => new(HttpStatusCode.BadRequest, reasonPhrase);

    /// <summary>
    ///     Creates a new HTTP response with the status code 400 Bad Request and the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public virtual HttpResponse BadRequest(string content, string contentType = "text/plain")
        => new(HttpStatusCode.BadRequest, Encoding.UTF8.GetBytes(content), contentType);

    /// <summary>
    ///     Creates a new HTTP response with the status code 400 Bad Request and the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public virtual HttpResponse BadRequest(byte[] content, string contentType)
        => new(HttpStatusCode.BadRequest, content, contentType);

    /// <summary>
    ///     Creates a new HTTP response with the status code 404 Not Found and an optional reason phrase.
    /// </summary>
    /// <param name="reasonPhrase">The reason phrase containing information about the status code, if necessary.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public virtual HttpResponse NotFound(string? reasonPhrase = null)
        => new(HttpStatusCode.NotFound, reasonPhrase);

    /// <summary>
    ///     Creates a new HTTP response with the status code 401 Unauthorized and an optional reason phrase.
    /// </summary>
    /// <param name="reasonPhrase">The reason phrase containing information about the status code, if necessary.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public virtual HttpResponse Unauthorized(string? reasonPhrase = null)
        => new(HttpStatusCode.Unauthorized, reasonPhrase);

    /// <summary>
    ///     Creates a new HTTP response with the status code 403 Forbidden and an optional reason phrase.
    /// </summary>
    /// <param name="reasonPhrase">The reason phrase containing information about the status code, if necessary.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public virtual HttpResponse Forbidden(string? reasonPhrase = null)
        => new(HttpStatusCode.Forbidden, reasonPhrase);

    /// <summary>
    ///     Creates a new HTTP response with the status code 500 Internal Server Error and an optional reason phrase.
    /// </summary>
    /// <param name="reasonPhrase">The reason phrase containing information about the status code, if necessary.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public virtual HttpResponse InternalServerError(string? reasonPhrase = null)
        => new(HttpStatusCode.InternalServerError, reasonPhrase);

    /// <summary>
    ///     Creates a new HTTP response with the status code 500 Internal Server Error and the specified content and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public virtual HttpResponse InternalServerError(string content, string contentType = "text/plain")
        => new(HttpStatusCode.InternalServerError, Encoding.UTF8.GetBytes(content), contentType);

    /// <summary>
    ///     Creates a new HTTP response with the status code 500 Internal Server Error and the specified content as a byte array and content type.
    /// </summary>
    /// <param name="content">The content to respond to the caller with.</param>
    /// <param name="contentType">The type of the content to respond to the caller with.</param>
    /// <returns>A new instance of <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</returns>
    public virtual HttpResponse InternalServerError(byte[] content, string contentType)
        => new(HttpStatusCode.InternalServerError, content, contentType);

    /// <summary>
    ///     Sets a response header for the HTTP response. This method allows you to add custom headers to the response, which can be useful for metadata or additional information.
    /// </summary>
    /// <param name="name">The name of the header.</param>
    /// <param name="value">The value of the header.</param>
    public virtual void SetResponseHeader(string name, object value)
        => Context.SetResponseHeader(name, value);

    /// <summary>
    ///     Sets the HTTP response based on the provided <see cref="HttpResponse"/> object. This method will set the status code, status description, content type, and content of the response.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</param>
    public virtual void SetResponse(HttpResponse response)
        => Context.SetResponse(response);

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
    ///     Responds to the HTTP request with the specified <see cref="HttpResponse"/> object, closing and disposing the used resources. 
    /// </summary>
    /// <remarks>
    ///     Additional calls to this method after the response has been sent will throw an exception.
    /// </remarks>
    /// <param name="response">The <see cref="HttpResponse"/> containing the values to be served to the caller invoking this operation.</param>
    /// <exception cref="InvalidOperationException">Thrown when the response has already been sent or closed.</exception>
    public virtual void Respond(HttpResponse response)
        => Context.Respond(response);
}
