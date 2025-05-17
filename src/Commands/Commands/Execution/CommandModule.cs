namespace Commands;

/// <summary>
///     Represents a <see cref="CommandModule"/> that implements an constrained accessor to the <see cref="IContext"/>.
/// </summary>
/// <typeparam name="TContext">The implementation of <see cref="IContext"/> known during command pipeline execution.</typeparam>
public abstract class CommandModule<TContext> : CommandModule
    where TContext : IContext
{
    private TContext? _context;

    /// <summary>
    ///     Gets the context of the command currently being executed.
    /// </summary>
    /// <remarks>
    ///     This property assumes the type of <typeparamref name="TContext"/> is the same as the provided <see cref="IContext"/>.
    /// </remarks>
    /// <exception cref="InvalidCastException">Thrown when the context cannot be cast to <typeparamref name="TContext"/></exception>
    public new TContext Context
    {
        get
        {
            _context ??= base.Context is TContext ctx
            ? ctx
                : throw new InvalidCastException($"The context of type {typeof(TContext)} is not available in the current scope, being an implementation of {base.Context.GetType()}");

            return _context;
        }
    }
}

/// <summary>
///     The base type required for writing modular commands using Commands.NET. This type can be derived from freely, to extend or implement functionality. 
///     All modules are transient. They are injected and instantiated when command methods run, being disposed on return.
/// </summary>
/// <remarks>
///      When an <see cref="IComponentProvider"/> is created, all derived types must be passed to it for discovery and registration.
/// </remarks>
public abstract class CommandModule
{
    /// <summary>
    ///     Gets the context of the command currently being executed.
    /// </summary>
    public IContext Context { get; internal set; } = null!;

    /// <summary>
    ///     Gets the information about the command currently being executed.
    /// </summary>
    public Command Command { get; internal set; } = null!;

    /// <summary>
    ///     Sends a response using the context of the current command.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>An awaitable <see cref="Task"/> containing the result of this operation.</returns>
    public Task Respond(object? message)
    {
        if (Context is AsyncContext asyncCtx)
            return asyncCtx.Respond(message);

        Context.Respond(message);

        return Task.CompletedTask;
    }
}
