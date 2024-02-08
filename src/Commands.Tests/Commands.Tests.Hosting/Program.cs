using Commands.Helpers;
using Commands.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands((context, builder) =>
    {
        builder.AddResultResolver((c, r, s) =>
        {
            Console.WriteLine(r);
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