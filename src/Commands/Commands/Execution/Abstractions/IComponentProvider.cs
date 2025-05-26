using Commands.Conditions;
using Commands.Parsing;

namespace Commands;

/// <summary>
///     A provider containing a root set of components representing a tree of commands. The provider is responsible for starting, executing, and finalizing the command pipeline.
/// </summary>
public interface IComponentProvider
{
    /// <summary>
    ///     Gets the root set of components representing the tree of commands for this provider.
    /// </summary>
    /// <remarks>
    ///     Components added to this tree are available in runtime. The tree is concurrently accessible, meaning it is safe to add or remove components while the tree is being used.
    /// </remarks>
    public ComponentTree Components { get; }


    /// <summary>
    ///     Invoked when a command has failed to be executed.
    /// </summary>
    /// <remarks>
    ///     The exception in this operation represents where in the pipeline the failure has occurred, and how it failed. Depending on the <see cref="IResult"/> of the operation, it will contain different data:
    ///     <list type="bullet">
    ///         <item>
    ///             When <see cref="IResult"/> is <see cref="SearchResult"/>, the exception can be <see cref="CommandNotFoundException"/> or <see cref="CommandRouteIncompleteException"/>.
    ///         </item>
    ///         <item>
    ///             When <see cref="IResult"/> is <see cref="ParseResult"/>, the exception can be <see cref="ParserException"/> or <see cref="Exception"/> types emitted by custom <see cref="IParser"/> implementations.
    ///         </item>
    ///         <item>
    ///             When <see cref="IResult"/> is <see cref="ConditionResult"/>, the exception can be <see cref="ConditionException"/> or <see cref="Exception"/> types emitted by custom <see cref="ICondition"/> implementations.
    ///         </item>
    ///         <item>
    ///             When <see cref="IResult"/> is <see cref="InvokeResult"/>, the exception can be any exception thrown by the command handler, or where the command failed to execute properly.
    ///         </item>
    ///     </list>
    /// </remarks>
    public event Action<IContext, IResult, Exception, IServiceProvider>? OnFailure;

    /// <summary>
    ///     Invoked when a command has succesfully been executed.
    /// </summary>
    public event Action<IContext, IResult, IServiceProvider>? OnSuccess;

    /// <summary>
    ///     Executes the command pipeline using the provided <see cref="IContext"/>. 
    ///     This method will use the provided <paramref name="options"/> to determine how to execute the command, and then fire the pipeline.
    /// </summary>
    /// <typeparam name="TContext">The type implementing <see cref="IContext"/> which represents the context of the command, which can hold the state of the execution and response logic.</typeparam>
    /// <param name="context">The <see cref="IContext"/> which represents the context of the command, which can hold the state of the execution and response logic.</param>
    /// <param name="options">The options which determine how the execution is approached.</param>
    /// <returns>An awaitable <see cref="Task"/> which contains the state of the execution. When <see cref="ExecutionOptions.ExecuteAsynchronously"/> is <see langword="true"/>, this operation yields <see cref="Task.CompletedTask"/>.</returns>
    public Task Execute<TContext>(TContext context, ExecutionOptions? options = null)
        where TContext : class, IContext;
}
