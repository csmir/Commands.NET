namespace Commands.Hosting;

/// <summary>
///     
/// </summary>
public interface ICommandExecutionFactory
{
    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="caller"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public Task StartExecution<TContext>(TContext caller, CommandOptions? options = null)
        where TContext : class, ICallerContext;
}
