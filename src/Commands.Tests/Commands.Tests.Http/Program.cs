
using Commands;
using Commands.Hosting;
using Commands.Http;
using Commands.Parsing;
using Microsoft.Extensions.Hosting;
using System.Net;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureHttpComponents(context =>
{
    context.ConfigureOptions(options =>
    {
        options.Parsers[typeof(Version)] = new TryParseParser<Version>(Version.TryParse);
    });

    context.ConfigureListener(listener =>
    {
        listener.Prefixes.Add("http://localhost:5000/");
        listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
    });
});

var host = builder.Build();

host.UseComponents(components =>
{
    components.AddRange(typeof(Program).Assembly.GetExportedTypes());

    components.Add(new Command([HttpGet] () =>
    {
        return HttpResponse.Ok("OK!");
    }, "ping"));
});

await host.RunAsync();
