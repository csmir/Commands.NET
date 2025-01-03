using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Commands.Samples;

public class Listener(ILogger<Listener> logger, IComponentTree tree) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Listening for commands...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var input = Console.ReadLine()!;

            var context = new HostedCallerContext(logger);

            await tree.ExecuteAsync(context, input);
        }

        logger.LogInformation("Stopped listening for commands.");
    }
}
