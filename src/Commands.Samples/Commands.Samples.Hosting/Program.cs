using Commands.Hosting;
using Commands.Parsing;
using Commands.Samples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureComponents(context =>
{
    context.ConfigureOptions(options =>
    {
        options.Parsers[typeof(Version)] = new TryParseParser<Version>(Version.TryParse);
    });
});

builder.ConfigureServices(services =>
{
    services.AddHostedService<CommandListener>();
    services.AddScoped<BasicService>();
});

var host = builder.Build();

host.UseComponents(tree =>
{
    tree.AddRange(typeof(Program).Assembly.GetExportedTypes());
});

await host.RunAsync();