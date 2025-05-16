namespace Commands.Hosting;

/// <summary>
///     Represents the execution of a command. This context can be accessed during the hosted execution process through an implementation of <see cref="ICommandExecutionFactory"/>.
/// </summary>
/// <remarks>
///     The context is scoped to the command execution. A scope is created when the factory begins executing a command, and is disposed of when the command finishes executing.
///     When implementing a custom <see cref="ICommandExecutionFactory"/>, you must also implement a custom <see cref="IExecutionScope"/> and populate it when the factory creates the scope.
/// </remarks>
public interface IExecutionScope : IDisposable
{
    /// <summary>
    ///     Gets a reference to the <see cref="CancellationToken"/> propagated through the execution pipeline. When this token is cancelled, the execution pipeline will be cancelled.
    /// </summary>
    public CancellationTokenSource CancellationSource { get; }

    /// <summary>
    ///     Attempts to obtain an implementation of <see cref="IContext"/> currently used by the execution pipeline.
    /// </summary>
    /// <typeparam name="TContext">The implementation of <see cref="IContext"/> this operation should output.</typeparam>
    /// <param name="context">The output value, being an implementation of <see cref="IContext"/> constrained to <typeparamref name="TContext"/>.</param>
    /// <returns><see langword="true"/> when the <see cref="IContext"/> is an implementation of <typeparamref name="TContext"/>; otherwise <see langword="false"/>.</returns>
    public bool TryGetContext<TContext>([NotNullWhen(true)] out TContext? context)
        where TContext : IContext;
}
