using Commands.Core;
using Commands.Helpers;
using Commands.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands((context, configuration) =>
    {
        configuration.TryAddAssembly(Assembly.GetEntryAssembly());
        configuration.OnFailure((context, result, services) =>
        {
            Console.WriteLine(result);

            return Task.CompletedTask;
        });
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<CommandHandler>();
    })
    .ConfigureLogging(x =>
    {
        x.AddSimpleConsole();
    })
    .RunConsoleAsync();