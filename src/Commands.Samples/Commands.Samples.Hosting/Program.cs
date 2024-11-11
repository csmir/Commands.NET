using Commands;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands((context, configuration) =>
    {

    })
    .RunConsoleAsync();