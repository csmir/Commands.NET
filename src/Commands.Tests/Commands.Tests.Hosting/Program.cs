using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Commands.Builders;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands(builder =>
    {

    })
    .ConfigureLogging(builder =>
    {
        builder.AddSimpleConsole();
    })
    .RunConsoleAsync();
