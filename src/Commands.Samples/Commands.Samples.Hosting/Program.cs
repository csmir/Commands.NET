using Commands;
using Commands.Hosting;
using Commands.Parsing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureCommands(commands =>
    {
        commands.WithConfiguration(configure =>
        {
            configure.AddParser(new TryParseParser<Version>(Version.TryParse));
        });
        commands.AddResultHandler<ConsoleContext>((c, e, s) => c.Respond(s));
        commands.AddComponentTypes(typeof(Program).Assembly.GetExportedTypes());
    })
    .WithCommandFactory<HostedCommandExecutionFactory>()
    .ConfigureLogging(logging =>
    {
        logging.AddSimpleConsole();
    })
    .RunConsoleAsync();