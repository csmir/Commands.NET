using Commands.Helpers;
using Commands.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands((context, configuration) =>
    {

    })
    .ConfigureServices((context, services) =>
    {
        services.TryAddResolver((context, result, services) =>
        {
            Console.WriteLine(result);
        });

        services.AddHostedService<CommandHandler>();
    })
    .ConfigureLogging(x =>
    {
        x.AddSimpleConsole();
    })
    .RunConsoleAsync();