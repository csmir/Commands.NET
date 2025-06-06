using System.ComponentModel;

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
    /// <param name="context">The context for execution.</param>
    /// <param name="scope">The scope for execution.</param>
    /// <param name="cancellationSource">The cancellation token source for execution.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Populate(IContext context, IServiceScope scope, CancellationTokenSource cancellationSource);
}
