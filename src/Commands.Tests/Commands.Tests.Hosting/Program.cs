using Commands.Helpers;
using Commands.Tests;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands((context, builder) =>
    {
        builder.AddResultResolver<CustomResultResolver>();
        builder.AddSourceResolver<CustomSourceResolver>();
    })
    .ConfigureLogging(builder =>
    {
        builder.AddSimpleConsole();
    })
    .RunConsoleAsync();