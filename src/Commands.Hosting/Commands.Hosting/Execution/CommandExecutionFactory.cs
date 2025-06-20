using Microsoft.Extensions.Logging;
using static System.Formats.Asn1.AsnWriter;

namespace Commands.Hosting;

/// <summary>
///     Represents a factory for firing commands in a hosted environment, creating and configuring the scope for the execution.
/// </summary>
/// <remarks>
///     To customize the factory pattern, implement this class and override the available methods.
/// </remarks>
public class CommandExecutionFactory
{
    /// <summary>
    ///     Gets the component provider that is used to execute commands and handle their results.
    /// </summary>
    public IComponentProvider Provider { get; }

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    /// <summary>
    ///     Creates a new instance of the <see cref="CommandExecutionFactory"/> using the provided services.
    /// </summary>
    public CommandExecutionFactory(IComponentProvider execProvider, IServiceProvider serviceProvider, ILogger logger, IEnumerable<ResultHandler> resultHandlers)
    {
        Provider = execProvider;

        _serviceProvider = serviceProvider;
        _logger = logger;

        var handlers = resultHandlers.OrderBy(x => x.Order).ToArray();

        execProvider.OnFailure += async (context, result, exception, services) =>
        {
            foreach (var handler in handlers)
            {
                // If handled, break the loop to avoid multiple handlers processing the same result.
                if (await handler.Failure(context, result, exception, services))
                    break;
            }

            logger.LogError("Execution failure for request: {Request} with exception: {Exception}", context, result.Exception);

            services.GetService<IExecutionScope>()?.Dispose();
        };

        execProvider.OnSuccess += async (context, result, services) =>
        {
            foreach (var handler in handlers)
            {
                // If handled, break the loop to avoid multiple handlers processing the same result.
                if (await handler.Success(context, result, services))
                    break;
            }

            logger.LogInformation("Execution succeeded for request: {Request}.", context);

            services.GetService<IExecutionScope>()?.Dispose();
        };

        logger.LogInformation("Consuming {ExecutionProvider}, with {HandlerCount} result handler{MoreOrOne}.", Provider.GetType().FullName, handlers.Length, handlers.Length > 1 ? "(s)" : "");

        var commands = execProvider.Components.GetCommands().ToArray();

        foreach (var command in commands)
            logger.LogDebug("Registered {Command} as \"{Name}\".", command.ToString(), command.GetFullName());

        if (commands.Length == 0)
            logger.LogWarning("No commands discovered. The factory will not handle inbound requests.");
        else
            logger.LogInformation("Discovered {CommandCount} command{MoreOrOne}.", commands.Length, commands.Length > 1 ? "s" : "");
    }

    /// <summary>
    ///     Executes a command from the given context with the given options, creating and configuring a scope and binding it to the lifetime of the execution. 
    /// </summary>
    /// <remarks>
    ///     When this pipeline ends, the scope is disposed.
    /// </remarks>
    /// <param name="scope"> The <see cref="IExecutionScope"/> to use for the command execution. This scope is created by the factory and will be disposed of when the command execution ends.</param>
    /// <param name="options">A set of options that change the pipeline behavior. This factory overrides a couple of settings.</param>
    /// <returns>An awaitable <see cref="Task"/> representing the execution of this operation.</returns>
    /// <exception cref="NotSupportedException">Thrown when the <see cref="IServiceProvider"/> cannot resolve the scoped <see cref="IExecutionScope"/> as its internal implementation. When customizing the <see cref="IExecutionScope"/> implementation, the factory must be overridden to support it.</exception>
    public virtual async Task ExecuteScope(IExecutionScope scope, ExecutionOptions? options = null)
    {
        Assert.NotNull(scope, nameof(scope));
        Assert.NotNull(scope.Context, nameof(scope.Context));

        options ??= ExecutionOptions.Default;

        _logger.LogDebug(
            "Starting execution for request: {Request}",
            scope.Context
        );

        await Provider.Execute(scope.Context, options);
    }

    /// <summary>
    ///     Creates a new execution scope for the command execution, which can be used to execute commands and handle their results.
    /// </summary>
    /// <param name="context">The <see cref="IContext"/> to populate this scope with. If not immediately provided, this can be set before <see cref="ExecuteScope(IExecutionScope, ExecutionOptions?)"/> is called.</param>
    /// <returns>A new <see cref="IExecutionScope"/> implementation from the <see cref="IServiceProvider"/> provided to this factory.</returns>
    public virtual IExecutionScope CreateScope(IContext? context = null)
    {
        var scope = _serviceProvider.CreateScope();

        var executionScope = scope.ServiceProvider.GetRequiredService<IExecutionScope>();

        executionScope.Scope = scope;
        executionScope.Context = context!;

        _logger.LogDebug(
            "Created execution scope of type {ExecutionScopeType}.",
            executionScope.GetType().Name
        );

        return executionScope;
    }
}
