namespace Commands.Hosting;

/// <summary>
///     Represents a factory for firing commands in a hosted environment, creating and configuring the scope for the execution.
/// </summary>
/// <remarks>
///     To customize the factory pattern, implement this class and override the available methods. When customizing the scope creation, you must also implement a custom <see cref="IExecutionContext"/> and populate it when the factory creates the scope.
/// </remarks>
/// <param name="executionProvider">The component collection representing all configured commands for the current host.</param>
/// <param name="serviceProvider">The global collection of services available for this host.</param>
public class CommandExecutionFactory(IExecutableComponentSet executionProvider, IServiceProvider serviceProvider) : ICommandExecutionFactory
{
    /// <inheritdoc />
    public virtual async Task StartExecution<TCaller>(TCaller caller, HostedCommandOptions? options = null)
        where TCaller : class, ICallerContext
    {
        var scope = serviceProvider.CreateScope();

        var executeOptions = new CommandOptions()
        {
            SkipConditions = options?.SkipConditions ?? false,
            RemainderSeparator = options?.RemainderSeparator ?? ' ',
            ExecuteAsynchronously = options?.ExecuteAsynchronously ?? true,
            ServiceProvider = scope.ServiceProvider,
        };

        var context = CreateContext(caller, scope, executeOptions);

        executeOptions.CancellationToken = context.CancellationSource.Token;

        await executionProvider.Execute(caller, executeOptions);
    }

    /// <summary>
    ///     Configures the scope for the execution context. This method is called when the factory creates a new scope for the command execution.
    /// </summary>
    /// <typeparam name="TCaller">The type of <see cref="ICallerContext"/> that serves as the caller for this execution.</typeparam>
    /// <param name="caller">The <see cref="ICallerContext"/> implementation that serves as the caller for this execution.</param>
    /// <param name="scope">The scope created to be configured by this method.</param>
    /// <param name="options">A set of options that change the pipeline behavior. This factory overrides a couple of settings, which are applied to the options provided to this method.</param>
    /// <returns>A configured implementation of <see cref="IExecutionContext"/> that represents the lifetime of the execution pipeline.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the <see cref="IServiceProvider"/> cannot resolve the scoped <see cref="IExecutionContext"/> as its internal implementation. When customizing the <see cref="IExecutionContext"/> implementation, the factory must be overridden to support it.</exception>
    protected virtual IExecutionContext CreateContext<TCaller>(TCaller caller, IServiceScope scope, CommandOptions options)
        where TCaller : class, ICallerContext
    {
        var token = new CancellationTokenSource();

        var context = scope.ServiceProvider.GetRequiredService<IExecutionContext>();

        if (context is not ExecutionContext contextImplementation)
            throw new InvalidOperationException($"Custom implementations of {nameof(IExecutionContext)} are not supported within the default {nameof(CommandExecutionFactory)}.");

        contextImplementation.CancellationSource ??= token;
        contextImplementation.Caller ??= caller;
        contextImplementation.Scope ??= scope;

        return contextImplementation;
    }
}
