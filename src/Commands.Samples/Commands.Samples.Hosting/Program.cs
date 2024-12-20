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
            // Format a nice prompt in convention with Microsoft.Extensions.Logging, which will print app startup messages in a similar format.
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("cin");
            Console.ResetColor();
            Console.WriteLine(":  Commands.Samples.DelegateSourceProvider");
            Console.Write("      ");

            Console.CursorVisible = true;

            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return SourceResult.FromError();

            var context = new HostedCallerContext()
            {
                Input = input
            };

            return SourceResult.FromSuccess(context, input);
        });
        configure.AddResultHandler<HostedCallerContext>((context, result, services) =>
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