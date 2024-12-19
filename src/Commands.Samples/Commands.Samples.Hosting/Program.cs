using Commands;
using Commands.Builders;
using Commands.Samples;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands(configure =>
    {
        configure.AddSourceProvider((services) =>
        {
            Console.CursorVisible = true;
            Console.Write("> ");

            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return SourceResult.FromError();

            var context = new HostedCallerContext();

            return SourceResult.FromSuccess(context, input);
        });

        configure.AddResultHandler((context, result, services) =>
        {
            switch (result)
            {
                case InvokeResult invokeResult:
                    context.Respond(result.Exception);
                    break;
                case SearchResult searchResult:
                    context.Respond("Invalid command.");
                    break;
            }
        });
    })
    .ConfigureLogging(configure =>
    {
        configure.AddSimpleConsole();
    })
    .RunConsoleAsync();