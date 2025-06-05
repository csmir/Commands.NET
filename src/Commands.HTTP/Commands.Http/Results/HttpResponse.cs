namespace Commands.Http;

/// <summary>
///     Represents the response of an HTTP request, containing the status code, content, and content type.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class HttpResponse
{
    /// <summary>
    ///     Gets the HTTP status code of the response, such as 200 OK, 404 Not Found, etc.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    ///     Gets the reason phrase or description of the status code, if any.
    /// </summary>
    public string? StatusDescription { get; set; }

    /// <summary>
    ///     Gets the content type of the response, if any.
    /// </summary>
    /// <remarks>
    ///     This is typically a MIME type such as "application/json" or "text/html".
    /// </remarks>
    public string? ContentType { get; set; }

    /// <summary>
    ///     Gets the encoding of the content, if any.
    /// </summary>
    /// <remarks>
    ///     When not provided, this defaults to UTF-8 encoding.
    /// </remarks>
    public Encoding? ContentEncoding { get; set; }

    /// <summary>
    ///     Gets the content of the response, if any.
    /// </summary>
    /// <remarks>
    ///      This is typically a byte array representing the body of the HTTP response.
    /// </remarks>
    public byte[]? Content { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpResponse"/> class with the specified status code and optional reason phrase.
    /// </summary>
    /// <param name="code">The status code of this result.</param>
    /// <param name="reasonPhrase">The phrase containing the description of the status code.</param>
    public HttpResponse(HttpStatusCode code, string? reasonPhrase = null)
    {
        StatusCode = code;
        StatusDescription = reasonPhrase ?? code.ToString();
    }

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
        StatusDescription = code.ToString();
    }

    /// <summary>
    ///     Gets a string representation of this HTTP response, including the status code and description.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => $"Status = {(int)StatusCode}; Description = {StatusDescription ?? string.Empty}";

    /// <summary>
    ///     Implicitly converts an <see cref="HttpResponse"/> to a <see cref="Task{HttpResult}"/> for asynchronous handling.
    /// </summary>
    /// <param name="result">The result to wrap in a task.</param>
    public static implicit operator Task<HttpResponse>(HttpResponse result) 
        => Task.FromResult(result);
}
