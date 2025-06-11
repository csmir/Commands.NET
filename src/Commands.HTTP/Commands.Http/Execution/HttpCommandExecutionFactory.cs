using Commands.Hosting;
using Microsoft.Extensions.Logging;

namespace Commands.Http;

/// <summary>
///     Represents a factory for executing commands over HTTP, using an <see cref="HttpListener"/> to listen for incoming requests.
/// </summary>
public class HttpCommandExecutionFactory(IComponentProvider executionProvider, IServiceProvider serviceProvider, ILogger<HttpCommandExecutionFactory> logger, IEnumerable<IResultHandler> resultHandlers, HttpListener httpListener)
    : CommandExecutionFactory(executionProvider, serviceProvider, logger, resultHandlers), IHostedService
{
    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        httpListener.Start();

        foreach (var prefix in httpListener.Prefixes)
            logger.LogInformation("Listening on {Prefix}", prefix);

        _ = StartListening(cancellationToken);

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

    private async Task Listened(Task<HttpListenerContext> contextTask)
    {
        var requestContext = await contextTask;

        var acquiredPrefixLength = -1;

        foreach (var prefix in httpListener.Prefixes)
        {
            var urlIndex = requestContext.Request.Url!.AbsoluteUri.IndexOf(prefix);

            // Find the best (shortest) matching prefix, so that the rest of the URL can be considered the command path.
            if (urlIndex >= 0 && (acquiredPrefixLength == -1 || urlIndex < acquiredPrefixLength))
                acquiredPrefixLength = prefix.Length;
        }

        var commandContext = new HttpCommandContext(requestContext, acquiredPrefixLength);

        logger.LogInformation("Received inbound request: {Request}", commandContext);

        await StartExecution(commandContext, new HostedCommandOptions()
        {
            ExecuteAsynchronously = false,
        });
    }
}
