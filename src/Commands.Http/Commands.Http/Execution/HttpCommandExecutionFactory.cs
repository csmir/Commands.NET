﻿using Commands.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Commands.Http;

/// <summary>
///     Represents a factory for executing commands over HTTP, using an <see cref="HttpListener"/> to listen for incoming requests.
/// </summary>
public class HttpCommandExecutionFactory(IComponentProvider executionProvider, IServiceProvider serviceProvider, IConfiguration configuration, ILogger<HttpCommandExecutionFactory> logger, IEnumerable<ResultHandler> resultHandlers, HttpListener httpListener)
    : CommandExecutionFactory(executionProvider, serviceProvider, logger, resultHandlers), IHostedService
{
    private Task? _runningTask;
    private CancellationTokenSource? _linkedTokenSrc;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Configure remaining prefixes if any were provided in configuration.
            foreach (var prefix in configuration.GetSection("Commands:Http:Prefixes").GetChildren())
            {
                logger.LogDebug("Configuring HTTP prefix: {Prefix}", prefix.Value);

                if (string.IsNullOrWhiteSpace(prefix.Value))
                    continue;

                if (!httpListener.Prefixes.Contains(prefix.Value!))
                    httpListener.Prefixes.Add(prefix.Value!);
            }

            httpListener.Start();

            foreach (var prefix in httpListener.Prefixes)
                logger.LogInformation("Listening on {Prefix}", prefix);

            _linkedTokenSrc = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _runningTask = Task.Run(() => StartListening(_linkedTokenSrc.Token), _linkedTokenSrc.Token);
        }
        catch (HttpListenerException ex)
        {
            logger.LogError(ex, "Failed to start HTTP listener. Ensure that the application has permission to use the specified prefixes, or specify permissions in case none are set.");

            throw;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_runningTask is null)
            return;

        logger.LogInformation("Stopping {FactoryType}...", nameof(HttpCommandExecutionFactory));

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

        logger.LogInformation("{FactoryType} stopped.", nameof(HttpCommandExecutionFactory));
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
    public virtual async Task Listened(Task<HttpListenerContext> contextTask, CancellationToken cancellationToken)
    {
        var requestContext = await contextTask;

        var scope = CreateScope();

        scope.Context = new HttpCommandContext(requestContext, scope.Scope.ServiceProvider);

        logger.LogInformation("Received inbound request: {Request}", scope.Context);

        await ExecuteScope(scope, new()
        {
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
