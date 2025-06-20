using Commands.Hosting;
using Microsoft.Extensions.Hosting;

namespace Commands.Samples;

// This class represents a command listener that runs in the background, reading commands from the console and executing them using the provided CommandExecutionFactory.
// This factory can be used to create and manage the execution of commands, handling their results and any exceptions that may occur during execution.
public sealed class CommandListener(CommandExecutionFactory factory) : BackgroundService
{
    // ExecuteAsync is inherited from BackgroundService, a base class for implementing long-running services in .NET. We can safely assume that this method will run in a background thread.
    // When the host is stopped, the stoppingToken will be triggered, allowing us to gracefully shut down the listener by exiting the loop.
    // A running command will not be interrupted, but no new commands will be accepted after that point.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // We create a default console context, which is a simple implementation of IContext that reads input from the console, and is able to send responses back to it.
            var context = new ConsoleContext(Console.ReadLine());

            // We start the execution of the command using the CommandExecutionFactory, which will handle the command's lifecycle, including parsing, executing, and handling results.
            await factory.StartExecution(context);
        }
    }
}
