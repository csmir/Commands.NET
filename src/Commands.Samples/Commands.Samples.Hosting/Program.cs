using Microsoft.Extensions.Hosting;
using Commands;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands((context, configuration) =>
    {

    })
    .RunConsoleAsync();