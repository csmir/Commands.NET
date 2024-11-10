using Commands;
using Commands.Tests;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands((context, builder) =>
    {
        builder.AddResultResolver(new CustomResultResolver());
        builder.AddSourceResolver<CustomSourceResolver>();

        builder.AddCommand("command", (CommandContext<ConsumerBase> context) =>
        {
            Console.WriteLine("test");
        });
    })
    .ConfigureLogging(builder =>
    {
        builder.AddSimpleConsole();
    })
    .RunConsoleAsync();
