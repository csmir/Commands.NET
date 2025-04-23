namespace Commands.Hosting;

/// <summary>
///     
/// </summary>
/// <param name="collection"></param>
/// <param name="provider"></param>
public class HostedCommandExecutionFactory(ComponentCollection collection, IServiceProvider provider) : ICommandExecutionFactory
{
    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="caller"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public virtual Task StartExecution<TContext>(TContext caller, CommandOptions? options = null)
        where TContext : class, ICallerContext
    {
        var scope = provider.CreateScope();

        var token = new CancellationTokenSource();

        options ??= new CommandOptions()
        {
            CancellationToken = token.Token,
            ExecuteAsynchronously = true,
            ServiceProvider = scope.ServiceProvider,
        };

        var context = scope.ServiceProvider.GetRequiredService<IExecutionContext>();

        if (context is not ExecutionContext contextImplementation)
            throw new InvalidOperationException($"Custom implementations of {nameof(IExecutionContext)} are not supported within the default {nameof(HostedCommandExecutionFactory)}.");

        contextImplementation.CancellationSource ??= token;
        contextImplementation.Caller ??= caller;
        contextImplementation.Scope ??= scope;

        return collection.Execute(caller, options);
    }
}
