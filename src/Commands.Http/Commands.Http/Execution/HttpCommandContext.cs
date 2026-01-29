
using System.Security.Principal;

namespace Commands.Http;

/// <summary>
///     Represents a context for handling HTTP commands, providing access to the HTTP request and response, as well as command arguments.
/// </summary>
public class HttpCommandContext : IResourceContext
{
    private bool _closed;

    private readonly HttpListenerResponse _response;
    private readonly IServiceProvider _services;

    /// <summary>
    ///     Gets the user associated with the HTTP request, if available. This can be used for authentication and authorization purposes.
    /// </summary>
    public IPrincipal? User { get; }

    /// <summary>
    ///     Gets the HTTP request associated with this command context.
    /// </summary>
    public HttpListenerRequest Request { get; }

    /// <summary>
    ///     Gets the HTTP response associated with this command context, allowing you to set headers, status codes, and content to respond to the request.
    /// </summary>
    /// <exception cref="InvalidOperationException">The HTTP response has already been sent, and cannot be accessed.</exception>
    public HttpListenerResponse Response => _closed
        ? throw new InvalidOperationException("The HTTP response has already been sent, and cannot be accessed.")
        : _response;

    /// <inheritdoc />
    public Arguments Arguments { get; }

    /// <summary>
    ///     A dictionary for storing arbitrary items related to this command context.
    /// </summary>
    public IDictionary<string, object?> Elements { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpCommandContext"/> class with the specified HTTP context and prefix length.
    /// </summary>
    /// <param name="httpContext">The context for which this command context exists.</param>
    /// <param name="services">Dependency injection services to resolve additional services for this context as needed.</param>
    public HttpCommandContext(HttpListenerContext httpContext, IServiceProvider services)
    {
        Request = httpContext.Request;
        _response = httpContext.Response;
        User = httpContext.User;

        _services = services;

        var arguments = new List<string>();
        var rootPath = Request.Url!.AbsolutePath[1..];

        while (true)
        {
            var indexOf = rootPath.IndexOf('/');

            if (indexOf == -1)
            {
                arguments.Add(rootPath);
                break;
            }
            else
            {
                arguments.Add(rootPath[..indexOf]);

                rootPath = rootPath[(indexOf + 1)..];

                if (string.IsNullOrEmpty(rootPath))
                    break;
            }
        }

        // Make this a variable as the getter of QueryString doesn't store the produced value.
        var queryString = Request.QueryString;

        var arg = new KeyValuePair<string, object?>[arguments.Count + queryString.Count];

        for (var i = 0; i < arguments.Count; i++)
            arg[i] = new(arguments[i], null);

        for (var i = 0; i < queryString.Count; i++)
            arg[arguments.Count + i] = new(queryString.GetKey(i)!, queryString.Get(i));

        Arguments = new(arg);
        Elements = new Dictionary<string, object?>();
    }

    /// <summary>
    ///     Responds to the HTTP request with the specified <see cref="HttpResult"/> object.
    /// </summary>
    /// <remarks>
    ///     Additional calls to this method after the response has been sent will throw an exception.
    /// </remarks>
    /// <param name="result">The result to send to the caller.</param>
    /// <exception cref="InvalidOperationException">Thrown if the HTTP response has already been sent.</exception>
    [UnconditionalSuppressMessage("AOT", "IL3050")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "End user can define custom JsonSerializerContext that has the required TypeInfo for the target type.")]
    public virtual void Respond(IHttpResult result)
    {
        if (_closed)
            throw new InvalidOperationException("The HTTP response has already been sent.");

        ArgumentNullException.ThrowIfNull(result);

        Response.StatusCode = (int)result.StatusCode;
        Response.StatusDescription = result.StatusCode.ToString();
        
        foreach (var (key, value) in result.Headers)
            Response.AddHeader(key, value);

        if (result.Content is not null)
        {
            Response.ContentType = result.ContentType ?? "text/plain";
            Response.ContentEncoding = result.ContentEncoding ?? Encoding.UTF8;

            if (result.Content is byte[] bytes)
            {
                Response.ContentLength64 = bytes.LongLength;

                using var outputStream = Response.OutputStream;
                outputStream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                string serializationResult;

                if (result.Content is Tuple<Type, object> objectReferredByT)
                    serializationResult = JsonSerializer.Serialize(objectReferredByT.Item2, objectReferredByT.Item1, _services.GetService<JsonSerializerOptions>());
                else
                    serializationResult = JsonSerializer.Serialize(result.Content, result.Content.GetType(), _services.GetService<JsonSerializerOptions>());

                var jsonBytes = Response.ContentEncoding.GetBytes(serializationResult);

                Response.ContentLength64 = jsonBytes.Length;

                using var outputStream = Response.OutputStream;
                outputStream.Write(jsonBytes, 0, jsonBytes.Length);
            }
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
        if (message is IHttpResult httpResult)
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