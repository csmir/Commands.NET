
namespace Commands.Http;

/// <summary>
///     Represents a context for handling HTTP commands, providing access to the HTTP request and response, as well as command arguments.
/// </summary>
public class HttpCommandContext : IResourceContext
{
    private bool _closed;

    private readonly HttpListenerContext _httpContext;

    /// <summary>
    ///     Gets the HTTP request associated with this command context.
    /// </summary>
    public HttpListenerRequest Request { get; }

    /// <summary>
    ///     Gets the HTTP response associated with this command context, allowing you to set headers, status codes, and content to respond to the request.
    /// </summary>
    public HttpListenerResponse Response { get; }

    /// <inheritdoc />
    public Arguments Arguments { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpCommandContext"/> class with the specified HTTP context and prefix length.
    /// </summary>
    /// <param name="httpContext">The context for which this command context exists.</param>
    public HttpCommandContext(HttpListenerContext httpContext)
    {
        Request = httpContext.Request;
        Response = httpContext.Response;

        _httpContext = httpContext;

        var rawArg = Request.Url!.AbsolutePath[1..].Split('/', StringSplitOptions.RemoveEmptyEntries);

        var rawQuery = !string.IsNullOrEmpty(Request.Url.Query) 
            ? Request.Url!.Query[1..].Split('&') 
            : [];

        var arg = new KeyValuePair<string, object?>[rawArg.Length + rawQuery.Length];

        for (var i = 0; i < rawArg.Length; i++)
            arg[i] = new(rawArg[i], null);

        for (var i = 0; i < rawQuery.Length; i++)
        {
            var entry = rawQuery[i];

            var indexOfEqual = entry.IndexOf('=');

            if (indexOfEqual == -1)
                arg[rawArg.Length + i] = new(entry, null);
            else
                arg[rawArg.Length + i] = new(entry[..indexOfEqual], entry[(indexOfEqual + 1)..]);
        }

        Arguments = new(arg);
    }

    /// <summary>
    ///     Responds to the HTTP request with the specified <see cref="HttpResult"/> object.
    /// </summary>
    /// <remarks>
    ///     Additional calls to this method after the response has been sent will throw an exception.
    /// </remarks>
    /// <param name="result">The result to send to the caller.</param>
    /// <exception cref="InvalidOperationException">Thrown if the HTTP response has already been sent.</exception>
    public virtual void Respond(HttpResult result)
    {
        Assert.NotNull(result, nameof(result));

        Response.StatusCode = (int)result.StatusCode;
        Response.StatusDescription = result.StatusCode.ToString();

        if (result.Content != null)
        {
            Response.ContentType = result.ContentType ?? "text/plain";
            Response.ContentLength64 = result.Content.LongLength;
            Response.ContentEncoding = result.ContentEncoding;

            using var outputStream = Response.OutputStream;
            outputStream.Write(result.Content, 0, result.Content.Length);
        }

        Respond();
    }

    /// <summary>
    ///     Closes the HTTP response, indicating that no further data will be sent and sending the data subsequently. This method should be called after setting the response headers and content.
    /// </summary>
    /// <remarks>
    ///     Additional calls to this method after the response has been sent will throw an exception.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the HTTP response has already been sent.</exception>
    public virtual void Respond()
    {
        if (_closed)
            throw new InvalidOperationException("The HTTP response has already been sent.");

        Response.Close();
        _closed = true;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"{Request.HttpMethod} {Request.Url?.AbsoluteUri}";

    void IContext.Respond(object? message)
    {
        if (message is HttpResult httpResult)
            Respond(httpResult);
        else
            Respond(HttpResult.Json(message!));
    }

    async ValueTask<object?> IResourceContext.GetResource()
    {
        using var reader = new StreamReader(Request.InputStream, Request.ContentEncoding);

        return await reader.ReadToEndAsync();
    }
}
