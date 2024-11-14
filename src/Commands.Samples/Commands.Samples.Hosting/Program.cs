using Commands;
using Commands.Results;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands((context, configuration) =>
    {
        configuration.AddSourceResolver(() =>
        {
            var input = Console.ReadLine();

            if (input == null)
            {
                return SourceResult.FromError();
            }

            return SourceResult.FromSuccess(new ConsumerBase(), input);
        });
    })
    .RunConsoleAsync();