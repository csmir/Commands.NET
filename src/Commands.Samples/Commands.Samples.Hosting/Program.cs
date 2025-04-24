using Commands;
using Commands.Hosting;
using Commands.Parsing;
using Commands.Samples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureComponents(commands =>
    {
        commands.WithConfiguration(configure =>
        {
            configure.AddParser(new TryParseParser<Version>(Version.TryParse));
        });
        commands.AddResultHandler<ConsoleCallerContext>((c, e, s) => c.Respond(e));
        commands.AddComponentTypes(typeof(Program).Assembly.GetExportedTypes());
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<CommandListener>();
        services.AddScoped<BasicService>();
    })
    .RunConsoleAsync();