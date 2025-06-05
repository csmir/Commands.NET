
using Commands.Hosting;
using Commands.Http;
using Commands.Parsing;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureHttpComponents(context =>
{
    context.Configure(options =>
    {
        options.Parsers[typeof(Version)] = new TryParseParser<Version>(Version.TryParse);
    });
});

var host = builder.Build();

host.UseComponents(components =>
{
    components.AddRange(typeof(Program).Assembly.GetExportedTypes());
});

await host.RunAsync();
