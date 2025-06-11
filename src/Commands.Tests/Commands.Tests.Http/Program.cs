using Commands;
using Commands.Hosting;
using Commands.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureHttpComponents(context =>
{
    context.ConfigureListener(listener =>
    {
        listener.Prefixes.Add("http://localhost:5000/");
    });

    context.ConfigureOptions(options =>
    {
        options.BuildCompleted = (component) =>
        {
            Console.WriteLine(component.GetFullName());
        };
    });
});

builder.ConfigureLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Debug);
});

var host = builder.Build();

host.UseComponents(components =>
{
    components.AddRange(typeof(Program).Assembly.GetExportedTypes());

    components.Add(new Command([HttpGet] () =>
    {
        return HttpResult.Ok("OK!");
    }, "ping"));

    components.Add(new Command([HttpGet] (IComponentProvider components, IContext context) =>
    {
        var commands = components.Components.GetCommands();

        var response = new StringBuilder("Available commands:\n");

        foreach (var command in commands)
            response.AppendLine($"- {command}");

        return HttpResult.Ok(response.ToString());
    }, "help"));
});

await host.RunAsync();
