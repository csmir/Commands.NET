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
    public virtual void Respond(HttpResult response)
        => Context.Respond(response);
}
