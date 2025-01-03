namespace Commands;

/// <summary>
///     Represents data on a command, serving as a parameterized variant of the state held in a <see cref="CommandModule"/>. This class cannot be inherited.
/// </summary>
/// <remarks>
///     This context is used for <see langword="static"/> and <see langword="delegate"/> commands. 
///     By marking it as the first parameter of any of these command types, the <see cref="IComponentTree"/> will automatically inject the context into the method.
/// </remarks>
public class CommandContext<T>(T caller, Command command, IComponentTree tree, CommandOptions options)
    where T : ICallerContext
{
    /// <summary>
    ///     Gets the caller that requested this command to be executed.
    /// </summary>
    public T Caller { get; } = caller;

    /// <summary>
    ///     Gets the options for the command currently in scope.
    /// </summary>
    public CommandOptions Options { get; } = options;

    /// <summary>
    ///     Gets the information about the command currently being executed.
    /// </summary>
    public Command Command { get; } = command;

    /// <summary>
    ///     Gets the command tree that is responsible for the current command execution.
    /// </summary>
    public IComponentTree Tree { get; } = tree;

    /// <summary>
    ///     Sends a response to the caller of the command.
    /// </summary>
    /// <param name="response">The response to send to the caller.</param>
    /// <returns>An awaitable <see cref="Task"/> containing the state of the response.</returns>
    public Task Respond(object? response)
    {
        if (Caller is IAsyncCallerContext asyncCaller)
            return asyncCaller.Respond(response);
        else
            Caller.Respond(response);

        return Task.CompletedTask;
    }
}
