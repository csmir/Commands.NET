using Commands.Hosting;
using Microsoft.Extensions.Logging;

namespace Commands.Http;

/// <summary>
///     Represents a factory for executing commands over HTTP, using an <see cref="HttpListener"/> to listen for incoming requests.
/// </summary>
public class HttpCommandExecutionFactory(IComponentProvider executionProvider, IServiceProvider serviceProvider, ILogger<HttpCommandExecutionFactory> logger, IEnumerable<ResultHandler> resultHandlers, HttpListener httpListener)
    : CommandExecutionFactory(executionProvider, serviceProvider, logger, resultHandlers), IHostedService
{
    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            httpListener.Start();

            foreach (var prefix in httpListener.Prefixes)
                logger.LogInformation("Listening on {Prefix}", prefix);

            _ = StartListening(cancellationToken);
        }
        catch (HttpListenerException ex)
        {
            logger.LogError(ex, "Failed to start HTTP listener. Ensure that the application has permission to use the specified prefixes.");

            throw;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        httpListener.Stop();
        httpListener.Close();

        logger.LogInformation("Stopping {FactoryType}...", nameof(HttpCommandExecutionFactory));

        return Task.CompletedTask;
    }

    private async Task StartListening(CancellationToken cancellationToken)
    {
        while (httpListener.IsListening && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                await httpListener.GetContextAsync().ContinueWith(Listened, cancellationToken);
            }
            catch (HttpListenerException)
            {
                // Listener was stopped, exit the loop
                break;
            }
        }
    }

    /// <summary>
    ///     Handles newly received HTTP requests by creating a command context and starting the execution of the command associated with the request. 
    /// </summary>
    /// <remarks>
    ///     This method can be overridden to provide custom handling of HTTP requests, such as logging, authentication, or other pre-processing steps before command execution.
    /// </remarks>
    /// <param name="contextTask">The received HTTP request context to handle.</param>
    /// <returns>An awaitable <see cref="Task"/> awaited by the asynchronous context retriever to handle subsequent requests to the API.</returns>
    public virtual async Task Listened(Task<HttpListenerContext> contextTask)
    {
        var requestContext = await contextTask;

        var scope = CreateScope();

        scope.Context = new HttpCommandContext(requestContext, scope.Scope.ServiceProvider);

        logger.LogInformation("Received inbound request: {Request}", scope.Context);

        await StartExecution(scope);
    }
}
