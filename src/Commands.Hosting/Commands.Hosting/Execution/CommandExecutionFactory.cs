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

    /// <summary>
    ///     Gets the logger instance used for logging within the factory. This logger is obtained from the provided <see cref="IServiceProvider"/> during construction, if possible. Otherwise, it may be null.
    /// </summary>
    protected ILogger? Logger { get; }

    /// <summary>
    ///     Gets the service provider used to resolve dependencies and create scopes for command execution.
    /// </summary>
    protected IServiceProvider Services { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="CommandExecutionFactory"/> using the provided services.
    /// </summary>
    public CommandExecutionFactory(IComponentProvider execProvider, IServiceProvider serviceProvider, IEnumerable<ResultHandler> resultHandlers)
    {
        Provider = execProvider;

        Services = serviceProvider;
        
        Logger = serviceProvider.GetService<ILogger<CommandExecutionFactory>>();

        var handlers = resultHandlers.OrderBy(x => x.Order).ToArray();

        execProvider.OnFailure += async (context, result, exception, services) =>
        {
            foreach (var handler in handlers)
            {
                // If handled, break the loop to avoid multiple handlers processing the same result.
                if (await handler.Failure(context, result, exception, services))
                    break;
            }

            Logger?.LogError("Execution failure for request: {Request} with exception: {Exception}", context, result.Exception);

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

            if (Logger?.IsEnabled(LogLevel.Information) == true)
                Logger.LogInformation("Execution succeeded for request: {Request}.", context);

            services.GetService<IExecutionScope>()?.Dispose();
        };

        if (Logger?.IsEnabled(LogLevel.Information) == true)
            Logger.LogInformation("Consuming {ExecutionProvider}, with {HandlerCount} result handler{MoreOrOne}.", Provider.GetType().FullName, handlers.Length, handlers.Length != 1 ? "(s)" : "");

        var commands = execProvider.Components.GetCommands().ToArray();

        foreach (var command in commands)
            if (Logger?.IsEnabled(LogLevel.Debug) == true)
                Logger.LogDebug("Registered {Command} as \"{Name}\".", command.ToString(), command.GetFullName());

        if (commands.Length == 0)
        {
            if (Logger?.IsEnabled(LogLevel.Warning) == true)
                Logger.LogWarning("No commands discovered. The factory will not handle inbound requests.");
        }
        else
        {
            if (Logger?.IsEnabled(LogLevel.Information) == true)
                Logger.LogInformation("Discovered {CommandCount} command{MoreOrOne}.", commands.Length, commands.Length > 1 ? "s" : "");
        }
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
        ArgumentNullException.ThrowIfNull(scope);

        if (scope?.Context is null)
            throw new InvalidOperationException($"{nameof(IExecutionScope)} must have a context set before executing it. Use {nameof(CreateScope)} with a provided context or assign the scope before it enters {nameof(ExecuteScope)}.");

        options ??= ExecutionOptions.Default;

        if (Logger?.IsEnabled(LogLevel.Debug) == true)
        {
            Logger.LogDebug(
                "Starting execution for request: {Request}",
                scope.Context
            );
        }

        await Provider.Execute(scope.Context, options);
    }

    /// <summary>
    ///     Creates a new execution scope for the command execution, which can be used to execute commands and handle their results.
    /// </summary>
    /// <param name="context">The <see cref="IContext"/> to populate this scope with. If not immediately provided, this can be set before <see cref="ExecuteScope(IExecutionScope, ExecutionOptions?)"/> is called.</param>
    /// <returns>A new <see cref="IExecutionScope"/> implementation from the <see cref="IServiceProvider"/> provided to this factory.</returns>
    public virtual IExecutionScope CreateScope(IContext? context = null)
    {
        var scope = Services.CreateScope();

        var executionScope = scope.ServiceProvider.GetRequiredService<IExecutionScope>();

        executionScope.Scope = scope;
        executionScope.Context = context!;

        if (Logger?.IsEnabled(LogLevel.Debug) == true)
        {
            Logger.LogDebug(
                "Created execution scope of type {ExecutionScopeType}.",
                executionScope.GetType().Name
            );
        }

        return executionScope;
    }
}
