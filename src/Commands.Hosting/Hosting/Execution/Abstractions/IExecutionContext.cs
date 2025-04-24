namespace Commands.Hosting;

/// <summary>
///     Represents the execution of a command. This context can be accessed during the hosted execution process through an implementation of <see cref="IExecutionFactory"/>.
/// </summary>
/// <remarks>
///     The context is scoped to the command execution. A scope is created when the factory begins executing a command, and is disposed of when the command finishes executing.
///     When implementing a custom <see cref="IExecutionFactory"/>, you must also implement a custom <see cref="IExecutionContext"/> and populate it when the factory creates the scope.
/// </remarks>
public interface IExecutionContext : IDisposable
{
    /// <summary>
    ///     Gets a reference to the <see cref="CancellationToken"/> propagated through the execution pipeline. When this token is cancelled, the execution pipeline will be cancelled.
    /// </summary>
    public CancellationTokenSource CancellationSource { get; }

    /// <summary>
    ///     Attempts to obtain an implementation of <see cref="ICallerContext"/> currently used by the execution pipeline.
    /// </summary>
    /// <typeparam name="TCaller">The implementation of <see cref="ICallerContext"/> this operation should output.</typeparam>
    /// <param name="caller">The output value, being an implementation of <see cref="ICallerContext"/> constrained to <typeparamref name="TCaller"/>.</param>
    /// <returns><see langword="true"/> when the <see cref="ICallerContext"/> is an implementation of <typeparamref name="TCaller"/>; otherwise <see langword="false"/>.</returns>
    public bool TryGetCaller<TCaller>([NotNullWhen(true)] out TCaller? caller)
        where TCaller : ICallerContext;
}
