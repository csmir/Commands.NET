namespace Commands;

/// <summary>
///     Defines mechanisms for executing commands based on a set of arguments.
/// </summary>
public interface IExecutionProvider : IComponentCollection
{
    /// <summary>
    ///     Attempts to execute a command based on the provided <paramref name="caller"/>, and returns the result.
    /// </summary>
    /// <remarks>
    ///     This method is <b>blocking</b>, meaning that the execution will finish before returning control to the caller. The result of the execution is returned as a <see cref="Task{TResult}"/>.
    /// </remarks>
    /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
    /// <param name="options">A collection of options that determines pipeline logic.</param>
    /// <returns>An awaitable <see cref="Task"/> containing the result of the command execution. If <see cref="CommandOptions.ExecuteAsynchronously"/> is true, this task will contain no result.</returns>
    public Task<IResult?> Execute<T>(T caller, CommandOptions? options = null)
        where T : class, ICallerContext;
}
