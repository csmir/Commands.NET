using System.Reflection;

namespace Commands.Hosting;

/// <summary>
///     Represents a factory for firing commands in a hosted environment, creating and configuring the scope for the execution.
/// </summary>
/// <remarks>
///     To customize the factory pattern, implement this class and override the available methods. When customizing the scope creation, you must also implement a custom <see cref="IExecutionScope"/> and populate it when the factory creates the scope.
/// </remarks>
public class CommandExecutionFactory
{
    private readonly IComponentProvider _executionProvider;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Creates a new instance of the <see cref="CommandExecutionFactory"/> using the provided services.
    /// </summary>
    public CommandExecutionFactory(IComponentProvider executionProvider, IServiceProvider serviceProvider, IEnumerable<IResultHandler> resultHandlers)
    {
        var orderedHandlers = resultHandlers.OrderBy(x => x.GetType().GetCustomAttribute<PriorityAttribute>()?.Priority ?? 0).ToList();

        executionProvider.OnFailure += async (context, result, exception, services) =>
        {
            foreach (var handler in orderedHandlers)
            {
                // If handled, break the loop to avoid multiple handlers processing the same result.
                if (await handler.Failure(context, result, exception, services))
                    break;
            }

            services.GetService<IExecutionScope>()?.Dispose();
        };

        executionProvider.OnSuccess += async (context, result, services) =>
        {
            foreach (var handler in orderedHandlers)
            {
                // If handled, break the loop to avoid multiple handlers processing the same result.
                if (await handler.Success(context, result, services))
                    break;
            }

            services.GetService<IExecutionScope>()?.Dispose();
        };

        _executionProvider = executionProvider;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     Executes a command from the given context with the given options, creating and configuring a scope and binding it to the lifetime of the execution. 
    /// </summary>
    /// <remarks>
    ///     When this pipeline ends, the scope is disposed.
    /// </remarks>
    /// <typeparam name="TContext">The type of <see cref="IContext"/> that serves as the context for this execution.</typeparam>
    /// <param name="context">The <see cref="IContext"/> implementation that serves as the context for this execution.</param>
    /// <param name="options">A set of options that change the pipeline behavior. This factory overrides a couple of settings.</param>
    /// <returns>An instance of <see cref="IExecutionScope"/> which represents the scope of the command, its lifetime and the logic to dispose necessary resources.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="context"/> is null.</exception>
    /// <exception cref="NotSupportedException">Thrown when the <see cref="IServiceProvider"/> cannot resolve the scoped <see cref="IExecutionScope"/> as its internal implementation. When customizing the <see cref="IExecutionScope"/> implementation, the factory must be overridden to support it.</exception>
    public async Task StartExecution<TContext>(TContext context, HostedCommandOptions? options = null)
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

        var token = new CancellationTokenSource();

        var executionScope = scope.ServiceProvider.GetRequiredService<IExecutionScope>();

        executionScope.CreateState(context, scope, token);
        executeOptions.CancellationToken = executionScope.CancellationSource.Token;

        await _executionProvider.Execute(context, executeOptions);
    }
}
