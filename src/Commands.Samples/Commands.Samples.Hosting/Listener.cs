using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Commands.Samples;

// The listener is a hosted service that listens for commands from the console.
// It uses the component manager to execute the commands from retrieved console input.
public sealed class Listener(ILogger<Listener> logger, ComponentCollection components) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Listening for commands...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var context = new HostedCallerContext(Console.ReadLine(), logger);

            await components.Execute(context);
        }

        logger.LogInformation("Stopped listening for commands.");
    }
}
