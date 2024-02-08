using Commands.Helpers;
using Commands.Parsing;
using Commands.Results;
using Commands.Tests.Hosting.Resolvers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands((context, builder) =>
    {
        builder.AddResultResolver((c, r, s) =>
        {
            Console.WriteLine(r);
        });
        builder.AddSourceResolver(() =>
        {
            var src = Console.ReadLine();

            Console.WriteLine(src);

            return SourceResult.FromSuccess(new(), StringParser.Parse(src));
        });
        builder.AddSourceResolver<CustomSourceResolver>();
    })
    .ConfigureServices((context, services) =>
    {
        //services.AddHostedService<CommandHandler>();
    })
    .ConfigureLogging(x =>
    {
        x.AddSimpleConsole();
    })
    .RunConsoleAsync();