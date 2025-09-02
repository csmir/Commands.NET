
namespace Commands.Http;

/// <summary>
///     An interface that represents the result of an HTTP operation, encapsulating the response details.
/// </summary>
public interface IHttpResult
{
    /// <summary>
    ///     Gets the HTTP status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    ///     Gets the content of the response, if any.
    /// </summary>
    public object? Content { get;  }

    /// <summary>
    ///     Gets the content type of the response, if any.
    /// </summary>
    /// <remarks>
    ///     This is the MIME type of the <see cref="Content"/>, such as "application/json" or "text/plain". If not set, it defaults to "text/plain". 
    /// </remarks>
    public string? ContentType { get; }

    /// <summary>
    ///     Gets the encoding of the content, if any.
    /// </summary>
    /// <remarks>
    ///     This is the encoding by which <see cref="Content"/> is encoded. If anything other than UTF-8 is used, it should be specified here. If not set, it defaults to UTF-8.
    /// </remarks>
    public Encoding? ContentEncoding { get; }

    /// <summary>
    ///     A collection of headers to include in the HTTP response. Append this dictionary with any headers you want to include in the response.
    /// </summary>
    IReadOnlyDictionary<string, string> Headers { get; }
}
