using Commands;
using Commands.Hosting;
using Commands.Http;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureHttpComponents(context =>
{
    context.ConfigureListener(listener =>
    {
        listener.Prefixes.Add("http://localhost:5000/");
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
