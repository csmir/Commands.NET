using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Commands.Samples;

public class Listener(ILogger<Listener> logger, ComponentManager manager) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Listening for commands...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var context = new HostedCallerContext(Console.ReadLine(), logger);

            await manager.ExecuteBlocking(context);
        }

        logger.LogInformation("Stopped listening for commands.");
    }
}
