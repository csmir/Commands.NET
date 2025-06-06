namespace Commands.Http;

/// <summary>
///     An interface that represents the result of an HTTP operation, encapsulating the response details.
/// </summary>
public interface IHttpResult
{
    /// <summary>
    ///     Gets the HTTP status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    ///     Gets the content of the response, if any.
    /// </summary>
    /// <remarks>
    ///     If this property is set, <see cref="ContentType"/> should also be set. If not, it defaults to "text/plain". The content should be a byte array representing the response body, such as JSON or plain text.
    /// </remarks>
    public byte[]? Content { get; set; }

    /// <summary>
    ///     Gets the content type of the response, if any.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    ///     Gets the encoding of the content, if any.
    /// </summary>
    /// <remarks>
    ///     This is the encoding by which <see cref="Content"/> is encoded. If anything other than UTF-8 is used, it should be specified here. If not set, it defaults to UTF-8.
    /// </remarks>
    public Encoding? ContentEncoding { get; set; }

}
