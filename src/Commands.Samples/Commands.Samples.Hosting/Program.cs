using Commands;
using Commands.Helpers;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands((context, configuration) =>
    {

    })
    .RunConsoleAsync();