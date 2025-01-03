namespace Commands;

/// <summary>
///     Represents a <see cref="CommandModule"/> that implements an implementation-friendly accessor to the <see cref="ICallerContext"/>.
/// </summary>
/// <typeparam name="T">The implementation of <see cref="ICallerContext"/> known during command pipeline execution.</typeparam>
public abstract class CommandModule<T> : CommandModule
    where T : ICallerContext
{
    private T? _caller;

    /// <summary>
    ///     Gets the caller that requested this command to be executed.
    /// </summary>
    /// <remarks>
    ///     This property assumes the type of <typeparamref name="T"/> is the same as the provided <see cref="ICallerContext"/>.
    /// </remarks>
    /// <exception cref="InvalidCastException">Thrown when the context cannot be cast to <typeparamref name="T"/></exception>
    public new T Caller
       => _caller ??= base.Caller is T caller ? caller : throw new InvalidCastException($"{base.Caller.GetType()} cannot be cast to {typeof(T)}.");
}

/// <summary>
///     The base type required for writing commands using Commands.NET. This type can be derived from freely, to extend or implement modular command functionality. 
///     All modules are transient. They are injected and instantiated when command methods run, being disposed on return.
/// </summary>
/// <remarks>
///      When an <see cref="IComponentTree"/> is created, all derived types must be passed to it for discovery and registration.
/// </remarks>
public abstract class CommandModule
{
    /// <summary>
    ///     Gets the caller that requested this command to be executed.
    /// </summary>
    public ICallerContext Caller { get; internal set; } = null!;

    /// <summary>
    ///     Gets the information about the command currently being executed.
    /// </summary>
    public Command Command { get; internal set; } = null!;

    /// <summary>
    ///     Gets the command tree that is responsible for the current command execution.
    /// </summary>
    public IComponentTree Tree { get; internal set; } = null!;

    /// <summary>
    ///     Sends a response to the caller.
    /// </summary>
    /// <param name="message">The message to send to the caller.</param>
    /// <returns>An awaitable <see cref="Task"/> containing the result of this operation.</returns>
    public Task Respond(object? message)
    {
        if (Caller is AsyncCallerContext asyncCaller)
            return asyncCaller.Respond(message);

        Caller.Respond(message);

        return Task.CompletedTask;
    }
}
