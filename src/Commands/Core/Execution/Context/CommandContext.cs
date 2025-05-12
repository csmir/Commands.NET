namespace Commands;

/// <summary>
///     Represents data on a command, serving as a parameterized variant of the state held in a <see cref="CommandModule"/>. This class cannot be inherited.
/// </summary>
/// <remarks>
///     This context is used for <see langword="static"/> and <see langword="delegate"/> commands. 
///     By marking it as the first parameter of any of these command types, the <see cref="IComponentProvider"/> will automatically inject the context into the method.
/// </remarks>
public class CommandContext<T>(T caller, Command command, ExecutionOptions options)
    where T : ICallerContext
{
    /// <summary>
    ///     Gets the caller that requested this command to be executed.
    /// </summary>
    public T Caller { get; } = caller;

    /// <summary>
    ///     Gets the options for the command currently in scope.
    /// </summary>
    public ExecutionOptions Options { get; } = options;

    /// <summary>
    ///     Gets the information about the command currently being executed.
    /// </summary>
    public Command Command { get; } = command;

    /// <summary>
    ///     Gets the <see cref="ComponentTree"/> that triggered the command, if this command was invoked from one.
    /// </summary>
    public IComponentProvider? Provider => Options.Provider;

    /// <summary>
    ///     Sends a response to the caller of the command.
    /// </summary>
    /// <param name="message">A message to send to the caller.</param>
    /// <returns>An awaitable <see cref="Task"/> containing the state of the response.</returns>
    public Task Respond(object? message)
    {
        if (Caller is AsyncCallerContext asyncCaller)
            return asyncCaller.Respond(message);
        else
            Caller.Respond(message);

        return Task.CompletedTask;
    }
}
