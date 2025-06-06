using Commands.Hosting;
using Microsoft.Extensions.Hosting;

namespace Commands.Samples;

public sealed class CommandListener(CommandExecutionFactory factory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var context = new ConsoleContext(Console.ReadLine());

            await factory.StartExecution(context);
        }
    }
}
