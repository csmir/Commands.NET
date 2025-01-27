namespace Commands;

/// <summary>
///     A default implementation of <see cref="ICallerContext"/> that writes responses to the console.
/// </summary>
public class ConsoleContext : ICallerContext
{
    /// <inheritdoc />
    public ArgumentArray Arguments { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="ConsoleContext"/> with the specified input.
    /// </summary>
    /// <remarks>
    ///      This input will be used to search for, match, parse, evaluate and run the target method for the resolved command.
    /// </remarks>
    /// <param name="input">A raw string which will be parsed into a set of arguments.</param>
    public ConsoleContext(string? input)
        => Arguments = ArgumentArray.From(input);

    /// <summary>
    ///     Creates a new instance of <see cref="ConsoleContext"/> with the specified input.
    /// </summary>
    /// <remarks>
    ///     This input will be used to search for, match, parse, evaluate and run the target method for the resolved command.
    /// </remarks>
    /// <param name="input">The CLI arguments passed to the application upon entry.</param>
    public ConsoleContext(string[] input)
        => Arguments = ArgumentArray.From(input);

    /// <summary>
    ///     Sends a response to the console.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public virtual void Respond(object? message)
        => Console.WriteLine(message);
}
