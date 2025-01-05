using Commands.Builders;

namespace Commands;

/// <summary>
///     Defines mechanisms for executing commands based on a set of arguments.
/// </summary>
public interface IExecutionProvider : IComponentCollection
{
    /// <summary>
    ///     Attempts to execute a command based on the provided <paramref name="args"/>.
    /// </summary>
    /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
    /// <param name="args">The input that is expected to discover, populate and invoke a target command.</param>
    /// <param name="options">A collection of options that determines pipeline logic.</param>
    public void TryExecute<T>(T caller, string? args, CommandOptions? options = null)
        where T : ICallerContext;

    /// <inheritdoc cref="TryExecute{T}(T, string, CommandOptions?)"/>
    public void TryExecute<T>(T caller, string[] args, CommandOptions? options = null)
        where T : ICallerContext;

    /// <inheritdoc cref="TryExecute{T}(T, string, CommandOptions?)"/>
    public void TryExecute<T>(T caller, ArgumentArray args, CommandOptions? options = null)
        where T : ICallerContext;

    /// <summary>
    ///     Attempts to execute a command based on the provided <paramref name="args"/>.
    /// </summary>
    /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
    /// <param name="args">The input that is expected to discover, populate and invoke a target command.</param>
    /// <param name="options">A collection of options that determines pipeline logic.</param>
    /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited or returned, even if <see cref="CommandOptions.AsynchronousExecution"/> is set to <see langword="true"/>.</returns>
    public Task TryExecuteAsync<T>(T caller, string? args, CommandOptions? options = null)
        where T : ICallerContext;

    /// <inheritdoc cref="TryExecuteAsync{T}(T, string, CommandOptions?)"/>
    public Task TryExecuteAsync<T>(T caller, string[] args, CommandOptions? options = null)
        where T : ICallerContext;

    /// <inheritdoc cref="TryExecuteAsync{T}(T, string, CommandOptions?)"/>
    public Task TryExecuteAsync<T>(T caller, ArgumentArray args, CommandOptions? options = null)
        where T : ICallerContext;
}
