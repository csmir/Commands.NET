namespace Commands.Hosting;

/// <summary>
///     A factory for executing commands in a hosted context. Implement <see cref="CommandExecutionFactory"/> to customize the execution startup.
/// </summary>
public interface IExecutionFactory
{
    /// <summary>
    ///     Executes a command from the given context with the given options, creating and configuring a scope and binding it to the lifetime of the execution. 
    /// </summary>
    /// <remarks>
    ///     When this pipeline ends, the scope is disposed.
    /// </remarks>
    /// <typeparam name="TCaller">The type of <see cref="ICallerContext"/> that serves as the caller for this execution.</typeparam>
    /// <param name="caller">The <see cref="ICallerContext"/> implementation that serves as the caller for this execution.</param>
    /// <param name="options">A set of options that change the pipeline behavior. This factory overrides a couple of settings.</param>
    /// <returns>An instance of <see cref="IExecutionContext"/> which represents the scope of the command, its lifetime and the logic to dispose necessary resources.</returns>
    public Task<IExecutionContext> StartExecution<TCaller>(TCaller caller, HostedCommandOptions? options = null)
        where TCaller : class, ICallerContext;
}
