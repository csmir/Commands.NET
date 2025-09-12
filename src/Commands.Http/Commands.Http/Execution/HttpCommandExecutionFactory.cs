using Commands.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Commands.Http;

/// <summary>
///     Represents a factory for executing commands over HTTP, using an <see cref="HttpListener"/> to listen for incoming requests.
/// </summary>
public class HttpCommandExecutionFactory(IComponentProvider executionProvider, IServiceProvider serviceProvider, IEnumerable<ResultHandler> resultHandlers, HttpListener httpListener)
    : CommandExecutionFactory(executionProvider, serviceProvider, resultHandlers), IHostedService
{
    private Task? _runningTask;
    private CancellationTokenSource? _linkedTokenSrc;

    /// <summary>
    ///     Starts the <see cref="HttpListener"/> and begin listening for incoming HTTP requests. This method will not wait, escaping after starting the listener.
    /// </summary>
    /// <remarks>
    ///     Any additional prefixes specified in the under "Commands:Http:Prefixes" application's configuration (if configured) are added to the listener before starting.
    /// </remarks>
    /// <param name="cancellationToken">The token that when cancelled, will notify a linked token to stop the HTTP pipeline and cleanly escape the listening process.</param>
    /// <returns>An awaitable <see cref="Task"/> representing the result of the start operation.</returns>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var configuration = Services.GetService<IConfiguration>();

            if (configuration != null)
            {
                // Configure remaining prefixes if any were provided in configuration.
                foreach (var prefix in configuration.GetSection("Commands:Http:Prefixes").GetChildren())
                {
                    Logger?.LogDebug("Configuring HTTP prefix: {Prefix}", prefix.Value);

                    if (string.IsNullOrWhiteSpace(prefix.Value))
                        continue;

                    if (!httpListener.Prefixes.Contains(prefix.Value!))
                        httpListener.Prefixes.Add(prefix.Value!);
                }
            }

            httpListener.Start();

            foreach (var prefix in httpListener.Prefixes)
                Logger?.LogInformation("Listening on {Prefix}", prefix);

            _linkedTokenSrc = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _runningTask = Task.Run(() => StartListening(_linkedTokenSrc.Token), _linkedTokenSrc.Token);
        }
        catch (HttpListenerException ex)
        {
            Logger?.LogError(ex, "Failed to start HTTP listener. Ensure that the application has permission to use the specified prefixes, or specify permissions in case none are set.");

            throw;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Stops the <see cref="HttpListener"/> and waits for any ongoing request processing to complete before returning. This method will wait until all ongoing requests are processed or the provided <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    /// <param name="cancellationToken">The token that when cancelled, will force the listening process to break regardless of its execution state.</param>
    /// <returns>An awaitable <see cref="Task"/> representing the result of the stop operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_runningTask is null)
            return;

        Logger?.LogInformation("Stopping {FactoryType}...", nameof(HttpCommandExecutionFactory));

        try
        {
            _linkedTokenSrc?.Cancel();

            httpListener.Stop();
            httpListener.Close();
        }
        finally
        {
            await _runningTask.WaitAsync(cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }

        Logger?.LogInformation("{FactoryType} stopped.", nameof(HttpCommandExecutionFactory));
    }

    /// <summary>
    ///     Handles newly received HTTP requests by creating a command context and starting the execution of the command associated with the request. 
    /// </summary>
    /// <remarks>
    ///     This method can be overridden to provide custom handling of HTTP requests, such as logging, authentication, or other pre-processing steps before command execution.
    /// </remarks>
    /// <param name="contextTask">The received HTTP request context to handle.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>An awaitable <see cref="Task"/> awaited by the asynchronous context retriever to handle subsequent requests to the API.</returns>
    protected virtual async Task Listened(Task<HttpListenerContext> contextTask, CancellationToken cancellationToken)
    {
        var requestContext = await contextTask;

        var scope = CreateScope();

        scope.Context = new HttpCommandContext(requestContext, scope.Scope.ServiceProvider);

        Logger?.LogInformation("Received inbound request: {Request}", scope.Context);

        await ExecuteScope(scope, new()
        {
            ServiceProvider = scope.Scope.ServiceProvider,
            CancellationToken = cancellationToken,
        });
    }

    private async Task StartListening(CancellationToken cancellationToken)
    {
        while (httpListener.IsListening && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                await httpListener.GetContextAsync()
                    .ContinueWith((task) => Listened(task, cancellationToken), cancellationToken);
            }
            catch (HttpListenerException)
            {
                // Listener was stopped, exit the loop
                break;
            }
        }
    }
}
