namespace Commands;

/// <summary>
///     A default implementation of <see cref="ICallerContext"/> that writes responses to the console.
/// </summary>
public class DefaultCallerContext : ICallerContext
{
    /// <summary>
    ///     Sends a response to the console.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public void Respond(object? message)
        => Console.WriteLine(message);
}
