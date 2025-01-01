using Commands;
using Commands.Builders;
using Commands.Samples;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands(configure =>
    {
        configure.WithTypes(typeof(Program).Assembly.GetExportedTypes());

        configure.AddSourceProvider((services) =>
        {
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return SourceResult.FromError();

            var context = new HostedCallerContext(input);

            return SourceResult.FromSuccess(context, input);
        });

        configure.AddResultHandler<HostedCallerContext>(async (context, result, services) =>
        {
            switch (result)
            {
                case InvokeResult invokeResult:
                    await context.Respond(result.Exception);
                    break;
                case SearchResult searchResult:
                    await context.Respond("Invalid command.");
                    break;
            }
        });
    })
    .ConfigureLogging(configure =>
    {
        configure.AddSimpleConsole();
    })
    .RunConsoleAsync();