using Commands.Conditions;
using Commands.Parsing;

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
    /// <returns>An awaitable <see cref="Task"/> containing the result of the command execution.</returns>
    public Task<IExecuteResult> ExecuteBlocking<T>(T caller, CommandOptions? options = null)
        where T : ICallerContext;

    /// <summary>
    ///     Attempts to execute a command based on the provided <paramref name="caller"/>. 
    /// </summary>
    /// <remarks>
    ///     This method is <b>non-blocking</b>, returning control to the caller immediately after the entrypoint is called. 
    ///     <br/>
    ///     This approach drastically changes the behavior of execution, and as such, there are a few things to consider when using it:
    ///     <list type="bullet">
    ///         <item>
    ///             The end-user must provide an implementation of <see cref="ResultHandler"/> to the <see cref="ComponentManager"/> that is used to execute the command, in order to handle the result of the command.
    ///         </item>
    ///         <item>
    ///             Objects, specifically those scoped to more than a single command must be made thread-safe, meaning they must be able to handle multiple requests at once.
    ///         </item>
    ///     </list>
    ///     When considering thread-safety, it is important to know what this actually means.
    ///     <br/>
    ///     For more information, consider reading this article: <see href="https://learn.microsoft.com/en-us/dotnet/standard/threading/managed-threading-best-practices"/>
    /// </remarks>
    /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
    /// <param name="options">A collection of options that determines pipeline logic.</param>
    public Task Execute<T>(T caller, CommandOptions? options = null)
        where T : ICallerContext;
}
