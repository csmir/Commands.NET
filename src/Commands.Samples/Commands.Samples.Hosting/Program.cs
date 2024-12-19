using Commands;
using Commands.Builders;
using Commands.Samples;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands(configure =>
    {
        configure.AddSourceResolver((services) =>
        {
            Console.CursorVisible = true;
            Console.Write("> ");

            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return SourceResult.FromError();

            var context = new HostedContext();

            return SourceResult.FromSuccess(context, input);
        });

        configure.AddResultResolver((context, result, services) =>
        {
            if (!result.Success)
            {
                switch (result)
                {
                    case InvokeResult invokeResult:
                        Console.WriteLine(invokeResult.Exception);
                        break;
                    case SearchResult searchResult:
                        Console.WriteLine("Invalid command.");
                        break;
                }
            }
        });
    })
    .ConfigureLogging(configure =>
    {
        configure.AddSimpleConsole();
    })
    .RunConsoleAsync();