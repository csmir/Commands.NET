using Commands;
using Commands.Hosting;
using Commands.Parsing;
using Commands.Samples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    //.ConfigureComponents(context =>
    //{
    //    context.Configuration.Parsers.Add(typeof(Version), new TryParseParser<Version>(Version.TryParse));

    //    context.Components.AddResultHandler<ConsoleCallerContext>((c, e, s) => c.Respond(e));
    //    context.Components.AddComponentTypes(typeof(Program).Assembly.GetExportedTypes());
    //})
    .ConfigureServices(services =>
    {
        services.AddHostedService<CommandListener>();
        services.AddScoped<BasicService>();
    })
    .RunConsoleAsync();