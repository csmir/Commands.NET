namespace Commands.Hosting;

/// <summary>
///     Represents the execution of a command.
/// </summary>
/// <remarks>
///     The context is scoped to the command execution. A scope is created when the factory begins executing a command, and is disposed of when the command finishes executing.
/// </remarks>
public interface IExecutionScope : IDisposable
{
    /// <summary>
    ///     Gets the <see cref="IContext"/> resembling the metadata and response mechanism of the command being executed in the scope.
    /// </summary>
    public IContext Context { get; set; }

    /// <summary>
    ///     Gets the <see cref="IServiceScope"/> that contains the services used to execute the command in this scope. This scope is created when the command execution starts and disposed of when the command execution ends.
    /// </summary>
    public IServiceScope Scope { get; set; }

    /// <summary>
    ///     Gets a reference to the <see cref="CancellationToken"/> propagated through the execution pipeline. When this token is cancelled, the execution pipeline will be cancelled.
    /// </summary>
    public CancellationTokenSource CancellationSource { get; set; }

    /// <summary>
    ///     Populates the execution scope with the provided context, scope, and cancellation token source. This method is called when the factory creates a new scope for the command execution.
    /// </summary>
    /// <remarks>
    ///     This method is called by the <see cref="CommandExecutionFactory"/> when it starts executing a command. 
    ///     It is responsible for creating the state of the execution scope, including the context, scope, and cancellation token source. The implementation of this method should ensure that the state is properly initialized and ready for execution.
    /// </remarks>
    /// <param name="context">The context for execution.</param>
    /// <param name="scope">The scope for execution.</param>
    /// <param name="cancellationSource">The cancellation token source for execution.</param>
    public void CreateState(IContext context, IServiceScope scope, CancellationTokenSource cancellationSource);
}
