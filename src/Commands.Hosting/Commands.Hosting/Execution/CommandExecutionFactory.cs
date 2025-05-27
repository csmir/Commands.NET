namespace Commands.Hosting;

/// <summary>
///     Represents a factory for firing commands in a hosted environment, creating and configuring the scope for the execution.
/// </summary>
/// <remarks>
///     To customize the factory pattern, implement this class and override the available methods. When customizing the scope creation, you must also implement a custom <see cref="IExecutionScope"/> and populate it when the factory creates the scope.
/// </remarks>
public class CommandExecutionFactory : ICommandExecutionFactory
{
    private readonly IComponentProvider _executionProvider;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Creates a new instance of the <see cref="CommandExecutionFactory"/> using the provided services.
    /// </summary>
    public CommandExecutionFactory(IComponentProvider executionProvider, IServiceProvider serviceProvider, IEnumerable<ResultHandler> resultHandlers)
    {
        executionProvider.OnFailure += async (context, result, exception, services) =>
        {
            foreach (var handler in resultHandlers)
                await handler.Failure(context, result, exception, services);

            services.GetService<IExecutionScope>()?.Dispose();
        };

        executionProvider.OnSuccess += async (context, result, services) =>
        {
            foreach (var handler in resultHandlers)
                await handler.Success(context, result, services);

            services.GetService<IExecutionScope>()?.Dispose();
        };

        _executionProvider = executionProvider;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="context"/> is null.</exception>
    /// <exception cref="NotSupportedException">Thrown when the <see cref="IServiceProvider"/> cannot resolve the scoped <see cref="IExecutionScope"/> as its internal implementation. When customizing the <see cref="IExecutionScope"/> implementation, the factory must be overridden to support it.</exception>
    public virtual async Task StartExecution<TContext>(TContext context, HostedCommandOptions? options = null)
        where TContext : class, IContext
    {
        Assert.NotNull(context, nameof(context));

        var scope = _serviceProvider.CreateScope();

        var executeOptions = new ExecutionOptions()
        {
            SkipConditions = options?.SkipConditions ?? false,
            RemainderSeparator = options?.RemainderSeparator ?? ' ',
            ExecuteAsynchronously = options?.ExecuteAsynchronously ?? true,
            ServiceProvider = scope.ServiceProvider,
        };

        var execScope = CreateExecutionScope(context, scope, executeOptions);

        executeOptions.CancellationToken = execScope.CancellationSource.Token;

        await _executionProvider.Execute(context, executeOptions);
    }

    /// <summary>
    ///     Configures the scope for the execution context. This method is called when the factory creates a new scope for the command execution.
    /// </summary>
    /// <typeparam name="TContext">The type of <see cref="IContext"/> that serves as the context for this execution.</typeparam>
    /// <param name="context">The <see cref="IContext"/> implementation that serves as the context for this execution.</param>
    /// <param name="scope">The scope created to be configured by this method.</param>
    /// <param name="options">A set of options that change the pipeline behavior. This factory overrides a couple of settings, which are applied to the options provided to this method.</param>
    /// <returns>A configured implementation of <see cref="IExecutionScope"/> that represents the lifetime of the execution pipeline.</returns>
    /// <exception cref="NotSupportedException">Thrown when the <see cref="IServiceProvider"/> cannot resolve the scoped <see cref="IExecutionScope"/> as its internal implementation. When customizing the <see cref="IExecutionScope"/> implementation, the factory must be overridden to support it.</exception>
    protected virtual IExecutionScope CreateExecutionScope<TContext>(TContext context, IServiceScope scope, ExecutionOptions options)
        where TContext : class, IContext
    {
        var token = new CancellationTokenSource();

        var executionScope = scope.ServiceProvider.GetRequiredService<IExecutionScope>();

        executionScope.CancellationSource ??= token;
        executionScope.Context ??= context;
        executionScope.Scope ??= scope;

        return executionScope;
    }
}
