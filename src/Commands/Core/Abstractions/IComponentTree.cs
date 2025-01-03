using Commands.Builders;

namespace Commands;

/// <summary>
///     Defines mechanisms for executing commands based on a set of arguments.
/// </summary>
/// <remarks>
///     To start using this tree, call <see cref="ComponentTree.CreateBuilder"/> and configure it using the fluent API's implemented by <see cref="ITreeBuilder"/>.
/// </remarks>
public interface IComponentTree : IComponentCollection
{
    /// <summary>
    ///     Attempts to execute a command based on the provided <paramref name="args"/>.
    /// </summary>
    /// <remarks>
    ///     Synchronous approach to the <see cref="IComponentTree"/> ignores the <see cref="CommandOptions.AsynchronousExecution"/> option.
    /// </remarks>
    /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
    /// <param name="args">An unparsed input that is expected to discover, populate and invoke a target command.</param>
    /// <param name="options">A collection of options that determines pipeline logic.</param>
    public void Execute<T>(T caller, string args, CommandOptions? options = null)
        where T : ICallerContext;

    /// <inheritdoc cref="Execute{T}(T, string, CommandOptions?)"/>
    /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container and type condition to succeed evaluations.</param>
    /// <param name="args">A parsed set of arguments that are expected to discover, populate and invoke a target command.</param>
    /// <param name="options">A collection of options that determines pipeline logic.</param>
    public void Execute<T>(T caller, IEnumerable<object> args, CommandOptions? options = null)
        where T : ICallerContext;

    /// <inheritdoc cref="Execute{T}(T, IEnumerable{object}, CommandOptions?)"/>
    public void Execute<T>(T caller, IEnumerable<KeyValuePair<string, object?>> args, CommandOptions? options = null)
        where T : ICallerContext;

    /// <inheritdoc cref="Execute{T}(T, IEnumerable{object}, CommandOptions?)"/>
    public void Execute<T>(T caller, ArgumentEnumerator args, CommandOptions options)
        where T : ICallerContext;

    /// <summary>
    ///     Attempts to execute a command based on the provided <paramref name="args"/>.
    /// </summary>
    /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
    /// <param name="args">An unparsed input that is expected to discover, populate and invoke a target command.</param>
    /// <param name="options">A collection of options that determines pipeline logic.</param>
    /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.AsynchronousExecution"/> is set to <see langword="true"/>.</returns>
    public Task ExecuteAsync<T>(T caller, string args, CommandOptions? options = null)
        where T : ICallerContext;

    /// <inheritdoc cref="ExecuteAsync{T}(T, string, CommandOptions?)"/>
    /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container and type condition to succeed evaluations.</param>
    /// <param name="args">A parsed set of arguments that are expected to discover, populate and invoke a target command.</param>
    /// <param name="options">A collection of options that determines pipeline logic.</param>
    public Task ExecuteAsync<T>(T caller, IEnumerable<object> args, CommandOptions? options = null)
        where T : ICallerContext;

    /// <inheritdoc cref="ExecuteAsync{T}(T, IEnumerable{object}, CommandOptions?)"/>
    public Task ExecuteAsync<T>(T caller, IEnumerable<KeyValuePair<string, object?>> args, CommandOptions? options = null)
        where T : ICallerContext;

    /// <inheritdoc cref="ExecuteAsync{T}(T, IEnumerable{object}, CommandOptions?)"/>
    public Task ExecuteAsync<T>(T caller, ArgumentEnumerator args, CommandOptions options)
        where T : ICallerContext;
}
