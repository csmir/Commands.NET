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
    ///     Executes the command pipeline using the provided <see cref="ICallerContext"/>. 
    ///     This method will use the provided <paramref name="options"/> to determine how to execute the command, and then fire the pipeline.
    /// </summary>
    /// <typeparam name="T">The type implementing <see cref="ICallerContext"/> which represents the caller/requester/source of the command, which can hold the state of the execution and response logic.</typeparam>
    /// <param name="caller">The <see cref="ICallerContext"/> which represents the caller/requester/source of the command, which can hold the state of the execution and response logic.</param>
    /// <param name="options">The options which determine how the execution is approached.</param>
    /// <returns>An awaitable <see cref="Task"/> which contains the state of the execution. When <see cref="ExecutionOptions.ExecuteAsynchronously"/> is <see langword="true"/>, this operation yields <see cref="Task.CompletedTask"/>.</returns>
    public Task Execute<T>(T caller, ExecutionOptions? options = null)
        where T : class, ICallerContext;
}
