using Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .UseCommands()
    .ConfigureLogging(builder =>
    {
        builder.AddSimpleConsole();
    })
    .RunConsoleAsync();
