using Commands;
using Commands.Hosting;
using Commands.Parsing;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureComponents(commands =>
    {
        commands.WithConfiguration(configure =>
        {
            configure.AddParser(new TryParseParser<Version>(Version.TryParse));
        });
        commands.AddResultHandler<ConsoleContext>((c, e, s) => c.Respond(s));
        commands.AddComponentTypes(typeof(Program).Assembly.GetExportedTypes());
    })
    .RunConsoleAsync();