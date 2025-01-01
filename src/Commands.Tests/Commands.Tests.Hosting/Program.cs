using Commands.Builders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands(builder =>
    {

    })
    .ConfigureLogging(builder =>
    {
        builder.AddSimpleConsole();
    })
    .RunConsoleAsync();
