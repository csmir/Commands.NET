using Microsoft.Extensions.Logging;

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

        _logger.LogDebug(
            "Starting execution for request: {Request} with options: SkipConditions = {SkipConditions}, ExecuteAsynchronously = {ExecuteAsynchronously}",
            context,
            executeOptions.SkipConditions,
            executeOptions.ExecuteAsynchronously
        );

        var token = new CancellationTokenSource();

        var executionScope = scope.ServiceProvider.GetRequiredService<IExecutionScope>();

        executionScope.CreateState(context, scope, token);
        executeOptions.CancellationToken = executionScope.CancellationSource.Token;

        _logger.LogDebug(
            "Created execution scope of type {ExecutionScopeType} for request: {Request}.",
            executionScope.GetType().Name,
            context
        );

        await Provider.Execute(context, executeOptions);
    }
}
