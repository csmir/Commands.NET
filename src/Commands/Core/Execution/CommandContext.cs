namespace Commands;

/// <summary>
///     Represents data on a command, serving as a parameterized variant of the state held in a <see cref="CommandModule"/>. This class cannot be inherited.
/// </summary>
/// <remarks>
///     This context is used for <see langword="static"/> and <see langword="delegate"/> commands. 
///     By marking it as the first parameter of any of these command types, the <see cref="IComponentTree"/> will automatically inject the context into the method.
/// </remarks>
public class CommandContext<T>(T consumer, CommandInfo command, IComponentTree tree, CommandOptions options)
    where T : ICallerContext
{
    /// <summary>
    ///     Gets the consumer of the command currently in scope.
    /// </summary>
    public T Caller { get; } = consumer;

    /// <summary>
    ///     Gets the options for the command currently in scope.
    /// </summary>
    public CommandOptions Options { get; } = options;

    /// <summary>
    ///     Gets the reflection information about the command currently in scope.
    /// </summary>
    public CommandInfo Command { get; } = command;

    /// <summary>
    ///     Gets the command manager responsible for executing the current pipeline.
    /// </summary>
    public IComponentTree Tree { get; } = tree;

    /// <summary>
    ///     Sends a response to the consumer of the command.
    /// </summary>
    /// <param name="response">The response to send to the consumer.</param>
    /// <returns>An awaitable <see cref="Task"/> containing the state of the response.</returns>
    public Task Respond(object? response)
        => Caller.Respond(response);
}
