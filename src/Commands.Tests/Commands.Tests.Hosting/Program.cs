using Commands.Core;
using Commands.Helpers;
using Commands.Tests;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands((context, builder) =>
    {
        builder.AddResultResolver<CustomResultResolver>();
        builder.AddSourceResolver<CustomSourceResolver>();

        builder.AddCommand((CommandContext context) =>
        {
            Console.WriteLine("test");

        }, "command");
    })
    .ConfigureLogging(builder =>
    {
        builder.AddSimpleConsole();
    })
    .RunConsoleAsync();