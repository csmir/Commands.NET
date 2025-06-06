using Commands.Hosting;

namespace Commands.Http;

/// <summary>
///     Represents a factory for executing commands over HTTP, using an <see cref="HttpListener"/> to listen for incoming requests.
/// </summary>
public class HttpCommandExecutionFactory(IComponentProvider executionProvider, IServiceProvider serviceProvider, IEnumerable<ResultHandler> resultHandlers, HttpListener httpListener)
    : CommandExecutionFactory(executionProvider, serviceProvider, resultHandlers), IHostedService
{
    private Task? _listenerTask;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        httpListener.Start();

        _listenerTask = new Task(() =>
        {
            while (httpListener.IsListening)
            {
                try
                {
                    // Begin accepting requests asynchronously
                    httpListener.BeginGetContext(async (result) => await OnRequestReceived(result), null);
                }
                catch (HttpListenerException)
                {
                    // Listener was stopped, exit the loop
                    break;
                }
            }
        }, cancellationToken);

        _listenerTask.Start();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        httpListener.Stop();
        httpListener.Close();

        return _listenerTask?.WaitAsync(cancellationToken) ?? Task.CompletedTask;
    }

    #region Internals

    private Task OnRequestReceived(IAsyncResult result)
    {
        var requestContext = httpListener.EndGetContext(result);
        var acquiredPrefixLength = -1;

        foreach (var prefix in httpListener.Prefixes)
        {
            var urlIndex = requestContext.Request.RawUrl!.IndexOf(prefix);

            // Find the best (shortest) matching prefix, so that the rest of the URL can be considered the command path.
            if (urlIndex >= 0 && (acquiredPrefixLength == -1 || urlIndex < acquiredPrefixLength))
                acquiredPrefixLength = prefix.Length;
        }

        var commandContext = new HttpCommandContext(requestContext, acquiredPrefixLength);

        return StartExecution(commandContext, new HostedCommandOptions()
        {
            ExecuteAsynchronously = true,
        });
    }

    #endregion
}
