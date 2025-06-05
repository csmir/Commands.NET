namespace Commands.Http;

/// <summary>
///     Represents a context for handling HTTP commands, providing access to the HTTP request and response, as well as command arguments.
/// </summary>
public class HttpCommandContext : IContext
{
    private bool _closed;

    private readonly HttpListenerContext _httpContext;

    /// <summary>
    ///     Gets the HTTP request associated with this command context.
    /// </summary>
    public HttpListenerRequest Request => _httpContext.Request;

    /// <summary>
    ///     Gets the arguments provided to the command for which this context was created.
    /// </summary>
    public Arguments Arguments { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpCommandContext"/> class with the specified HTTP context and prefix length.
    /// </summary>
    /// <param name="httpContext">The context for which this command context exists.</param>
    /// <param name="acquiredPrefixLength">The prefix which represents the host URL that isn't handled by the command pipeline.</param>
    public HttpCommandContext(HttpListenerContext httpContext, int acquiredPrefixLength)
    {
        _httpContext = httpContext;

        var url = _httpContext.Request.RawUrl!.Split('?');

        var rawArg = url[0][acquiredPrefixLength..].Split('/');
        var rawQuery = url.Length > 1 ? url[1].Split('&') : [];

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
    ///     Responds to the HTTP request with the specified <see cref="HttpResponse"/> object.
    /// </summary>
    /// <remarks>
    ///     Additional calls to this method after the response has been sent will throw an exception.
    /// </remarks>
    /// <param name="result">The result to send to the caller.</param>
    /// <exception cref="InvalidOperationException">Thrown if the HTTP response has already been sent.</exception>
    public virtual void Respond(HttpResponse result)
    {
        SetResponse(result);

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

        _httpContext.Response.Close();
        _closed = true;
    }

    /// <summary>
    ///     Sets the HTTP response based on the provided <see cref="HttpResponse"/> object. This method will set the status code, status description, content type, and content of the response.
    /// </summary>
    /// <param name="result">The result to send to the caller.</param>
    public virtual void SetResponse(HttpResponse result)
    {
        Assert.NotNull(result, nameof(result));

        var resp = _httpContext.Response;

        resp.StatusCode = (int)result.StatusCode;
        resp.StatusDescription = result.StatusDescription ?? string.Empty;

        if (result.Content != null)
        {
            resp.ContentType = result.ContentType ?? "text/plain";
            resp.ContentLength64 = result.Content.LongLength;
            resp.ContentEncoding = result.ContentEncoding;

            using var outputStream = resp.OutputStream;
            outputStream.Write(result.Content, 0, result.Content.Length);
        }
    }

    /// <summary>
    ///     Sets a response header for the HTTP response. This method allows you to add custom headers to the response, which can be useful for metadata or additional information.
    /// </summary>
    /// <param name="name">The name of the header.</param>
    /// <param name="value">The value of the header.</param>
    public virtual void SetResponseHeader(string name, object value)
    {
        Assert.NotNullOrEmpty(name, nameof(name));
        Assert.NotNull(value, nameof(value));

        _httpContext.Response.Headers[name] = value is string str
            ? str
            : value.ToString();
    }

    /// <summary>
    ///     Gets the body of the HTTP request as a stream and deserializes it into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    ///     When using this method, ensure that the request body is a valid JSON representation of the type <typeparamref name="T"/>. If the body is empty, it will return the default value for <typeparamref name="T"/>.
    ///     <br/>
    ///     When using Native-AOT compilation, ensure that the type <typeparamref name="T"/> is registered in a <see cref="JsonSerializerContext"/> with the required TypeInfo for serialization and deserialization to work correctly.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown when the body is not a valid json, and could not be interpreted as an instance of <typeparamref name="T"/>.</exception>
    [UnconditionalSuppressMessage("AOT", "IL3050")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "End user can define custom JsonSerializerContext that has the required TypeInfo for the target type.")]
    public T? GetJsonBody<T>(JsonSerializerOptions? options = null)
    {
        Request.InputStream.Position = 0;

        using var reader = new StreamReader(Request.InputStream, Request.ContentEncoding);

        var body = reader.ReadToEnd();

        if (string.IsNullOrEmpty(body))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(body, options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to deserialize the request body.", ex);
        }
    }

    void IContext.Respond(object? message)
    {
        if (message is HttpResponse httpResult)
            Respond(httpResult);
        else
            throw new NotSupportedException($"The provided message type '{message?.GetType()}' is not supported for HTTP responses. Use {nameof(HttpResponse)} instead.");
    }
}
