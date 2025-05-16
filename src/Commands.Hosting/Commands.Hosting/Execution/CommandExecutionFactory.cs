namespace Commands.Hosting;

/// <summary>
///     Represents a factory for firing commands in a hosted environment, creating and configuring the scope for the execution.
/// </summary>
/// <remarks>
///     To customize the factory pattern, implement this class and override the available methods. When customizing the scope creation, you must also implement a custom <see cref="IExecutionScope"/> and populate it when the factory creates the scope.
/// </remarks>
/// <param name="executionProvider">The component collection representing all configured commands for the current host.</param>
/// <param name="serviceProvider">The global collection of services available for this host.</param>
public class CommandExecutionFactory(IComponentProvider executionProvider, IServiceProvider serviceProvider) : ICommandExecutionFactory
{
    /// <inheritdoc />
    public virtual async Task StartExecution<TContext>(TContext context, HostedCommandOptions? options = null)
        where TContext : class, IContext
    {
        var scope = serviceProvider.CreateScope();

        var executeOptions = new ExecutionOptions()
        {
            SkipConditions = options?.SkipConditions ?? false,
            RemainderSeparator = options?.RemainderSeparator ?? ' ',
            ExecuteAsynchronously = options?.ExecuteAsynchronously ?? true,
            ServiceProvider = scope.ServiceProvider,
        };

        var execScope = CreateExecutionScope(context, scope, executeOptions);

        executeOptions.CancellationToken = execScope.CancellationSource.Token;

        await executionProvider.Execute(context, executeOptions);
    }

    /// <summary>
    ///     Configures the scope for the execution context. This method is called when the factory creates a new scope for the command execution.
    /// </summary>
    /// <typeparam name="TContext">The type of <see cref="IContext"/> that serves as the context for this execution.</typeparam>
    /// <param name="context">The <see cref="IContext"/> implementation that serves as the context for this execution.</param>
    /// <param name="scope">The scope created to be configured by this method.</param>
    /// <param name="options">A set of options that change the pipeline behavior. This factory overrides a couple of settings, which are applied to the options provided to this method.</param>
    /// <returns>A configured implementation of <see cref="IExecutionScope"/> that represents the lifetime of the execution pipeline.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the <see cref="IServiceProvider"/> cannot resolve the scoped <see cref="IExecutionScope"/> as its internal implementation. When customizing the <see cref="IExecutionScope"/> implementation, the factory must be overridden to support it.</exception>
    protected virtual IExecutionScope CreateExecutionScope<TContext>(TContext context, IServiceScope scope, ExecutionOptions options)
        where TContext : class, IContext
    {
        var token = new CancellationTokenSource();

        var executionScope = scope.ServiceProvider.GetRequiredService<IExecutionScope>();

        if (executionScope is not ExecutionContext scopeImplementation)
            throw new InvalidOperationException($"Custom implementations of {nameof(IExecutionScope)} are not supported within the default {nameof(CommandExecutionFactory)}.");

        scopeImplementation.CancellationSource ??= token;
        scopeImplementation.Context ??= context;
        scopeImplementation.Scope ??= scope;

        return scopeImplementation;
    }
}
