namespace Commands;

/// <summary>
///     Defines mechanisms for executing commands based on a set of arguments.
/// </summary>
public interface IExecutionProvider : IComponentCollection
{
    /// <summary>
    ///     Attempts to execute a command based on the provided <paramref name="caller"/>.
    /// </summary>
    /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
    /// <param name="options">A collection of options that determines pipeline logic.</param>
    public IExecuteResult TryExecute<T>(T caller, CommandOptions? options = null)
        where T : ICallerContext;

    /// <summary>
    ///     Attempts to execute a command based on the provided <paramref name="caller"/>.
    /// </summary>
    /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
    /// <param name="options">A collection of options that determines pipeline logic.</param>
    /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited or returned, even if <see cref="CommandOptions.AsynchronousExecution"/> is set to <see langword="true"/>.</returns>
    public Task<IExecuteResult> TryExecuteAsync<T>(T caller, CommandOptions? options = null)
        where T : ICallerContext;
}
