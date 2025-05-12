namespace Commands;

/// <summary>
///     A default implementation of <see cref="ICallerContext"/> that writes responses to the console.
/// </summary>
public class ConsoleCallerContext : ICallerContext
{
    /// <inheritdoc />
    public Arguments Arguments { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="ConsoleCallerContext"/> with the specified input.
    /// </summary>
    /// <remarks>
    ///      This input will be used to search for, match, parse, evaluate and run the target method for the resolved command.
    /// </remarks>
    /// <param name="input">A raw string which will be parsed into a set of arguments.</param>
    public ConsoleCallerContext(string? input)
        => Arguments = new(input);

    /// <summary>
    ///     Creates a new instance of <see cref="ConsoleCallerContext"/> with the specified input.
    /// </summary>
    /// <remarks>
    ///     This input will be used to search for, match, parse, evaluate and run the target method for the resolved command.
    /// </remarks>
    /// <param name="input">The CLI arguments passed to the application upon entry.</param>
    public ConsoleCallerContext(string[] input)
        => Arguments = new(input);

    /// <summary>
    ///     Sends a response to the console.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public virtual void Respond(object? message)
    {
        if (message is IEnumerable enumerable and not string)
        {
            foreach (var item in enumerable)
                Console.WriteLine(item);
        }
        else
        {
            Console.WriteLine(message);
        }
    }
}
